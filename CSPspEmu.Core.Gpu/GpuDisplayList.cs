using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading;
using CSPspEmu.Core.Gpu.Run;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Threading.Synchronization;
using CSharpUtils;

namespace CSPspEmu.Core.Gpu
{
	public sealed unsafe class GpuDisplayList
	{
		private static readonly Logger Logger = Logger.GetLogger("Gpu");

		private static bool Debug = false;
		//private static bool Debug = true;

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
		volatile public uint _InstructionAddressStart;

		/// <summary>
		/// 
		/// </summary>
		volatile public uint _InstructionAddressCurrent;

		/// <summary>
		/// 
		/// </summary>
		volatile private uint _InstructionAddressStall;

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
		private GlobalGpuState GlobalGpuState;

		/// <summary>
		/// 
		/// </summary>
		public uint InstructionAddressStart
		{
			get { return _InstructionAddressStart; }
			set { _InstructionAddressStart = value & PspMemory.MemoryMask; }
		}
		//volatile public uint InstructionAddressStart;

		/// <summary>
		/// 
		/// </summary>
		public uint InstructionAddressCurrent
		{
			get { return _InstructionAddressCurrent; }
			set { _InstructionAddressCurrent = value & PspMemory.MemoryMask; }
		}
		//volatile public uint InstructionAddressCurrent;

		/// <summary>
		/// 
		/// </summary>
		public uint InstructionAddressStall
		{
			get { return _InstructionAddressStall; }
		}

		/// <summary>
		/// Stack with the InstructionAddressCurrent for the CALL/RET opcodes.
		/// </summary>
		private readonly Stack<IntPtr> ExecutionStack = new Stack<IntPtr>();

		/// <summary>
		/// Current status of the DisplayList.
		/// </summary>
		public readonly WaitableStateMachine<DisplayListStatusEnum> Status = new WaitableStateMachine<DisplayListStatusEnum>();

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

		//PspWaitEvent OnFreed = new PspWaitEvent();
		//public enum Status2Enum
		//{
		//	Drawing,
		//	Free,
		//}
		//public readonly WaitableStateMachine<Status2Enum> Status2 = new WaitableStateMachine<Status2Enum>(Debug: false);

		public PspGeCallbackData Callbacks;
		public int CallbacksId;

		public SignalBehavior Signal;

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
			this.GpuDisplayListRunner = new GpuDisplayListRunner(this, GpuProcessor.GlobalGpuState);
		}

		public void SetInstructionAddressStall(uint value)
		{
			_InstructionAddressStall = value & PspMemory.MemoryMask;
			if (InstructionAddressStall != 0 && !PspMemory.IsAddressValid(InstructionAddressStall))
			{
				throw (new InvalidOperationException(String.Format("Invalid StallAddress! 0x{0}", InstructionAddressStall)));
			}
			if (Debug) Console.WriteLine("GpuDisplayList.SetInstructionAddressStall:{0:X8}", value);
			StallAddressUpdated.Set();
		}

		/// <summary>
		/// Executes this Display List.
		/// </summary>
		internal void Process()
		{
			Status.SetValue(DisplayListStatusEnum.Drawing);

			if (Debug) Console.WriteLine("Process() : {0} : 0x{1:X8} : 0x{2:X8} : 0x{3:X8}", this.Id, this.InstructionAddressCurrent, this.InstructionAddressStart, this.InstructionAddressStall);

			//for (Done = false; !Done; _InstructionAddressCurrent += 4)
			Done = false;
			while (!Done)
			{
				//Console.WriteLine("{0:X}", (uint)InstructionAddressCurrent);
				//if ((InstructionAddressStall != 0) && (InstructionAddressCurrent >= InstructionAddressStall)) break;
				if ((InstructionAddressStall != 0) && (InstructionAddressCurrent >= InstructionAddressStall))
				{
					if (Debug) Console.WriteLine("- STALLED --------------------------------------------------------------------");
					Status.SetValue(DisplayListStatusEnum.Stalling);
					StallAddressUpdated.WaitOne();
				}

				ProcessInstruction();
			}

			Status.SetValue(DisplayListStatusEnum.Completed);
		}

