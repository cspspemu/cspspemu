using System;
using System.Collections.Generic;
using System.Linq;
using SafeILGenerator;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast.Generators;
using CSharpUtils;
using CSPspEmu.Core.Cpu.Dynarec.Ast;

namespace CSPspEmu.Core.Cpu.Table
{
    public class EmitLookupGenerator
    {
        private static AstGenerator ast = AstGenerator.Instance;

        public static Func<uint, TRetType> GenerateInfoDelegate<TType, TRetType>(Func<uint, TType, TRetType> Callback,
            TType Instance)
        {
            return Value => { return Callback(Value, Instance); };
        }

        private static string DefaultNameConverter(string Name)
        {
            if (Name == "Default") return "unknown";
            if (Name == "break") return "_break";
            return Name.Replace('.', '_');
        }

        public static Action<uint, TType> GenerateSwitchDelegate<TType>(string Name,
            IEnumerable<InstructionInfo> InstructionInfoList, Func<String, String> NameConverter = null)
        {
            if (NameConverter == null) NameConverter = DefaultNameConverter;

            return GenerateSwitch<Action<uint, TType>>(Name, InstructionInfoList, (InstructionInfo) =>
            {
                var InstructionInfoName = NameConverter((InstructionInfo != null) ? InstructionInfo.Name : "Default");
                var MethodInfo = typeof(TType).GetMethod(InstructionInfoName);
                if (MethodInfo == null)
                    throw (new Exception("Cannot find method '" + InstructionInfoName + "' on type '" +
                                         typeof(TType).Name + "'"));

                //Console.WriteLine("MethodInfo: {0}", MethodInfo);
                //Console.WriteLine("Argument(1): {0}", typeof(TType));

                if (MethodInfo.IsStatic)
                {
                    return ast.Statement(ast.CallStatic(MethodInfo, ast.Argument<TType>(1)));
                }
                else
                {
                    return ast.Statement(ast.CallInstance(ast.Argument<TType>(1), MethodInfo));
                }
            });
        }

        public static Func<uint, TType, TRetType> GenerateSwitchDelegateReturn<TType, TRetType>(string Name,
            IEnumerable<InstructionInfo> InstructionInfoList, Func<String, String> NameConverter = null,
            bool ThrowOnUnexistent = true)
        {
            if (NameConverter == null) NameConverter = DefaultNameConverter;

            return GenerateSwitch<Func<uint, TType, TRetType>>(Name, InstructionInfoList, (InstructionInfo) =>
            {
                var InstructionInfoName = NameConverter((InstructionInfo != null) ? InstructionInfo.Name : "Default");
                var MethodInfo = typeof(TType).GetMethod(InstructionInfoName);

                if (MethodInfo == null && !ThrowOnUnexistent)
                {
                    MethodInfo = typeof(TType).GetMethod("unhandled");
                }

                if (MethodInfo == null)
                {
                    throw (new Exception("Cannot find method '" + InstructionInfoName + "' on type '" +
                                         typeof(TType).Name + "'"));
                }

                if (MethodInfo.ReturnType == typeof(TRetType))
                {
                    if (MethodInfo.IsStatic)
                    {
                        return ast.Return(ast.CallStatic(MethodInfo, ast.Argument<TType>(1)));
                    }
                    else
                    {
                        return ast.Return(ast.CallInstance(ast.Argument<TType>(1), MethodInfo));
                    }
                }
                else
                {
                    throw (new Exception(String.Format("Invalid method: '{0}' should return '{1}'", MethodInfo,
                        typeof(TRetType))));
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="Name"></param>
        /// <param name="InstructionInfoList"></param>
        /// <param name="GenerateCallDelegate"></param>
        /// <returns></returns>
        public static TType GenerateSwitch<TType>(string Name, IEnumerable<InstructionInfo> InstructionInfoList,
            Func<InstructionInfo, AstNodeStm> GenerateCallDelegate)
        {
            //Console.WriteLine(GenerateSwitchCode(InstructionInfoList, GenerateCallDelegate).Optimize(null).ToCSharpString());
            //Console.WriteLine(GenerateSwitchCode(InstructionInfoList, GenerateCallDelegate).Optimize(null).ToILString<TType>());
            return GenerateSwitchCode(InstructionInfoList, GenerateCallDelegate)
                .GenerateDelegate<TType>("EmitLookupGenerator.GenerateSwitch::" + Name);
        }

        /// <summary>
        /// Generates an assembly code that will decode an integer with a set of InstructionInfo.
        /// </summary>
        /// <param name="SafeILGenerator"></param>
        /// <param name="InstructionInfoList"></param>
        /// <param name="GenerateCallDelegate"></param>
        /// <param name="Level"></param>
        public static AstNodeStm GenerateSwitchCode(IEnumerable<InstructionInfo> InstructionInfoList,
            Func<InstructionInfo, AstNodeStm> GenerateCallDelegate, int Level = 0)
        {
            //var ILGenerator = SafeILGenerator._UnsafeGetILGenerator();
            var CommonMask =
                InstructionInfoList.Aggregate(0xFFFFFFFF, (Base, InstructionInfo) => Base & InstructionInfo.Mask);
            var MaskGroups = InstructionInfoList.GroupBy((InstructionInfo) => InstructionInfo.Value & CommonMask);

#if false
			int ShiftOffset = 0;
			var CommonMaskShifted = CommonMask >> ShiftOffset;
			uint MinValue = 0;
#else
            int ShiftOffset = BitUtils.GetFirstBit1(CommonMask);
            var CommonMaskShifted = CommonMask >> ShiftOffset;
            uint MinValue = MaskGroups
                .Select(MaskGroup => (uint) ((MaskGroup.First().Value >> ShiftOffset) & CommonMaskShifted)).Min();
#endif

            //var MaskGroupsCount = MaskGroups.Count();

            //Console.WriteLine("[" + Level + "]{0:X}", CommonMask);
            //var MaskedLocal = SafeILGenerator.DeclareLocal<int>();

            return ast.Statements(
                ast.Switch(
                    ast.Binary(
                        ast.Binary(ast.Binary(ast.Argument<uint>(0), ">>", ShiftOffset), "&", (uint) CommonMaskShifted),
                        "-", MinValue),
                    ast.Default(GenerateCallDelegate(null)),
                    MaskGroups.Select(MaskGroup =>
                        ast.Case(
                            (uint) ((MaskGroup.First().Value >> ShiftOffset) & CommonMaskShifted) - MinValue,
                            (MaskGroup.Count() > 1)
                                ? GenerateSwitchCode(MaskGroup, GenerateCallDelegate, Level + 1)
                                : GenerateCallDelegate(MaskGroup.First())
                        )
                    ).ToArray()
                ),
                ast.Throw(ast.New<Exception>("Unexpected reach!"))
            );
        }
    }
}