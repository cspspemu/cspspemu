namespace CSPspEmu.Gui.Winforms
{
	partial class AboutForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.button1 = new System.Windows.Forms.Button();
			this.cspspemuLabel = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.GpuPluginInfoLabel = new System.Windows.Forms.Label();
			this.AudioPluginInfoLabel = new System.Windows.Forms.Label();
			this.versionLabel = new System.Windows.Forms.Label();
			this.GitRevisionLabelLabel = new System.Windows.Forms.Label();
			this.GitRevisionValueLinkLabel = new System.Windows.Forms.LinkLabel();
			this.TwitterPictureBox = new System.Windows.Forms.PictureBox();
			this.FacebookPictureBox = new System.Windows.Forms.PictureBox();
			this.pictureBox2 = new System.Windows.Forms.PictureBox();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.CreditsListPanel = new System.Windows.Forms.FlowLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.TwitterPictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FacebookPictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button1.Location = new System.Drawing.Point(507, 512);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(84, 35);
			this.button1.TabIndex = 0;
			this.button1.Text = "&Accept";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// cspspemuLabel
			// 
			this.cspspemuLabel.AutoSize = true;
			this.cspspemuLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cspspemuLabel.Location = new System.Drawing.Point(264, 12);
			this.cspspemuLabel.Name = "cspspemuLabel";
			this.cspspemuLabel.Size = new System.Drawing.Size(270, 37);
			this.cspspemuLabel.TabIndex = 2;
			this.cspspemuLabel.Text = "Soywiz\'s PspEmu";
			this.cspspemuLabel.Click += new System.EventHandler(this.label1_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(268, 58);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(61, 16);
			this.label2.TabIndex = 4;
			this.label2.Text = "Credits:";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label7.Location = new System.Drawing.Point(12, 309);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(63, 16);
			this.label7.TabIndex = 9;
			this.label7.Text = "Plugins:";
			// 
			// GpuPluginInfoLabel
			// 
			this.GpuPluginInfoLabel.AutoSize = true;
			this.GpuPluginInfoLabel.Location = new System.Drawing.Point(12, 336);
			this.GpuPluginInfoLabel.Name = "GpuPluginInfoLabel";
			this.GpuPluginInfoLabel.Size = new System.Drawing.Size(74, 13);
			this.GpuPluginInfoLabel.TabIndex = 10;
			this.GpuPluginInfoLabel.Text = "GpuPluginInfo";
			// 
			// AudioPluginInfoLabel
			// 
			this.AudioPluginInfoLabel.AutoSize = true;
			this.AudioPluginInfoLabel.Location = new System.Drawing.Point(12, 360);
			this.AudioPluginInfoLabel.Name = "AudioPluginInfoLabel";
			this.AudioPluginInfoLabel.Size = new System.Drawing.Size(81, 13);
			this.AudioPluginInfoLabel.TabIndex = 11;
			this.AudioPluginInfoLabel.Text = "AudioPluginInfo";
			// 
			// versionLabel
			// 
			this.versionLabel.AutoSize = true;
			this.versionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.versionLabel.Location = new System.Drawing.Point(12, 230);
			this.versionLabel.Name = "versionLabel";
			this.versionLabel.Size = new System.Drawing.Size(53, 13);
			this.versionLabel.TabIndex = 12;
			this.versionLabel.Text = "Version:";
			// 
			// GitRevisionLabelLabel
			// 
			this.GitRevisionLabelLabel.AutoSize = true;
			this.GitRevisionLabelLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.GitRevisionLabelLabel.Location = new System.Drawing.Point(12, 252);
			this.GitRevisionLabelLabel.Name = "GitRevisionLabelLabel";
			this.GitRevisionLabelLabel.Size = new System.Drawing.Size(80, 13);
			this.GitRevisionLabelLabel.TabIndex = 15;
			this.GitRevisionLabelLabel.Text = "Git Revision:";
			// 
			// GitRevisionValueLinkLabel
			// 
			this.GitRevisionValueLinkLabel.AutoSize = true;
			this.GitRevisionValueLinkLabel.Location = new System.Drawing.Point(12, 277);
			this.GitRevisionValueLinkLabel.Name = "GitRevisionValueLinkLabel";
			this.GitRevisionValueLinkLabel.Size = new System.Drawing.Size(241, 13);
			this.GitRevisionValueLinkLabel.TabIndex = 16;
			this.GitRevisionValueLinkLabel.TabStop = true;
			this.GitRevisionValueLinkLabel.Text = "0123456789abcdef0123456789abcdef01234567";
			this.GitRevisionValueLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GitRevisionValueLinkLabel_LinkClicked);
			// 
			// TwitterPictureBox
			// 
			this.TwitterPictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.TwitterPictureBox.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.twitter_icon_png8;
			this.TwitterPictureBox.Location = new System.Drawing.Point(15, 497);
			this.TwitterPictureBox.Name = "TwitterPictureBox";
			this.TwitterPictureBox.Size = new System.Drawing.Size(51, 50);
			this.TwitterPictureBox.TabIndex = 14;
			this.TwitterPictureBox.TabStop = false;
			this.TwitterPictureBox.Click += new System.EventHandler(this.TwitterPictureBox_Click);
			// 
			// FacebookPictureBox
			// 
			this.FacebookPictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.FacebookPictureBox.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.facebook_icon_png8;
			this.FacebookPictureBox.Location = new System.Drawing.Point(72, 497);
			this.FacebookPictureBox.Name = "FacebookPictureBox";
			this.FacebookPictureBox.Size = new System.Drawing.Size(51, 50);
			this.FacebookPictureBox.TabIndex = 13;
			this.FacebookPictureBox.TabStop = false;
			this.FacebookPictureBox.Click += new System.EventHandler(this.FacebookPictureBox_Click);
			// 
			// pictureBox2
			// 
			this.pictureBox2.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.psp_3000_small;
			this.pictureBox2.Location = new System.Drawing.Point(12, 12);
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.Size = new System.Drawing.Size(220, 202);
			this.pictureBox2.TabIndex = 3;
			this.pictureBox2.TabStop = false;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pictureBox1.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.paypal_donate_button;
			this.pictureBox1.Location = new System.Drawing.Point(143, 504);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(83, 35);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 1;
			this.pictureBox1.TabStop = false;
			this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
			// 
			// CreditsListPanel
			// 
			this.CreditsListPanel.Location = new System.Drawing.Point(271, 83);
			this.CreditsListPanel.Margin = new System.Windows.Forms.Padding(0);
			this.CreditsListPanel.Name = "CreditsListPanel";
			this.CreditsListPanel.Size = new System.Drawing.Size(320, 418);
			this.CreditsListPanel.TabIndex = 36;
			// 
			// AboutForm
			// 
			this.AcceptButton = this.button1;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(605, 559);
			this.Controls.Add(this.CreditsListPanel);
			this.Controls.Add(this.GitRevisionValueLinkLabel);
			this.Controls.Add(this.GitRevisionLabelLabel);
			this.Controls.Add(this.TwitterPictureBox);
			this.Controls.Add(this.FacebookPictureBox);
			this.Controls.Add(this.versionLabel);
			this.Controls.Add(this.AudioPluginInfoLabel);
			this.Controls.Add(this.GpuPluginInfoLabel);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.pictureBox2);
			this.Controls.Add(this.cspspemuLabel);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.button1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AboutForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "About Soywiz\'s PspEmu";
			this.Load += new System.EventHandler(this.AboutForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.TwitterPictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FacebookPictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label cspspemuLabel;
		private System.Windows.Forms.PictureBox pictureBox2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label GpuPluginInfoLabel;
		private System.Windows.Forms.Label AudioPluginInfoLabel;
		private System.Windows.Forms.Label versionLabel;
		private System.Windows.Forms.PictureBox FacebookPictureBox;
		private System.Windows.Forms.PictureBox TwitterPictureBox;
		private System.Windows.Forms.Label GitRevisionLabelLabel;
		private System.Windows.Forms.LinkLabel GitRevisionValueLinkLabel;
		private System.Windows.Forms.FlowLayoutPanel CreditsListPanel;
	}
}