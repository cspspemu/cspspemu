using System;
using System.Windows.Forms;

namespace CSPspEmu.Gui.Winforms.Winforms
{
    public sealed unsafe class PspOpenglDisplayControl : GLControl, IGuiWindowInfo
    {
        CommonGuiDisplayOpengl DisplayOpengl;

        public PspOpenglDisplayControl()
        {
            //this.CanFocus = false;
            DisplayOpengl = new CommonGuiDisplayOpengl(PspDisplayForm.Singleton.IGuiExternalInterface, this);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            this.Context.MakeCurrent();
        }

        private void PspOpenglDisplayControl_Load(object sender, EventArgs e)
        {
        }

        protected override void OnDrawFrame()
        {
            var Size = new System.Drawing.Size(PspDisplayForm.Singleton.ClientSize.Width,
                PspDisplayForm.Singleton.ClientSize.Height - this.Top);
            var Top = PspDisplayForm.Singleton.MainMenuStripHeight;
            if (this.Top != Top) this.Top = Top;
            if (this.Size != Size) this.Size = Size;
            try
            {
                DisplayOpengl.DrawVram(PspDisplayForm.Singleton.StoredConfig.EnableSmaa);
            }
            catch (Exception Exception)
            {
                Console.Error.WriteLineColored(ConsoleColor.Red, "OnDrawFrame: {0}", Exception);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // PspOpenglDisplayControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Name = "PspOpenglDisplayControl";
            this.Size = new System.Drawing.Size(480, 272);
            this.Load += new System.EventHandler(this.PspOpenglDisplayControl_Load);
            this.ResumeLayout(false);
        }

        bool IGuiWindowInfo.EnableRefreshing
        {
            get
            {
                return (PspDisplayForm.Singleton.WindowState != FormWindowState.Minimized) &&
                       PspDisplayForm.Singleton.EnableRefreshing;
            }
        }

        void IGuiWindowInfo.SwapBuffers()
        {
            this.Context.SwapBuffers();
        }

        GuiRectangle IGuiWindowInfo.ClientRectangle
        {
            get
            {
                return new GuiRectangle(this.ClientRectangle.X, this.ClientRectangle.Y, this.ClientRectangle.Width,
                    this.ClientRectangle.Height);
            }
        }
    }
}