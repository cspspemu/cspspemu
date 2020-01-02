using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Gtk;
using GL = CSharpPlatform.GL.GL;
using Window = Avalonia.Controls.Window;

namespace CSPspEmu.Frontend.HelloWorld
{
    class Program
    {
        /*
        [STAThread]
        static void Main(string[] args)
        {
            Application.Init();
            var win = new Window(WindowType.Toplevel);
            var button2 = new Button {Label = "HELLO"};
            button2.SetSizeRequest(100, 100);

            var button = new GLArea();
            //button.SetRequiredVersion(3, 2);
            button.SetRequiredVersion(4, 0);
            button.AutoRender = true;
            button.Realized += (sender, eventArgs) =>
            {
                Console.WriteLine("REALIZED");
                button.MakeCurrent();
            };
            button.Render += (e, args) =>
            {
                Console.WriteLine("RENDER");
                args.Context.MakeCurrent();
                GL.glClearColor(.7f, .3f, 0, 1);
                GL.glClear(GL.GL_COLOR_BUFFER_BIT);
                GL.glFlush();
                GL.glFinish();
                //gtk_gl_swap_buffers(button.Handle);
                //button.QueueRender();
                //GL.ClearColor(.3f, .3f, .3f, 1f);
                //args.Context.
            };
            button.SetSizeRequest(100, 100);
            //button.Left = 10;
            //button.Top = 10;
            var fix = new Fixed {button, button2};
            fix.Move(button, 100, 100);
            fix.Move(button2, 200, 100);

            var menu = new MenuBar();
            menu.Add(new MenuItem() {Label = "hello"});

            fix.Add(menu);
            win.Add(fix);
            win.Hide();
            win.SetPosition(WindowPosition.Center);
            win.ShowAll();
            win.Destroyed += (sender, eventArgs) =>
            {
                Environment.Exit(0);
            };
            button.QueueRender();
            Application.Run();

            Console.WriteLine("Hello World!");
        }
        */
        
        /*
        public static void Main (string[] args)
        {
            Application.Init();
            Window win = new Window ("Menu Sample App");
            win.DeleteEvent += new DeleteEventHandler (delete_cb);
            win.DefaultWidth = 200;
            win.DefaultHeight = 150;

            VBox box = new VBox (false, 2);

            MenuBar mb = new MenuBar ();
            Menu file_menu = new Menu ();
            MenuItem exit_item = new MenuItem("Exit");
            exit_item.Activated += new EventHandler (exit_cb);
            file_menu.Append (exit_item);
            MenuItem file_item = new MenuItem("File");
            file_item.Submenu = file_menu;
            mb.Append (file_item);
            box.PackStart(mb, false, false, 0);

            Button btn = new Button ("Yep, that's a menu");
            box.PackStart(btn, true, true, 0);
			
            win.Add (box);
            win.ShowAll ();

            Application.Run ();
        }

        static void delete_cb (object o, DeleteEventArgs args)
        {
            Application.Quit ();
            args.RetVal = true;
        }

        static void exit_cb (object o, EventArgs args)
        {
            Application.Quit ();
        }
        */

        static public void Main(string[] args)
        {
            var window = new Window();
            window.Show();
        }
    }
}