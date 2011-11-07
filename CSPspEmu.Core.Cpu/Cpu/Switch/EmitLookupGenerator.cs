using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

namespace CSPspEmu.Core.Cpu.Table
{
	public class EmitLookupGenerator
	{
		static public Func<uint, TRetType> GenerateInfoDelegate<TType, TRetType>(Func<uint, TType, TRetType> Callback, TType Instance)
		{
			return V =>
			{
				return Callback(V, Instance);
			};
		}

		static public Action<uint, TType> GenerateSwitchDelegate<TType>(IEnumerable<InstructionInfo> InstructionInfoList, Func<String, String> NameConverter = null)
		{
			if (NameConverter == null)
			{
				NameConverter = Name =>
				{
					if (Name == "Default") return "unknown";
					if (Name == "break") return "_break";
					return Name.Replace('.', '_');
				};
			}
			return GenerateSwitchDelegate<TType>(InstructionInfoList, (ILGenerator, InstructionInfo) =>
			{
				var InstructionInfoName = NameConverter((InstructionInfo != null) ? InstructionInfo.Name : "Default");
				ILGenerator.Emit(OpCodes.Ldarg_1);
				var MethodInfo = typeof(TType).GetMethod(InstructionInfoName);
				if (MethodInfo == null) throw (new Exception("Cannot find method '" + InstructionInfoName + "' on type '" + typeof(TType).Name + "'"));
				ILGenerator.Emit(OpCodes.Call, MethodInfo);
			});
		}

		static public Func<uint, TType, TRetType> GenerateSwitchDelegateReturn<TType, TRetType>(IEnumerable<InstructionInfo> InstructionInfoList, Func<String, String> NameConverter = null)
		{
			if (NameConverter == null)
			{
				NameConverter = Name =>
				{
					if (Name == "Default") return "unknown";
					if (Name == "break") return "_break";
					return Name.Replace('.', '_');
				};
			}
			return GenerateSwitchDelegateReturn<TType, TRetType>(InstructionInfoList, (ILGenerator, InstructionInfo) =>
			{
				var InstructionInfoName = NameConverter((InstructionInfo != null) ? InstructionInfo.Name : "Default");
				ILGenerator.Emit(OpCodes.Ldarg_1);
				var MethodInfo = typeof(TType).GetMethod(InstructionInfoName);
				if (MethodInfo == null) throw (new Exception("Cannot find method '" + InstructionInfoName + "' on type '" + typeof(TType).Name + "'"));
				ILGenerator.Emit(OpCodes.Call, MethodInfo);
			});
		}

		static public Action<uint, TType> GenerateSwitchDelegate<TType>(IEnumerable<InstructionInfo> InstructionInfoList, Action<ILGenerator, InstructionInfo> GenerateCallDelegate)
		{
			var DynamicMethod = new DynamicMethod("", typeof(void), new Type[] { typeof(uint), typeof(TType) });
			var ILGenerator = DynamicMethod.GetILGenerator();
			GenerateSwitchCode(ILGenerator, InstructionInfoList, GenerateCallDelegate);
			return (Action<uint, TType>)DynamicMethod.CreateDelegate(typeof(Action<uint, TType>));
		}

		static public Func<uint, TType, TRetType> GenerateSwitchDelegateReturn<TType, TRetType>(IEnumerable<InstructionInfo> InstructionInfoList, Action<ILGenerator, InstructionInfo> GenerateCallDelegate)
		{
			var DynamicMethod = new DynamicMethod("", typeof(TRetType), new Type[] { typeof(uint), typeof(TType) });
			var ILGenerator = DynamicMethod.GetILGenerator();
			GenerateSwitchCode(ILGenerator, InstructionInfoList, GenerateCallDelegate);
			return (Func<uint, TType, TRetType>)DynamicMethod.CreateDelegate(typeof(Func<uint, TType, TRetType>));
		}

		static public Func<uint, TRetType> GenerateSwitchDelegateReturn<TRetType>(IEnumerable<InstructionInfo> InstructionInfoList, Action<ILGenerator, InstructionInfo> GenerateCallDelegate)
		{
			var DynamicMethod = new DynamicMethod("", typeof(TRetType), new Type[] { typeof(uint) });
			var ILGenerator = DynamicMethod.GetILGenerator();
			GenerateSwitchCode(ILGenerator, InstructionInfoList, GenerateCallDelegate);
			return (Func<uint, TRetType>)DynamicMethod.CreateDelegate(typeof(Func<uint, TRetType>));
		}

		/// <summary>
		/// Generates an assembly code that will decode an integer with a set of InstructionInfo.
		/// </summary>
		/// <param name="ILGenerator"></param>
		/// <param name="InstructionInfoList"></param>
		/// <param name="GenerateCallDelegate"></param>
		/// <param name="Level"></param>
		static public void GenerateSwitchCode(ILGenerator ILGenerator, IEnumerable<InstructionInfo> InstructionInfoList, Action<ILGenerator, InstructionInfo> GenerateCallDelegate, int Level = 0)
		{
			var CommonMask = InstructionInfoList.Aggregate(0xFFFFFFFF, (Base, InstructionInfo) => Base & InstructionInfo.Mask);
			var MaskGroups = InstructionInfoList.GroupBy((InstructionInfo) => InstructionInfo.Value & CommonMask);
			var MaskGroupsCount = MaskGroups.Count();

			//Console.WriteLine("[" + Level + "]{0:X}", CommonMask);

			foreach (var MaskGroup in MaskGroups.Select(MaskGroup => MaskGroup.ToArray()))
			{
				var NextLabel = ILGenerator.DefineLabel();
				ILGenerator.Emit(OpCodes.Ldarg_0);
				ILGenerator.Emit(OpCodes.Ldc_I4, CommonMask);
				ILGenerator.Emit(OpCodes.And);
				ILGenerator.Emit(OpCodes.Ldc_I4, MaskGroup[0].Value & CommonMask);
				ILGenerator.Emit(OpCodes.Bne_Un, NextLabel);
				//Console.WriteLine("[" + Level + "] || (Value & " + CommonMask + " == " + (MaskGroup[0].Value & CommonMask) + ")");

				if (MaskGroup.Length > 1)
				{
					GenerateSwitchCode(ILGenerator, MaskGroup, GenerateCallDelegate, Level + 1);
				}
				else
				{
					GenerateCallDelegate(ILGenerator, MaskGroup[0]);
				}
				ILGenerator.Emit(OpCodes.Ret);
				ILGenerator.MarkLabel(NextLabel);
			}

			GenerateCallDelegate(ILGenerator, null);
			ILGenerator.Emit(OpCodes.Ret);
		}
	}
}
