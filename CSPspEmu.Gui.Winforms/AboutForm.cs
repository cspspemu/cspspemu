using System;
using System.Windows.Forms;
using System.Diagnostics;
using CSPspEmu.Core;

namespace CSPspEmu.Gui.Winforms
{
	public partial class AboutForm : Form
	{
		public AboutForm(Form ParentForm, IGuiExternalInterface IGuiExternalInterface)
		{
			this.Icon = ParentForm.Icon;
			InitializeComponent();
			GpuPluginInfoLabel.Text = "GPU " + IGuiExternalInterface.GetGpuPluginInfo().ToString();
			AudioPluginInfoLabel.Text = "Audio " + IGuiExternalInterface.GetAudioPluginInfo().ToString();
			versionLabel.Text = "Version: " + PspGlobalConfiguration.CurrentVersion + " : r" + PspGlobalConfiguration.CurrentVersionNumeric;
			GitRevisionValueLinkLabel.Text = PspGlobalConfiguration.GitRevision;
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

		private void GitRevisionValueLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(@"https://github.com/soywiz/cspspemu/commit/" + PspGlobalConfiguration.GitRevision);
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(@"http://tgames.fr.nf/");
		}

		private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(@"https://github.com/lioncash");
		}
	}
}
