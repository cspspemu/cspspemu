using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSPspEmu.Gui.Winforms.Winforms
{
	public class PspOpenglDisplayWindow : GameWindow, IGuiWindowInfo
	{
		CommonGuiDisplayOpengl DisplayOpengl;

		public PspOpenglDisplayWindow(GraphicsMode mode)
			: base(480, 272, mode, "Soywiz's Psp Emulator")
		{
			//this.CanFocus = false;
			DisplayOpengl = new CommonGuiDisplayOpengl(PspDisplayForm.Singleton.IGuiExternalInterface, this);
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			//this.Top = PspDisplayForm.Singleton.MainMenuStripHeight;
			this.Size = new System.Drawing.Size(PspDisplayForm.Singleton.ClientSize.Width, PspDisplayForm.Singleton.ClientSize.Height);
			DisplayOpengl.DrawVram();

			base.OnRenderFrame(e);
		}

		bool IGuiWindowInfo.EnableRefreshing
		{
			get
			{
				return (PspDisplayForm.Singleton.WindowState != FormWindowState.Minimized) && PspDisplayForm.Singleton.EnableRefreshing;
			}
		}

		void IGuiWindowInfo.SwapBuffers()
		{
			this.SwapBuffers();
		}

		GuiRectangle IGuiWindowInfo.ClientRectangle
		{
			get { return new GuiRectangle(this.ClientRectangle.X, this.ClientRectangle.Y, this.ClientRectangle.Width, this.ClientRectangle.Height); }
		}
	}
}
