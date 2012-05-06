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
using CSharpUtils;

namespace CSPspEmu.Core.Gpu
{
	sealed unsafe public class GpuDisplayList
	{
		static Logger Logger = Logger.GetLogger("Gpu");

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

		GlobalGpuState GlobalGpuState;

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
			this.GlobalGpuState = GpuProcessor.GlobalGpuState;
			GpuDisplayListRunner = new GpuDisplayListRunner()
			{
				GpuDisplayList = this,
				GlobalGpuState = GpuProcessor.GlobalGpuState,
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
			var DynamicMethod = new DynamicMethod("GpuDisplayList.GenerateSwitch", typeof(void), new Type[] { typeof(GpuDisplayListRunner), typeof(GpuOpCodes), typeof(uint) });
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

			//var Instruction = Memory.Read4Unsafe(_InstructionAddressCurrent);
			var Instruction = Memory.ReadSafe<uint>(_InstructionAddressCurrent);

			var WritePC = Memory.GetPCWriteAddress(_InstructionAddressCurrent);
			var OpCode = (GpuOpCodes)((Instruction >> 24) & 0xFF);
			var Params = ((Instruction) & 0xFFFFFF);

			//if (OpCode == GpuOpCodes.Unknown0xFF)
			if (Debug)
			{
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
			GpuDisplayListRunner.PC = _InstructionAddressCurrent;
			InstructionSwitch(GpuDisplayListRunner, OpCode, Params);
		}

		internal void JumpRelativeOffset(uint Address)
		{
			InstructionAddressCurrent = GlobalGpuState.GetAddressRelativeToBaseOffset(Address - 4);
			//throw new NotImplementedException();
		}

		internal void JumpAbsolute(uint Address)
		{
			InstructionAddressCurrent = (Address - 4);
		}

		internal void CallRelativeOffset(uint Address)
		{
			CallStack.Push(InstructionAddressCurrent + 4);
			CallStack.Push((uint)GlobalGpuState.BaseOffset);
			//CallStack.Push(InstructionAddressCurrent);
			JumpRelativeOffset(Address);
			//throw new NotImplementedException();
		}

		internal void Ret()
		{
			if (CallStack.Count > 0)
			{
				GlobalGpuState.BaseOffset = (int)CallStack.Pop();
				JumpAbsolute(CallStack.Pop());
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
		public int CallbacksId;

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
			PSP_GE_SIGNAL_HANDLER_SUSPEND  = 0x01,
			PSP_GE_SIGNAL_HANDLER_CONTINUE = 0x02,
			PSP_GE_SIGNAL_HANDLER_PAUSE    = 0x03,
			PSP_GE_SIGNAL_SYNC             = 0x08,
			PSP_GE_SIGNAL_JUMP             = 0x10,
			PSP_GE_SIGNAL_CALL             = 0x11,
			PSP_GE_SIGNAL_RETURN           = 0x12,
			PSP_GE_SIGNAL_TBP0_REL         = 0x20,
			PSP_GE_SIGNAL_TBP1_REL         = 0x21,
			PSP_GE_SIGNAL_TBP2_REL         = 0x22,
			PSP_GE_SIGNAL_TBP3_REL         = 0x23,
			PSP_GE_SIGNAL_TBP4_REL         = 0x24,
			PSP_GE_SIGNAL_TBP5_REL         = 0x25,
			PSP_GE_SIGNAL_TBP6_REL         = 0x26,
			PSP_GE_SIGNAL_TBP7_REL         = 0x27,
			PSP_GE_SIGNAL_TBP0_REL_OFFSET  = 0x28,
			PSP_GE_SIGNAL_TBP1_REL_OFFSET  = 0x29,
			PSP_GE_SIGNAL_TBP2_REL_OFFSET  = 0x2A,
			PSP_GE_SIGNAL_TBP3_REL_OFFSET  = 0x2B,
			PSP_GE_SIGNAL_TBP4_REL_OFFSET  = 0x2C,
			PSP_GE_SIGNAL_TBP5_REL_OFFSET  = 0x2D,
			PSP_GE_SIGNAL_TBP6_REL_OFFSET  = 0x2E,
			PSP_GE_SIGNAL_TBP7_REL_OFFSET  = 0x2F,
			PSP_GE_SIGNAL_BREAK            = 0xFF,
		}

		public void Finish(uint Arg)
		{
			if (Callbacks.FinishFunction != 0)
			{
#if true
				// triggerAsyncCallback(cbid, listId, PSP_GE_SIGNAL_HANDLER_SUSPEND, callbackNotifyArg1, finishCallbacks);

				GpuProcessor.HleInterop.ExecuteFunctionLater(
					Callbacks.FinishFunction,
					(Result) =>
					{
						//Console.Error.WriteLine("OP_FINISH! : ENDED : {0}", Result);
					},
					CallbacksId,
					Callbacks.FinishArgument
				);
#else
				GpuProcessor.HleInterop.ExecuteFunctionLater(
					Callbacks.FinishFunction,
					(Result) =>
					{
						//Console.Error.WriteLine("OP_FINISH! : ENDED : {0}", Result);
					},
					CallbacksId,
					Id,
					Arg
				);
#endif
				//Callbacks.FinishFunction();
				//GpuProcessor.interop
				//GpuProcessor.
				//Console.Error.WriteLine("OP_FINISH! : SCHEDULED");
			}
		}

		public void Signal(uint Signal, GuBehavior Behavior)
		{
			Console.WriteLine("SIGNAL : Behavior:{0}", Behavior);

			if (Callbacks.SignalFunction != 0)
			{
				Console.Error.WriteLine("OP_SIGNAL! ({0}, {1})", Signal, Behavior);
			}
			switch (Behavior)
			{
				default:
				{
					//Logger.
					/*
					var Result = GpuProcessor.HleInterop.ExecuteFunctionNow(
						Callbacks.SignalFunction,
						//CallbacksId,
						(int)Behavior,
						Callbacks.SignalArgument
					);
					Console.Error.WriteLine("OP_SIGNAL! : ENDED : {0}", Result);
					*/
					GpuProcessor.HleInterop.ExecuteFunctionLater(
						Callbacks.SignalFunction,
						(Result) =>
						{
							Console.Error.WriteLine("OP_SIGNAL! : ENDED : {0}", Result);
						},
						//CallbacksId,
						(int)Behavior,
						Callbacks.SignalArgument
					);
					Console.Error.WriteLine("OP_SIGNAL! : ENQUEUED");

					Logger.Error("Not implemented Signal Behavior: " + Behavior);
				}
				break;
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
