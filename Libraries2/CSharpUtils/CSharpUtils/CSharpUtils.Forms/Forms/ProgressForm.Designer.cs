namespace CSharpUtils.Forms
{
	partial class ProgressForm
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
			this.buttonCancel = new System.Windows.Forms.Button();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.labelAction = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.labelDestination = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.labelOrigin = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// buttonCancel
			// 
			this.buttonCancel.Location = new System.Drawing.Point(112, 157);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(122, 23);
			this.buttonCancel.TabIndex = 0;
			this.buttonCancel.Text = "&Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.button1_Click);
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(12, 119);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(340, 23);
			this.progressBar1.TabIndex = 1;
			// 
			// labelAction
			// 
			this.labelAction.AutoEllipsis = true;
			this.labelAction.Location = new System.Drawing.Point(12, 79);
			this.labelAction.Name = "labelAction";
			this.labelAction.Size = new System.Drawing.Size(340, 23);
			this.labelAction.TabIndex = 2;
			this.labelAction.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(12, 41);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(57, 23);
			this.label2.TabIndex = 5;
			this.label2.Text = "To:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelDestination
			// 
			this.labelDestination.AutoEllipsis = true;
			this.labelDestination.Location = new System.Drawing.Point(75, 41);
			this.labelDestination.Name = "labelDestination";
			this.labelDestination.Size = new System.Drawing.Size(277, 23);
			this.labelDestination.TabIndex = 4;
			this.labelDestination.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(12, 12);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(57, 23);
			this.label4.TabIndex = 7;
			this.label4.Text = "From:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelOrigin
			// 
			this.labelOrigin.AutoEllipsis = true;
			this.labelOrigin.Location = new System.Drawing.Point(75, 12);
			this.labelOrigin.Name = "labelOrigin";
			this.labelOrigin.Size = new System.Drawing.Size(277, 23);
			this.labelOrigin.TabIndex = 6;
			this.labelOrigin.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ProgressForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(364, 193);
			this.ControlBox = false;
			this.Controls.Add(this.label4);
			this.Controls.Add(this.labelOrigin);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.labelDestination);
			this.Controls.Add(this.labelAction);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.buttonCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ProgressForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Progress...";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Label labelAction;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label labelDestination;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label labelOrigin;
	}
}