using System;
using System.Windows.Forms;

namespace CSPspEmu.Gui.Winforms
{
    public partial class ProgressForm : Form
    {
        bool AllowClose = false;

        public ProgressForm()
        {
            InitializeComponent();
        }

        public void SetProgress(string Title, int Current, int Total)
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    label1.Text = Title;
                    try
                    {
                        Current = Math.Min(Math.Max(0, Current), Total);
                        progressBar1.Minimum = 0;
                        progressBar1.Maximum = Total;
                        progressBar1.Value = Current;
                    }
                    catch
                    {
                    }
                }));
            }
            catch
            {
            }
        }

        private void ProgressForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!AllowClose)
            {
                e.Cancel = true;
            }
        }

        public void End()
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    try
                    {
                        AllowClose = true;
                        this.Close();
                    }
                    catch
                    {
                    }
                }));
            }
            catch
            {
            }
        }
    }
}