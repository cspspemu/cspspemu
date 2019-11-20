using CSharpPlatform.UI;

namespace CSPspEmu.Hle.Modules.utility
{
    public unsafe partial class sceUtility
    {
        /// <summary>
        /// Create a message dialog
        /// </summary>
        /// <param name="Params">Dialog parameters</param>
        /// <returns>0 on success</returns>
        [HlePspFunction(NID = 0x2AD8E239, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceUtilityMsgDialogInitStart(pspUtilityMsgDialogParams* Params)
        {
            var Message = Params->Message;
            var IsError = Params->Mode == pspUtilityMsgDialogMode.PSP_UTILITY_MSGDIALOG_MODE_ERROR;
            var DialogType = IsError ? Dialog.Type.Error : Dialog.Type.Message;

            CurrentDialogStep = DialogStepEnum.PROCESSING;

            Dialog.ShowDialog((Result) =>
            {
                switch (Result)
                {
                    case Dialog.Result.Yes:
                        Params->ButtonPressed = pspUtilityMsgDialogPressed.PSP_UTILITY_MSGDIALOG_RESULT_YES;
                        break;
                    case Dialog.Result.No:
                        Params->ButtonPressed = pspUtilityMsgDialogPressed.PSP_UTILITY_MSGDIALOG_RESULT_NO;
                        break;
                    case Dialog.Result.Back:
                        Params->ButtonPressed = pspUtilityMsgDialogPressed.PSP_UTILITY_MSGDIALOG_RESULT_BACK;
                        break;
                }
                CurrentDialogStep = DialogStepEnum.SUCCESS;
            }, Message, DialogType);

            //throw(new NotImplementedException());
            return 0;
        }

        /// <summary>
        /// Remove a message dialog currently active.  After calling this
        /// function you need to keep calling GetStatus and Update until
        /// you get a status of 4.
        /// </summary>
        [HlePspFunction(NID = 0x67AF3428, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public void sceUtilityMsgDialogShutdownStart()
        {
            CurrentDialogStep = DialogStepEnum.SHUTDOWN;
        }

        /// <summary>
        /// Refresh the GUI for a message dialog currently active
        /// </summary>
        /// <param name="Value">Unknown, pass 1</param>
        [HlePspFunction(NID = 0x95FC253B, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public void sceUtilityMsgDialogUpdate(int Value)
        {
            //throw (new NotImplementedException());
        }

        /// <summary>
        /// Get the current status of a message dialog currently active.
        /// </summary>
        /// <returns></returns>
        [HlePspFunction(NID = 0x9A1C91D7, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public DialogStepEnum sceUtilityMsgDialogGetStatus()
        {
            try
            {
                return CurrentDialogStep;
            }
            finally
            {
                if (CurrentDialogStep == DialogStepEnum.SHUTDOWN)
                {
                    CurrentDialogStep = DialogStepEnum.NONE;
                }
            }
        }

        [HlePspFunction(NID = 0x4928BD96, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceUtilityMsgDialogAbort()
        {
            return 0;
        }
    }
}