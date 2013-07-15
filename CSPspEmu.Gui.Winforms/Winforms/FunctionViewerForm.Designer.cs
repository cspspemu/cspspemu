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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.PcListBox = new System.Windows.Forms.ListBox();
			this.ViewTextBox = new System.Windows.Forms.TextBox();
			this.LanguageComboBox = new System.Windows.Forms.ComboBox();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveILAsDLLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.splitContainer1, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 24);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(853, 481);
			this.tableLayoutPanel1.TabIndex = 4;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Margin = new System.Windows.Forms.Padding(0);
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
			this.splitContainer1.Size = new System.Drawing.Size(853, 449);
			this.splitContainer1.SplitterDistance = 179;
			this.splitContainer1.TabIndex = 4;
			// 
			// PcListBox
			// 
			this.PcListBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PcListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.PcListBox.FormattingEnabled = true;
			this.PcListBox.Location = new System.Drawing.Point(0, 0);
			this.PcListBox.Name = "PcListBox";
			this.PcListBox.Size = new System.Drawing.Size(179, 449);
			this.PcListBox.TabIndex = 1;
			this.PcListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.PcListBox_DrawItem_1);
			this.PcListBox.SelectedIndexChanged += new System.EventHandler(this.PcListBox_SelectedIndexChanged);
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
			this.ViewTextBox.Size = new System.Drawing.Size(670, 428);
			this.ViewTextBox.TabIndex = 4;
			this.ViewTextBox.WordWrap = false;
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
			this.LanguageComboBox.Size = new System.Drawing.Size(670, 21);
			this.LanguageComboBox.TabIndex = 3;
			this.LanguageComboBox.SelectedIndexChanged += new System.EventHandler(this.LanguageComboBox_SelectedIndexChanged);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(853, 24);
			this.menuStrip1.TabIndex = 5;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveILAsDLLToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// saveILAsDLLToolStripMenuItem
			// 
			this.saveILAsDLLToolStripMenuItem.Name = "saveILAsDLLToolStripMenuItem";
			this.saveILAsDLLToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
			this.saveILAsDLLToolStripMenuItem.Text = "Save IL as DLL...";
			this.saveILAsDLLToolStripMenuItem.Click += new System.EventHandler(this.saveILAsDLLToolStripMenuItem_Click);
			// 
			// FunctionViewerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(853, 505);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.menuStrip1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "FunctionViewerForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Function Viewer";
			this.Load += new System.EventHandler(this.FunctionViewerForm_Load);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ListBox PcListBox;
		private System.Windows.Forms.TextBox ViewTextBox;
		private System.Windows.Forms.ComboBox LanguageComboBox;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveILAsDLLToolStripMenuItem;


	}
}