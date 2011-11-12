using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CSPspEmu.Core.Threading.Synchronization;
using CSPspEmu.Core.Gpu.State;
using System.Reflection.Emit;
using CSPspEmu.Core.Gpu.Run;

namespace CSPspEmu.Core.Gpu
{
	sealed unsafe public class GpuDisplayList
	{
		public struct OptionalParams
		{
			public int ContextAddress;
			public int StackDepth;
			public int StackAddress;
		}

		public enum StatusEnum
		{
			Done = 0,
			Queued = 1,
			DrawingDone = 2,
			StallReached = 3,
			CancelDone = 4,
		}

		/// <summary>
		/// A value between 0 and 63 inclusive.
		/// </summary>
		public int Id;

		public uint* InstructionAddressStart;
		public uint* InstructionAddressCurrent;
		public uint* InstructionAddressStall;
		public GpuStateStruct* GpuStateStructPointer;

		/// <summary>
		/// Stack with the InstructionAddressCurrent for the CALL/RET opcodes.
		/// </summary>
		readonly private Stack<IntPtr> ExecutionStack = new Stack<IntPtr>();

		/*
		private bool Finished;
		private bool Paused;
		private bool Ended;
		private bool Reset;
		private bool Restarted;
		*/

		/// <summary>
		/// Current status of the DisplayList.
		/// </summary>
		readonly public WaitableStateMachine<StatusEnum> Status = new WaitableStateMachine<StatusEnum>();

		/// <summary>
		/// Indicates if the list can be used.
		/// </summary>
		public bool Available { set; get; }

		/// <summary>
		/// 
		/// </summary>
		public OptionalParams pspGeListOptParam;

		/// <summary>
		/// 
		/// </summary>
		internal bool Done;

		/// <summary>
		/// 
		/// </summary>
		GpuDisplayListRunner GpuDisplayListRunner;

		//Action[] InstructionSwitch = new Action[256];

		/// <summary>
		/// Constructor
		/// </summary>
		internal GpuDisplayList()
		{
			GpuDisplayListRunner = new GpuDisplayListRunner()
			{
				GpuDisplayList = this,
			};

			/*
			var Names = typeof(GpuOpCodes).GetEnumNames();

			for (int n = 0; n < InstructionSwitch.Length; n++)
			{
				var MethodInfo = typeof(GpuDisplayListRunner).GetMethod(Names[n]);
				if (MethodInfo == null)
				{
					MethodInfo = typeof(GpuDisplayListRunner).GetMethod("UNKNOWN");
				}
				InstructionSwitch[n] = (Action)Delegate.CreateDelegate(typeof(Action), GpuDisplayListRunner, MethodInfo);
			}
			*/
		}

		/// <summary>
		/// Executes this Display List.
		/// </summary>
		internal void Process()
		{
			for (Done = false; !Done ; InstructionAddressCurrent++)
			{
				if ((InstructionAddressStall != null) && (InstructionAddressCurrent >= InstructionAddressStall)) break;
				ProcessInstruction();
			}

			if (InstructionAddressStall == null)
			{
				Status.Value = StatusEnum.Done;
				return;
			}

			if (InstructionAddressCurrent == InstructionAddressStall)
			{
				Status.Value = StatusEnum.StallReached;
				return;
			}
		}

		public delegate void GpuDisplayListRunnerDelegate(GpuDisplayListRunner GpuDisplayListRunner, GpuOpCodes GpuOpCode, uint Params);

		static public GpuDisplayListRunnerDelegate InstructionSwitch;

		static public GpuDisplayListRunnerDelegate GenerateSwitch()
		{
			//GpuDisplayListRunnerDelegate.
			var DynamicMethod = new DynamicMethod("", typeof(void), new Type[] { typeof(GpuDisplayListRunner), typeof(GpuOpCodes), typeof(uint) });
			ILGenerator ILGenerator = DynamicMethod.GetILGenerator();
			var SwitchLabels = new Label[typeof(GpuOpCodes).GetEnumValues().Length];
			var Names = typeof(GpuOpCodes).GetEnumNames();
			for (int n = 0; n < SwitchLabels.Length; n++)
			{
				SwitchLabels[n] = ILGenerator.DefineLabel();
			}
			ILGenerator.Emit(OpCodes.Ldarg_1);
			ILGenerator.Emit(OpCodes.Switch, SwitchLabels);
			ILGenerator.Emit(OpCodes.Ret);

			for (int n = 0; n < SwitchLabels.Length; n++)
			{
				ILGenerator.MarkLabel(SwitchLabels[n]);
				var MethodInfo = typeof(GpuDisplayListRunner).GetMethod("OP_" + Names[n]);
				if (MethodInfo == null)
				{
					Console.Error.WriteLine("Warning! Can't find Gpu.OpCode '" + Names[n] + "'");
					MethodInfo = typeof(GpuDisplayListRunner).GetMethod("OP_UNKNOWN");
				}
				if (MethodInfo.GetCustomAttributes(typeof(GpuOpCodesNotImplementedAttribute), true).Length > 0)
				{
					var MethodInfo2 = typeof(GpuDisplayListRunner).GetMethod("UNIMPLEMENTED_NOTICE");
					ILGenerator.Emit(OpCodes.Ldarg_0);
					//ILGenerator.Emit(OpCodes.Ldarg_1);
					//ILGenerator.Emit(OpCodes.Ldarg_2);
					ILGenerator.Emit(OpCodes.Call, MethodInfo2);
				}
				{
					ILGenerator.Emit(OpCodes.Ldarg_0);
					//ILGenerator.Emit(OpCodes.Ldarg_1);
					//ILGenerator.Emit(OpCodes.Ldarg_2);
					ILGenerator.Emit(OpCodes.Call, MethodInfo);
				}
				ILGenerator.Emit(OpCodes.Ret);
			}

			return (GpuDisplayListRunnerDelegate)DynamicMethod.CreateDelegate(typeof(GpuDisplayListRunnerDelegate));
		}

		private void ProcessInstruction()
		{
			var Instruction = *InstructionAddressCurrent;
			var OpCode = (GpuOpCodes)((Instruction >> 24) & 0xFF);
			var Params = ((Instruction) & 0xFFFFFF);

			/*
			if (OpCode == GpuOpCodes.END)
			{
				Done = true;
				return;
			}
			*/

			GpuDisplayListRunner.OpCode = OpCode;
			GpuDisplayListRunner.Params = Params;
			//InstructionSwitch[(int)OpCode]();
			InstructionSwitch(GpuDisplayListRunner, OpCode, Params);
		}
	}
}
