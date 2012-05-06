using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CSharpUtils;

namespace CSPspEmu.Gui.Winforms
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="http://www.codeproject.com/Articles/16009/A-Much-Easier-to-Use-ListView"/>
	public partial class GameListComponent : UserControl
	{
		public GameListComponent()
		{
			InitializeComponent();
		}

		public class Entry
		{
			public string Title;
			public string Title2;

			public string GetTitle() {
				return Title;
			}
		}

		TypedObjectListView<GameList.GameEntry> TypedObjectListViewEntry;

		OLVColumn BannerColumn = new OLVColumn("Banner", "");
		OLVColumn DiscIdColumn = new OLVColumn("Id", "");
		OLVColumn TitleColumn = new OLVColumn("Title", "");
		OLVColumn FirmwareColumn = new OLVColumn("Firmware", "");
		OLVColumn PathColumn = new OLVColumn("Path", "");
		OLVColumn RegionColumn = new OLVColumn("Region", "");
		OLVColumn MediaTypeColumn = new OLVColumn("Media", "");
		OLVColumn LicenseTypeColumn = new OLVColumn("License", "");
		OLVColumn ReleaseTypeColumn = new OLVColumn("Release", "");

		/*
		class MyFilter : IModelFilter
		{
			public bool Filter(object modelObject)
			{
				var Entry = (GameList.GameEntry)modelObject;
				return true;
			}
		}
		*/

		Font Font2 = new Font("MS Gothic Normal", 13);
		Font Font3 = new Font("MS Gothic Normal", 13, FontStyle.Strikeout);

		private void GameListForm_Load(object sender, EventArgs e)
		{
			TypedObjectListViewEntry = new TypedObjectListView<GameList.GameEntry>(objectListView1);
			objectListView1.ShowGroups = false;
			objectListView1.FullRowSelect = true;
			DiscIdColumn.TextAlign = HorizontalAlignment.Center;
			FirmwareColumn.TextAlign = HorizontalAlignment.Center;

			objectListView1.StateImageList = new ImageList();
			var img = new Bitmap(640, 120);
			var g = Graphics.FromImage(img);
			g.DrawLine(new Pen(new SolidBrush(Color.Red)), new Point(0, 0), new Point(100, 100));
			objectListView1.StateImageList.Images.Add("test", img);
			objectListView1.OwnerDraw = true;

			//var IconSize = new Size(144, 80);
			var IconSize = new Size(108, 60);

			objectListView1.RowHeight = IconSize.Height;
			//objectListView1.AllowColumnReorder = true;
			//objectListView1.AutoResizeColumns();

			objectListView1.Resize += objectListView1_Resize;
			ResetColumns();
			objectListView1.GridLines = true;

			objectListView1.Sort(TitleColumn, SortOrder.Ascending);

			//TitleColumn.HeaderFont = new Font("MS Gothic Normal", 16);
			TitleColumn.RendererDelegate = (ee, gg, rr, oo) =>
			{
				var Entry = ((GameList.GameEntry)oo);
				var Selected = (objectListView1.SelectedObjects.Contains((object)Entry));
				gg.FillRectangle(new SolidBrush(!Selected ? SystemColors.Window : SystemColors.Highlight), new Rectangle(rr.Left - 1, rr.Top - 1, rr.Width + 1, rr.Height + 1));

				var Text = Entry.TITLE;
				Font Font;

				if (Entry.PatchedWithPrometheus)
				{
					Text = Text + " *PATCHED*";
					Font = Font3;
				}
				else
				{
					Font = Font2;
				}

				var Measure = gg.MeasureString(Text, Font, new Size(rr.Width, rr.Height));
				gg.Clip = new System.Drawing.Region(rr);
				gg.DrawString(
					Text,
					Font,
					new SolidBrush(!Selected ? SystemColors.WindowText : SystemColors.HighlightText),
					new Rectangle(
						new Point(rr.Left + 8, (int)(rr.Top + rr.Height / 2 - Measure.Height / 2)),
						new Size(rr.Width, rr.Height)
					)
				);
				//gg.FillRectangle(new SolidBrush(Color.White), rr);
				//gg.DrawImageUnscaled(Entry.CachedBitmap, new Point(rr.Left, rr.Top));

				return true;
			};

			BannerColumn.MaximumWidth = BannerColumn.Width = BannerColumn.MinimumWidth = IconSize.Width;
			//BannerColumn.AspectGetter = delegate(object _entry) { return null; };
			BannerColumn.RendererDelegate = (ee, gg, rr, oo) =>
			{
				var Entry = ((GameList.GameEntry)oo);
				var Data = Entry.Icon0Png;
				if (Entry.CachedBitmap == null)
				{
					Entry.CachedBitmap = new Bitmap(IconSize.Width, IconSize.Height);
					using (var gg2 = Graphics.FromImage(Entry.CachedBitmap))
					{
						var IconToBlit = Image.FromStream(new MemoryStream(Data));

						var TempBuffer = new Bitmap(144, 80);
						using (var gg3 = Graphics.FromImage(TempBuffer))
						{
							gg3.CompositingQuality = CompositingQuality.HighQuality;
							gg3.Clear(Color.Transparent);
							gg3.DrawImage(IconToBlit, new Rectangle(TempBuffer.Width / 2 - IconToBlit.Width / 2, 0, IconToBlit.Width, IconToBlit.Height));
						}

						//Console.WriteLine("{0}x{1}", IconToBlit.Width, IconToBlit.Height);

						gg2.CompositingQuality = CompositingQuality.HighQuality;
						if (Entry.PatchedWithPrometheus)
						{
							gg2.Clear(Color.Red);
						}
						else
						{
							gg2.Clear(Color.White);
						}
						gg2.DrawImage(TempBuffer, new Rectangle(0, 0, IconSize.Width, IconSize.Height));
						if (Entry.PatchedWithPrometheus)
						{
							gg2.DrawLine(new Pen(Color.Red), new Point(0, 0), new Point(IconSize.Width, IconSize.Height));
							gg2.DrawLine(new Pen(Color.Red), new Point(IconSize.Width, 0), new Point(0, IconSize.Height));
						}
					}
				}

				gg.FillRectangle(new SolidBrush(Entry.PatchedWithPrometheus ? Color.Red : Color.White), new Rectangle(rr.Left - 1, rr.Top - 1, rr.Width + 1, rr.Height + 1));
				gg.DrawImageUnscaled(Entry.CachedBitmap, new Point(rr.Left - 1, rr.Top - 1));

				return true;
			};

			TitleColumn.AspectGetter = delegate(object _entry) { try { return ((GameList.GameEntry)_entry).TITLE; } catch { return "*ERROR*"; } };
			DiscIdColumn.AspectGetter = delegate(object _entry) { try { return ((GameList.GameEntry)_entry).DiscId0; } catch { return "*ERROR*"; } };
			PathColumn.AspectGetter = delegate(object _entry) { try { return Path.GetFileName(((GameList.GameEntry)_entry).IsoFile); } catch { return "*ERROR*"; } };
			MediaTypeColumn.AspectGetter = delegate(object _entry)
			{
				var Entry = ((GameList.GameEntry)_entry);

				var DISC_ID = Entry.DISC_ID;
				switch (DISC_ID[0])
				{
					case 'S': return "CD/DVD";
					case 'U': return "UMD";
					case 'B': return "BluRay";
					default: return "Unknown";
				}
			};
			LicenseTypeColumn.AspectGetter = delegate(object _entry)
			{
				var Entry = ((GameList.GameEntry)_entry);

				var DISC_ID = Entry.DISC_ID;
				switch (DISC_ID[1])
				{
					case 'C': return "Sony";
					case 'L': return "Other";
					default: return "Unknown";
				}
			};
			RegionColumn.AspectGetter = delegate(object _entry) {
				var Entry = ((GameList.GameEntry)_entry);

				var DISC_ID = Entry.DISC_ID;
				switch (DISC_ID[2])
				{
					case 'P':
					case 'J': return "Japan";
					case 'E': return "Europe";
					case 'K': return "Korea";
					case 'U': return "USA";
					case 'A': return "Asia";
					default: return "Unknown";
				}
			};
			ReleaseTypeColumn.AspectGetter = delegate(object _entry) {
				var Entry = ((GameList.GameEntry)_entry);

				var DISC_ID = Entry.DISC_ID;
				switch (DISC_ID[3])
				{
					case 'D': return "Demo";
					case 'M': return "Malasian";
					case 'S': return "Retail";
					default: return "Unknown";
				}
			};

			FirmwareColumn.AspectGetter = delegate(object _entry) { try { return ((GameList.GameEntry)_entry).PSP_SYSTEM_VER; } catch { return "*ERROR*"; } };
		}

		public int GetColumnsWidth(params ColumnSize[] Except)
		{
			int Width = 0;

			var Columns = new[] { BannerColumnSize, DiscIdColumnSize, TitleColumnSize, FirmwareColumnSize, RegionColumnSize, PathColumnSize };

			foreach (var Column in Columns)
			{
				if (!Except.Contains(Column))
				{
					Width += Column.Width;
				}
			}
			return Width;
		}

		void ResetColumns()
		{
			ResetColumnsWidths();
			UpdateColumnsWidths();

			objectListView1.Columns.Clear();
			objectListView1.Columns.Add(BannerColumn);
			objectListView1.Columns.Add(DiscIdColumn);
			objectListView1.Columns.Add(TitleColumn);
			objectListView1.Columns.Add(FirmwareColumn);
			objectListView1.Columns.Add(RegionColumn);
			//objectListView1.Columns.Add(MediaTypeColumn);
			//objectListView1.Columns.Add(LicenseTypeColumn);
			//objectListView1.Columns.Add(ReleaseTypeColumn);
			objectListView1.Columns.Add(PathColumn);
		}

		ColumnSize BannerColumnSize = new ColumnSize();
		ColumnSize DiscIdColumnSize = new ColumnSize();
		ColumnSize TitleColumnSize = new ColumnSize();
		ColumnSize FirmwareColumnSize = new ColumnSize();
		ColumnSize RegionColumnSize = new ColumnSize();
		ColumnSize PathColumnSize = new ColumnSize();

		public class ColumnSize
		{
			public int MinimumWidth = 0;
			public int MaximumWidth = 1024;
			private int _Width = 120;
			public int Width
			{
				get
				{
					return _Width;
				}
				set
				{
					_Width = MathUtils.Clamp(value, MinimumWidth, MaximumWidth);
				}
			}
		}

		void ResetColumnsWidths()
		{
			DiscIdColumnSize.MaximumWidth = DiscIdColumnSize.MinimumWidth = DiscIdColumnSize.Width = 80;
			FirmwareColumnSize.MaximumWidth = FirmwareColumnSize.MinimumWidth = FirmwareColumnSize.Width = 60;
			PathColumnSize.Width = 120;
			TitleColumnSize.Width = 400;
			TitleColumnSize.MinimumWidth = 120;
			RegionColumnSize.Width = 60;
		}

		void UpdateColumnWidths(ColumnSize ColumnSize, OLVColumn Column)
		{
			if (Column.MinimumWidth != ColumnSize.MinimumWidth) Column.MinimumWidth = ColumnSize.MinimumWidth;
			if (Column.MaximumWidth != ColumnSize.MaximumWidth) Column.MaximumWidth = ColumnSize.MaximumWidth;
			if (Column.Width != ColumnSize.Width) Column.Width = ColumnSize.Width;
		}

		void UpdateColumnsWidths()
		{
			UpdateColumnWidths(BannerColumnSize, BannerColumn);
			UpdateColumnWidths(DiscIdColumnSize, DiscIdColumn);
			UpdateColumnWidths(TitleColumnSize, TitleColumn);
			UpdateColumnWidths(FirmwareColumnSize, FirmwareColumn);
			UpdateColumnWidths(RegionColumnSize, RegionColumn);
			UpdateColumnWidths(PathColumnSize, PathColumn);
		}

		void objectListView1_Resize(object sender, EventArgs e)
		{
			ResetColumnsWidths();

			bool Updated = false;

			//ResetColumns();
			var InitialWidth = objectListView1.Width - 32;
			var RestColumnWidth = GetColumnsWidth(TitleColumnSize);
			TitleColumnSize.Width = InitialWidth - RestColumnWidth;

			/*
			if (GetColumnsWidth() > InitialWidth)
			{
				PathColumn.Width = PathColumn.MinimumWidth = 0;
			}
			*/

			UpdateColumnsWidths();
		}

		public void Init(string IsosPath, string CachePath)
		{
			if (IsosPath == null)
			{
				return;
			}
			var ProgressForm = new ProgressForm();
			try
			{
				ThreadPool.QueueUserWorkItem((state) =>
				{
					var GameList = new GameList();
					Console.WriteLine("Reading ISOs...");
					GameList.Progress += (Title, Current, Total) =>
					{
						//Console.WriteLine("Progress: {0}, {1}/{2}", Title, Current, Total);
						ProgressForm.SetProgress(Title, Current, Total);
					};

					var List = new List<GameList.GameEntry>();

					GameList.EntryAdded += (Entry, Cached) =>
					{
						//Console.WriteLine("aaaaaaaa");
						List.Add(Entry);
						if (!Cached)
						{
							objectListView1.AddObject(Entry);
						}
					};

					GameList.ScanPath(IsosPath, CachePath);

					this.Invoke(new Action(() =>
					{
						objectListView1.SetObjects(List);
						objectListView1.Sort(TitleColumn, SortOrder.Ascending);
					}));

					Console.WriteLine("Done");
					/*
					foreach (var Entry in GameList.Entries)
					{
						Console.WriteLine(Entry.TITLE);
					}
					*/

					ProgressForm.End();
				}, null);

				this.Invoke(new Action(() =>
				{
					ProgressForm.ShowDialog();
				}));
			}
			finally
			{
				ProgressForm.End();
			}
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			var Text = textBox1.Text;
			if (Text.Length > 0)
			{
				TextMatchFilter filter = new TextMatchFilter(objectListView1, Text);
				objectListView1.UseFiltering = true;
				objectListView1.ModelFilter = filter;
				//objectListView1.DefaultRenderer = new HighlightTextRenderer(filter);
			}
			else
			{
				objectListView1.UseFiltering = false;
				objectListView1.ModelFilter = null;
				//objectListView1.DefaultRenderer = new BaseRenderer();
			}
		}

		private void textBox1_Enter(object sender, EventArgs e)
		{
			//Console.WriteLine("aaaaaaaaaa");
			textBox1.SelectAll();
		}

		public event Action<string> SelectedItem;

		private void objectListView1_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			var Entry = (GameList.GameEntry)objectListView1.SelectedObject;
			SelectedItem(Entry.IsoFile);
		}

		private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
		{

		}
	}
}
