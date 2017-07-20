using System;

namespace CSPspEmu.Core.Gpu.Run
{
    public sealed unsafe partial class GpuDisplayListRunner
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

        [GpuInstructionAttribute(GpuOpCodes.JUMP)]
        public void OP_JUMP() => GpuDisplayList.JumpRelativeOffset((uint) (Params24 & ~3));

        [GpuInstructionAttribute(GpuOpCodes.END)]
        public void OP_END()
        {
            GpuDisplayList.Done = true;
            GpuDisplayList.GpuProcessor.GpuImpl.End(GpuState);
        }

        [GpuInstructionAttribute(GpuOpCodes.FINISH)]
        public void OP_FINISH()
        {
            GpuDisplayList.GpuProcessor.GpuImpl.Finish(GpuDisplayList.GpuStateStructPointer);
            GpuDisplayList.DoFinish(Pc, Params24, ExecuteNow: true);
        }

        //[GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.CALL)]
        public void OP_CALL() => GpuDisplayList.CallRelativeOffset((uint) (Params24 & ~3));

        //[GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.RET)]
        public void OP_RET() => GpuDisplayList.Ret();

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
        [GpuInstructionAttribute(GpuOpCodes.SIGNAL)]
        public void OP_SIGNAL()
        {
            var signal = Extract(0, 16);
            var behaviour = (SignalBehavior) Extract(16, 8);

            Console.Out.WriteLineColored(ConsoleColor.Green, "OP_SIGNAL: {0}, {1}", signal, behaviour);

            switch (behaviour)
            {
                case SignalBehavior.PSP_GE_SIGNAL_NONE:
                    break;
                case SignalBehavior.PSP_GE_SIGNAL_HANDLER_CONTINUE:
                case SignalBehavior.PSP_GE_SIGNAL_HANDLER_PAUSE:
                case SignalBehavior.PSP_GE_SIGNAL_HANDLER_SUSPEND:
                    var next = GpuDisplayList.ReadInstructionAndMoveNext();
                    if (next.OpCode != GpuOpCodes.END)
                    {
                        throw new NotImplementedException("Error! Next Signal not an END! : " + next.OpCode);
                    }
                    break;
                default:
                    throw new NotImplementedException($"Not implemented {behaviour}");
            }

            GpuDisplayList.DoSignal(Pc, signal, behaviour, ExecuteNow: true);
        }
    }
}