		public delegate void GpuDisplayListRunnerDelegate(GpuDisplayListRunner GpuDisplayListRunner, GpuOpCodes GpuOpCode, uint Params);

		private static readonly GpuDisplayListRunnerDelegate InstructionSwitch = GpuDisplayList.GenerateSwitch();
		private PspMemory Memory;

		private static GpuDisplayListRunnerDelegate GenerateSwitch()
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
					MethodInfo_Operation = ((Action)GpuDisplayListRunner.Methods.OP_UNKNOWN).Method;
				}
				if (MethodInfo_Operation.GetCustomAttributes(typeof(GpuOpCodesNotImplementedAttribute), true).Length > 0)
				{
					var MethodInfo_Unimplemented = ((Action)GpuDisplayListRunner.Methods.UNIMPLEMENTED_NOTICE).Method;
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

			//var Instruction = *(uint *)Memory.PspAddressToPointerUnsafe(_InstructionAddressCurrent);
			//var Instruction = Memory.Read4(_InstructionAddressCurrent);
			var Instruction = Memory.ReadSafe<uint>(_InstructionAddressCurrent);

			var WritePC = Memory.GetPCWriteAddress(_InstructionAddressCurrent);
			var OpCode = (GpuOpCodes)((Instruction >> 24) & 0xFF);
			var Params = ((Instruction) & 0xFFFFFF);

			//if (OpCode == GpuOpCodes.Unknown0xFF)
			GpuDisplayListRunner.PC = _InstructionAddressCurrent;
			GpuDisplayListRunner.OpCode = OpCode;
			GpuDisplayListRunner.Params24 = Params;

			InstructionSwitch(GpuDisplayListRunner, OpCode, Params);

			if (Debug)
			{
				Console.Error.WriteLine(
					"CODE(0x{0:X}-0x{1:X}) : PC(0x{2:X}) : {3} : 0x{4:X} : Done:{5}",
					InstructionAddressCurrent,
					InstructionAddressStall,
					WritePC,
					OpCode,
					Params,
					Done
				);
			}

			_InstructionAddressCurrent += 4;
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
				GlobalGpuState.BaseOffset = CallStack.Pop();
				JumpAbsolute(CallStack.Pop());
			}
			else
			{
				Console.Error.WriteLine("Stack is empty");
			}
		}

		public void GeListSync(Action NotifyOnceCallback)
		{
			//Thread.Sleep(200);
			//Status2.CallbackOnStateOnce(Status2Enum.Free, NotifyOnceCallback);
			Status.CallbackOnStateOnce(DisplayListStatusEnum.Completed, NotifyOnceCallback);
		}

		public void DoFinish(uint PC, uint Arg)
		{
			if (Debug) Console.WriteLine("FINISH: Arg:{0}", Arg);

			if (Callbacks.FinishFunction != 0)
			{
				GpuProcessor.Connector.Finish(PC, Callbacks, Arg);
			}
		}

		public void DoSignal(uint PC, uint Signal, SignalBehavior Behavior)
		{
			if (Debug) Console.WriteLine("SIGNAL : Behavior:{0}", Behavior);

			Status.SetValue(DisplayListStatusEnum.Paused);

			if (Callbacks.SignalFunction != 0)
			{
				//Console.Error.WriteLine("OP_SIGNAL! ({0}, {1})", Signal, Behavior);
				GpuProcessor.Connector.Signal(PC, Callbacks, Signal, Behavior);
			}

			Status.SetValue(DisplayListStatusEnum.Drawing);
		}

		public void SetQueued()
		{
			Status.SetValue(DisplayListStatusEnum.Queued);
		}

		public void SetDequeued()
		{
			//Status2.SetValue(Status2Enum.Dequeued);
		}

		public void SetFree()
		{
			//Status2.SetValue(Status2Enum.Free);
			Available = true;
		}

		public DisplayListStatusEnum PeekStatus()
		{
			return Status.Value;
		}

		public void DeQueue()
		{
			this.Done = true;
			GpuProcessor.DisplayListQueue.Remove(this);
		}
	}
}
