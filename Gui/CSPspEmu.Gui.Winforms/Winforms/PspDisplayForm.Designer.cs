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
			this.FileMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.FileOpenMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.FileSetIsoFolderMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			this.FileOpenRecentMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem14 = new System.Windows.Forms.ToolStripSeparator();
			this.FileExitMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.UtilsMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.UtilsDisplayMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.UtilsDisplay1xMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.UtilsDisplay2xMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.UtilsDisplay3xMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.UtilsDisplay4xMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.UtilsRenderScaleMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.UtilsRenderScale1xMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.UtilsRenderScale2xMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.UtilsRenderScale4xMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.UtilsRenderScale8xMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.UtilsEnableSmaaMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.UtilsLanguageMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.UtilsConfigureControllerMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripSeparator();
			this.UtilsTakeScreenshotMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.UtilsFrameLimitingMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
			this.UtilsUseFastmemMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.UtilsAstOptimizationsMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripSeparator();
			this.UtilsAssociateExtensionsMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.RunMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.RunRunResumeMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.RunPauseMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.DebugMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.DebugTraceSyscallsMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.DebugTraceUnimplementedSyscallsMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.DebugTraceUnimplementedGpuMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem11 = new System.Windows.Forms.ToolStripSeparator();
			this.UtilsDumpMainMemoryMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.DebugDumpGpuFrameMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem12 = new System.Windows.Forms.ToolStripSeparator();
			this.DebugShowThreadInfoMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem15 = new System.Windows.Forms.ToolStripSeparator();
			this.DebugCWCheatEditorMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem13 = new System.Windows.Forms.ToolStripSeparator();
			this.DebugFunctionViewer = new System.Windows.Forms.ToolStripMenuItem();
			this.DebugTextureViewer = new System.Windows.Forms.ToolStripMenuItem();
			this.HelpMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.HelpWebsiteMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
			this.HelpKawagamesMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.HelpEmureleasesMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.HelpBlogMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.HelpGithubMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
			this.HelpCheckForUpdatesMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
			this.HelpReportAnIssueMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripSeparator();
			this.HelpAboutMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.UtilsScaleTexturesMenu = new System.Windows.Forms.ToolStripMenuItem();
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
            this.FileSetIsoFolderMenu,
            this.toolStripMenuItem3,
            this.FileOpenRecentMenu,
            this.toolStripMenuItem14,
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
			// FileSetIsoFolderMenu
			// 
			this.FileSetIsoFolderMenu.Name = "FileSetIsoFolderMenu";
			this.FileSetIsoFolderMenu.Size = new System.Drawing.Size(155, 22);
			this.FileSetIsoFolderMenu.Text = "Set &Iso Folder...";
			this.FileSetIsoFolderMenu.Click += new System.EventHandler(this.setIsoFolderToolStripMenuItem_Click);
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.toolStripMenuItem3.Size = new System.Drawing.Size(152, 6);
			// 
			// FileOpenRecentMenu
			// 
			this.FileOpenRecentMenu.Name = "FileOpenRecentMenu";
			this.FileOpenRecentMenu.Size = new System.Drawing.Size(155, 22);
			this.FileOpenRecentMenu.Text = "Open &Recent";
			this.FileOpenRecentMenu.Click += new System.EventHandler(this.openRecentToolStripMenuItem_Click);
			// 
			// toolStripMenuItem14
			// 
			this.toolStripMenuItem14.Name = "toolStripMenuItem14";
			this.toolStripMenuItem14.Size = new System.Drawing.Size(152, 6);
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
            this.UtilsRenderScaleMenu,
            this.UtilsEnableSmaaMenu,
            this.UtilsScaleTexturesMenu,
            this.toolStripMenuItem2,
            this.UtilsLanguageMenu,
            this.UtilsConfigureControllerMenu,
            this.toolStripMenuItem9,
            this.UtilsTakeScreenshotMenu,
            this.toolStripMenuItem1,
            this.UtilsFrameLimitingMenu,
            this.toolStripMenuItem7,
            this.UtilsUseFastmemMenu,
            this.UtilsAstOptimizationsMenu,
            this.toolStripMenuItem10,
            this.UtilsAssociateExtensionsMenu});
			this.UtilsMenu.Name = "UtilsMenu";
			this.UtilsMenu.Size = new System.Drawing.Size(42, 20);
			this.UtilsMenu.Text = "&Utils";
			// 
			// UtilsDisplayMenu
			// 
			this.UtilsDisplayMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.UtilsDisplay1xMenu,
            this.UtilsDisplay2xMenu,
            this.UtilsDisplay3xMenu,
            this.UtilsDisplay4xMenu});
			this.UtilsDisplayMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.magnifier;
			this.UtilsDisplayMenu.Name = "UtilsDisplayMenu";
			this.UtilsDisplayMenu.Size = new System.Drawing.Size(306, 22);
			this.UtilsDisplayMenu.Text = "&Display";
			// 
			// UtilsDisplay1xMenu
			// 
			this.UtilsDisplay1xMenu.Name = "UtilsDisplay1xMenu";
			this.UtilsDisplay1xMenu.ShortcutKeyDisplayString = "1";
			this.UtilsDisplay1xMenu.Size = new System.Drawing.Size(98, 22);
			this.UtilsDisplay1xMenu.Text = "&1x";
			this.UtilsDisplay1xMenu.Click += new System.EventHandler(this.xToolStripMenuItem1_Click);
			// 
			// UtilsDisplay2xMenu
			// 
			this.UtilsDisplay2xMenu.Name = "UtilsDisplay2xMenu";
			this.UtilsDisplay2xMenu.ShortcutKeyDisplayString = "2";
			this.UtilsDisplay2xMenu.Size = new System.Drawing.Size(98, 22);
			this.UtilsDisplay2xMenu.Text = "&2x";
			this.UtilsDisplay2xMenu.Click += new System.EventHandler(this.xToolStripMenuItem2_Click);
			// 
			// UtilsDisplay3xMenu
			// 
			this.UtilsDisplay3xMenu.Name = "UtilsDisplay3xMenu";
			this.UtilsDisplay3xMenu.ShortcutKeyDisplayString = "3";
			this.UtilsDisplay3xMenu.Size = new System.Drawing.Size(98, 22);
			this.UtilsDisplay3xMenu.Text = "&3x";
			this.UtilsDisplay3xMenu.Click += new System.EventHandler(this.xToolStripMenuItem3_Click);
			// 
			// UtilsDisplay4xMenu
			// 
			this.UtilsDisplay4xMenu.Name = "UtilsDisplay4xMenu";
			this.UtilsDisplay4xMenu.ShortcutKeyDisplayString = "4";
			this.UtilsDisplay4xMenu.Size = new System.Drawing.Size(98, 22);
			this.UtilsDisplay4xMenu.Text = "&4x";
			this.UtilsDisplay4xMenu.Click += new System.EventHandler(this.xToolStripMenuItem4_Click);
			// 
			// UtilsRenderScaleMenu
			// 
			this.UtilsRenderScaleMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.UtilsRenderScale1xMenu,
            this.UtilsRenderScale2xMenu,
            this.UtilsRenderScale4xMenu,
            this.UtilsRenderScale8xMenu});
			this.UtilsRenderScaleMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.quality;
			this.UtilsRenderScaleMenu.Name = "UtilsRenderScaleMenu";
			this.UtilsRenderScaleMenu.Size = new System.Drawing.Size(306, 22);
			this.UtilsRenderScaleMenu.Text = "&Render scale";
			this.UtilsRenderScaleMenu.Click += new System.EventHandler(this.UtilsRenderScaleMenu_Click);
			// 
			// UtilsRenderScale1xMenu
			// 
			this.UtilsRenderScale1xMenu.Name = "UtilsRenderScale1xMenu";
			this.UtilsRenderScale1xMenu.Size = new System.Drawing.Size(85, 22);
			this.UtilsRenderScale1xMenu.Tag = "1";
			this.UtilsRenderScale1xMenu.Text = "1x";
			this.UtilsRenderScale1xMenu.Click += new System.EventHandler(this.UtilsRenderScale1xMenu_Click);
			// 
			// UtilsRenderScale2xMenu
			// 
			this.UtilsRenderScale2xMenu.Name = "UtilsRenderScale2xMenu";
			this.UtilsRenderScale2xMenu.Size = new System.Drawing.Size(85, 22);
			this.UtilsRenderScale2xMenu.Tag = "2";
			this.UtilsRenderScale2xMenu.Text = "2x";
			this.UtilsRenderScale2xMenu.Click += new System.EventHandler(this.UtilsRenderScale2xMenu_Click);
			// 
			// UtilsRenderScale4xMenu
			// 
			this.UtilsRenderScale4xMenu.Name = "UtilsRenderScale4xMenu";
			this.UtilsRenderScale4xMenu.Size = new System.Drawing.Size(85, 22);
			this.UtilsRenderScale4xMenu.Tag = "4";
			this.UtilsRenderScale4xMenu.Text = "4x";
			this.UtilsRenderScale4xMenu.Click += new System.EventHandler(this.UtilsRenderScale4xMenu_Click);
			// 
			// UtilsRenderScale8xMenu
			// 
			this.UtilsRenderScale8xMenu.Name = "UtilsRenderScale8xMenu";
			this.UtilsRenderScale8xMenu.Size = new System.Drawing.Size(85, 22);
			this.UtilsRenderScale8xMenu.Tag = "8";
			this.UtilsRenderScale8xMenu.Text = "8x";
			this.UtilsRenderScale8xMenu.Click += new System.EventHandler(this.xToolStripMenuItem_Click);
			// 
			// UtilsEnableSmaaMenu
			// 
			this.UtilsEnableSmaaMenu.Name = "UtilsEnableSmaaMenu";
			this.UtilsEnableSmaaMenu.ShortcutKeys = System.Windows.Forms.Keys.F7;
			this.UtilsEnableSmaaMenu.Size = new System.Drawing.Size(306, 22);
			this.UtilsEnableSmaaMenu.Text = "Enable &SMAA";
			this.UtilsEnableSmaaMenu.Click += new System.EventHandler(this.enableSMAAToolStripMenuItem_Click);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(303, 6);
			// 
			// UtilsLanguageMenu
			// 
			this.UtilsLanguageMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.world;
			this.UtilsLanguageMenu.Name = "UtilsLanguageMenu";
			this.UtilsLanguageMenu.Size = new System.Drawing.Size(306, 22);
			this.UtilsLanguageMenu.Text = "&Language";
			// 
			// UtilsConfigureControllerMenu
			// 
			this.UtilsConfigureControllerMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.pspicon_16;
			this.UtilsConfigureControllerMenu.Name = "UtilsConfigureControllerMenu";
			this.UtilsConfigureControllerMenu.Size = new System.Drawing.Size(306, 22);
			this.UtilsConfigureControllerMenu.Text = "Configure &Controller...";
			this.UtilsConfigureControllerMenu.Click += new System.EventHandler(this.configureControllerToolStripMenuItem_Click);
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
			// UtilsAstOptimizationsMenu
			// 
			this.UtilsAstOptimizationsMenu.Name = "UtilsAstOptimizationsMenu";
			this.UtilsAstOptimizationsMenu.Size = new System.Drawing.Size(306, 22);
			this.UtilsAstOptimizationsMenu.Text = "Enable &Ast optimizations";
			this.UtilsAstOptimizationsMenu.Click += new System.EventHandler(this.UtilsAstOptimizations_Click);
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
            this.toolStripMenuItem11,
            this.UtilsDumpMainMemoryMenu,
            this.DebugDumpGpuFrameMenu,
            this.toolStripMenuItem12,
            this.DebugShowThreadInfoMenu,
            this.toolStripMenuItem15,
            this.DebugCWCheatEditorMenu,
            this.toolStripMenuItem13,
            this.DebugFunctionViewer,
            this.DebugTextureViewer});
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
			// toolStripMenuItem11
			// 
			this.toolStripMenuItem11.Name = "toolStripMenuItem11";
			this.toolStripMenuItem11.Size = new System.Drawing.Size(232, 6);
			// 
			// UtilsDumpMainMemoryMenu
			// 
			this.UtilsDumpMainMemoryMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.safe;
			this.UtilsDumpMainMemoryMenu.Name = "UtilsDumpMainMemoryMenu";
			this.UtilsDumpMainMemoryMenu.Size = new System.Drawing.Size(235, 22);
			this.UtilsDumpMainMemoryMenu.Text = "&Dump Main Memory...";
			// 
			// DebugDumpGpuFrameMenu
			// 
			this.DebugDumpGpuFrameMenu.Name = "DebugDumpGpuFrameMenu";
			this.DebugDumpGpuFrameMenu.ShortcutKeys = System.Windows.Forms.Keys.F9;
			this.DebugDumpGpuFrameMenu.Size = new System.Drawing.Size(235, 22);
			this.DebugDumpGpuFrameMenu.Text = "Dump &Gpu Frame";
			this.DebugDumpGpuFrameMenu.Click += new System.EventHandler(this.DebugDumpGpuFrameMenu_Click);
			// 
			// toolStripMenuItem12
			// 
			this.toolStripMenuItem12.Name = "toolStripMenuItem12";
			this.toolStripMenuItem12.Size = new System.Drawing.Size(232, 6);
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
			// toolStripMenuItem15
			// 
			this.toolStripMenuItem15.Name = "toolStripMenuItem15";
			this.toolStripMenuItem15.Size = new System.Drawing.Size(232, 6);
			// 
			// DebugCWCheatEditorMenu
			// 
			this.DebugCWCheatEditorMenu.Name = "DebugCWCheatEditorMenu";
			this.DebugCWCheatEditorMenu.Size = new System.Drawing.Size(235, 22);
			this.DebugCWCheatEditorMenu.Text = "&CWCheat editor...";
			this.DebugCWCheatEditorMenu.Click += new System.EventHandler(this.cWCheatEditorToolStripMenuItem_Click);
			// 
			// toolStripMenuItem13
			// 
			this.toolStripMenuItem13.Name = "toolStripMenuItem13";
			this.toolStripMenuItem13.Size = new System.Drawing.Size(232, 6);
			// 
			// DebugFunctionViewer
			// 
			this.DebugFunctionViewer.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.script_binary_icon;
			this.DebugFunctionViewer.Name = "DebugFunctionViewer";
			this.DebugFunctionViewer.Size = new System.Drawing.Size(235, 22);
			this.DebugFunctionViewer.Text = "&Function viewer...";
			this.DebugFunctionViewer.Click += new System.EventHandler(this.functionViewerToolStripMenuItem_Click);
			// 
			// DebugTextureViewer
			// 
			this.DebugTextureViewer.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.Gallery_icon;
			this.DebugTextureViewer.Name = "DebugTextureViewer";
			this.DebugTextureViewer.Size = new System.Drawing.Size(235, 22);
			this.DebugTextureViewer.Text = "&Texture viewer...";
			this.DebugTextureViewer.Click += new System.EventHandler(this.DebugTextureViewer_Click);
			// 
			// HelpMenu
			// 
			this.HelpMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.HelpWebsiteMenu,
            this.toolStripMenuItem5,
            this.HelpKawagamesMenu,
            this.HelpEmureleasesMenu,
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
			// HelpEmureleasesMenu
			// 
			this.HelpEmureleasesMenu.Image = global::CSPspEmu.Gui.Winforms.Properties.Resources.favicon;
			this.HelpEmureleasesMenu.Name = "HelpEmureleasesMenu";
			this.HelpEmureleasesMenu.Size = new System.Drawing.Size(235, 22);
			this.HelpEmureleasesMenu.Text = "emu&releases.com";
			this.HelpEmureleasesMenu.Click += new System.EventHandler(this.emureleasescomToolStripMenuItem_Click);
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
			// UtilsScaleTexturesMenu
			// 
			this.UtilsScaleTexturesMenu.Name = "UtilsScaleTexturesMenu";
			this.UtilsScaleTexturesMenu.Size = new System.Drawing.Size(306, 22);
			this.UtilsScaleTexturesMenu.Text = "&Scale Textures HQ2X";
			this.UtilsScaleTexturesMenu.Click += new System.EventHandler(this.UtilsScaleTexturesMenu_Click);
			// 
			// PspDisplayForm
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(525, 291);
			this.Controls.Add(this.menuStrip1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip1;
			this.MaximizeBox = false;
			this.Name = "PspDisplayForm";
			this.Text = "Soywiz\'s Psp Emulator";
			this.Load += new System.EventHandler(this.PspDisplayForm_Load_1);
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.PspDisplayForm_DragDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.PspDisplayForm_DragEnter);
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
		public System.Windows.Forms.ToolStripMenuItem UtilsDisplay1xMenu;
		public System.Windows.Forms.ToolStripMenuItem UtilsDisplay2xMenu;
		public System.Windows.Forms.ToolStripMenuItem UtilsDisplay4xMenu;
		public System.Windows.Forms.ToolStripMenuItem UtilsDisplay3xMenu;
		public System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
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
		public System.Windows.Forms.ToolStripSeparator toolStripMenuItem9;
		public System.Windows.Forms.ToolStripMenuItem FileMenu;
		public System.Windows.Forms.ToolStripMenuItem UtilsMenu;
		public System.Windows.Forms.ToolStripMenuItem RunMenu;
		public System.Windows.Forms.ToolStripMenuItem DebugMenu;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem10;
		public System.Windows.Forms.ToolStripMenuItem UtilsUseFastmemMenu;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem11;
		public System.Windows.Forms.ToolStripMenuItem UtilsDumpMainMemoryMenu;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem12;
		public System.Windows.Forms.ToolStripMenuItem DebugDumpGpuFrameMenu;
		private System.Windows.Forms.ToolStripMenuItem UtilsConfigureControllerMenu;
		private System.Windows.Forms.ToolStripMenuItem FileOpenRecentMenu;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem14;
		private System.Windows.Forms.ToolStripMenuItem FileSetIsoFolderMenu;
		private System.Windows.Forms.ToolStripMenuItem HelpEmureleasesMenu;
		private System.Windows.Forms.ToolStripMenuItem UtilsAstOptimizationsMenu;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem15;
		private System.Windows.Forms.ToolStripMenuItem DebugFunctionViewer;
		private System.Windows.Forms.ToolStripMenuItem UtilsRenderScaleMenu;
		private System.Windows.Forms.ToolStripMenuItem UtilsRenderScale1xMenu;
		private System.Windows.Forms.ToolStripMenuItem UtilsRenderScale2xMenu;
		private System.Windows.Forms.ToolStripMenuItem UtilsRenderScale4xMenu;
		private System.Windows.Forms.ToolStripMenuItem UtilsRenderScale8xMenu;
		private System.Windows.Forms.ToolStripMenuItem DebugTextureViewer;
		private System.Windows.Forms.ToolStripMenuItem UtilsEnableSmaaMenu;
		private System.Windows.Forms.ToolStripMenuItem DebugCWCheatEditorMenu;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem13;
		private System.Windows.Forms.ToolStripMenuItem UtilsScaleTexturesMenu;
	}
}