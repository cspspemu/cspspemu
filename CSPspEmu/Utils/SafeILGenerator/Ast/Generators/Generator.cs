//#define USE_NORMAL_INVOKE

using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SafeILGenerator.Ast.Generators
{
    public delegate void MapDelegate(object This, AstNode astNode);

    public class MappingInfo
    {
#if USE_NORMAL_INVOKE
		public MethodInfo MethodInfo;
#else
        public MapDelegate Action;
#endif

        public void Call(object This, AstNode astNode)
        {
#if USE_NORMAL_INVOKE
			MethodInfo.Invoke(This, new object[] { AstNode });
#else
            Action(This, astNode);
#endif
        }

#if USE_NORMAL_INVOKE
		static public MappingInfo FromMethodInfo<T>(Generator<T> that, MethodInfo MethodInfo)
		{
			return new MappingInfo()
			{
				MethodInfo = MethodInfo
			};
		}
#else
        public static MappingInfo FromMethodInfo<T>(Generator<T> that, MethodInfo methodInfo)
        {
            var dynamicMethod = new DynamicMethod(
                "MappingInfo.FromMethodInfo::" + typeof(T).Name + "::" + methodInfo.Name + "(" +
                methodInfo.GetParameters().First().ParameterType.Name + ")", typeof(void),
                new Type[] {typeof(object), typeof(AstNode)}, that.GetType());
            var ilGenerator = dynamicMethod.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Castclass, that.GetType());

            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Castclass, methodInfo.GetParameters()[0].ParameterType);
            ilGenerator.Emit(OpCodes.Call, methodInfo);
            if (methodInfo.ReturnType != typeof(void)) ilGenerator.Emit(OpCodes.Pop);

            ilGenerator.Emit(OpCodes.Ret);

            return new MappingInfo()
            {
                Action = (MapDelegate) dynamicMethod.CreateDelegate(typeof(MapDelegate)),
            };
        }
#endif
    }

    public abstract class Generator<TGenerator>
    {
        private Dictionary<Type, MappingInfo> GenerateMappings = new Dictionary<Type, MappingInfo>();

        protected Generator()
        {
            foreach (var method in GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(method => method.ReturnType == typeof(void))
                .Where(method => method.GetParameters().Length == 1)
            )
            {
                var parameterType = method.GetParameters().First().ParameterType;
                if (parameterType.IsSubclassOf(typeof(AstNode)))
                {
                    GenerateMappings[parameterType] = MappingInfo.FromMethodInfo(this, method);
                }
            }

            Reset();
        }

        //static Generator<GeneratorCSharp> Test;
        //
        //private void MyMethod(AstNode AstNode)
        //{
        //	Test.MyMethod(AstNode);
        //}

        public virtual TGenerator Reset()
        {
            return (TGenerator) (object) this;
        }

        /// <summary>
        /// Determine dinamically which method to call.
        /// </summary>
        /// <param name="astNode"></param>
        public TGenerator GenerateRoot(AstNode astNode)
        {
            Reset();
            Generate(astNode);
            return (TGenerator) (object) this;
        }

        protected virtual void Generate(AstNode astNode)
        {
            //if (AstNode == null) return;

            var astNodeType = astNode.GetType();
            if (!GenerateMappings.ContainsKey(astNodeType))
            {
                foreach (var generateMapping in GenerateMappings)
                {
                    Console.WriteLine(generateMapping);
                }
                throw (new NotImplementedException($"Don't know how to generate {astNodeType} for {GetType()}"));
            }

            GenerateMappings[astNodeType].Call(this, astNode);
        }
    }
}