using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Reflection;
using System.Web;
using System.Windows.Forms;
using CSharpUtils;
using CSPspEmu.Core;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Controller;
using CSPspEmu.Core.Display;
using CSPspEmu.Core.Utils;
using CSPspEmu.Resources;

namespace CSPspEmu.Gui.Winforms
{
	public unsafe partial class PspDisplayForm : Form
	{
		/// <summary>
		/// 
		/// </summary>
		public Bitmap Buffer = new Bitmap(512, 272);

		/// <summary>
		/// 
		/// </summary>
		public Graphics BufferGraphics;

		/// <summary>
		/// 
		/// </summary>
		public SceCtrlData SceCtrlData;

		/// <summary>
		/// 
		/// </summary>
		public IGuiExternalInterface IGuiExternalInterface;

		/// <summary>
		/// 
		/// </summary>
		public PspMemory Memory { get { return IGuiExternalInterface.GetMemory(); } }

		/// <summary>
		/// 
		/// </summary>
		public PspDisplay PspDisplay { get { return IGuiExternalInterface.GetDisplay(); } }

		/// <summary>
		/// 
		/// </summary>
		public PspConfig PspConfig { get { return IGuiExternalInterface.GetConfig(); } }

		/// <summary>
		/// 
		/// </summary>
		public PspController PspController { get { return IGuiExternalInterface.GetController(); } }

		float AnalogX = 0.0f, AnalogY = 0.0f;

		public void SendControllerFrame()
		{
			if (IGuiExternalInterface.IsInitialized())
			{
				SceCtrlData.X = 0;
				SceCtrlData.Y = 0;

				bool AnalogXUpdated = false;
				bool AnalogYUpdated = false;
				if (AnalogUp) { AnalogY -= 0.4f; AnalogYUpdated = true; }
				if (AnalogDown) { AnalogY += 0.4f; AnalogYUpdated = true; }
				if (AnalogLeft) { AnalogX -= 0.4f; AnalogXUpdated = true; }
				if (AnalogRight) { AnalogX += 0.4f; AnalogXUpdated = true; }
				if (!AnalogXUpdated) AnalogX /= 2.0f;
				if (!AnalogYUpdated) AnalogY /= 2.0f;

				AnalogX = MathFloat.Clamp(AnalogX, -1.0f, 1.0f);
				AnalogY = MathFloat.Clamp(AnalogY, -1.0f, 1.0f);

				//Console.WriteLine("{0}, {1}", AnalogX, AnalogY);

				SceCtrlData.X = AnalogX;
				SceCtrlData.Y = AnalogY;

				this.PspController.InsertSceCtrlData(SceCtrlData);
			}
		}

		bool ShowMenus;
		bool AutoLoad;

		public PspDisplayForm(IGuiExternalInterface IGuiExternalInterface, bool ShowMenus = true, bool AutoLoad = false, int DefaultDisplayScale = 1)
		{
			this.IGuiExternalInterface = IGuiExternalInterface;
			this.ShowMenus = ShowMenus;
			this.AutoLoad = AutoLoad;

			InitializeComponent();
			HandleCreated += new EventHandler(PspDisplayForm_HandleCreated);

			this.ShowIcon = ShowMenus;
			this.MainMenuStrip.Visible = ShowMenus;
			
			/*
			this.MainMenuStrip = null;
			this.PerformLayout();
			*/
			//this.MainMenuStrip.Visible = false;

			DefaultDisplayScale = IGuiExternalInterface.GetConfig().StoredConfig.DisplayScale;

			DisplayScale = DefaultDisplayScale;

			BufferGraphics = Graphics.FromImage(Buffer);
			//BufferGraphics.Clear(Color.Red);
			BufferGraphics.Clear(Color.Black);

			updateResumePause();
			UpdateCheckMenusFromConfig();
			updateDebugGpu();
			ReLoadControllerConfig();
			UpdateRecentList();

			var Timer = new System.Windows.Forms.Timer();
			Timer.Interval = 1000 / 60;
			Timer.Tick += new EventHandler(Timer_Tick);
			Timer.Start();
		}

		void PspDisplayForm_HandleCreated(object sender, EventArgs e)
		{
			LanguageUpdated();
		}

		private int _DisplayScale;

		public int MainMenuStripHeight
		{
			get 
			{
				if (ShowMenus)
				{
					//Console.WriteLine(this.menuStrip1.Height);
					//Console.ReadKey();
					//return 24;
					return this.menuStrip1.Height;
				}
				return 0;
			}
		}

