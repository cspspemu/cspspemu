﻿namespace CSPspEmu.Gui.Winforms
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
			this.FileMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.FileOpenMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			this.FileExitMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.UtilsMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.UtilsDisplayMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.xToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.xToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
			this.xToolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
			this.xToolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.UtilsLanguageMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.UtilsLanguageEnglishMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.UtilsLanguageSpanishMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripSeparator();
			this.UtilsTakeScreenshotMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.UtilsDumpMainMemoryMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.UtilsFrameLimitingMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
			this.UtilsUseFastmemMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripSeparator();
			this.UtilsAssociateExtensionsMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.RunMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.RunRunResumeMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.RunPauseMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.DebugMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.DebugTraceSyscallsMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.DebugTraceUnimplementedSyscallsMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.DebugTraceUnimplementedGpuMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.DebugShowThreadInfoMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.HelpMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.HelpWebsiteMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
			this.HelpKawagamesMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.HelpBlogMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.HelpGithubMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
			this.HelpCheckForUpdatesMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
			this.HelpReportAnIssueMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripSeparator();
			this.HelpAboutMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenu,
            this.UtilsMenu,
            this.RunMenu,
            this.DebugMenu,
            this.HelpMenu});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(525, 24);
			this.menuStrip1.TabIndex = 1;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// FileMenu
			// 
			this.FileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileOpenMenu,
            this.toolStripMenuItem3,
            this.FileExitMenu});
			this.FileMenu.Name = "FileMenu";
			this.FileMenu.Size = new System.Drawing.Size(37, 20);
			this.FileMenu.Text = "&File";
			// 
			// FileOpenMenu
			// 
			this.FileOpenMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.folder;
			this.FileOpenMenu.Name = "FileOpenMenu";
			this.FileOpenMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.FileOpenMenu.Size = new System.Drawing.Size(155, 22);
			this.FileOpenMenu.Text = "&Open...";
			this.FileOpenMenu.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.toolStripMenuItem3.Size = new System.Drawing.Size(152, 6);
			// 
			// FileExitMenu
			// 
			this.FileExitMenu.Name = "FileExitMenu";
			this.FileExitMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
			this.FileExitMenu.Size = new System.Drawing.Size(155, 22);
			this.FileExitMenu.Text = "&Exit";
			this.FileExitMenu.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// UtilsMenu
			// 
			this.UtilsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.UtilsDisplayMenu,
            this.toolStripMenuItem2,
            this.UtilsLanguageMenu,
            this.toolStripMenuItem9,
            this.UtilsTakeScreenshotMenu,
            this.UtilsDumpMainMemoryMenu,
            this.toolStripMenuItem1,
            this.UtilsFrameLimitingMenu,
            this.toolStripMenuItem7,
            this.UtilsUseFastmemMenu,
            this.toolStripMenuItem10,
            this.UtilsAssociateExtensionsMenu});
			this.UtilsMenu.Name = "UtilsMenu";
			this.UtilsMenu.Size = new System.Drawing.Size(42, 20);
			this.UtilsMenu.Text = "&Utils";
			// 
			// UtilsDisplayMenu
			// 
			this.UtilsDisplayMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.xToolStripMenuItem1,
            this.xToolStripMenuItem2,
            this.xToolStripMenuItem3,
            this.xToolStripMenuItem4});
			this.UtilsDisplayMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.magnifier;
			this.UtilsDisplayMenu.Name = "UtilsDisplayMenu";
			this.UtilsDisplayMenu.Size = new System.Drawing.Size(306, 22);
			this.UtilsDisplayMenu.Text = "&Display";
			// 
			// xToolStripMenuItem1
			// 
			this.xToolStripMenuItem1.Name = "xToolStripMenuItem1";
			this.xToolStripMenuItem1.ShortcutKeyDisplayString = "1";
			this.xToolStripMenuItem1.Size = new System.Drawing.Size(98, 22);
			this.xToolStripMenuItem1.Text = "&1x";
			this.xToolStripMenuItem1.Click += new System.EventHandler(this.xToolStripMenuItem1_Click);
			// 
			// xToolStripMenuItem2
			// 
			this.xToolStripMenuItem2.Name = "xToolStripMenuItem2";
			this.xToolStripMenuItem2.ShortcutKeyDisplayString = "2";
			this.xToolStripMenuItem2.Size = new System.Drawing.Size(98, 22);
			this.xToolStripMenuItem2.Text = "&2x";
			this.xToolStripMenuItem2.Click += new System.EventHandler(this.xToolStripMenuItem2_Click);
			// 
			// xToolStripMenuItem3
			// 
			this.xToolStripMenuItem3.Name = "xToolStripMenuItem3";
			this.xToolStripMenuItem3.ShortcutKeyDisplayString = "3";
			this.xToolStripMenuItem3.Size = new System.Drawing.Size(98, 22);
			this.xToolStripMenuItem3.Text = "&3x";
			this.xToolStripMenuItem3.Click += new System.EventHandler(this.xToolStripMenuItem3_Click);
			// 
			// xToolStripMenuItem4
			// 
			this.xToolStripMenuItem4.Name = "xToolStripMenuItem4";
			this.xToolStripMenuItem4.ShortcutKeyDisplayString = "4";
			this.xToolStripMenuItem4.Size = new System.Drawing.Size(98, 22);
			this.xToolStripMenuItem4.Text = "&4x";
			this.xToolStripMenuItem4.Click += new System.EventHandler(this.xToolStripMenuItem4_Click);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(303, 6);
			// 
			// UtilsLanguageMenu
			// 
			this.UtilsLanguageMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.UtilsLanguageEnglishMenu,
            this.UtilsLanguageSpanishMenu});
			this.UtilsLanguageMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.world;
			this.UtilsLanguageMenu.Name = "UtilsLanguageMenu";
			this.UtilsLanguageMenu.Size = new System.Drawing.Size(306, 22);
			this.UtilsLanguageMenu.Text = "&Language";
			// 
			// UtilsLanguageEnglishMenu
			// 
			this.UtilsLanguageEnglishMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.us;
			this.UtilsLanguageEnglishMenu.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.UtilsLanguageEnglishMenu.Name = "UtilsLanguageEnglishMenu";
			this.UtilsLanguageEnglishMenu.Size = new System.Drawing.Size(152, 22);
			this.UtilsLanguageEnglishMenu.Text = "&English";
			this.UtilsLanguageEnglishMenu.Click += new System.EventHandler(this.englishToolStripMenuItem_Click);
			// 
			// UtilsLanguageSpanishMenu
			// 
			this.UtilsLanguageSpanishMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.es;
			this.UtilsLanguageSpanishMenu.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.UtilsLanguageSpanishMenu.Name = "UtilsLanguageSpanishMenu";
			this.UtilsLanguageSpanishMenu.Size = new System.Drawing.Size(152, 22);
			this.UtilsLanguageSpanishMenu.Text = "&Spanish";
			this.UtilsLanguageSpanishMenu.Click += new System.EventHandler(this.spanishToolStripMenuItem_Click);
			// 
			// toolStripMenuItem9
			// 
			this.toolStripMenuItem9.Name = "toolStripMenuItem9";
			this.toolStripMenuItem9.Size = new System.Drawing.Size(303, 6);
			// 
			// UtilsTakeScreenshotMenu
			// 
			this.UtilsTakeScreenshotMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.camera;
			this.UtilsTakeScreenshotMenu.Name = "UtilsTakeScreenshotMenu";
			this.UtilsTakeScreenshotMenu.ShortcutKeys = System.Windows.Forms.Keys.F4;
			this.UtilsTakeScreenshotMenu.Size = new System.Drawing.Size(306, 22);
			this.UtilsTakeScreenshotMenu.Text = "Take &Screenshot...";
			this.UtilsTakeScreenshotMenu.Click += new System.EventHandler(this.takeScreenshotToolStripMenuItem_Click);
			// 
			// UtilsDumpMainMemoryMenu
			// 
			this.UtilsDumpMainMemoryMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.safe;
			this.UtilsDumpMainMemoryMenu.Name = "UtilsDumpMainMemoryMenu";
			this.UtilsDumpMainMemoryMenu.Size = new System.Drawing.Size(306, 22);
			this.UtilsDumpMainMemoryMenu.Text = "&Dump Main Memory...";
			this.UtilsDumpMainMemoryMenu.Click += new System.EventHandler(this.dumpRamToolStripMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(303, 6);
			// 
			// UtilsFrameLimitingMenu
			// 
			this.UtilsFrameLimitingMenu.Checked = true;
			this.UtilsFrameLimitingMenu.CheckOnClick = true;
			this.UtilsFrameLimitingMenu.CheckState = System.Windows.Forms.CheckState.Checked;
			this.UtilsFrameLimitingMenu.Name = "UtilsFrameLimitingMenu";
			this.UtilsFrameLimitingMenu.ShortcutKeys = System.Windows.Forms.Keys.F3;
			this.UtilsFrameLimitingMenu.Size = new System.Drawing.Size(306, 22);
			this.UtilsFrameLimitingMenu.Text = "&Frame Limiting";
			this.UtilsFrameLimitingMenu.Click += new System.EventHandler(this.frameSkippingToolStripMenuItem_Click);
			// 
			// toolStripMenuItem7
			// 
			this.toolStripMenuItem7.Name = "toolStripMenuItem7";
			this.toolStripMenuItem7.Size = new System.Drawing.Size(303, 6);
			// 
			// UtilsUseFastmemMenu
			// 
			this.UtilsUseFastmemMenu.Checked = true;
			this.UtilsUseFastmemMenu.CheckState = System.Windows.Forms.CheckState.Checked;
			this.UtilsUseFastmemMenu.Name = "UtilsUseFastmemMenu";
			this.UtilsUseFastmemMenu.Size = new System.Drawing.Size(306, 22);
			this.UtilsUseFastmemMenu.Text = "Use &Fast and unsafe memory";
			this.UtilsUseFastmemMenu.Click += new System.EventHandler(this.useFastAndUnsafeMemoryToolStripMenuItem_Click);
			// 
			// toolStripMenuItem10
			// 
			this.toolStripMenuItem10.Name = "toolStripMenuItem10";
			this.toolStripMenuItem10.Size = new System.Drawing.Size(303, 6);
			// 
			// UtilsAssociateExtensionsMenu
			// 
			this.UtilsAssociateExtensionsMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.link;
			this.UtilsAssociateExtensionsMenu.Name = "UtilsAssociateExtensionsMenu";
			this.UtilsAssociateExtensionsMenu.Size = new System.Drawing.Size(306, 22);
			this.UtilsAssociateExtensionsMenu.Text = "&Associate extensions (.PBP, .CSO, .PRX, .ELF)";
			this.UtilsAssociateExtensionsMenu.Click += new System.EventHandler(this.associateWithPBPAndCSOToolStripMenuItem_Click);
			// 
			// RunMenu
			// 
			this.RunMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RunRunResumeMenu,
            this.RunPauseMenu});
			this.RunMenu.Name = "RunMenu";
			this.RunMenu.Size = new System.Drawing.Size(40, 20);
			this.RunMenu.Text = "&Run";
			// 
			// RunRunResumeMenu
			// 
			this.RunRunResumeMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.control_play;
			this.RunRunResumeMenu.Name = "RunRunResumeMenu";
			this.RunRunResumeMenu.ShortcutKeys = System.Windows.Forms.Keys.F5;
			this.RunRunResumeMenu.Size = new System.Drawing.Size(161, 22);
			this.RunRunResumeMenu.Text = "&Run/Resume";
			this.RunRunResumeMenu.Click += new System.EventHandler(this.resumeToolStripMenuItem_Click);
			// 
			// RunPauseMenu
			// 
			this.RunPauseMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.control_pause;
			this.RunPauseMenu.Name = "RunPauseMenu";
			this.RunPauseMenu.ShortcutKeys = System.Windows.Forms.Keys.F6;
			this.RunPauseMenu.Size = new System.Drawing.Size(161, 22);
			this.RunPauseMenu.Text = "&Pause";
			this.RunPauseMenu.Click += new System.EventHandler(this.pauseToolStripMenuItem_Click);
			// 
			// DebugMenu
			// 
			this.DebugMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.DebugTraceSyscallsMenu,
            this.DebugTraceUnimplementedSyscallsMenu,
            this.DebugTraceUnimplementedGpuMenu,
            this.DebugShowThreadInfoMenu});
			this.DebugMenu.Name = "DebugMenu";
			this.DebugMenu.Size = new System.Drawing.Size(54, 20);
			this.DebugMenu.Text = "&Debug";
			this.DebugMenu.Click += new System.EventHandler(this.debugToolStripMenuItem_Click);
			// 
			// DebugTraceSyscallsMenu
			// 
			this.DebugTraceSyscallsMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.application_view_xp_terminal;
			this.DebugTraceSyscallsMenu.Name = "DebugTraceSyscallsMenu";
			this.DebugTraceSyscallsMenu.Size = new System.Drawing.Size(235, 22);
			this.DebugTraceSyscallsMenu.Text = "Trace &Syscalls";
			this.DebugTraceSyscallsMenu.Click += new System.EventHandler(this.traceSyscallsToolStripMenuItem_Click);
			// 
			// DebugTraceUnimplementedSyscallsMenu
			// 
			this.DebugTraceUnimplementedSyscallsMenu.Name = "DebugTraceUnimplementedSyscallsMenu";
			this.DebugTraceUnimplementedSyscallsMenu.Size = new System.Drawing.Size(235, 22);
			this.DebugTraceUnimplementedSyscallsMenu.Text = "Trace Unimplemented Syscalls";
			this.DebugTraceUnimplementedSyscallsMenu.Click += new System.EventHandler(this.traceUnimplementedSyscallsToolStripMenuItem_Click);
			// 
			// DebugTraceUnimplementedGpuMenu
			// 
			this.DebugTraceUnimplementedGpuMenu.Name = "DebugTraceUnimplementedGpuMenu";
			this.DebugTraceUnimplementedGpuMenu.Size = new System.Drawing.Size(235, 22);
			this.DebugTraceUnimplementedGpuMenu.Text = "Trace Unimplemented Gpu";
			this.DebugTraceUnimplementedGpuMenu.Click += new System.EventHandler(this.traceUnimplementedGpuToolStripMenuItem_Click);
			// 
			// DebugShowThreadInfoMenu
			// 
			this.DebugShowThreadInfoMenu.Name = "DebugShowThreadInfoMenu";
			this.DebugShowThreadInfoMenu.ShortcutKeyDisplayString = "";
			this.DebugShowThreadInfoMenu.ShortcutKeys = System.Windows.Forms.Keys.F2;
			this.DebugShowThreadInfoMenu.Size = new System.Drawing.Size(235, 22);
			this.DebugShowThreadInfoMenu.Text = "Show &Thread Info";
			this.DebugShowThreadInfoMenu.Click += new System.EventHandler(this.showThreadInfoToolStripMenuItem_Click);
			// 
			// HelpMenu
			// 
			this.HelpMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.HelpWebsiteMenu,
            this.toolStripMenuItem5,
            this.HelpKawagamesMenu,
            this.HelpBlogMenu,
            this.HelpGithubMenu,
            this.toolStripMenuItem6,
            this.HelpCheckForUpdatesMenu,
            this.toolStripMenuItem4,
            this.HelpReportAnIssueMenu,
            this.toolStripMenuItem8,
            this.HelpAboutMenu});
			this.HelpMenu.Name = "HelpMenu";
			this.HelpMenu.Size = new System.Drawing.Size(44, 20);
			this.HelpMenu.Text = "&Help";
			// 
			// HelpWebsiteMenu
			// 
			this.HelpWebsiteMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.world;
			this.HelpWebsiteMenu.Name = "HelpWebsiteMenu";
			this.HelpWebsiteMenu.Size = new System.Drawing.Size(235, 22);
			this.HelpWebsiteMenu.Text = "&Website";
			this.HelpWebsiteMenu.Click += new System.EventHandler(this.websiteToolStripMenuItem_Click);
			// 
			// toolStripMenuItem5
			// 
			this.toolStripMenuItem5.Name = "toolStripMenuItem5";
			this.toolStripMenuItem5.Size = new System.Drawing.Size(232, 6);
			// 
			// HelpKawagamesMenu
			// 
			this.HelpKawagamesMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.kawagames;
			this.HelpKawagamesMenu.Name = "HelpKawagamesMenu";
			this.HelpKawagamesMenu.Size = new System.Drawing.Size(235, 22);
			this.HelpKawagamesMenu.Text = "&kawagames.com";
			this.HelpKawagamesMenu.Click += new System.EventHandler(this.indieGamesToolStripMenuItem_Click);
			// 
			// HelpBlogMenu
			// 
			this.HelpBlogMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.blog;
			this.HelpBlogMenu.Name = "HelpBlogMenu";
			this.HelpBlogMenu.Size = new System.Drawing.Size(235, 22);
			this.HelpBlogMenu.Text = "en.blog.cballesterosvelasco.es";
			this.HelpBlogMenu.Click += new System.EventHandler(this.blogcballesterosvelascoesToolStripMenuItem_Click);
			// 
			// HelpGithubMenu
			// 
			this.HelpGithubMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.github_icon;
			this.HelpGithubMenu.Name = "HelpGithubMenu";
			this.HelpGithubMenu.Size = new System.Drawing.Size(235, 22);
			this.HelpGithubMenu.Text = "github.com/soywiz/cspspemu";
			this.HelpGithubMenu.Click += new System.EventHandler(this.githubcomsoywizcspspemuToolStripMenuItem_Click);
			// 
			// toolStripMenuItem6
			// 
			this.toolStripMenuItem6.Name = "toolStripMenuItem6";
			this.toolStripMenuItem6.Size = new System.Drawing.Size(232, 6);
			// 
			// HelpCheckForUpdatesMenu
			// 
			this.HelpCheckForUpdatesMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.feed;
			this.HelpCheckForUpdatesMenu.Name = "HelpCheckForUpdatesMenu";
			this.HelpCheckForUpdatesMenu.Size = new System.Drawing.Size(235, 22);
			this.HelpCheckForUpdatesMenu.Text = "&Check for updates...";
			this.HelpCheckForUpdatesMenu.Click += new System.EventHandler(this.checkForUpdatesToolStripMenuItem_Click);
			// 
			// toolStripMenuItem4
			// 
			this.toolStripMenuItem4.Name = "toolStripMenuItem4";
			this.toolStripMenuItem4.Size = new System.Drawing.Size(232, 6);
			// 
			// HelpReportAnIssueMenu
			// 
			this.HelpReportAnIssueMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.bug;
			this.HelpReportAnIssueMenu.Name = "HelpReportAnIssueMenu";
			this.HelpReportAnIssueMenu.Size = new System.Drawing.Size(235, 22);
			this.HelpReportAnIssueMenu.Text = "&Report an issue...";
			this.HelpReportAnIssueMenu.Click += new System.EventHandler(this.reportAnIssueToolStripMenuItem_Click);
			// 
			// toolStripMenuItem8
			// 
			this.toolStripMenuItem8.Name = "toolStripMenuItem8";
			this.toolStripMenuItem8.Size = new System.Drawing.Size(232, 6);
			// 
			// HelpAboutMenu
			// 
			this.HelpAboutMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.help;
			this.HelpAboutMenu.Name = "HelpAboutMenu";
			this.HelpAboutMenu.ShortcutKeys = System.Windows.Forms.Keys.F1;
			this.HelpAboutMenu.Size = new System.Drawing.Size(235, 22);
			this.HelpAboutMenu.Text = "&About...";
			this.HelpAboutMenu.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
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
			this.Load += new System.EventHandler(this.PspDisplayForm_Load_1);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PspDisplayForm_KeyDown);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.PspDisplayForm_KeyUp);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public System.Windows.Forms.MenuStrip menuStrip1;
		public System.Windows.Forms.ToolStripMenuItem FileExitMenu;
		public System.Windows.Forms.ToolStripMenuItem UtilsTakeScreenshotMenu;
		public System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		public System.Windows.Forms.ToolStripMenuItem UtilsFrameLimitingMenu;
		public System.Windows.Forms.ToolStripMenuItem HelpMenu;
		public System.Windows.Forms.ToolStripMenuItem HelpWebsiteMenu;
		public System.Windows.Forms.ToolStripMenuItem UtilsDisplayMenu;
		public System.Windows.Forms.ToolStripMenuItem xToolStripMenuItem1;
		public System.Windows.Forms.ToolStripMenuItem xToolStripMenuItem2;
		public System.Windows.Forms.ToolStripMenuItem xToolStripMenuItem4;
		public System.Windows.Forms.ToolStripMenuItem xToolStripMenuItem3;
		public System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		public System.Windows.Forms.ToolStripMenuItem UtilsDumpMainMemoryMenu;
		public System.Windows.Forms.ToolStripMenuItem FileOpenMenu;
		public System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
		public System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
		public System.Windows.Forms.ToolStripMenuItem HelpAboutMenu;
		public System.Windows.Forms.ToolStripMenuItem RunRunResumeMenu;
		public System.Windows.Forms.ToolStripMenuItem RunPauseMenu;
		public System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
		public System.Windows.Forms.ToolStripMenuItem HelpKawagamesMenu;
		public System.Windows.Forms.ToolStripMenuItem HelpBlogMenu;
		public System.Windows.Forms.ToolStripMenuItem DebugTraceSyscallsMenu;
		public System.Windows.Forms.ToolStripMenuItem DebugTraceUnimplementedGpuMenu;
		public System.Windows.Forms.ToolStripMenuItem DebugShowThreadInfoMenu;
		public System.Windows.Forms.ToolStripMenuItem DebugTraceUnimplementedSyscallsMenu;
		public System.Windows.Forms.ToolStripSeparator toolStripMenuItem6;
		public System.Windows.Forms.ToolStripMenuItem HelpCheckForUpdatesMenu;
		public System.Windows.Forms.ToolStripSeparator toolStripMenuItem7;
		public System.Windows.Forms.ToolStripMenuItem UtilsAssociateExtensionsMenu;
		public System.Windows.Forms.ToolStripMenuItem HelpGithubMenu;
		public System.Windows.Forms.ToolStripMenuItem HelpReportAnIssueMenu;
		public System.Windows.Forms.ToolStripSeparator toolStripMenuItem8;
		public System.Windows.Forms.ToolStripMenuItem UtilsLanguageMenu;
		public System.Windows.Forms.ToolStripMenuItem UtilsLanguageEnglishMenu;
		public System.Windows.Forms.ToolStripMenuItem UtilsLanguageSpanishMenu;
		public System.Windows.Forms.ToolStripSeparator toolStripMenuItem9;
		public System.Windows.Forms.ToolStripMenuItem FileMenu;
		public System.Windows.Forms.ToolStripMenuItem UtilsMenu;
		public System.Windows.Forms.ToolStripMenuItem RunMenu;
		public System.Windows.Forms.ToolStripMenuItem DebugMenu;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem10;
		public System.Windows.Forms.ToolStripMenuItem UtilsUseFastmemMenu;
	}
}