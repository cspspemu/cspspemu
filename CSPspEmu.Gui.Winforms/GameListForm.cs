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

namespace CSPspEmu.Gui.Winforms
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="http://www.codeproject.com/Articles/16009/A-Much-Easier-to-Use-ListView"/>
	public partial class GameListForm : Form
	{
		public GameListForm()
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

		class MyRenderer : IRenderer
		{
			public bool RenderItem(DrawListViewItemEventArgs e, Graphics g, Rectangle itemBounds, object rowObject)
			{
				throw new NotImplementedException();
			}

			public bool RenderSubItem(DrawListViewSubItemEventArgs e, Graphics g, Rectangle cellBounds, object rowObject)
			{
				throw new NotImplementedException();
			}

			public void HitTest(OlvListViewHitTestInfo hti, int x, int y)
			{
				throw new NotImplementedException();
			}

			public Rectangle GetEditRectangle(Graphics g, Rectangle cellBounds, OLVListItem item, int subItemIndex)
			{
				throw new NotImplementedException();
			}
		}

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

		private void GameListForm_Load(object sender, EventArgs e)
		{
			TypedObjectListViewEntry = new TypedObjectListView<GameList.GameEntry>(objectListView1);
			objectListView1.ShowGroups = false;
			objectListView1.FullRowSelect = true;
			DiscIdColumn.MaximumWidth = DiscIdColumn.MinimumWidth = DiscIdColumn.Width = 80;
			FirmwareColumn.MaximumWidth = FirmwareColumn.MinimumWidth = FirmwareColumn.Width = 60;
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
			objectListView1.AllowColumnReorder = true;

			objectListView1.Columns.Add(BannerColumn);
			objectListView1.Columns.Add(DiscIdColumn);
			objectListView1.Columns.Add(TitleColumn);
			objectListView1.Columns.Add(FirmwareColumn);
			objectListView1.Columns.Add(RegionColumn);
			objectListView1.Columns.Add(MediaTypeColumn);
			objectListView1.Columns.Add(LicenseTypeColumn);
			objectListView1.Columns.Add(ReleaseTypeColumn);
			objectListView1.Columns.Add(PathColumn);
			objectListView1.GridLines = true;

			TitleColumn.Width = 400;
			PathColumn.Width = 120;

			objectListView1.Sort(TitleColumn, SortOrder.Ascending);

			//TitleColumn.HeaderFont = new Font("MS Gothic Normal", 16);
			TitleColumn.RendererDelegate = (ee, gg, rr, oo) =>
			{
				var Entry = ((GameList.GameEntry)oo);
				var Selected = (objectListView1.GetSelectedObjects().Contains((object)Entry));
				var Font = new Font("MS Gothic Normal", 13);
				gg.FillRectangle(new SolidBrush(!Selected ? SystemColors.Window : SystemColors.Highlight), new Rectangle(rr.Left - 1, rr.Top - 1, rr.Width + 1, rr.Height + 1));
				var Measure = gg.MeasureString(Entry.TITLE, Font);
				gg.Clip = new System.Drawing.Region(rr);
				gg.DrawString(Entry.TITLE, Font, new SolidBrush(!Selected ? SystemColors.WindowText : SystemColors.HighlightText), new Point(rr.Left + 8, (int)(rr.Top + rr.Height / 2 - Measure.Height / 2)));
				//gg.FillRectangle(new SolidBrush(Color.White), rr);
				//gg.DrawImageUnscaled(Entry.CachedBitmap, new Point(rr.Left, rr.Top));

				return true;
			};

			BannerColumn.MinimumWidth = BannerColumn.MaximumWidth = BannerColumn.Width = IconSize.Width;
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
						gg2.CompositingQuality = CompositingQuality.HighQuality;
						gg2.Clear(Color.White);
						gg2.DrawImage(Image.FromStream(new MemoryStream(Data)), new Rectangle(0, 0, IconSize.Width, IconSize.Height));
					}
				}

				gg.FillRectangle(new SolidBrush(Color.White), new Rectangle(rr.Left - 1, rr.Top - 1, rr.Width + 1, rr.Height + 1));
				gg.DrawImageUnscaled(Entry.CachedBitmap, new Point(rr.Left - 1, rr.Top - 1));

				return true;
			};

			TitleColumn.AspectGetter = delegate(object _entry) { return ((GameList.GameEntry)_entry).TITLE; };
			DiscIdColumn.AspectGetter = delegate(object _entry) { return ((GameList.GameEntry)_entry).DiscId0; };
			PathColumn.AspectGetter = delegate(object _entry) { return Path.GetFileName(((GameList.GameEntry)_entry).IsoFile); };
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

			FirmwareColumn.AspectGetter = delegate(object _entry) { return ((GameList.GameEntry)_entry).PSP_SYSTEM_VER; };
		}

		private void GameListForm_Shown(object sender, EventArgs e)
		{
			var ProgressForm = new ProgressForm();

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

				GameList.EntryAdded += (Entry) =>
				{
					List.Add(Entry);
				};

				//GameList.ScanPath(@"e:\isos\psp");
				GameList.ScanPath(@"e:\isos\psp", @"c:\temp");

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

			ProgressForm.ShowDialog();
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

		private void objectListView1_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			var Entry = (GameList.GameEntry)objectListView1.GetSelectedObject();
			Console.WriteLine("Double click! : {0}", Entry.IsoFile);
		}
	}
}
