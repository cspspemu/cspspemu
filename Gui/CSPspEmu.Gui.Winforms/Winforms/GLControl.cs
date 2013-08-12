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
		protected IOpenglContext Context;

		public GLControl()
		{
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			if (!DesignMode)
			{
				this.Context = OpenglContextFactory.CreateFromWindowHandle(this.Handle);
			}
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			if (!DesignMode)
			{
				this.Context.Dispose();
			}
			base.OnHandleDestroyed(e);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (!DesignMode)
			{
				this.Context.MakeCurrent();
				GL.glClearColor(0.5f, 0, 1, 1);
				GL.glClear(GL.GL_COLOR_BUFFER_BIT);
				if (RenderFrame != null) RenderFrame();
				Context.SwapBuffers();
			}
		}

		public event Action RenderFrame;

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			if (DesignMode)
			{
				e.Graphics.FillRectangle(new SolidBrush(Color.BlueViolet), e.ClipRectangle);
			}
		}

		public Bitmap GrabScreenshot()
		{
			throw new NotImplementedException();
		}

		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// GLControl
			// 
			this.Name = "GLControl";
			this.Load += new System.EventHandler(this.GLControl_Load);
			this.ResumeLayout(false);
		}

		private void GLControl_Load(object sender, EventArgs e)
		{

		}
	}
}