		public int DisplayScale
		{
			set
			{
				_DisplayScale = value;
				var InnerSize = new Size(480 * _DisplayScale, 272 * _DisplayScale + MainMenuStripHeight);
				MinimumSize = new Size(1, 1);
				MaximumSize = new Size(2048, 2048);
				SetClientSizeCore(InnerSize.Width, InnerSize.Height);
				MinimumSize = MaximumSize = Size;
				CenterToScreen();

				UtilsDisplay1xMenu.Checked = (_DisplayScale == 1);
				UtilsDisplay2xMenu.Checked = (_DisplayScale == 2);
				UtilsDisplay3xMenu.Checked = (_DisplayScale == 3);
				UtilsDisplay4xMenu.Checked = (_DisplayScale == 4);

				IGuiExternalInterface.GetConfig().StoredConfig.DisplayScale = _DisplayScale;
			}
			get
			{
				return _DisplayScale;
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
		}

		public struct BGRA
		{
			public byte B, G, R, A;
		}

		byte* OldFrameBuffer = (byte *)-1;
		ulong LastHash = unchecked((ulong)-1);
		String LastText = "";

		OutputPixel[] BitmapDataDecode = new OutputPixel[512 * 512];

		private void UpdateTitle()
		{
			try
			{
				if (LastText != PspConfig.GameTitle)
				{
					LastText = PspConfig.GameTitle;
					//this.Font = new Font("Lucida Console", 16);
					if (ShowMenus)
					{
						this.Text = "Soywiz's PspEmu - " + PspGlobalConfiguration.CurrentVersion + " :: r" + PspGlobalConfiguration.CurrentVersionNumeric + " :: " + Platform.Architecture + " :: " + LastText;
					}
					else
					{
						this.Text = LastText;
					}
				}
			}
			catch
			{
			}
		}

		protected override void OnPaintBackground(PaintEventArgs PaintEventArgs)
		{
			if (!IGuiExternalInterface.IsInitialized())
			{
				var Buffer = new Bitmap(512, 272);
				var BufferGraphics = Graphics.FromImage(Buffer);
				BufferGraphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 0, Buffer.Width, Buffer.Height));
				//BufferGraphics.DrawString("Initializing...", new Font("Arial", 10), new SolidBrush(Color.White), new PointF(8, 8));
				PaintEventArgs.Graphics.DrawImage(Buffer, new Rectangle(0, MainMenuStripHeight, 512 * DisplayScale, 272 * DisplayScale));
				return;
			}

			UpdateTitle();

			if (EnableRefreshing)
			{
				try
				{
					int Width = 512;
					int Height = 272;
					//var Address = PspDisplay.CurrentInfo.Address | 0x04000000; // It causes artifacts
					var Address = PspDisplay.CurrentInfo.Address;
					byte* FrameBuffer = null;
					try
					{
						FrameBuffer = (byte*)Memory.PspAddressToPointerSafe(
							Address,
							PixelFormatDecoder.GetPixelsSize(PspDisplay.CurrentInfo.PixelFormat, Width * Height)
						);
					}
					catch (Exception Exception)
					{
						Console.Error.WriteLine(Exception);
					}

					//Console.Error.WriteLine("FrameBuffer == 0x{0:X}!!", (long)FrameBuffer);

					if (FrameBuffer == null)
					{
						//Console.Error.WriteLine("FrameBuffer == null!!");
					}

					//Console.WriteLine("{0:X}", Address);

					var Hash = PixelFormatDecoder.Hash(
						PspDisplay.CurrentInfo.PixelFormat,
						(void*)FrameBuffer,
						Width, Height
					);

					if (Hash != LastHash)
					{
						LastHash = Hash;
						Buffer.LockBitsUnlock(PixelFormat.Format32bppArgb, (BitmapData) =>
						{
							var Count = Width * Height;
							fixed (OutputPixel* BitmapDataDecodePtr = BitmapDataDecode)
							{
								var BitmapDataPtr = (BGRA*)BitmapData.Scan0.ToPointer();

								//var LastRow = (FrameBuffer + 512 * 260 * 4 + 4 * 10);
								//Console.WriteLine("{0},{1},{2},{3}", LastRow[0], LastRow[1], LastRow[2], LastRow[3]);

								if (FrameBuffer == null)
								{
									if (OldFrameBuffer != null)
									{
										Console.Error.WriteLine("FrameBuffer == null");
									}
								}
								else if (BitmapDataPtr == null)
								{
									Console.Error.WriteLine("BitmapDataPtr == null");
								}
								else
								{
									PixelFormatDecoder.Decode(
										PspDisplay.CurrentInfo.PixelFormat,
										(void*)FrameBuffer,
										BitmapDataDecodePtr,
										Width, Height
									);
								}

								// Converts the decoded data to Window's format.
								for (int n = 0; n < Count; n++)
								{
									BitmapDataPtr[n].R = BitmapDataDecodePtr[n].R;
									BitmapDataPtr[n].G = BitmapDataDecodePtr[n].G;
									BitmapDataPtr[n].B = BitmapDataDecodePtr[n].B;
									BitmapDataPtr[n].A = 0xFF;
								}

								OldFrameBuffer = FrameBuffer;
							}
						});
					}
					//else
					{
						//Console.WriteLine("Display not updated!");
					}
				}
				catch (Exception Exception)
				{
					Console.Error.WriteLine(Exception);
				}
			}
			//Console.WriteLine(this.ClientRectangle);
			PaintEventArgs.Graphics.CompositingMode = CompositingMode.SourceCopy;
			PaintEventArgs.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
			PaintEventArgs.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
			PaintEventArgs.Graphics.DrawImage(
				Buffer,
				new Rectangle(
					0, MainMenuStripHeight,
					512 * DisplayScale, 272 * DisplayScale
				)
			);
			//PaintEventArgs.Graphics.DrawImageUnscaled(Buffer, new Point(0, menuStrip1.Height));
		}

