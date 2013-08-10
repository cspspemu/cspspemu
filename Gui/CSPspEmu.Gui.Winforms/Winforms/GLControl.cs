using CSharpPlatform.GL;
using CSharpPlatform.GL.Impl;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSPspEmu.Gui.Winforms.Winforms
{
	public class GLControl : UserControl
	{
		//private System.ComponentModel.IContainer components = null;
		//
		//protected override void Dispose(bool disposing)
		//{
		//	if (disposing && (components != null)) components.Dispose();
		//	base.Dispose(disposing);
		//}
		//
		//private void InitializeComponent()
		//{
		//	this.SuspendLayout();
		//	// 
		//	// NewGLControl
		//	// 
		//	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
		//	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		//	this.BackColor = System.Drawing.Color.Black;
		//	this.Name = "NewGLControl";
		//	this.ResumeLayout(false);
		//
		//	design_mode = DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;
		//
		//	InitializeComponent();
		//}

		protected IOpenglContext Context;

		public GLControl()
		{
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			this.Context = OpenglContextFactory.CreateFromWindowHandle(this.Handle);
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			base.OnHandleDestroyed(e);
			this.Context.Dispose();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			GL.glClearColor(0.5f, 0, 1, 1);
			GL.glClear(GL.GL_COLOR_BUFFER_BIT);
			Context.SwapBuffers();
		}

		public Bitmap GrabScreenshot()
		{
			throw new NotImplementedException();
		}
	}
}
