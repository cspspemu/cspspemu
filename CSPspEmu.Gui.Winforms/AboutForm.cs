using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using CSPspEmu.Core;

namespace CSPspEmu.Gui.Winforms
{
	public partial class AboutForm : Form
	{
		public AboutForm(IGuiExternalInterface IGuiExternalInterface)
		{
			InitializeComponent();
			GpuPluginInfoLabel.Text = "GPU " + IGuiExternalInterface.GetGpuPluginInfo().ToString();
			AudioPluginInfoLabel.Text = "Audio " + IGuiExternalInterface.GetAudioPluginInfo().ToString();
			versionLabel.Text = "Version: " + PspGlobalConfiguration.CurrentVersion;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void pictureBox1_Click(object sender, EventArgs e)
		{
			Process.Start(@"https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=J9DXYUSNPH5SC");
		}

		private void label1_Click(object sender, EventArgs e)
		{

		}

		private void AboutForm_Load(object sender, EventArgs e)
		{

		}

		private void TwitterPictureBox_Click(object sender, EventArgs e)
		{
			Process.Start(@"http://twitter.com/dpspemu");
		}

		private void FacebookPictureBox_Click(object sender, EventArgs e)
		{
			Process.Start(@"http://www.facebook.com/pspemu");
		}
	}
}
