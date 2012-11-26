using System;
using System.Collections.Generic;
using System.Linq;
using SafeILGenerator;
using System.Reflection;

namespace CSPspEmu.Core.Cpu.Table
{
	public class EmitLookupGenerator
	{
		public static Func<uint, TRetType> GenerateInfoDelegate<TType, TRetType>(Func<uint, TType, TRetType> Callback, TType Instance)
		{
			return Value =>
			{
				return Callback(Value, Instance);
			};
		}

		static private string DefaultNameConverter(string Name)
		{
			if (Name == "Default") return "unknown";
			if (Name == "break") return "_break";
			return Name.Replace('.', '_');
		}

		public static Action<uint, TType> GenerateSwitchDelegate<TType>(IEnumerable<InstructionInfo> InstructionInfoList, Func<String, String> NameConverter = null)
		{
			if (NameConverter == null) NameConverter = DefaultNameConverter;

			return GenerateSwitch<Action<uint, TType>>(InstructionInfoList, (SafeILGenerator, InstructionInfo) =>
			{
				var InstructionInfoName = NameConverter((InstructionInfo != null) ? InstructionInfo.Name : "Default");
				SafeILGenerator.LoadArgument<TType>(1);
				var MethodInfo = typeof(TType).GetMethod(InstructionInfoName);
				if (MethodInfo == null) throw (new Exception("Cannot find method '" + InstructionInfoName + "' on type '" + typeof(TType).Name + "'"));
				SafeILGenerator.Call(MethodInfo);
			});
		}

		public static Func<uint, TType, TRetType> GenerateSwitchDelegateReturn<TType, TRetType>(IEnumerable<InstructionInfo> InstructionInfoList, Func<String, String> NameConverter = null, bool ThrowOnUnexistent = true)
		{
			if (NameConverter == null) NameConverter = DefaultNameConverter;

			return GenerateSwitch<Func<uint, TType, TRetType>>(InstructionInfoList, (SafeILGenerator, InstructionInfo) =>
			{
				var InstructionInfoName = NameConverter((InstructionInfo != null) ? InstructionInfo.Name : "Default");
				SafeILGenerator.LoadArgument<TType>(1);
				//ILGenerator.Emit(OpCodes.Ldarg_1);
				var MethodInfo = typeof(TType).GetMethod(InstructionInfoName);

				if (MethodInfo == null && !ThrowOnUnexistent)
				{
					MethodInfo = typeof(TType).GetMethod("unhandled");
				}

				if (MethodInfo == null)
				{
					throw (new Exception("Cannot find method '" + InstructionInfoName + "' on type '" + typeof(TType).Name + "'"));
				}

				if (MethodInfo.ReturnType == typeof(TRetType))
				{
					SafeILGenerator.Call(MethodInfo);
					//ILGenerator.Emit(OpCodes.Call, MethodInfo);
				}
				else
				{
					//SafeILGenerator.LoadNull();
					throw (new Exception(String.Format("Invalid method: '{0}' should return '{1}'", MethodInfo, typeof(TRetType))));
				}

			});
		}

		public static object GenerateSwitch(Type Type, IEnumerable<InstructionInfo> InstructionInfoList, Action<CSafeILGenerator, InstructionInfo> GenerateCallDelegate)
		{
			return CSafeILGenerator.Generate(Type, "EmitLookupGenerator.GenerateSwitch", (Generator) =>
			{
				GenerateSwitchCode(Generator, InstructionInfoList, GenerateCallDelegate);
			});
		}

		public static TType GenerateSwitch<TType>(IEnumerable<InstructionInfo> InstructionInfoList, Action<CSafeILGenerator, InstructionInfo> GenerateCallDelegate)
		{
			return (TType)GenerateSwitch(typeof(TType), InstructionInfoList, GenerateCallDelegate);
		}

		/// <summary>
		/// Generates an assembly code that will decode an integer with a set of InstructionInfo.
		/// </summary>
		/// <param name="SafeILGenerator"></param>
		/// <param name="InstructionInfoList"></param>
		/// <param name="GenerateCallDelegate"></param>
		/// <param name="Level"></param>
		public static void GenerateSwitchCode(CSafeILGenerator SafeILGenerator, IEnumerable<InstructionInfo> InstructionInfoList, Action<CSafeILGenerator, InstructionInfo> GenerateCallDelegate, int Level = 0)
		{
			//var ILGenerator = SafeILGenerator._UnsafeGetILGenerator();
			var CommonMask = InstructionInfoList.Aggregate(0xFFFFFFFF, (Base, InstructionInfo) => Base & InstructionInfo.Mask);
			var MaskGroups = InstructionInfoList.GroupBy((InstructionInfo) => InstructionInfo.Value & CommonMask);
			var MaskGroupsCount = MaskGroups.Count();

			//Console.WriteLine("[" + Level + "]{0:X}", CommonMask);

			//var MaskedLocal = SafeILGenerator.DeclareLocal<int>();

			var MaskedLocalValue = SafeILGenerator.DeclareLocal<int>();
			SafeILGenerator.LoadArgument<int>(0);
			SafeILGenerator.Push((int)CommonMask);
			SafeILGenerator.BinaryOperation(SafeBinaryOperator.And);
			SafeILGenerator.StoreLocal(MaskedLocalValue);

#if true
			SafeILGenerator.LoadLocal(MaskedLocalValue);

			SafeILGenerator.Switch(
				// List
				MaskGroups.Select(MaskGroup => MaskGroup.ToArray()),
				// Get int Key
				(MaskGroup) =>
				{
					return (int)(MaskGroup[0].Value & CommonMask);
				},
				// Case
				(MaskGroup) =>
				{
					if (MaskGroup.Length > 1)
					{
						GenerateSwitchCode(SafeILGenerator, MaskGroup, GenerateCallDelegate, Level + 1);
					}
					else
					{
						GenerateCallDelegate(SafeILGenerator, MaskGroup[0]);
					}
				},
				// Default
				() => 
				{
					GenerateCallDelegate(SafeILGenerator, null);
				}
			);

			SafeILGenerator.Return(typeof(void));
#else
			foreach (var MaskGroup in MaskGroups.Select(MaskGroup => MaskGroup.ToArray()))
			{
				var NextLabel = SafeILGenerator.DefineLabel("NextLabel");
				SafeILGenerator.LoadLocal(MaskedLocalValue);
				SafeILGenerator.Push(MaskGroup[0].Value & CommonMask);
				SafeILGenerator.BranchBinaryComparison(SafeBinaryComparison.NotEquals, NextLabel);
				//Console.WriteLine("[" + Level + "] || (Value & " + CommonMask + " == " + (MaskGroup[0].Value & CommonMask) + ")");

				if (MaskGroup.Length > 1)
				{
					GenerateSwitchCode(SafeILGenerator, MaskGroup, GenerateCallDelegate, Level + 1);
				}
				else
				{
					GenerateCallDelegate(SafeILGenerator, MaskGroup[0]);
				}
				SafeILGenerator.Return();
				NextLabel.Mark();
			}

			GenerateCallDelegate(SafeILGenerator, null);
			SafeILGenerator.Return();
#endif
		}
	}
}
