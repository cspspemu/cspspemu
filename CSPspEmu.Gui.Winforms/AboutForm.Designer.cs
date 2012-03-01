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
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.GpuPluginInfoLabel = new System.Windows.Forms.Label();
			this.AudioPluginInfoLabel = new System.Windows.Forms.Label();
			this.versionLabel = new System.Windows.Forms.Label();
			this.TwitterPictureBox = new System.Windows.Forms.PictureBox();
			this.FacebookPictureBox = new System.Windows.Forms.PictureBox();
			this.pictureBox2 = new System.Windows.Forms.PictureBox();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.GitRevisionLabelLabel = new System.Windows.Forms.Label();
			this.GitRevisionValueLinkLabel = new System.Windows.Forms.LinkLabel();
			this.label1 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.TwitterPictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FacebookPictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button1.Location = new System.Drawing.Point(525, 320);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(90, 26);
			this.button1.TabIndex = 0;
			this.button1.Text = "&Accept";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// cspspemuLabel
			// 
			this.cspspemuLabel.AutoSize = true;
			this.cspspemuLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cspspemuLabel.Location = new System.Drawing.Point(249, 7);
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
			this.label2.Location = new System.Drawing.Point(259, 119);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(61, 16);
			this.label2.TabIndex = 4;
			this.label2.Text = "Credits:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(259, 142);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(104, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Main Programmer";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(259, 166);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(38, 13);
			this.label4.TabIndex = 6;
			this.label4.Text = "soywiz";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Location = new System.Drawing.Point(402, 142);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(95, 13);
			this.label5.TabIndex = 7;
			this.label5.Text = "Special Thanks";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(402, 166);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(32, 13);
			this.label6.TabIndex = 8;
			this.label6.Text = "Noxa";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label7.Location = new System.Drawing.Point(259, 226);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(63, 16);
			this.label7.TabIndex = 9;
			this.label7.Text = "Plugins:";
			// 
			// GpuPluginInfoLabel
			// 
			this.GpuPluginInfoLabel.AutoSize = true;
			this.GpuPluginInfoLabel.Location = new System.Drawing.Point(259, 253);
			this.GpuPluginInfoLabel.Name = "GpuPluginInfoLabel";
			this.GpuPluginInfoLabel.Size = new System.Drawing.Size(74, 13);
			this.GpuPluginInfoLabel.TabIndex = 10;
			this.GpuPluginInfoLabel.Text = "GpuPluginInfo";
			// 
			// AudioPluginInfoLabel
			// 
			this.AudioPluginInfoLabel.AutoSize = true;
			this.AudioPluginInfoLabel.Location = new System.Drawing.Point(259, 277);
			this.AudioPluginInfoLabel.Name = "AudioPluginInfoLabel";
			this.AudioPluginInfoLabel.Size = new System.Drawing.Size(81, 13);
			this.AudioPluginInfoLabel.TabIndex = 11;
			this.AudioPluginInfoLabel.Text = "AudioPluginInfo";
			// 
			// versionLabel
			// 
			this.versionLabel.AutoSize = true;
			this.versionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.versionLabel.Location = new System.Drawing.Point(259, 61);
			this.versionLabel.Name = "versionLabel";
			this.versionLabel.Size = new System.Drawing.Size(53, 13);
			this.versionLabel.TabIndex = 12;
			this.versionLabel.Text = "Version:";
			// 
			// TwitterPictureBox
			// 
			this.TwitterPictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.TwitterPictureBox.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.twitter_icon_png8;
			this.TwitterPictureBox.Location = new System.Drawing.Point(12, 296);
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
			this.FacebookPictureBox.Location = new System.Drawing.Point(69, 296);
			this.FacebookPictureBox.Name = "FacebookPictureBox";
			this.FacebookPictureBox.Size = new System.Drawing.Size(51, 50);
			this.FacebookPictureBox.TabIndex = 13;
			this.FacebookPictureBox.TabStop = false;
			this.FacebookPictureBox.Click += new System.EventHandler(this.FacebookPictureBox_Click);
			// 
			// pictureBox2
			// 
			this.pictureBox2.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.psp_3000_small;
			this.pictureBox2.Location = new System.Drawing.Point(12, 64);
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.Size = new System.Drawing.Size(220, 202);
			this.pictureBox2.TabIndex = 3;
			this.pictureBox2.TabStop = false;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pictureBox1.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.btn_donate_LG;
			this.pictureBox1.Location = new System.Drawing.Point(427, 320);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(92, 26);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBox1.TabIndex = 1;
			this.pictureBox1.TabStop = false;
			this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
			// 
			// GitRevisionLabelLabel
			// 
			this.GitRevisionLabelLabel.AutoSize = true;
			this.GitRevisionLabelLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.GitRevisionLabelLabel.Location = new System.Drawing.Point(259, 83);
			this.GitRevisionLabelLabel.Name = "GitRevisionLabelLabel";
			this.GitRevisionLabelLabel.Size = new System.Drawing.Size(80, 13);
			this.GitRevisionLabelLabel.TabIndex = 15;
			this.GitRevisionLabelLabel.Text = "Git Revision:";
			// 
			// GitRevisionValueLinkLabel
			// 
			this.GitRevisionValueLinkLabel.AutoSize = true;
			this.GitRevisionValueLinkLabel.Location = new System.Drawing.Point(345, 83);
			this.GitRevisionValueLinkLabel.Name = "GitRevisionValueLinkLabel";
			this.GitRevisionValueLinkLabel.Size = new System.Drawing.Size(241, 13);
			this.GitRevisionValueLinkLabel.TabIndex = 16;
			this.GitRevisionValueLinkLabel.TabStop = true;
			this.GitRevisionValueLinkLabel.Text = "0123456789abcdef0123456789abcdef01234567";
			this.GitRevisionValueLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GitRevisionValueLinkLabel_LinkClicked);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(546, 142);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(49, 13);
			this.label1.TabIndex = 17;
			this.label1.Text = "Testers";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(546, 166);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(46, 13);
			this.label8.TabIndex = 18;
			this.label8.Text = "MaXiMu";
			// 
			// AboutForm
			// 
			this.AcceptButton = this.button1;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(630, 359);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.GitRevisionValueLinkLabel);
			this.Controls.Add(this.GitRevisionLabelLabel);
			this.Controls.Add(this.TwitterPictureBox);
			this.Controls.Add(this.FacebookPictureBox);
			this.Controls.Add(this.versionLabel);
			this.Controls.Add(this.AudioPluginInfoLabel);
			this.Controls.Add(this.GpuPluginInfoLabel);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
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
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label GpuPluginInfoLabel;
		private System.Windows.Forms.Label AudioPluginInfoLabel;
		private System.Windows.Forms.Label versionLabel;
		private System.Windows.Forms.PictureBox FacebookPictureBox;
		private System.Windows.Forms.PictureBox TwitterPictureBox;
		private System.Windows.Forms.Label GitRevisionLabelLabel;
		private System.Windows.Forms.LinkLabel GitRevisionValueLinkLabel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label8;
	}
}