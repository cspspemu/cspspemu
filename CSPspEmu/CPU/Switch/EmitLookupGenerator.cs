using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSharpUtils;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Cpu.Dynarec.Ast;
using CSPspEmu.Core.Cpu.Table;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.Core.Cpu.Switch
{
    public class EmitLookupGenerator
    {
        private static readonly AstGenerator Ast = AstGenerator.Instance;

        public static Func<uint, TRetType> GenerateInfoDelegate<TType, TRetType>(Func<uint, TType, TRetType> callback,
            TType instance)
        {
            return value => callback(value, instance);
        }

        public static Action<uint, TType> GenerateSwitchDelegate<TType>(string name,
            IEnumerable<InstructionInfo> instructionInfoList)
        {
            return GenerateSwitch<Action<uint, TType>>(name, instructionInfoList, instructionInfo =>
            {
                var instructionInfoName = (instructionInfo != null) ? instructionInfo.Name : "Default";
                var methodInfo = typeof(TType).GetMethod(instructionInfoName);
                if (methodInfo == null)
                    throw (new Exception(
                        $"Cannot find method \'{instructionInfoName}\' on type \'{typeof(TType).Name}\' {name}, {instructionInfo?.Name} ")
                    );

                //Console.WriteLine("MethodInfo: {0}", MethodInfo);
                //Console.WriteLine("Argument(1): {0}", typeof(TType));

                if (methodInfo.IsStatic)
                {
                    return Ast.Statement(Ast.CallStatic(methodInfo, Ast.Argument<TType>(1)));
                }
                else
                {
                    return Ast.Statement(Ast.CallInstance(Ast.Argument<TType>(1), methodInfo));
                }
            });
        }

        public static Func<uint, TType, TRetType> GenerateSwitchDelegateReturn<TType, TRetType>(string name,
            IEnumerable<InstructionInfo> instructionInfoList,
            bool throwOnUnexistent = true, bool warnUnmapped = true)
        {
            var customNamesToMethodInfo = new Dictionary<string, MethodInfo>();

            string NormalizeName(string nname)
            {
                return nname.ToLower().Replace('.', '_').Trim('_');
            }

            //Console.WriteLine("aaaaaaaaaaaaaaaaaaaaaaaaaaa");
            foreach (var methodInfo in typeof(TType).GetMethods())
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                var instructionName =
                    (methodInfo.GetCustomAttributes(typeof(InstructionName), true) as InstructionName[])
                    .FirstOrDefault();
                if (instructionName != null)
                {
                    customNamesToMethodInfo[instructionName.Name] = methodInfo;
                    //Console.WriteLine($"{instructionName.Name} -> {methodInfo}");
                    if (NormalizeName(instructionName.Name) != NormalizeName(methodInfo.Name))
                    {
                        Console.WriteLine(
                            $"WARNING! Normalized name mismatch: {instructionName.Name} != {methodInfo.Name}");
                    }
                }
                //Console.WriteLine("methodInfo:" + methodInfo + " : " + instructionName);
            }

            return GenerateSwitch<Func<uint, TType, TRetType>>(name, instructionInfoList, instructionInfo =>
            {
                var instructionInfoName = (instructionInfo != null) ? instructionInfo.Name : "unknown";
                var methodInfo = customNamesToMethodInfo.GetOrDefault(instructionInfoName, null);

                //if (methodInfo == null)
                //{
                //    Console.WriteLine(
                //        $"WARNING! Not annotated instruction: {instructionInfoName} in type {typeof(TType)} :: {name}");
                //    foreach (var name3 in customNamesToMethodInfo)
                //    {
                //        Console.WriteLine($"- {name3}");
                //    }
                //    methodInfo = typeof(TType).GetMethod(instructionInfoName);
                //}

                if (methodInfo == null && !throwOnUnexistent)
                {
                    methodInfo = customNamesToMethodInfo.GetOrDefault("unhandled", null);
                }

                if (methodInfo == null)
                {
                    throw (new Exception(
                        $"Cannot find method \'{instructionInfoName}\' on type \'{typeof(TType).Name}\' : {name}, {instructionInfo?.Name}")
                    );
                }

                if (methodInfo.ReturnType == typeof(TRetType))
                {
                    if (methodInfo.IsStatic)
                    {
                        return Ast.Return(Ast.CallStatic(methodInfo, Ast.Argument<TType>(1)));
                    }
                    else
                    {
                        return Ast.Return(Ast.CallInstance(Ast.Argument<TType>(1), methodInfo));
                    }
                }
                else
                {
                    throw new Exception($"Invalid method: '{methodInfo}' should return '{typeof(TRetType)}'");
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="name"></param>
        /// <param name="instructionInfoList"></param>
        /// <param name="generateCallDelegate"></param>
        /// <returns></returns>
        public static TType GenerateSwitch<TType>(string name, IEnumerable<InstructionInfo> instructionInfoList,
            Func<InstructionInfo, AstNodeStm> generateCallDelegate)
        {
            //Console.WriteLine(GenerateSwitchCode(InstructionInfoList, GenerateCallDelegate).Optimize(null).ToCSharpString());
            //Console.WriteLine(GenerateSwitchCode(InstructionInfoList, GenerateCallDelegate).Optimize(null).ToILString<TType>());
            return GenerateSwitchCode(instructionInfoList, generateCallDelegate)
                .GenerateDelegate<TType>("EmitLookupGenerator.GenerateSwitch::" + name);
        }

        /// <summary>
        /// Generates an assembly code that will decode an integer with a set of InstructionInfo.
        /// </summary>
        /// <param name="instructionInfoList"></param>
        /// <param name="generateCallDelegate"></param>
        /// <param name="level"></param>
        public static AstNodeStm GenerateSwitchCode(IEnumerable<InstructionInfo> instructionInfoList,
            Func<InstructionInfo, AstNodeStm> generateCallDelegate, int level = 0)
        {
            //var ILGenerator = SafeILGenerator._UnsafeGetILGenerator();
            var instructionInfos = instructionInfoList as InstructionInfo[] ?? instructionInfoList.ToArray();
            var commonMask =
                instructionInfos.Aggregate(0xFFFFFFFF, (Base, instructionInfo) => Base & instructionInfo.Mask);
            var maskGroups = instructionInfos.GroupBy(instructionInfo => instructionInfo.Value & commonMask);

#if false
			int ShiftOffset = 0;
			var CommonMaskShifted = CommonMask >> ShiftOffset;
			uint MinValue = 0;
#else
            var shiftOffset = BitUtils.GetFirstBit1(commonMask);
            var commonMaskShifted = commonMask >> shiftOffset;
            var enumerable = maskGroups as IGrouping<uint, InstructionInfo>[] ?? maskGroups.ToArray();
            var minValue = enumerable
                .Select(maskGroup => (maskGroup.First().Value >> shiftOffset) & commonMaskShifted).Min();
#endif

            //var MaskGroupsCount = MaskGroups.Count();

            //Console.WriteLine("[" + Level + "]{0:X}", CommonMask);
            //var MaskedLocal = SafeILGenerator.DeclareLocal<int>();

            return Ast.Statements(
                Ast.Switch(
                    Ast.Binary(
                        Ast.Binary(Ast.Binary(Ast.Argument<uint>(0), ">>", shiftOffset), "&", commonMaskShifted),
                        "-", minValue),
                    Ast.Default(generateCallDelegate(null)),
                    enumerable.Select(maskGroup =>
                        Ast.Case(
                            ((maskGroup.First().Value >> shiftOffset) & commonMaskShifted) - minValue,
                            (maskGroup.Count() > 1)
                                ? GenerateSwitchCode(maskGroup, generateCallDelegate, level + 1)
                                : generateCallDelegate(maskGroup.First())
                        )
                    ).ToArray()
                ),
                Ast.Throw(Ast.New<Exception>("Unexpected reach!"))
            );
        }
    }
}