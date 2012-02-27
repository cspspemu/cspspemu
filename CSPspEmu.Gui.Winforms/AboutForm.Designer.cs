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
			this.pictureBox2 = new System.Windows.Forms.PictureBox();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.GpuPluginInfoLabel = new System.Windows.Forms.Label();
			this.AudioPluginInfoLabel = new System.Windows.Forms.Label();
			this.versionLabel = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button1.Location = new System.Drawing.Point(428, 343);
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
			this.cspspemuLabel.Size = new System.Drawing.Size(182, 37);
			this.cspspemuLabel.TabIndex = 2;
			this.cspspemuLabel.Text = "CSPspEmu";
			this.cspspemuLabel.Click += new System.EventHandler(this.label1_Click);
			// 
			// pictureBox2
			// 
			this.pictureBox2.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.psp_3000_small;
			this.pictureBox2.Location = new System.Drawing.Point(12, 56);
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.Size = new System.Drawing.Size(220, 202);
			this.pictureBox2.TabIndex = 3;
			this.pictureBox2.TabStop = false;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pictureBox1.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.btn_donate_LG;
			this.pictureBox1.Location = new System.Drawing.Point(330, 343);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(92, 26);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBox1.TabIndex = 1;
			this.pictureBox1.TabStop = false;
			this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(259, 92);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(50, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Credits:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(280, 121);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(104, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Main Programmer";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(280, 145);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(38, 13);
			this.label4.TabIndex = 6;
			this.label4.Text = "soywiz";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Location = new System.Drawing.Point(280, 172);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(95, 13);
			this.label5.TabIndex = 7;
			this.label5.Text = "Special Thanks";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(280, 196);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(32, 13);
			this.label6.TabIndex = 8;
			this.label6.Text = "Noxa";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label7.Location = new System.Drawing.Point(259, 232);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(52, 13);
			this.label7.TabIndex = 9;
			this.label7.Text = "Plugins:";
			// 
			// GpuPluginInfoLabel
			// 
			this.GpuPluginInfoLabel.AutoSize = true;
			this.GpuPluginInfoLabel.Location = new System.Drawing.Point(280, 262);
			this.GpuPluginInfoLabel.Name = "GpuPluginInfoLabel";
			this.GpuPluginInfoLabel.Size = new System.Drawing.Size(74, 13);
			this.GpuPluginInfoLabel.TabIndex = 10;
			this.GpuPluginInfoLabel.Text = "GpuPluginInfo";
			// 
			// AudioPluginInfoLabel
			// 
			this.AudioPluginInfoLabel.AutoSize = true;
			this.AudioPluginInfoLabel.Location = new System.Drawing.Point(280, 286);
			this.AudioPluginInfoLabel.Name = "AudioPluginInfoLabel";
			this.AudioPluginInfoLabel.Size = new System.Drawing.Size(81, 13);
			this.AudioPluginInfoLabel.TabIndex = 11;
			this.AudioPluginInfoLabel.Text = "AudioPluginInfo";
			// 
			// versionLabel
			// 
			this.versionLabel.AutoSize = true;
			this.versionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.versionLabel.Location = new System.Drawing.Point(259, 60);
			this.versionLabel.Name = "versionLabel";
			this.versionLabel.Size = new System.Drawing.Size(53, 13);
			this.versionLabel.TabIndex = 12;
			this.versionLabel.Text = "Version:";
			// 
			// AboutForm
			// 
			this.AcceptButton = this.button1;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(530, 381);
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
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "About CSPspEmu";
			this.Load += new System.EventHandler(this.AboutForm_Load);
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
	}
}