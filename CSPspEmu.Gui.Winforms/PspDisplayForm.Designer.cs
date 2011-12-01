namespace CSPspEmu.Gui.Winforms
{
	partial class PspDisplayForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PspDisplayForm));
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.utilsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.displayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.xToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.xToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
			this.xToolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
			this.xToolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.takeScreenshotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.dumpRamToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.frameSkippingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.runToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.resumeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.pauseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.traceSyscallsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.websiteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
			this.indieGamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.blogcballesterosvelascoesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.traceUnimplementedGpuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.utilsToolStripMenuItem,
            this.runToolStripMenuItem,
            this.debugToolStripMenuItem,
            this.helpToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(525, 24);
			this.menuStrip1.TabIndex = 1;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.toolStripMenuItem3,
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
			this.openToolStripMenuItem.Text = "&Open...";
			this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.toolStripMenuItem3.Size = new System.Drawing.Size(109, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
			this.exitToolStripMenuItem.Text = "&Exit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// utilsToolStripMenuItem
			// 
			this.utilsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.displayToolStripMenuItem,
            this.toolStripMenuItem2,
            this.takeScreenshotToolStripMenuItem,
            this.dumpRamToolStripMenuItem,
            this.toolStripMenuItem1,
            this.frameSkippingToolStripMenuItem});
			this.utilsToolStripMenuItem.Name = "utilsToolStripMenuItem";
			this.utilsToolStripMenuItem.Size = new System.Drawing.Size(42, 20);
			this.utilsToolStripMenuItem.Text = "&Utils";
			// 
			// displayToolStripMenuItem
			// 
			this.displayToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.xToolStripMenuItem1,
            this.xToolStripMenuItem2,
            this.xToolStripMenuItem3,
            this.xToolStripMenuItem4});
			this.displayToolStripMenuItem.Name = "displayToolStripMenuItem";
			this.displayToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
			this.displayToolStripMenuItem.Text = "&Display";
			// 
			// xToolStripMenuItem1
			// 
			this.xToolStripMenuItem1.Name = "xToolStripMenuItem1";
			this.xToolStripMenuItem1.Size = new System.Drawing.Size(85, 22);
			this.xToolStripMenuItem1.Text = "&1x";
			this.xToolStripMenuItem1.Click += new System.EventHandler(this.xToolStripMenuItem1_Click);
			// 
			// xToolStripMenuItem2
			// 
			this.xToolStripMenuItem2.Name = "xToolStripMenuItem2";
			this.xToolStripMenuItem2.Size = new System.Drawing.Size(85, 22);
			this.xToolStripMenuItem2.Text = "&2x";
			this.xToolStripMenuItem2.Click += new System.EventHandler(this.xToolStripMenuItem2_Click);
			// 
			// xToolStripMenuItem3
			// 
			this.xToolStripMenuItem3.Name = "xToolStripMenuItem3";
			this.xToolStripMenuItem3.Size = new System.Drawing.Size(85, 22);
			this.xToolStripMenuItem3.Text = "&3x";
			this.xToolStripMenuItem3.Click += new System.EventHandler(this.xToolStripMenuItem3_Click);
			// 
			// xToolStripMenuItem4
			// 
			this.xToolStripMenuItem4.Name = "xToolStripMenuItem4";
			this.xToolStripMenuItem4.Size = new System.Drawing.Size(85, 22);
			this.xToolStripMenuItem4.Text = "&4x";
			this.xToolStripMenuItem4.Click += new System.EventHandler(this.xToolStripMenuItem4_Click);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(191, 6);
			// 
			// takeScreenshotToolStripMenuItem
			// 
			this.takeScreenshotToolStripMenuItem.Name = "takeScreenshotToolStripMenuItem";
			this.takeScreenshotToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
			this.takeScreenshotToolStripMenuItem.Text = "Take &Screenshot...";
			this.takeScreenshotToolStripMenuItem.Click += new System.EventHandler(this.takeScreenshotToolStripMenuItem_Click);
			// 
			// dumpRamToolStripMenuItem
			// 
			this.dumpRamToolStripMenuItem.Name = "dumpRamToolStripMenuItem";
			this.dumpRamToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
			this.dumpRamToolStripMenuItem.Text = "&Dump Main Memory...";
			this.dumpRamToolStripMenuItem.Click += new System.EventHandler(this.dumpRamToolStripMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(191, 6);
			// 
			// frameSkippingToolStripMenuItem
			// 
			this.frameSkippingToolStripMenuItem.Checked = true;
			this.frameSkippingToolStripMenuItem.CheckOnClick = true;
			this.frameSkippingToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			this.frameSkippingToolStripMenuItem.Name = "frameSkippingToolStripMenuItem";
			this.frameSkippingToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
			this.frameSkippingToolStripMenuItem.Text = "&VSync";
			this.frameSkippingToolStripMenuItem.Click += new System.EventHandler(this.frameSkippingToolStripMenuItem_Click);
			// 
			// runToolStripMenuItem
			// 
			this.runToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resumeToolStripMenuItem,
            this.pauseToolStripMenuItem});
			this.runToolStripMenuItem.Name = "runToolStripMenuItem";
			this.runToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
			this.runToolStripMenuItem.Text = "&Run";
			// 
			// resumeToolStripMenuItem
			// 
			this.resumeToolStripMenuItem.Name = "resumeToolStripMenuItem";
			this.resumeToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
			this.resumeToolStripMenuItem.Text = "&Run/Resume";
			this.resumeToolStripMenuItem.Click += new System.EventHandler(this.resumeToolStripMenuItem_Click);
			// 
			// pauseToolStripMenuItem
			// 
			this.pauseToolStripMenuItem.Name = "pauseToolStripMenuItem";
			this.pauseToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
			this.pauseToolStripMenuItem.Text = "&Pause";
			this.pauseToolStripMenuItem.Click += new System.EventHandler(this.pauseToolStripMenuItem_Click);
			// 
			// debugToolStripMenuItem
			// 
			this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.traceSyscallsToolStripMenuItem,
            this.traceUnimplementedGpuToolStripMenuItem});
			this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
			this.debugToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
			this.debugToolStripMenuItem.Text = "&Debug";
			// 
			// traceSyscallsToolStripMenuItem
			// 
			this.traceSyscallsToolStripMenuItem.Name = "traceSyscallsToolStripMenuItem";
			this.traceSyscallsToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
			this.traceSyscallsToolStripMenuItem.Text = "Trace &Syscalls";
			this.traceSyscallsToolStripMenuItem.Click += new System.EventHandler(this.traceSyscallsToolStripMenuItem_Click);
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.websiteToolStripMenuItem,
            this.toolStripMenuItem5,
            this.indieGamesToolStripMenuItem,
            this.blogcballesterosvelascoesToolStripMenuItem,
            this.toolStripMenuItem4,
            this.aboutToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			this.helpToolStripMenuItem.Text = "&Help";
			// 
			// websiteToolStripMenuItem
			// 
			this.websiteToolStripMenuItem.Name = "websiteToolStripMenuItem";
			this.websiteToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
			this.websiteToolStripMenuItem.Text = "&Website";
			this.websiteToolStripMenuItem.Click += new System.EventHandler(this.websiteToolStripMenuItem_Click);
			// 
			// toolStripMenuItem5
			// 
			this.toolStripMenuItem5.Name = "toolStripMenuItem5";
			this.toolStripMenuItem5.Size = new System.Drawing.Size(229, 6);
			// 
			// indieGamesToolStripMenuItem
			// 
			this.indieGamesToolStripMenuItem.Name = "indieGamesToolStripMenuItem";
			this.indieGamesToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
			this.indieGamesToolStripMenuItem.Text = "&kawagames.com";
			this.indieGamesToolStripMenuItem.Click += new System.EventHandler(this.indieGamesToolStripMenuItem_Click);
			// 
			// blogcballesterosvelascoesToolStripMenuItem
			// 
			this.blogcballesterosvelascoesToolStripMenuItem.Name = "blogcballesterosvelascoesToolStripMenuItem";
			this.blogcballesterosvelascoesToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
			this.blogcballesterosvelascoesToolStripMenuItem.Text = "en.blog.cballesterosvelasco.es";
			this.blogcballesterosvelascoesToolStripMenuItem.Click += new System.EventHandler(this.blogcballesterosvelascoesToolStripMenuItem_Click);
			// 
			// toolStripMenuItem4
			// 
			this.toolStripMenuItem4.Name = "toolStripMenuItem4";
			this.toolStripMenuItem4.Size = new System.Drawing.Size(229, 6);
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
			this.aboutToolStripMenuItem.Text = "&About...";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
			// 
			// traceUnimplementedGpuToolStripMenuItem
			// 
			this.traceUnimplementedGpuToolStripMenuItem.Name = "traceUnimplementedGpuToolStripMenuItem";
			this.traceUnimplementedGpuToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
			this.traceUnimplementedGpuToolStripMenuItem.Text = "Trace Unimplemented Gpu";
			this.traceUnimplementedGpuToolStripMenuItem.Click += new System.EventHandler(this.traceUnimplementedGpuToolStripMenuItem_Click);
			// 
			// PspDisplayForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(525, 291);
			this.Controls.Add(this.menuStrip1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip1;
			this.MaximizeBox = false;
			this.Name = "PspDisplayForm";
			this.Text = "CSPspEmu";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PspDisplayForm_KeyDown);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.PspDisplayForm_KeyUp);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem utilsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem takeScreenshotToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem frameSkippingToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem websiteToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem displayToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem xToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem xToolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem xToolStripMenuItem4;
		private System.Windows.Forms.ToolStripMenuItem xToolStripMenuItem3;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem dumpRamToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem runToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem resumeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem pauseToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
		private System.Windows.Forms.ToolStripMenuItem indieGamesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem blogcballesterosvelascoesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem traceSyscallsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem traceUnimplementedGpuToolStripMenuItem;
	}
}