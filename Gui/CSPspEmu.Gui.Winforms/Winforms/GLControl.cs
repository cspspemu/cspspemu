using CSharpPlatform.GL;
using CSharpPlatform.GL.Impl;
using CSharpUtils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSPspEmu.Gui.Winforms.Winforms
{
    unsafe public class GLControl : UserControl
    {
        protected IGLContext Context;

        public GLControl()
        {
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!DesignMode)
            {
                this.Context = GLContextFactory.CreateFromWindowHandle(this.Handle);
                this.Context.MakeCurrent();
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

        bool MustRefresh = true;

        public void ReDraw()
        {
            if (MustRefresh)
            {
                MustRefresh = false;
                this.Refresh();
            }
            else
            {
                this.Context.MakeCurrent();
                OnDrawFrame();
            }
        }

        //public override void Refresh()
        //{
        //	this.Context.MakeCurrent();
        //	OnDrawFrame();
        //}

        virtual protected void OnDrawFrame()
        {
            GL.glClearColor(0, 0, 0, 1);
            GL.glClear(GL.GL_COLOR_BUFFER_BIT);
            if (RenderFrame != null) RenderFrame();
            Context.SwapBuffers();
        }

        sealed protected override void OnPaint(PaintEventArgs e)
        {
            if (!DesignMode)
            {
                this.Context.MakeCurrent();
                OnDrawFrame();
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
            int Width = Size.Width;
            int Height = Size.Height;
            var Data = new byte[Width * Height * 4];
            fixed (byte* DataPtr = Data)
            {
                GL.glReadPixels(0, 0, Width, Height, GL.GL_RGBA, GL.GL_UNSIGNED_BYTE, DataPtr);
            }
            var Bitmap = new Bitmap(Width, Height).SetChannelsDataInterleaved(Data, BitmapChannelList.RGBA);
            Bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return Bitmap;
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