namespace CSPspEmu.Gui.Winforms
{
	partial class FunctionViewerForm
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
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.PcListBox = new System.Windows.Forms.ListBox();
			this.ViewTextBox = new System.Windows.Forms.TextBox();
			this.LanguageComboBox = new System.Windows.Forms.ComboBox();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Margin = new System.Windows.Forms.Padding(8);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.PcListBox);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.ViewTextBox);
			this.splitContainer1.Panel2.Controls.Add(this.LanguageComboBox);
			this.splitContainer1.Size = new System.Drawing.Size(632, 403);
			this.splitContainer1.SplitterDistance = 89;
			this.splitContainer1.TabIndex = 3;
			// 
			// PcListBox
			// 
			this.PcListBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PcListBox.FormattingEnabled = true;
			this.PcListBox.Location = new System.Drawing.Point(0, 0);
			this.PcListBox.Name = "PcListBox";
			this.PcListBox.Size = new System.Drawing.Size(89, 403);
			this.PcListBox.TabIndex = 1;
			this.PcListBox.SelectedIndexChanged += new System.EventHandler(this.PcListBox_SelectedIndexChanged_1);
			// 
			// ViewTextBox
			// 
			this.ViewTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ViewTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ViewTextBox.Location = new System.Drawing.Point(0, 21);
			this.ViewTextBox.Multiline = true;
			this.ViewTextBox.Name = "ViewTextBox";
			this.ViewTextBox.ReadOnly = true;
			this.ViewTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.ViewTextBox.Size = new System.Drawing.Size(539, 382);
			this.ViewTextBox.TabIndex = 4;
			this.ViewTextBox.TextChanged += new System.EventHandler(this.ViewTextBox_TextChanged_1);
			// 
			// LanguageComboBox
			// 
			this.LanguageComboBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.LanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.LanguageComboBox.FormattingEnabled = true;
			this.LanguageComboBox.Items.AddRange(new object[] {
            "C#",
            "Ast",
            "IL",
            "Mips"});
			this.LanguageComboBox.Location = new System.Drawing.Point(0, 0);
			this.LanguageComboBox.Name = "LanguageComboBox";
			this.LanguageComboBox.Size = new System.Drawing.Size(539, 21);
			this.LanguageComboBox.TabIndex = 3;
			this.LanguageComboBox.SelectedIndexChanged += new System.EventHandler(this.LanguageComboBox_SelectedIndexChanged_1);
			// 
			// FunctionViewerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(632, 403);
			this.Controls.Add(this.splitContainer1);
			this.Name = "FunctionViewerForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Function Viewer";
			this.Load += new System.EventHandler(this.FunctionViewerForm_Load);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ListBox PcListBox;
		private System.Windows.Forms.TextBox ViewTextBox;
		private System.Windows.Forms.ComboBox LanguageComboBox;

	}
}