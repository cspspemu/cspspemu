using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using CSPspEmu.Core.Gpu.Run;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Threading.Synchronization;
using CSharpUtils;
using System.Runtime.InteropServices;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Gpu
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
    public struct GpuInstruction
    {
        public uint Instruction;

        public GpuOpCodes OpCode => (GpuOpCodes) ((Instruction >> 24) & 0xFF);

        public uint Params => ((Instruction) & 0xFFFFFF);
    }

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
        private volatile uint InstructionAddressStart;

        /// <summary>
        /// 
        /// </summary>
        private volatile uint InstructionAddressCurrent;

        /// <summary>
        /// 
        /// </summary>
        private volatile uint InstructionAddressStall;

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

        //volatile public uint InstructionAddressCurrent;

        /// <summary>
        /// Stack with the InstructionAddressCurrent for the CALL/RET opcodes.
        /// </summary>
        private readonly Stack<IntPtr> ExecutionStack = new Stack<IntPtr>();

        /// <summary>
        /// Current status of the DisplayList.
        /// </summary>
        public readonly WaitableStateMachine<DisplayListStatusEnum> Status =
            new WaitableStateMachine<DisplayListStatusEnum>();

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
        public Stack<uint> CallStack = new Stack<uint>(0x10);

        public delegate void GpuDisplayListRunnerDelegate(GpuDisplayListRunner GpuDisplayListRunner,
            GpuOpCodes GpuOpCode, uint Params);

        private static readonly GpuDisplayListRunnerDelegate InstructionSwitch = GenerateSwitch();
        internal PspMemory Memory;


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
            GlobalGpuState = GpuProcessor.GlobalGpuState;
            GpuDisplayListRunner = new GpuDisplayListRunner(this, GpuProcessor.GlobalGpuState);
        }

        public void SetInstructionAddressStartAndCurrent(uint value)
        {
            InstructionAddressCurrent = value & PspMemory.MemoryMask;
            InstructionAddressStart = value & PspMemory.MemoryMask;
        }

        public void SetInstructionAddressStall(uint value)
        {
            InstructionAddressStall = value & PspMemory.MemoryMask;
            if (InstructionAddressStall != 0 && !PspMemory.IsAddressValid(InstructionAddressStall))
            {
                throw (new InvalidOperationException($"Invalid StallAddress! 0x{InstructionAddressStall}"));
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

            if (Debug)
                Console.WriteLine("Process() : {0} : 0x{1:X8} : 0x{2:X8} : 0x{3:X8}", Id,
                    InstructionAddressCurrent, InstructionAddressStart, InstructionAddressStall);

            Done = false;
            while (!Done)
            {
                if ((InstructionAddressStall != 0) && (InstructionAddressCurrent >= InstructionAddressStall))
                {
                    if (Debug)
                        Console.WriteLine(
                            "- STALLED --------------------------------------------------------------------");
                    Status.SetValue(DisplayListStatusEnum.Stalling);
                    while (!StallAddressUpdated.WaitOne(TimeSpan.FromSeconds(2)))
                    {
                        ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Magenta, () =>
                        {
                            Console.WriteLine("DisplayListQueue.GetCountLock(): {0}",
                                GpuProcessor.DisplayListQueue.GetCountLock());
                            Console.WriteLine("CurrentGpuDisplayList.Status: {0}", Status.ToStringDefault());
                        });
                        if (GpuProcessor.Syncing)
                        {
                            Done = true;
                            Status.SetValue(DisplayListStatusEnum.Completed);
                            return;
                        }
                    }
                }

                ProcessInstruction();
            }

            Status.SetValue(DisplayListStatusEnum.Completed);
        }

        private static GpuDisplayListRunnerDelegate GenerateSwitch()
        {
            //GpuDisplayListRunnerDelegate.
            var DynamicMethod = new DynamicMethod("GpuDisplayList.GenerateSwitch", typeof(void),
                new[] {typeof(GpuDisplayListRunner), typeof(GpuOpCodes), typeof(uint)});
            ILGenerator ILGenerator = DynamicMethod.GetILGenerator();
            var switchLabels = new Label[typeof(GpuOpCodes).GetEnumValues().Length];
            var names = typeof(GpuOpCodes).GetEnumNames();
            for (var n = 0; n < switchLabels.Length; n++)
            {
                switchLabels[n] = ILGenerator.DefineLabel();
            }
            ILGenerator.Emit(OpCodes.Ldarg_1);
            ILGenerator.Emit(OpCodes.Switch, switchLabels);
            ILGenerator.Emit(OpCodes.Ret);

            var opcodesToMethods = new Dictionary<GpuOpCodes, MethodInfo>();

            foreach (var methodInfo in typeof(GpuDisplayListRunner).GetMethods())
            {
                var gpuInstructionAttributes = methodInfo.GetAttribute<GpuInstructionAttribute>().FirstOrDefault();
                if (gpuInstructionAttributes != null)
                {
                    opcodesToMethods[gpuInstructionAttributes.Opcode] = methodInfo;
                }
            }

            for (var n = 0; n < switchLabels.Length; n++)
            {
                ILGenerator.MarkLabel(switchLabels[n]);
                var MethodInfo_Operation = opcodesToMethods[(GpuOpCodes) n];
                if (MethodInfo_Operation == null)
                {
                    throw new InvalidProgramException($"Can't find method for gpu opcode {(GpuOpCodes) n}");
                }
                //var MethodInfo_Operation = typeof(GpuDisplayListRunner).GetMethod("OP_" + Names[n]);
                //var MethodInfo_Operation = typeof(GpuDisplayListRunner).GetMethod("OP_" + Names[n]);
                if (MethodInfo_Operation == null)
                {
                    Console.Error.WriteLine("Warning! Can't find Gpu.OpCode '" + names[n] + "'");
                    MethodInfo_Operation = ((Action) GpuDisplayListRunner.Methods.OP_UNKNOWN).Method;
                }
                if (MethodInfo_Operation.GetCustomAttributes(typeof(GpuOpCodesNotImplementedAttribute), true).Length >
                    0)
                {
                    var MethodInfo_Unimplemented = ((Action) GpuDisplayListRunner.Methods.UNIMPLEMENTED_NOTICE).Method;
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

            return (GpuDisplayListRunnerDelegate) DynamicMethod.CreateDelegate(typeof(GpuDisplayListRunnerDelegate));
        }

        internal GpuInstruction ReadInstructionAndMoveNext()
        {
            var Value = *(GpuInstruction*) Memory.PspAddressToPointerUnsafe(InstructionAddressCurrent);
            InstructionAddressCurrent += 4;
            return Value;
        }

        private void ProcessInstruction()
        {
            GpuDisplayListRunner.Pc = InstructionAddressCurrent;
            var Instruction = ReadInstructionAndMoveNext();
            GpuDisplayListRunner.OpCode = Instruction.OpCode;
            GpuDisplayListRunner.Params24 = Instruction.Params;

            InstructionSwitch(GpuDisplayListRunner, Instruction.OpCode, Instruction.Params);

            if (Debug)
            {
                var WritePC = Memory.GetPCWriteAddress(GpuDisplayListRunner.Pc);

                Console.Error.WriteLine(
                    "CODE(0x{0:X}-0x{1:X}) : PC(0x{2:X}) : {3} : 0x{4:X} : Done:{5}",
                    InstructionAddressCurrent,
                    InstructionAddressStall,
                    WritePC,
                    Instruction.OpCode,
                    Instruction.Params,
                    Done
                );
            }
        }

        internal void JumpRelativeOffset(uint Address)
        {
            InstructionAddressCurrent = GpuStateStructPointer->GetAddressRelativeToBaseOffset(Address);
        }

        internal void JumpAbsolute(uint Address)
        {
            InstructionAddressCurrent = Address;
        }

        internal void CallRelativeOffset(uint Address)
        {
            CallStack.Push(InstructionAddressCurrent);
            CallStack.Push((uint) GpuStateStructPointer->BaseOffset);
            //CallStack.Push(InstructionAddressCurrent);
            JumpRelativeOffset(Address);
            //throw new NotImplementedException();
        }

        internal void Ret()
        {
            if (CallStack.Count > 0)
            {
                GpuStateStructPointer->BaseOffset = CallStack.Pop();
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

        public void DoFinish(uint PC, uint Arg, bool ExecuteNow)
        {
            if (Debug) Console.WriteLine("FINISH: Arg:{0}", Arg);

            if (Callbacks.FinishFunction != 0)
            {
                GpuProcessor.Connector.Finish(PC, Callbacks, Arg, ExecuteNow);
            }
        }

        public void DoSignal(uint PC, uint Signal, SignalBehavior Behavior, bool ExecuteNow)
        {
            if (Debug) Console.WriteLine("SIGNAL : {0}: Behavior:{1}", Signal, Behavior);

            Status.SetValue(DisplayListStatusEnum.Paused);

            if (Callbacks.SignalFunction != 0)
            {
                //Console.Error.WriteLine("OP_SIGNAL! ({0}, {1})", Signal, Behavior);
                GpuProcessor.Connector.Signal(PC, Callbacks, Signal, Behavior, ExecuteNow);
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
            Done = true;
            GpuProcessor.DisplayListQueue.Remove(this);
        }
    }
}


/*
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading;
using CSPspEmu.Core.Gpu.Run;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Threading.Synchronization;
using CSharpUtils;
using System.Runtime.InteropServices;
using CSharpUtils.Threading;

namespace CSPspEmu.Core.Gpu
{
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
	public struct GpuInstruction
	{
		public uint Instruction;
		public GpuOpCodes OpCode { get { return (GpuOpCodes)((Instruction >> 24) & 0xFF); } }
		public uint Params { get { return ((Instruction) & 0xFFFFFF); } }
	}

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
		volatile private uint InstructionAddressStart;

		/// <summary>
		/// 
		/// </summary>
		volatile private uint InstructionAddressCurrent;

		/// <summary>
		/// 
		/// </summary>
		volatile private uint InstructionAddressStall;

		/// <summary>
		/// 
		/// </summary>
		public GpuStateStruct* GpuStateStructPointer { get; set; }

		/// <summary>
		/// 
		/// </summary>
		private GlobalGpuState GlobalGpuState;

		//volatile public uint InstructionAddressCurrent;

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
		public Stack<uint> CallStack = new Stack<uint>(0x10);

		public delegate void GpuDisplayListRunnerDelegate(GpuDisplayListRunner GpuDisplayListRunner, GpuOpCodes GpuOpCode, uint Params);
		private static readonly GpuDisplayListRunnerDelegate InstructionSwitch = GpuDisplayList.GenerateSwitch();
		internal PspMemory Memory;


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

		public void SetInstructionAddressStartAndCurrent(uint value)
		{
			this.InstructionAddressCurrent = value & PspMemory.MemoryMask;
			this.InstructionAddressStart = value & PspMemory.MemoryMask;
		}

		public void SetInstructionAddressStall(uint value)
		{
			value &= PspMemory.MemoryMask;

			if (value != 0 && !PspMemory.IsAddressValid(value))
			{
				throw (new InvalidOperationException(String.Format("Invalid StallAddress! 0x{0}", value)));
			}
			//if (Debug) Console.WriteLine("GpuDisplayList.SetInstructionAddressStall[Start]:{0:X8}", value);

			//InstructionAddressStall = value;
			ThreadTaskQueue.EnqueueWithoutWaiting(() =>
			{
				//if (Debug) Console.WriteLine("GpuDisplayList.SetInstructionAddressStall[End]:{0:X8}", value);
				InstructionAddressStall = value;
			});
		}

		private readonly TaskQueue ThreadTaskQueue = new TaskQueue();

		/// <summary>
		/// Executes this Display List.
		/// </summary>
		internal void Process()
		{
			Status.SetValue(DisplayListStatusEnum.Drawing);

			if (Debug) Console.WriteLine("Process() : {0} : 0x{1:X8} : 0x{2:X8} : 0x{3:X8}", this.Id, this.InstructionAddressCurrent, this.InstructionAddressStart, this.InstructionAddressStall);

			ThreadTaskQueue.HandleEnqueued();

			Done = false;
			while (!Done)
			{
				if ((InstructionAddressStall != 0) && (InstructionAddressCurrent >= InstructionAddressStall))
				{
					Status.SetTemporalValue(DisplayListStatusEnum.Stalling, () =>
					{
						while ((InstructionAddressStall != 0) && (InstructionAddressCurrent >= InstructionAddressStall))
						{
							if (Debug) Console.Out.WriteLineColored(ConsoleColor.Red, "- STALLED[{0}] --------------------------------------------------------------------", this.Id);

								ThreadTaskQueue.WaitAndHandleEnqueued();

								//ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Magenta, () =>
								//{
								//	Console.WriteLine("DisplayListQueue.GetCountLock(): {0}", GpuProcessor.DisplayListQueue.GetCountLock());
								//	Console.WriteLine("CurrentGpuDisplayList.Status: {0}", Status.ToStringDefault());
								//});

							if (GpuProcessor.Syncing)
							{
								Done = true;
								Status.SetValue(DisplayListStatusEnum.Completed);
								return;
							}
						}
					});
				}

				//Console.WriteLine("[1]");
				ProcessInstruction();
			}

			Status.SetValue(DisplayListStatusEnum.Completed);
		}

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

		internal GpuInstruction ReadInstructionAndMoveNext()
		{
			var Value = *(GpuInstruction*)Memory.PspAddressToPointerUnsafe(InstructionAddressCurrent);
			InstructionAddressCurrent += 4;
			return Value;
		}

		private void ProcessInstruction()
		{
			GpuDisplayListRunner.PC = InstructionAddressCurrent;
			var Instruction = ReadInstructionAndMoveNext();
			GpuDisplayListRunner.OpCode = Instruction.OpCode;
			GpuDisplayListRunner.Params24 = Instruction.Params;

			InstructionSwitch(GpuDisplayListRunner, Instruction.OpCode, Instruction.Params);

			if (Debug)
			{
				var WritePC = Memory.GetPCWriteAddress(GpuDisplayListRunner.PC);

				Console.Error.WriteLineColored(
					ConsoleColor.Cyan,
					"CODE(0x{0:X}-0x{1:X}-0x{2:X}) : PC(0x{3:X}) : {4} : 0x{5:X} : Done:{6}",
					InstructionAddressStart,
					GpuDisplayListRunner.PC,
					InstructionAddressStall,
					WritePC,
					Instruction.OpCode,
					Instruction.Params,
					Done
				);
			}
		}

		internal void JumpRelativeOffset(uint Address)
		{
			InstructionAddressCurrent = GpuStateStructPointer->GetAddressRelativeToBaseOffset(Address);
		}

		internal void JumpAbsolute(uint Address)
		{
			InstructionAddressCurrent = Address;
		}

		internal void CallRelativeOffset(uint Address)
		{
			CallStack.Push(InstructionAddressCurrent);
			CallStack.Push((uint)GpuStateStructPointer->BaseOffset);
			//CallStack.Push(InstructionAddressCurrent);
			JumpRelativeOffset(Address);
			//throw new NotImplementedException();
		}

		internal void Ret()
		{
			if (CallStack.Count > 0)
			{
				GpuStateStructPointer->BaseOffset = CallStack.Pop();
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

		public void DoFinish(uint PC, uint Arg, bool ExecuteNow)
		{
			if (Debug) Console.WriteLine("FINISH: Arg:{0}", Arg);

			if (Callbacks.FinishFunction != 0)
			{
				GpuProcessor.Connector.Finish(PC, Callbacks, Arg, ExecuteNow);
			}
		}

		public void DoSignal(uint PC, uint Signal, SignalBehavior Behavior, bool ExecuteNow)
		{
			if (Debug) Console.WriteLine("SIGNAL : {0}: Behavior:{1}", Signal, Behavior);

			Status.SetValue(DisplayListStatusEnum.Paused);

			if (Callbacks.SignalFunction != 0)
			{
				//Console.Error.WriteLine("OP_SIGNAL! ({0}, {1})", Signal, Behavior);
				GpuProcessor.Connector.Signal(PC, Callbacks, Signal, Behavior, ExecuteNow);
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
*/