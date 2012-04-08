using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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
			this.Invoke(new Action(() =>
			{
				label1.Text = Title;
				progressBar1.Minimum = 0;
				progressBar1.Value = Current;
				progressBar1.Maximum = Total;
			}));
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
