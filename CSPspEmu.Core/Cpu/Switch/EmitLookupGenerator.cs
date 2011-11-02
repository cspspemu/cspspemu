using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

namespace CSPspEmu.Core.Cpu.Table
{
	public class EmitLookupGenerator
	{
		public Action<uint, TType> GenerateSwitchDelegate<TType>(IEnumerable<InstructionInfo> InstructionInfoList)
		{
			return GenerateSwitchDelegate<TType>(InstructionInfoList, Name => Name);
		}

		public Action<uint, TType> GenerateSwitchDelegate<TType>(IEnumerable<InstructionInfo> InstructionInfoList, Func<String, String> NameConverter)
		{
			var DynamicMethod = new DynamicMethod("", typeof(void), new Type[] { typeof(uint), typeof(TType) });
			var ILGenerator = DynamicMethod.GetILGenerator();
			GenerateSwitchCode(ILGenerator, InstructionInfoList, (CurrentILGenerator, InstructionInfo) =>
			{
				var InstructionInfoName = NameConverter((InstructionInfo != null) ? InstructionInfo.Name : "Default");
				ILGenerator.Emit(OpCodes.Ldarg_1);
				var MethodInfo = typeof(TType).GetMethod(InstructionInfoName);
				if (MethodInfo == null) throw (new Exception("Cannot find method '" + InstructionInfoName + "' on type '" + typeof(TType).Name + "'"));
				ILGenerator.Emit(OpCodes.Call, MethodInfo);
			});
			return (Action<uint, TType>)DynamicMethod.CreateDelegate(typeof(Action<uint, TType>));
		}

		public void GenerateSwitchCode(ILGenerator ILGenerator, IEnumerable<InstructionInfo> InstructionInfoList, Action<ILGenerator, InstructionInfo> GenerateCallDelegate, int Level = 0)
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

			/*
			var DefaultLabel = ILGenerator.DefineLabel();
			var SwitchLabels = new Label[MaskGroupsCount];
			for (var n = 0; n < MaskGroupsCount; n++) SwitchLabels[n] = ILGenerator.DefineLabel();

			ILGenerator.Emit(OpCodes.Switch, SwitchLabels);
			ILGenerator.Emit(OpCodes.Br_S, DefaultLabel);

			uint Case = 0;
			foreach (var MaskGroup in MaskGroups.Select(MaskGroup => MaskGroup.ToArray()))
			{
				ILGenerator.MarkLabel(SwitchLabels[Case]);
				
				if (MaskGroup.Length > 1)
				{
					GenerateSwitchCode(ILGenerator, MaskGroup, GenerateCallDelegate);
				}
				else
				{
					GenerateCallDelegate(ILGenerator, MaskGroup[0]);
				}
				ILGenerator.Emit(OpCodes.Ret);

				Case++;
			}

			ILGenerator.MarkLabel(DefaultLabel);
			ILGenerator.Emit(OpCodes.Ret);
			*/
		}
	}
}
