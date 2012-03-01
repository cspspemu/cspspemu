using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CSPspEmu.Core.Threading.Synchronization;
using CSPspEmu.Core.Gpu.State;
using System.Reflection.Emit;
using CSPspEmu.Core.Gpu.Run;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Core.Gpu
{
	sealed unsafe public class GpuDisplayList
	{
		//private const bool Debug = false;
		private bool Debug = false;
		//private const bool Debug = true;

		/// <summary>
		/// 
		/// </summary>
		public struct OptionalParams
		{
			public int ContextAddress;
			public int StackDepth;
			public int StackAddress;
		}

		/// <summary>
		/// 
		/// </summary>
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

		/// <summary>
		/// 
		/// </summary>
		public GpuProcessor GpuProcessor;

		/// <summary>
		/// 
		/// </summary>
		public uint _InstructionAddressStart;
		//volatile public uint InstructionAddressStart;

		/// <summary>
		/// 
		/// </summary>
		public uint _InstructionAddressCurrent;
		//volatile public uint InstructionAddressCurrent;

		/// <summary>
		/// 
		/// </summary>
		private uint _InstructionAddressStall;

		/// <summary>
		/// 
		/// </summary>
		AutoResetEvent StallAddressUpdated = new AutoResetEvent(false);

		/// <summary>
		/// 
		/// </summary>
		public GpuStateStruct* GpuStateStructPointer { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public uint InstructionAddressStart {
			get
			{
				return _InstructionAddressStart;
			}
			set
			{
				_InstructionAddressStart = value & PspMemory.MemoryMask;
			}
		}
		//volatile public uint InstructionAddressStart;

		/// <summary>
		/// 
		/// </summary>
		public uint InstructionAddressCurrent {
			get
			{
				return _InstructionAddressCurrent;
			}
			set
			{
				_InstructionAddressCurrent = value & PspMemory.MemoryMask;
			}
		}
		//volatile public uint InstructionAddressCurrent;

		/// <summary>
		/// 
		/// </summary>
		public uint InstructionAddressStall
		{
			get
			{
				return _InstructionAddressStall;
			}
			set
			{
				_InstructionAddressStall = value & PspMemory.MemoryMask;
				if (InstructionAddressStall != 0 && !Memory.IsAddressValid(InstructionAddressStall))
				{
					throw (new InvalidOperationException(String.Format("Invalid StallAddress! 0x{0}", InstructionAddressStall)));
				}
				StallAddressUpdated.Set();
			}
		}

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
		private GpuDisplayListRunner GpuDisplayListRunner;

		/// <summary>
		/// 
		/// </summary>
		public Stack<uint> CallStack = new Stack<uint>();

		//Action[] InstructionSwitch = new Action[256];

		/// <summary>
		/// Constructor
		/// </summary>
		internal GpuDisplayList(PspMemory Memory, GpuProcessor GpuProcessor, int Id)
		{
			this.Memory = Memory;
			this.GpuProcessor = GpuProcessor;
			this.Id = Id;
			GpuDisplayListRunner = new GpuDisplayListRunner()
			{
				GpuDisplayList = this,
			};
		}

		/// <summary>
		/// Executes this Display List.
		/// </summary>
		internal void Process()
		{
			Status2.SetValue(Status2Enum.Drawing);

		Loop:
			for (Done = false; !Done; _InstructionAddressCurrent += 4)
			{
				//Console.WriteLine("{0:X}", (uint)InstructionAddressCurrent);
				//if ((InstructionAddressStall != 0) && (InstructionAddressCurrent >= InstructionAddressStall)) break;
				if ((InstructionAddressStall != 0) && (InstructionAddressCurrent == InstructionAddressStall)) break;
				ProcessInstruction();
			}

			if (Debug) Console.WriteLine("[1]");

			if (Done)
			{
				if (Debug) Console.WriteLine("- DONE0 ----------------------------------------------------------------------");
				Status.SetValue(StatusEnum.Done);
				Status2.SetValue(Status2Enum.Completed);
				return;
			}

			if (InstructionAddressStall == 0)
			{
				if (Debug) Console.WriteLine("- DONE1 ----------------------------------------------------------------------");
				Status.SetValue(StatusEnum.Done);
				Status2.SetValue(Status2Enum.Completed);
				return;
			}

			if (InstructionAddressCurrent == InstructionAddressStall)
			{
				if (Debug) Console.WriteLine("- STALLED --------------------------------------------------------------------");
				Status.SetValue(StatusEnum.StallReached);
				Status2.SetValue(Status2Enum.Completed);
				StallAddressUpdated.WaitOne();
				goto Loop;
			}

			if (Debug) Console.WriteLine("- DONE2 ----------------------------------------------------------------------");
			Status.SetValue(StatusEnum.Done);
		}

		public delegate void GpuDisplayListRunnerDelegate(GpuDisplayListRunner GpuDisplayListRunner, GpuOpCodes GpuOpCode, uint Params);

		static public GpuDisplayListRunnerDelegate InstructionSwitch;
		private unsafe PspMemory Memory;

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
				var MethodInfo_Operation = typeof(GpuDisplayListRunner).GetMethod("OP_" + Names[n]);
				if (MethodInfo_Operation == null)
				{
					Console.Error.WriteLine("Warning! Can't find Gpu.OpCode '" + Names[n] + "'");
					MethodInfo_Operation = typeof(GpuDisplayListRunner).GetMethod("OP_UNKNOWN");
				}
				if (MethodInfo_Operation.GetCustomAttributes(typeof(GpuOpCodesNotImplementedAttribute), true).Length > 0)
				{
					var MethodInfo_Unimplemented = typeof(GpuDisplayListRunner).GetMethod("UNIMPLEMENTED_NOTICE");
					ILGenerator.Emit(OpCodes.Ldarg_0);
					//ILGenerator.Emit(OpCodes.Ldarg_1);
					//ILGenerator.Emit(OpCodes.Ldarg_2);
					ILGenerator.Emit(OpCodes.Call, MethodInfo_Unimplemented);
				}
				{
					ILGenerator.Emit(OpCodes.Ldarg_0);
					//ILGenerator.Emit(OpCodes.Ldarg_1);
					//ILGenerator.Emit(OpCodes.Ldarg_2);
					ILGenerator.Emit(OpCodes.Call, MethodInfo_Operation);
				}
				ILGenerator.Emit(OpCodes.Ret);
			}

			return (GpuDisplayListRunnerDelegate)DynamicMethod.CreateDelegate(typeof(GpuDisplayListRunnerDelegate));
		}

		private void ProcessInstruction()
		{
			//Console.WriteLine("{0:X}", InstructionAddressCurrent);
			var Instruction = Memory.Read4(_InstructionAddressCurrent);
			var WritePC = Memory.GetPCWriteAddress(_InstructionAddressCurrent);
			var OpCode = (GpuOpCodes)((Instruction >> 24) & 0xFF);
			var Params = ((Instruction) & 0xFFFFFF);

			//if (OpCode == GpuOpCodes.Unknown0xFF)
			if (Debug) {
				Console.WriteLine(
					"CODE(0x{0:X}-0x{1:X}) : PC(0x{2:X}:0x{3:X}) : {4} : 0x{5:X}",
					InstructionAddressCurrent,
					InstructionAddressStall,
					WritePC,
					WritePC - GpuProcessor.PspConfig.RelocatedBaseAddress,
					OpCode,
					Params
				);
			}

			GpuDisplayListRunner.OpCode = OpCode;
			GpuDisplayListRunner.Params24 = Params;
			//InstructionSwitch[(int)OpCode]();
			InstructionSwitch(GpuDisplayListRunner, OpCode, Params);
		}

		internal void Jump(uint Address)
		{
			InstructionAddressCurrent = Address - 4;
			//throw new NotImplementedException();
		}

		internal void Call(uint Address)
		{
			CallStack.Push(InstructionAddressCurrent + 4);
			Jump(Address);
			//throw new NotImplementedException();
		}

		internal void Ret()
		{
			if (CallStack.Count > 0)
			{
				Jump(CallStack.Pop());
			}
			else
			{
				Console.Error.WriteLine("Stack is empty");
			}
		}

		//PspWaitEvent OnFreed = new PspWaitEvent();
		public enum Status2Enum
		{
			Drawing = 0,
			Completed = 1,
			Free = 2,
		}
		readonly public WaitableStateMachine<Status2Enum> Status2 = new WaitableStateMachine<Status2Enum>(Debug: false);
		public PspGeCallbackData Callbacks;

		public void Freed()
		{
			//Console.Error.WriteLine(Id);
			lock (this)
			{
				//OnFreed.Signal();
				Status2.SetValue(Status2Enum.Free);
				Available = true;
			}
		}

		public void GeListSync(Gpu.GpuProcessor.SyncTypeEnum SyncType, Action NotifyOnceCallback)
		{
			//Console.WriteLine("GeListSync");
			if (SyncType != Gpu.GpuProcessor.SyncTypeEnum.ListDone) throw new NotImplementedException();
			lock (this)
			{
				Status2.CallbackOnStateOnce(Status2Enum.Free, () =>
				{
					NotifyOnceCallback();
				});
			}
			//Status.CallbackOnStateOnce(StatusEnum.Done, NotifyOnceCallback);
			/*
			CallbackOnStateOnce
			Console.WriteLine("Waiting for DONE");
			Status.WaitForState(StatusEnum.Done);
			Console.WriteLine("Ended Waiting for DONE");
			*/
		}

		public enum GuBehavior
		{
			GU_BEHAVIOR_SUSPEND = 1,
			GU_BEHAVIOR_CONTINUE = 2
		}

		public void Finish()
		{
			if (Callbacks.FinishFunction != 0)
			{
				//GpuProcessor.
				//Console.Error.WriteLine("OP_FINISH!");
			}
		}

		public void Signal(uint Signal, GuBehavior Behavior)
		{
			if (Callbacks.SignalFunction != 0)
			{
				Console.Error.WriteLine("OP_SIGNAL! ({0}, {1})", Signal, Behavior);
			}
			//GpuProcessor.PspConfig
			/*
			auto signal   = command.extract!(uint, 16,  8);
			auto behavior = cast(GU_BEHAVIOR)command.extract!(uint,  0, 16);
			writefln("*OP_SIGNAL(%d, %d)", signal, behavior);

			auto call = delegate() {
				gpu.signalEvent(signal);
			};
		

			final switch (behavior) {
				case GU_BEHAVIOR.GU_BEHAVIOR_SUSPEND:
					call();
				break;
				case GU_BEHAVIOR.GU_BEHAVIOR_CONTINUE:
					Thread thread = new Thread(call);
					thread.name = "Gpu.OP_SIGNAL";
					thread.start();
				break;
			}
			*/
		}

		public void WaitCompletedSync()
		{
			//Status2.SetValue(Status2Enum.Completed);
			Status.WaitForAnyState(StatusEnum.StallReached, StatusEnum.DrawingDone, StatusEnum.Done);
			//throw new NotImplementedException();
		}
	}
}