		protected bool EnableRefreshing = true;

		void Timer_Tick(object sender, EventArgs e)
		{
			SendControllerFrame();
			
			if (GameListComponent == null || !GameListComponent.Visible)
			{
				Refresh();
			}

			if (IGuiExternalInterface.IsInitialized())
			{
				PspDisplay.TriggerVBlankStart();
				Thread.Sleep(TimeSpan.FromMilliseconds(4));
				PspDisplay.TriggerVBlankEnd();
			}
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		protected void PauseResume(Action Action)
		{
			try
			{
				EnableRefreshing = false;
				IGuiExternalInterface.PauseResume(Action);
			}
			finally
			{
				EnableRefreshing = true;
			}
		}

		private void takeScreenshotToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PauseResume(() =>
			{
				var SaveFileDialog = new SaveFileDialog();
				SaveFileDialog.Filter = "PNG|*.png|All Files|*.*";
				SaveFileDialog.FileName = String.Format("{0} - screenshot.png", PspConfig.GameTitle);
				SaveFileDialog.AddExtension = true;
				SaveFileDialog.DefaultExt = "png";
				if (SaveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					var Buffer2 = new Bitmap(480, 272);
					Graphics.FromImage(Buffer2).DrawImage(Buffer, Point.Empty);
					Buffer2.Save(SaveFileDialog.FileName, ImageFormat.Png);
				}
			});
		}

		/*
		public void UpdateFlags(PspCtrlButtons PspCtrlButtons)
		{
			SceCtrlData.Buttons
		}
		*/

