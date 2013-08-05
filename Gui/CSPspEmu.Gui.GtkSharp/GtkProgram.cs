using Gtk;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Gui.GtkSharp
{
	public class GtkProgram
	{
		static Window Window;
		static GLWidget GLWidget;
		static FileChooserDialog FileChooserDialog;

		public static void Main(string[] args)
		{
			//var Window = new Window("Hello world!");
			Application.Init();
			Window = new Window("Hello world!");
			GLWidget = new GLWidget();
			Window.WindowPosition = WindowPosition.Center;
			Window.SetSizeRequest(480, 272);
			Window.Child = GLWidget;
			GLWidget.RenderFrame += GLWidget_RenderFrame;
			GLWidget.SetSizeRequest(480, 272);
			//GLWidget.ShowAll();
			Window.ShowAll();
			Window.Removed += Window_Removed;
			FileChooserDialog = new FileChooserDialog("Open Game", Window, FileChooserAction.Open, "Open");
			FileChooserDialog.FileActivated += FileChooserDialog_FileActivated;
			FileChooserDialog.ShowAll();
			Application.Run();
		}

		static void Window_Removed(object o, RemovedArgs args)
		{
			Console.WriteLine("BBBBBBBBBBBBBBB");
			Application.Quit();
			//throw new NotImplementedException();
		}

		static void FileChooserDialog_FileActivated(object sender, EventArgs e)
		{
			Console.WriteLine("AAAAAAAAAAA: {0}", FileChooserDialog.Uri);
			//throw new NotImplementedException();
		}

		static void GLWidget_RenderFrame(object sender, EventArgs e)
		{
			GL.ClearColor(1f, 0f, 0f, 1f);
			GL.Clear(ClearBufferMask.ColorBufferBit);
			//GLWidget.swap
		}
	}
}
