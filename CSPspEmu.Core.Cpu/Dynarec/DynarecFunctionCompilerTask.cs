using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Memory;
using System.Threading.Tasks;
using System.Threading;
using CSPspEmu.Core.Threading.Synchronization;

namespace CSPspEmu.Core.Cpu.Dynarec
{
	public class DynarecFunctionCompilerTask : PspEmulatorComponent
	{
		[Inject]
		protected DynarecFunctionCompiler DynarecFunctionCompiler;

		[Inject]
		protected PspMemory PspMemory;

		[Inject]
		protected MethodCacheFast MethodCacheFast;

		IInstructionReader InstructionReader;

		public class FunctionQueueItem
		{
			public uint PC;
			public DynarecFunction Function;
		}

		MessagePipe<FunctionQueueItem> FunctionToGeneratePipe = new MessagePipe<FunctionQueueItem>();
		AutoResetEvent CheckQueueEvent = new AutoResetEvent(false);
		Thread ProcessThread;

		public override void InitializeComponent()
		{
			InstructionReader = new InstructionStreamReader(new PspMemoryStream(PspMemory));
			ProcessThread = new Thread(ProcessMain);
			ProcessThread.Name = "DynarecFunctionCompilerTaskThread";
			ProcessThread.IsBackground = true;
			ProcessThread.Start();

			MethodCacheFast.OnClearRange += new Action<uint, uint>(MethodCacheFast_OnClearRange);
		}

		void MethodCacheFast_OnClearRange(uint Low, uint High)
		{
		}

		private void ExploreNewPc(uint PC)
		{
#if true
			if (PC != 0)
			{
				FunctionToGeneratePipe.PushLast(new FunctionQueueItem() { PC = PC });
			}
#endif
		}

		private void ProcessMain()
		{
			while (true)
			{
				FunctionToGeneratePipe.Receive((Dequeued) =>
				{
#if false
					Console.WriteLine("DynarecFunctionCompilerTask.Processing 0x{0:X}", Dequeued.PC);
#endif

					var DynarecFunction = MethodCacheFast.TryGetMethodAt(Dequeued.PC);
					if (DynarecFunction == null)
					{
						DynarecFunction = DynarecFunctionCompiler.CreateFunction(InstructionReader, Dequeued.PC, ExploreNewPc);
						MethodCacheFast.SetMethodAt(Dequeued.PC, DynarecFunction);
					}
					Dequeued.Function = DynarecFunction;
				});
			}
		}

		public DynarecFunction GetFunctionForAddress(uint PC)
		{
			var DynarecFunction = MethodCacheFast.TryGetMethodAt(PC);
			if (DynarecFunction == null)
			{
				var FunctionQueueItem = new FunctionQueueItem() { PC = PC };

				FunctionToGeneratePipe.PushFirstAndWait(FunctionQueueItem);
				
				MethodCacheFast.SetMethodAt(PC, DynarecFunction = FunctionQueueItem.Function);
			}
			return DynarecFunction;
		}

		public override void Dispose()
		{
			base.Dispose();
		}
	}
}
