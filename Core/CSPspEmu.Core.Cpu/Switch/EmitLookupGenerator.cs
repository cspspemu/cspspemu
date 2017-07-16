using System;
using System.Collections.Generic;
using System.Linq;
using SafeILGenerator;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast.Generators;
using CSharpUtils;

namespace CSPspEmu.Core.Cpu.Table
{
	public class EmitLookupGenerator
	{
		static private AstGenerator ast = AstGenerator.Instance;

		public static Func<uint, TRetType> GenerateInfoDelegate<TType, TRetType>(Func<uint, TType, TRetType> callback, TType instance)
		{
			return value =>
			{
				return callback(value, instance);
			};
		}

		static private string DefaultNameConverter(string name)
		{
			if (name == "Default") return "unknown";
			if (name == "break") return "_break";
			return name.Replace('.', '_');
		}

		public static Action<uint, TType> GenerateSwitchDelegate<TType>(string name, IEnumerable<InstructionInfo> instructionInfoList, Func<string, string> nameConverter = null)
		{
			if (nameConverter == null) nameConverter = DefaultNameConverter;

			return GenerateSwitch<Action<uint, TType>>(name, instructionInfoList, (instructionInfo) =>
			{
				var instructionInfoName = nameConverter((instructionInfo != null) ? instructionInfo.Name : "Default");
				var methodInfo = typeof(TType).GetMethod(instructionInfoName);
				if (methodInfo == null) throw (new Exception("Cannot find method '" + instructionInfoName + "' on type '" + typeof(TType).Name + "'"));
				
				//Console.WriteLine("MethodInfo: {0}", MethodInfo);
				//Console.WriteLine("Argument(1): {0}", typeof(TType));

				if (methodInfo.IsStatic)
				{
					return ast.Statement(ast.CallStatic(methodInfo, ast.Argument<TType>(1)));
				}
				else
				{
					return ast.Statement(ast.CallInstance(ast.Argument<TType>(1), methodInfo));
				}
			});
		}

		public static Func<uint, TType, TRetType> GenerateSwitchDelegateReturn<TType, TRetType>(string name, IEnumerable<InstructionInfo> instructionInfoList, Func<string, string> nameConverter = null, bool throwOnUnexistent = true)
		{
			if (nameConverter == null) nameConverter = DefaultNameConverter;

			return GenerateSwitch<Func<uint, TType, TRetType>>(name, instructionInfoList, (instructionInfo) =>
			{
				var instructionInfoName = nameConverter((instructionInfo != null) ? instructionInfo.Name : "Default");
				var methodInfo = typeof(TType).GetMethod(instructionInfoName);

				if (methodInfo == null && !throwOnUnexistent)
				{
					methodInfo = typeof(TType).GetMethod("unhandled");
				}

				if (methodInfo == null)
				{
					throw (new Exception("Cannot find method '" + instructionInfoName + "' on type '" + typeof(TType).Name + "'"));
				}

				if (methodInfo.ReturnType == typeof(TRetType))
				{
					if (methodInfo.IsStatic)
					{
						return ast.Return(ast.CallStatic(methodInfo, ast.Argument<TType>(1)));
					}
					else
					{
						return ast.Return(ast.CallInstance(ast.Argument<TType>(1), methodInfo));
					}
				}
				else
				{
					throw (new Exception(String.Format("Invalid method: '{0}' should return '{1}'", methodInfo, typeof(TRetType))));
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
		public static TType GenerateSwitch<TType>(string name, IEnumerable<InstructionInfo> instructionInfoList, Func<InstructionInfo, AstNodeStm> generateCallDelegate)
		{
			//Console.WriteLine(GenerateSwitchCode(InstructionInfoList, GenerateCallDelegate).Optimize(null).ToCSharpString());
			//Console.WriteLine(GenerateSwitchCode(InstructionInfoList, GenerateCallDelegate).Optimize(null).ToILString<TType>());
			return GenerateSwitchCode(instructionInfoList, generateCallDelegate).GenerateDelegate<TType>("EmitLookupGenerator.GenerateSwitch::" + name);
		}

		/// <summary>
		/// Generates an assembly code that will decode an integer with a set of InstructionInfo.
		/// </summary>
		/// <param name="instructionInfoList"></param>
		/// <param name="generateCallDelegate"></param>
		/// <param name="level"></param>
		public static AstNodeStm GenerateSwitchCode(IEnumerable<InstructionInfo> instructionInfoList, Func<InstructionInfo, AstNodeStm> generateCallDelegate, int level = 0)
		{
			//var ILGenerator = SafeILGenerator._UnsafeGetILGenerator();
			var instructionInfos = instructionInfoList as InstructionInfo[] ?? instructionInfoList.ToArray();
			var commonMask = instructionInfos.Aggregate(0xFFFFFFFF, (Base, instructionInfo) => Base & instructionInfo.Mask);
			var maskGroups = instructionInfos.GroupBy((instructionInfo) => instructionInfo.Value & commonMask).ToArray();
			
#if false
			int ShiftOffset = 0;
			var CommonMaskShifted = CommonMask >> ShiftOffset;
			uint MinValue = 0;
#else
			var shiftOffset = BitUtils.GetFirstBit1(commonMask);
			var commonMaskShifted = commonMask >> shiftOffset;
			var minValue = maskGroups.Select(maskGroup => (maskGroup.First().Value >> shiftOffset) & commonMaskShifted).Min();
#endif

			//var MaskGroupsCount = MaskGroups.Count();

			//Console.WriteLine("[" + Level + "]{0:X}", CommonMask);
			//var MaskedLocal = SafeILGenerator.DeclareLocal<int>();

			return ast.Statements(
				ast.Switch(
					ast.Binary(ast.Binary(ast.Binary(ast.Argument<uint>(0), ">>", shiftOffset), "&", commonMaskShifted), "-", minValue),
					ast.Default(generateCallDelegate(null)),
					maskGroups.Select(maskGroup => 
						ast.Case(
							((maskGroup.First().Value >> shiftOffset) & commonMaskShifted) - minValue,
							(maskGroup.Count() > 1)
							? GenerateSwitchCode(maskGroup, generateCallDelegate, level + 1)
							: generateCallDelegate(maskGroup.First())
						)
					).ToArray()
				),
				ast.Throw(ast.New<Exception>("Unexpected reach!"))
			);
		}
	}
}
