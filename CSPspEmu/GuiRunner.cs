using CSPspEmu.Core;
using CSPspEmu.Gui.GtkSharp;
using CSPspEmu.Gui.Winforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSPspEmu
{
	class GuiRunner
	{
		private PspEmulator PspEmulator;

		public GuiRunner(PspEmulator PspEmulator)
		{
			this.PspEmulator = PspEmulator;
		}

		public void Start()
		{
			//if (Platform.IsMono) { StartGtkSharp(); return; }
			StartWinforms(); return;
		}

		private void StartWinforms()
		{
			PspDisplayForm.RunStart(PspEmulator);
		}

		private void StartGtkSharp()
		{
			GtkProgram.Main(new string[] {});
		}
	}
}
