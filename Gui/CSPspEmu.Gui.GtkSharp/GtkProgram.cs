using CSPspEmu.Core;
using CSPspEmu.Core.Display;
using CSPspEmu.Core.Gpu.Impl.Opengl;
using Gtk;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Gui.GtkSharp
{
	public class GtkProgram : IGuiWindowInfo
	{
		private Window Window;
		private GLWidget GLWidget;
		private FileChooserDialog FileChooserDialog;
		private IGuiExternalInterface IGuiExternalInterface;
		private CommonGuiDisplayOpengl CommonGuiDisplayOpengl;
		private CommonGuiInput CommonGuiInput;

		public GtkProgram()
		{
		}

		public void Run(IGuiExternalInterface IGuiExternalInterface)
		{
			CommonGuiDisplayOpengl = new CommonGuiDisplayOpengl(IGuiExternalInterface, this);
			CommonGuiInput = new CommonGuiInput(IGuiExternalInterface);
			this.IGuiExternalInterface = IGuiExternalInterface;
			//var Window = new Window("Hello world!");
			Application.Init();
			Window = new Window("Soywiz's Psp Emulator");
			this.GLWidget = new GLWidget(OpenglGpuImpl.UsedGraphicsMode);
			Window.WindowPosition = WindowPosition.Center;
			Window.SetSizeRequest(480, 272);
			Window.Child = GLWidget;
			GLWidget.RenderFrame += GLWidget_RenderFrame;
			GLWidget.SetSizeRequest(480, 272);
			//GLWidget.ShowAll();
			Window.Removed += Window_Removed;
			Window.KeyPressEvent += Window_KeyPressEvent;
			Window.KeysChanged += Window_KeysChanged;
			Window.KeyReleaseEvent += Window_KeyReleaseEvent;

			Window.ShowAll();

			//GLWidget.KeyPressEvent += Window_KeyPressEvent;
			//GLWidget.KeyReleaseEvent += Window_KeyReleaseEvent;

			PspDisplay.DrawEvent += PspDisplay_DrawEvent;

			OpenOpenDialog();
			CommonGuiInput.ReLoadControllerConfig();

			Application.Run();
		}

		void Window_KeysChanged(object sender, EventArgs e)
		{
			Console.WriteLine("Window_KeysChanged");
		}

		private void OpenOpenDialog()
		{
			FileChooserDialog = new FileChooserDialog("Open Game", Window, FileChooserAction.Open, "Open");
			FileChooserDialog.SetCurrentFolderUri("F:/isos/psp2");
			//FileChooserDialog.AddShortcutFolder("F:/isos/psp2");
			//FileChooserDialog.
			//FileChooserDialog.SelectionChanged += FileChooserDialog_FileActivated;
			FileChooserDialog.FileActivated += FileChooserDialog_FileActivated;
			FileChooserDialog.ShowAll();
		}

		void Window_KeyReleaseEvent(object o, KeyReleaseEventArgs args)
		{
			Console.WriteLine("Releasing: {0}", args.Event.Key);
			CommonGuiInput.KeyRelease(args.Event.Key.ToString());
			args.RetVal = true;
		}

		void Window_KeyPressEvent(object o, KeyPressEventArgs args)
		{
			Console.WriteLine("Pressing: {0}", args.Event.Key);
			CommonGuiInput.KeyPress(args.Event.Key.ToString());
			args.RetVal = true;
		}

		void PspDisplay_DrawEvent()
		{
			GLWidget.QueueDraw();
			CommonGuiInput.SendControllerFrame();
		}

		public static void RunStart(IGuiExternalInterface IGuiExternalInterface)
		{
			new GtkProgram().Run(IGuiExternalInterface);
		}

		void Window_Removed(object o, RemovedArgs args)
		{
			Console.WriteLine("GtkProgram.Window_Removed()");
			Application.Quit();
			//throw new NotImplementedException();
		}

		void FileChooserDialog_FileActivated(object sender, EventArgs e)
		{
			FileChooserDialog.HideAll();
			var Path = new Uri(FileChooserDialog.Uri).LocalPath;
			Console.WriteLine("GtkProgram.FileChooserDialog_FileActivated(): {0}", Path);
			IGuiExternalInterface.LoadFile(Path);
			//throw new NotImplementedException();
		}

		void GLWidget_RenderFrame(object sender, EventArgs e)
		{
			CommonGuiDisplayOpengl.DrawVram();
		}

		bool IGuiWindowInfo.EnableRefreshing
		{
			get { return true; }
		}

		void IGuiWindowInfo.SwapBuffers()
		{
		}

		GuiRectangle IGuiWindowInfo.ClientRectangle
		{
			get {
				var Rect = GLWidget.Allocation;
				return new GuiRectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height);
			}
		}
	}
}
