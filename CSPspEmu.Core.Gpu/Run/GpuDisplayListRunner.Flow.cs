using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.Run
{
	unsafe sealed public partial class GpuDisplayListRunner
	{
		/**
		 * Start filling a new display-context
		 *
		 * Contexts available are:
		 *   - GU_DIRECT - Rendering is performed as list is filled
		 *   - GU_CALL - List is setup to be called from the main list
		 *   - GU_SEND - List is buffered for a later call to sceGuSendList()
		 *
		 * The previous context-type is stored so that it can be restored at sceGuFinish().
		 *
		 * @param cid - Context Type
		 * @param list - Pointer to display-list (16 byte aligned)
		 **/
		// void sceGuStart(int cid, void* list);

		/**
		 * Finish current display list and go back to the parent context
		 *
		 * If the context is GU_DIRECT, the stall-address is updated so that the entire list will
		 * execute. Otherwise, only the terminating action is written to the list, depending on
		 * context-type.
		 *
		 * The finish-callback will get a zero as argument when using this function.
		 *
		 * This also restores control back to whatever context that was active prior to this call.
		 *
		 * @return Size of finished display list
		 **/
		// int sceGuFinish(void);

		/**
		 * Finish current display list and go back to the parent context, sending argument id for
		 * the finish callback.
		 *
		 * If the context is GU_DIRECT, the stall-address is updated so that the entire list will
		 * execute. Otherwise, only the terminating action is written to the list, depending on
		 * context-type.
		 *
		 * @param id - Finish callback id (16-bit)
		 * @return Size of finished display list
		 **/
		// int sceGuFinishId(unsigned int id);

		/**
		 * Call previously generated display-list
		 *
		 * @param list - Display list to call
		 **/
		// void sceGuCallList(const void* list);

		/**
		 * Set wether to use stack-based calls or signals to handle execution of called lists.
		 *
		 * @param mode - GU_TRUE(1) to enable signals, GU_FALSE(0) to disable signals and use
		 * normal calls instead.
		 **/
		// void sceGuCallMode(int mode);

		/**
		 * Check how large the current display-list is
		 *
		 * @return The size of the current display list
		 **/
		// int sceGuCheckList(void);

		/**
		 * Send a list to the GE directly
		 *
		 * Available modes are:
		 *   - GU_TAIL - Place list last in the queue, so it executes in-order
		 *   - GU_HEAD - Place list first in queue so that it executes as soon as possible
		 *
		 * @param mode - Whether to place the list first or last in queue
		 * @param list - List to send
		 * @param context - Temporary storage for the GE context
		 **/
		// void sceGuSendList(int mode, const void* list, PspGeContext* context);

		public void OP_JUMP()
		{
			GpuDisplayList.Jump((uint)(
				GpuDisplayList.GpuStateStructPointer->BaseAddress | (Params24 & ~3)
			));
		}

		public void OP_END()
		{
			GpuDisplayList.GpuProcessor.GpuImpl.End(GpuState);
			GpuDisplayList.Done = true;
		}

		public void OP_FINISH()
		{
			GpuDisplayList.GpuProcessor.GpuImpl.Finish(GpuDisplayList.GpuStateStructPointer);
			//gpu.storeFrameBuffer();
			/*
			gpu.impl.flush();
			gpu.finishEvent();
			*/
		}

		//[GpuOpCodesNotImplemented]
		public void OP_CALL()
		{
			GpuDisplayList.Call((uint)(
				GpuDisplayList.GpuStateStructPointer->BaseAddress | (Params24 & ~3)
			));
		}

		//[GpuOpCodesNotImplemented]
		public void OP_RET()
		{
			GpuDisplayList.Ret();
		}

		enum GU_BEHAVIOR
		{
			GU_BEHAVIOR_SUSPEND = 1,
			GU_BEHAVIOR_CONTINUE = 2
		}

		/**
		 * Trigger signal to call code from the command stream
		 *
		 * Available behaviors are:
		 *   - GU_BEHAVIOR_SUSPEND - Stops display list execution until callback function finished
		 *   - GU_BEHAVIOR_CONTINUE - Do not stop display list execution during callback
		 *
		 * @param signal - Signal to trigger
		 * @param behavior - Behavior type
		 **/
		// void sceGuSignal(int signal, int behavior);
		[GpuOpCodesNotImplemented]
		public void OP_SIGNAL()
		{
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
	}
}