		private void frameSkippingToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.PspConfig.VerticalSynchronization = UtilsFrameLimitingMenu.Checked;
		}

		private void websiteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			//Process.Start(@"http://en.blog.cballesterosvelasco.es/search/label/pspemu/?rf=csp");
			Process.Start(@"http://pspemu.soywiz.com/?rf=csp&version=" + PspGlobalConfiguration.CurrentVersion + "&version2=" + PspGlobalConfiguration.CurrentVersionNumeric);
		}

		private void xToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			DisplayScale = 1;
		}

		private void xToolStripMenuItem2_Click(object sender, EventArgs e)
		{
			DisplayScale = 2;
		}

		private void xToolStripMenuItem3_Click(object sender, EventArgs e)
		{
			DisplayScale = 3;
		}

		private void xToolStripMenuItem4_Click(object sender, EventArgs e)
		{
			DisplayScale = 4;
		}

		private void dumpRamToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PauseResume(() =>
			{
				var SaveFileDialog = new SaveFileDialog();
				SaveFileDialog.Filter = "DUMP|*.dump|All Files|*.*";
				SaveFileDialog.FileName = String.Format("memory-{0}.dump", (long)(DateTime.UtcNow - new DateTime(0)).TotalMilliseconds);
				SaveFileDialog.AddExtension = true;
				SaveFileDialog.DefaultExt = "dump";
				if (SaveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					var Stream = SaveFileDialog.OpenFile();
					Stream.WriteStream(new PspMemoryStream(Memory).SliceWithBounds(PspMemory.MainSegment.Low, PspMemory.MainSegment.High - 1));
					Stream.Flush();
					Stream.Close();
				}
			});
		}

		private PspCtrlButtons GetButtonsFromKeys(Keys Key)
		{
			if (!KeyMap.ContainsKey(Key)) return PspCtrlButtons.None;
			return KeyMap[Key];
		}

		//float AnalogX, AnalogY;
		bool AnalogUp = false;
		bool AnalogDown = false;
		bool AnalogLeft = false;
		bool AnalogRight = false;

		private void PspDisplayForm_KeyDown(object sender, KeyEventArgs e)
		{
			//Console.WriteLine("aaaaaaaaaaaa");
			switch (e.KeyCode)
			{
				case Keys.D1: DisplayScale = 1; break;
				case Keys.D2: DisplayScale = 2; break;
				case Keys.D3: DisplayScale = 3; break;
				case Keys.D4: DisplayScale = 4; break;
				//case Keys.F2: IGuiExternalInterface.ShowDebugInformation(); break;
			}

			TryUpdateAnalog(e.KeyCode, true);

			SceCtrlData.UpdateButtons(GetButtonsFromKeys(e.KeyCode), true);
		}

		private void TryUpdateAnalog(Keys Key, bool Press)
		{
			if (AnalogKeyMap.ContainsKey(Key))
			{
				switch (AnalogKeyMap[Key])
				{
					case PspCtrlAnalog.Up: AnalogUp = Press; break;
					case PspCtrlAnalog.Down: AnalogDown = Press; break;
					case PspCtrlAnalog.Left: AnalogLeft = Press; break;
					case PspCtrlAnalog.Right: AnalogRight = Press; break;
				}
			}
		}

		private void PspDisplayForm_KeyUp(object sender, KeyEventArgs e)
		{
			//Console.WriteLine("Keup!");
			var Key = e.KeyCode;

			TryUpdateAnalog(e.KeyCode, false);

			SceCtrlData.UpdateButtons(GetButtonsFromKeys(e.KeyCode), false);
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var OpenFileDialog = new OpenFileDialog();
			var Result = default(DialogResult);

			PauseResume(() =>
			{
				OpenFileDialog.Filter = "Compatible Formats (*.elf, *.pbp, *.iso, *.cso, *.dax, *.prx)|*.elf;*.pbp;*.iso;*.cso;*.dax;*.prx|All Files|*.*";
				Result = OpenFileDialog.ShowDialog();
			});

			if (Result == System.Windows.Forms.DialogResult.OK)
			{
				OpenFileRealOnNewThreadLock(OpenFileDialog.FileName);
			}
		}

		private void OpenFileRealOnNewThreadLock(string FilePath)
		{
			var BackgroundThread = new Thread(() =>
			{
				PauseResume(() =>
				{
					OpenFileReal(FilePath);
				});
			});
			BackgroundThread.IsBackground = true;
			BackgroundThread.Start();
		}

		private void OpenFileReal(string FilePath)
		{
			this.Invoke(new Action(() =>
			{
				if (GameListComponent != null)
				{
					GameListComponent.Visible = false;
				}
				this.Focus();
			}));

			OpenRecentHook(FilePath);
			IGuiExternalInterface.LoadFile(FilePath);
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PauseResume(() =>
			{
				new AboutForm(this, IGuiExternalInterface).ShowDialog();
			});
		}

		private void updateResumePause()
		{
			var Paused = IGuiExternalInterface.IsPaused();
			RunPauseMenu.Checked = Paused;
			RunRunResumeMenu.Checked = !Paused;
		}

		private void updateDebugGpu()
		{
			DebugTraceUnimplementedGpuMenu.Checked = PspConfig.NoticeUnimplementedGpuCommands;
		}

		private void resumeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IGuiExternalInterface.Resume();
			updateResumePause();
		}

		private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IGuiExternalInterface.Pause();
			updateResumePause();
		}

		private void indieGamesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start(@"http://kawagames.com/?rf=csp");
		}

		private void blogcballesterosvelascoesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start(@"http://en.blog.cballesterosvelasco.es/?rf=csp");
		}

		private void traceSyscallsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PspConfig.DebugSyscalls = !PspConfig.DebugSyscalls;
			UpdateCheckMenusFromConfig();
		}

		private void traceUnimplementedGpuToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PspConfig.NoticeUnimplementedGpuCommands = !PspConfig.NoticeUnimplementedGpuCommands;
			UpdateCheckMenusFromConfig();
		}

		private void showThreadInfoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IGuiExternalInterface.ShowDebugInformation();
		}

		private void debugToolStripMenuItem_Click(object sender, EventArgs e)
		{

		}

		private void traceUnimplementedSyscallsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PspConfig.DebugNotImplemented = !PspConfig.DebugNotImplemented;
			UpdateCheckMenusFromConfig();
		}

		private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CheckForUpdates(NotifyIfNotFound: true);
		}

		public static string GetComparableVersion(string VersionName)
		{
			string Return = "";
			foreach (var Char in VersionName)
			{
				if (Char == '/') break;
				if (Char >= '0' && Char <= '9')
				{
					Return += Char;
				}
			}
			return Return;
		}

		public void CheckForUpdates(bool NotifyIfNotFound)
		{
			var CurrentVersion = PspGlobalConfiguration.CurrentVersion;

			IGuiExternalInterface.GetConfig().StoredConfig.LastCheckedTime = DateTime.UtcNow;

			var CheckForUpdatesThread = new Thread(() =>
			{
				try
				{
					var Request = HttpWebRequest.Create(new Uri("https://raw.github.com/soywiz/cspspemu/master/version_last.txt"));
					Request.Proxy = null;
					var Response = Request.GetResponse();
					var Stream = Response.GetResponseStream();
					var LastVersion = Stream.ReadAllContentsAsString(null, false);

					//int CurrentVersionInt = int.Parse(CurrentVersion);
					//int LastVersionInt = int.Parse(LastVersion);

					var ComparableCurrentVersion = GetComparableVersion(LastVersion);
					var ComparableLastVersion = GetComparableVersion(CurrentVersion);

					Console.WriteLine("{0} -> {1}", CurrentVersion, LastVersion);
					Console.WriteLine("{0} -> {1}", ComparableCurrentVersion, ComparableLastVersion);

					if (String.CompareOrdinal(ComparableCurrentVersion, ComparableLastVersion) > 0)
					{
						if (MessageBox.Show(
							String.Format("There is a new version of the emulator.\n") +
							String.Format("\n") +
							String.Format("Current Version: {0}\n", CurrentVersion) +
							String.Format("Last Version: {0}\n", LastVersion) +
							String.Format("\n") +
							String.Format("Download the new version?") +
							"",
							"New Version", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
						{
							Process.Start(@"http://pspemu.soywiz.com/?rf=csp&version=" + CurrentVersion);
						}
					}
					else
					{
						if (NotifyIfNotFound)
						{
							MessageBox.Show("You have the lastest version", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
						}
					}
				}
				catch (Exception Exception)
				{
					MessageBox.Show(Exception.ToString(), "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			});
			/*
			foreach (var Embed in Assembly.GetEntryAssembly().GetManifestResourceNames())
			{
				Console.WriteLine(Embed);
			}
			*/
			CheckForUpdatesThread.IsBackground = true;
			CheckForUpdatesThread.Start();
		}

		private static bool RunProgramInBackground(string ApplicationPath, string ApplicationArguments)
		{
			// This snippet needs the "System.Diagnostics"
			// library


			// Application path and command line arguments
			//string ApplicationPath = ApplicationPaths.ExecutablePath;
			//string ApplicationArguments = "/associate";

			//Console.WriteLine(ExecutablePath);

			// Create a new process object
			Process ProcessObj = new Process();

			ProcessObj.StartInfo = new ProcessStartInfo()
			{
				// StartInfo contains the startup information of the new process
				FileName = ApplicationPath,
				Arguments = ApplicationArguments,

				UseShellExecute = true,
				Verb = "runas",

				// These two optional flags ensure that no DOS window appears
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				//RedirectStandardOutput = true,
			};

			bool Error = false;
			// Wait that the process exits
			try
			{
				// Start the process
				ProcessObj.Start();

				ProcessObj.WaitForExit();
			}
			catch
			{
				Error = true;
			}

			return !Error && (ProcessObj.ExitCode == 0);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <seealso cref="http://social.msdn.microsoft.com/Forums/en-US/winforms/thread/db6647a3-85ca-4dc4-b661-fbbd36bd561f/"/>
		private void associateWithPBPAndCSOToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var Success = !RunProgramInBackground(ApplicationPaths.ExecutablePath, "/associate");

			if (!Success)
			{
				MessageBox.Show("Associations done!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else
			{
				MessageBox.Show("Can't associate", "Done", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			// Now read the output of the DOS application
			//string Result = ProcessObj.StandardOutput.ReadToEnd();
		}

		private void githubcomsoywizcspspemuToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start(@"https://github.com/soywiz/pspemu/?rf=csp");
		}

		private void reportAnIssueToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start(@"https://github.com/soywiz/cspspemu/issues/new?title=" + HttpUtility.UrlEncode("Problem with " + PspConfig.GameTitle + "\n") + "&body=" + HttpUtility.UrlEncode("Version: " + PspGlobalConfiguration.CurrentVersion + "\nTitle: " + PspConfig.GameTitle + "\n") + "&labels[]=" + HttpUtility.UrlEncode("Games"));
		}

		private void PspDisplayForm_Load(object sender, EventArgs e)
		{

		}
		Dictionary<ToolStripMenuItem, CultureInfo> LanguagePairs = new Dictionary<ToolStripMenuItem, CultureInfo>();

		public IEnumerable<Control> GetAll(Control control, Type type)
		{
			var controls = control.Controls.Cast<Control>();

			return controls.SelectMany(ctrl => GetAll(ctrl, type)).Concat(controls).Where(c => c.GetType() == type);
		}

		private void LanguageUpdated()
		{
			this.UtilsLanguageMenu.DropDownItems.Clear();
			LanguagePairs.Clear();

			foreach (var AvailableLanguage in Translations.AvailableLanguages)
			{
				var CultureInfo = new CultureInfo(Translations.GetString("info", "CultureInfo", AvailableLanguage));
				var ToolStrip = new ToolStripMenuItem()
				{
					Image = Translations.GetLangFlagImage(AvailableLanguage),
					ImageScaling = ToolStripItemImageScaling.None,
					Size = new Size(152, 22),
					Text = Translations.GetString("languages", AvailableLanguage),
					Tag = CultureInfo,
				};
				ToolStrip.Click += LanguageMenuItem_Click;
				this.UtilsLanguageMenu.DropDownItems.Add(ToolStrip);

				LanguagePairs.Add(ToolStrip, CultureInfo);
			}

			foreach (var LanguagePair in LanguagePairs)
			{
				LanguagePair.Key.Tag = LanguagePair.Value;
				LanguagePair.Key.Checked = (LanguagePair.Value.CompareInfo == Thread.CurrentThread.CurrentUICulture.CompareInfo);
			}

			foreach (var Field in this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
			{
				if (Field.FieldType == typeof(ToolStripMenuItem))
				{
					var ToolStripMenuItem = (ToolStripMenuItem)Field.GetValue(this);
					var Translation = Translations.GetString("menus", ToolStripMenuItem.Name);
					ToolStripMenuItem.Text = ((Translation != null) ? Translation : ToolStripMenuItem.Text);
				}
			}

			UpdateTitle();
		}

		private void LanguageMenuItem_Click(object sender, EventArgs e)
		{
			Thread.CurrentThread.CurrentUICulture = (CultureInfo)((ToolStripMenuItem)sender).Tag;
			LanguageUpdated();
		}

		GameListComponent GameListComponent;

		private void PspDisplayForm_Load_1(object sender, EventArgs e)
		{
			UtilsFrameLimitingMenu.Checked = this.PspConfig.VerticalSynchronization;
			UtilsAstOptimizationsMenu.Checked = IGuiExternalInterface.GetConfig().StoredConfig.EnableAstOptimizations;
			UtilsUseFastmemMenu.Checked = IGuiExternalInterface.GetConfig().StoredConfig.UseFastMemory;

			Debug.WriteLine(String.Format("Now: {0}", DateTime.UtcNow));
			Debug.WriteLine(String.Format("LastCheckedTime: {0}", IGuiExternalInterface.GetConfig().StoredConfig.LastCheckedTime));
			Debug.WriteLine(String.Format("Elapsed: {0}", (DateTime.UtcNow - IGuiExternalInterface.GetConfig().StoredConfig.LastCheckedTime)));
			if ((DateTime.UtcNow - IGuiExternalInterface.GetConfig().StoredConfig.LastCheckedTime).TotalDays > 3)
			{
				CheckForUpdates(NotifyIfNotFound: false);
			}

			if (Platform.OperatingSystem == Platform.OS.Windows)
			//if (false)
			{
				GameListComponent = new GameListComponent();

				GameListComponent.SelectedItem += (IsoFile) =>
				{
					OpenFileRealOnNewThreadLock(IsoFile);
				};
				GameListComponent.Dock = DockStyle.Fill;

				//PspConfig.IsosPath = @"e:\isos\pspa";
				if (!AutoLoad)
				{
					RefreshGameList();
					GameListComponent.Visible = true;
				}
				else
				{
					GameListComponent.Visible = false;
				}

				GameListComponent.Parent = this;
			}
		}

		public void RefreshGameList()
		{
			ThreadPool.QueueUserWorkItem((state) =>
			{
				if (GameListComponent != null) GameListComponent.Init(PspConfig.StoredConfig.IsosPath, ApplicationPaths.MemoryStickRootFolder);
			});
		}

		private void RestartOptions()
		{
			IGuiExternalInterface.GetConfig().StoredConfig.UseFastMemory = UtilsUseFastmemMenu.Checked;
			IGuiExternalInterface.GetConfig().StoredConfig.EnableAstOptimizations = UtilsAstOptimizationsMenu.Checked;
			if (MessageBox.Show("This option requires restarting the emulator.\n\nDo you want to restart the emulator?", "Warning", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				Application.Restart();
			}
		}

		private void useFastAndUnsafeMemoryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			UtilsUseFastmemMenu.Checked = !UtilsUseFastmemMenu.Checked;
			RestartOptions();
		}

		private void UtilsAstOptimizations_Click(object sender, EventArgs e)
		{
			UtilsAstOptimizationsMenu.Checked = !UtilsAstOptimizationsMenu.Checked;
			RestartOptions();
		}

		private void DebugDumpGpuFrameMenu_Click(object sender, EventArgs e)
		{
			IGuiExternalInterface.CaptureGpuFrame();
		}

		private void enableMpegProcessinginestableYetToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IGuiExternalInterface.GetConfig().StoredConfig.EnableMpeg = !UtilsEnableMpegMenu.Checked;
			UpdateCheckMenusFromConfig();
		}

		private void UpdateCheckMenusFromConfig()
		{
			DebugTraceSyscallsMenu.Checked = PspConfig.DebugSyscalls;
			DebugTraceUnimplementedSyscallsMenu.Checked = PspConfig.DebugNotImplemented;
			DebugTraceUnimplementedGpuMenu.Checked = PspConfig.NoticeUnimplementedGpuCommands;
			UtilsEnableMpegMenu.Checked = IGuiExternalInterface.GetConfig().StoredConfig.EnableMpeg;
			//UtilsUseFastmemMenu.Checked = !PspConfig.;
		}

		private void installWavDestDirectShowFilterToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var Success = !RunProgramInBackground(ApplicationPaths.ExecutablePath, "/installat3");

			if (!Success)
			{
				MessageBox.Show("Registered done!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else
			{
				MessageBox.Show("Can't register WavDest.dll", "Done", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void configureControllerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PauseResume(() =>
			{
				new ButtonMappingForm(PspConfig).ShowDialog();
				ReLoadControllerConfig();
				StoreConfig();
			});
		}

		private void ReLoadControllerConfig()
		{
			var ControllerConfig = PspConfig.StoredConfig.ControllerConfig;

			AnalogKeyMap = new Dictionary<Keys, PspCtrlAnalog>();
			{
				AnalogKeyMap[ParseKeyName(ControllerConfig.AnalogLeft)] = PspCtrlAnalog.Left;
				AnalogKeyMap[ParseKeyName(ControllerConfig.AnalogRight)] = PspCtrlAnalog.Right;
				AnalogKeyMap[ParseKeyName(ControllerConfig.AnalogUp)] = PspCtrlAnalog.Up;
				AnalogKeyMap[ParseKeyName(ControllerConfig.AnalogDown)] = PspCtrlAnalog.Down;
			}

			KeyMap = new Dictionary<Keys, PspCtrlButtons>();
			{
				KeyMap[ParseKeyName(ControllerConfig.DigitalLeft)] = PspCtrlButtons.Left;
				KeyMap[ParseKeyName(ControllerConfig.DigitalRight)] = PspCtrlButtons.Right;
				KeyMap[ParseKeyName(ControllerConfig.DigitalUp)] = PspCtrlButtons.Up;
				KeyMap[ParseKeyName(ControllerConfig.DigitalDown)] = PspCtrlButtons.Down;

				KeyMap[ParseKeyName(ControllerConfig.TriangleButton)] = PspCtrlButtons.Triangle;
				KeyMap[ParseKeyName(ControllerConfig.CrossButton)] = PspCtrlButtons.Cross;
				KeyMap[ParseKeyName(ControllerConfig.SquareButton)] = PspCtrlButtons.Square;
				KeyMap[ParseKeyName(ControllerConfig.CircleButton)] = PspCtrlButtons.Circle;

				KeyMap[ParseKeyName(ControllerConfig.StartButton)] = PspCtrlButtons.Start;
				KeyMap[ParseKeyName(ControllerConfig.SelectButton)] = PspCtrlButtons.Select;

				KeyMap[ParseKeyName(ControllerConfig.LeftTriggerButton)] = PspCtrlButtons.LeftTrigger;
				KeyMap[ParseKeyName(ControllerConfig.RightTriggerButton)] = PspCtrlButtons.RightTrigger;
			}

			Console.WriteLine("KeyMapping:");

			foreach (var Map in AnalogKeyMap)
			{
				Console.WriteLine("  '{0}' -> PspCtrlAnalog.{1}", Map.Key, Map.Value);
			}

			foreach (var Map in KeyMap)
			{
				Console.WriteLine("  '{0}' -> PspCtrlButtons.{1}", Map.Key, Map.Value);
			}
		}

		[Flags]
		public enum PspCtrlAnalog
		{
			Left = (1 << 0),
			Right = (1 << 1),
			Up = (1 << 2),
			Down = (1 << 3),
		}

		private Dictionary<Keys, PspCtrlButtons> KeyMap = new Dictionary<Keys, PspCtrlButtons>();
		private Dictionary<Keys, PspCtrlAnalog> AnalogKeyMap = new Dictionary<Keys, PspCtrlAnalog>();

		private void StoreConfig()
		{
			PspConfig.StoredConfig.Save();
		}

		private void OpenRecentHook(string Path)
		{
			//Console.WriteLine("PATH: {0}", Path);
			var RecentFiles = PspConfig.StoredConfig.RecentFiles;
			try { RecentFiles.Remove(Path); } catch { }
			RecentFiles.Insert(0, Path);
			while (RecentFiles.Count > 9) RecentFiles.RemoveAt(RecentFiles.Count - 1);
			PspConfig.StoredConfig.Save();
			UpdateRecentList();
		}

		private void UpdateRecentList()
		{
			this.Invoke(new Action(() =>
			{
				try
				{
					var Items = FileOpenRecentMenu.DropDownItems;
					Items.Clear();
					foreach (var RecentFile in PspConfig.StoredConfig.RecentFiles)
					{
						var Item = new ToolStripMenuItem()
						{
							Text = RecentFile,
							ShowShortcutKeys = true,
							ShortcutKeys = ((Keys)((Keys.Control | (Keys.D1 + Items.Count)))),
						};
						Item.Click += Recent_Click;
						Items.Add(Item);
					}
				}
				catch (Exception Exception)
				{
					Console.Error.WriteLine(Exception);
				}
			}));
		}

		void Recent_Click(object sender, EventArgs e)
		{
			var ToolStripMenuItem = (ToolStripMenuItem)sender;
			OpenFileRealOnNewThreadLock(ToolStripMenuItem.Text);
		}

		private static Keys ParseKeyName(string KeyName)
		{
			return (Keys)Enum.Parse(typeof(Keys), KeyName);
		}

		private void openRecentToolStripMenuItem_Click(object sender, EventArgs e)
		{

		}

		private void PspDisplayForm_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
			{
				e.Effect = DragDropEffects.All;
			}  
		}

		private void PspDisplayForm_DragDrop(object sender, DragEventArgs e)
		{
			string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

			this.TopMost = true;
			this.Focus();
			this.TopMost = false;

			foreach (string file in files)
			{
				OpenFileRealOnNewThreadLock(file);

				break;
			}  
		}

		private void setIsoFolderToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var Dialog = new FolderBrowserDialog();
			Dialog.ShowNewFolderButton = true;
			//Dialog.RootFolder = PspConfig.StoredConfig.IsosPath;

			if (Dialog.ShowDialog() == DialogResult.OK)
			{
				PspConfig.StoredConfig.IsosPath = Dialog.SelectedPath;
				PspConfig.StoredConfig.Save();
			}

			RefreshGameList();
			//new FileDialog();
		}

		private void emureleasescomToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start(@"http://www.emureleases.com/?rf=csp");
		}
	}
}
