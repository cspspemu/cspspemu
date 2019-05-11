using CSPspEmu.Core;
using CSPspEmu.Gui.GtkSharp;
using CSPspEmu.Gui.Winforms;

namespace CSPspEmu
{
    class GuiRunner
    {
        private readonly PspEmulator _pspEmulator;

        public GuiRunner(PspEmulator pspEmulator)
        {
            _pspEmulator = pspEmulator;
        }

        public void Start()
        {
#if true
			if (Platform.IsMono) { StartGtkSharp(); return; }
#endif
            StartWinforms();
        }

        private void StartWinforms()
        {
            PspDisplayForm.RunStart(_pspEmulator);
        }

        private void StartGtkSharp()
        {
            GtkProgram.RunStart(_pspEmulator);
        }
    }
}