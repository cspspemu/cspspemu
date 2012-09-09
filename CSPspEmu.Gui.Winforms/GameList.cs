using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CSPspEmu.Hle.Formats;
using CSPspEmu.Hle.Vfs.Iso;

namespace CSPspEmu.Gui.Winforms
{
	public class GameList
	{
		public class GameEntry
		{
			public string IsoFile;
			public long IsoSize;
			public string DiscId0;
			public string APP_VER;
			public bool BOOTABLE;
			public string CATEGORY;
			public string DISC_ID;
			public int DISC_NUMBER;
			public int DISC_TOTAL;
			public string DISC_VERSION;
			public string DRIVER_PATH;
			public string GAMEDATA_ID;
			public int HRKGMP_VER;
			public int PARENTAL_LEVEL;
			public string PSP_SYSTEM_VER;
			public int REGION;
			public string TITLE;
			public bool USE_USB;
			public byte[] Icon0Png;
			public Image CachedBitmap;
			public string Hash;
			public bool PatchedWithPrometheus;
		}

		public List<GameEntry> Entries = new List<GameEntry>();

		public void ScanPath(string Folder, string CacheFolder, int MaxCount = int.MaxValue)
		{
			Entries.Clear();

			try
			{
				var CheckList = new List<string>();

				foreach (var File in Directory.EnumerateFiles(Folder))
				{
					switch (Path.GetExtension(File).ToLowerInvariant())
					{
						case ".iso":
						case ".cso":
						case ".dax":
							{
								CheckList.Add(File);
							}
							break;
					}
					if (CheckList.Count > MaxCount) break;
				}

				int Current = 0;
				int Total = CheckList.Count;

				Parallel.ForEach(CheckList, (IsoFile) =>
				{
					try
					{
						//Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
						if (Progress != null) Progress(IsoFile, Current, Total);

						//Serializer.Serialize(Console.Out, Entry);

						var Hash = GetHash(IsoFile);
						try { Directory.CreateDirectory(CacheFolder + "/cspspemu_iso_cache"); } catch { } 
						var CacheFile = CacheFolder + "/cspspemu_iso_cache/cspspemu_iso_cache_" + Hash + ".xml";

						GameEntry Entry;
						bool Cached = false;

						if (File.Exists(CacheFile))
						{
							Entry = (GameEntry)Serializer.Deserialize(File.OpenRead(CacheFile));
							Cached = true;
						}
						else
						{
							Entry = HandleIso(IsoFile);
							using (var CacheFileStream = File.OpenWrite(CacheFile))
							{
								Serializer.Serialize(CacheFileStream, Entry);
							}
						}

						Entries.Add(Entry);

						if (EntryAdded != null && Entry != null)
						{
							EntryAdded(Entry, Cached);
						}
					}
					catch (Exception Exception)
					{
						Console.Error.WriteLine(Exception);
					}
					Current++;
				});
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
			}
			Progress("Done!", 1, 1);
		}

		public event Action<string, int, int> Progress;
		public event Action<GameEntry, bool> EntryAdded;
		static XmlSerializer Serializer = new XmlSerializer(typeof(GameEntry));

		public static String GetHash(string IsoFile)
		{
			var IsoFileInfo = new FileInfo(IsoFile);
			return BitConverter.ToString(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(IsoFileInfo.FullName + "_" + IsoFileInfo.Length))).Replace("-", "");
		}

		public GameEntry HandleIso(string IsoFile)
		{
			var IsoFileInfo = new FileInfo(IsoFile);

			var Iso = IsoLoader.GetIso(IsoFile);
			var FileSystem = new HleIoDriverIso(Iso);
		    string UmdData = string.Empty;
            if (FileSystem.FileExists("/UMD_DATA.BIN"))
                UmdData = FileSystem.OpenRead("/UMD_DATA.BIN").ReadAllContentsAsString();
		    var ParamSfo = new Psf(new MemoryStream(FileSystem.OpenRead("/PSP_GAME/PARAM.SFO").ReadAll()));
		    byte[] Icon0Png ;
            if (FileSystem.FileExists("/PSP_GAME/ICON0.PNG"))
                Icon0Png = FileSystem.OpenRead("/PSP_GAME/ICON0.PNG").ReadAll();
            else
                Icon0Png = (byte[])System.ComponentModel.TypeDescriptor.GetConverter(Properties.Resources.icon0).ConvertTo(Properties.Resources.icon0, typeof(byte[]));
			var Entries = ParamSfo.EntryDictionary;

			var Entry = new GameEntry();

			if (
				FileSystem.FileExists("/PSP_GAME/SYSDIR/prometheus.prx")
				|| FileSystem.FileExists("/PSP_GAME/SYSDIR/EBOOT.OLD")
			)
			{
				Entry.PatchedWithPrometheus = true;
			}
			else
			{
				Entry.PatchedWithPrometheus = false;
			}

			Entry.IsoSize = IsoFileInfo.Length;
			Entry.Hash = GetHash(IsoFile);
			Entry.IsoFile = IsoFile;
			Entry.DiscId0 = UmdData.Split('|')[0];
			try { Entry.APP_VER = (string)Entries["APP_VER"]; } catch { }
			try { Entry.BOOTABLE = ((int)Entries["BOOTABLE"]) != 0; } catch { }
			try { Entry.CATEGORY = (string)Entries["CATEGORY"]; } catch { }
			try { 
                Entry.DISC_ID = (string)Entries["DISC_ID"];
                if (string.IsNullOrWhiteSpace(Entry.DiscId0))
			        Entry.DiscId0 = Entry.DISC_ID.Substring(0, 4) +"-"+ Entry.DISC_ID.Substring(4);
			}
            catch { }
			try { Entry.DISC_NUMBER = (int)Entries["DISC_NUMBER"]; } catch { }
			try { Entry.DISC_TOTAL = (int)Entries["DISC_TOTAL"]; } catch { }
			try { Entry.DISC_VERSION = (string)Entries["DISC_VERSION"]; } catch { }
			try { Entry.DRIVER_PATH = (string)Entries["DRIVER_PATH"]; } catch { }
			try { Entry.GAMEDATA_ID = (string)Entries["GAMEDATA_ID"]; } catch { }
			try { Entry.HRKGMP_VER = (int)Entries["HRKGMP_VER"]; } catch { }
			try { Entry.PARENTAL_LEVEL = (int)Entries["PARENTAL_LEVEL"]; } catch { }
			try { Entry.PSP_SYSTEM_VER = (string)Entries["PSP_SYSTEM_VER"]; } catch { }
			try { Entry.REGION = (int)Entries["REGION"]; } catch { }
			try { Entry.TITLE = (string)Entries["TITLE"]; } catch { }
			try { Entry.USE_USB = ((int)Entries["USE_USB"]) != 0; } catch { }
			try { Entry.Icon0Png = Icon0Png; } catch { }

			/**
			GameId: ULJM-05753
			APP_VER : 01.00
			BOOTABLE : 1
			CATEGORY : UG
			DISC_ID : ULJM05753
			DISC_NUMBER : 1
			DISC_TOTAL : 1
			DISC_VERSION : 1.02
			DRIVER_PATH : 
			GAMEDATA_ID : ULJM05753
			HRKGMP_VER : 19
			PARENTAL_LEVEL : 5
			PSP_SYSTEM_VER : 6.31
			REGION : 32768
			TITLE : タクティクスオウガ　運命の輪
			USE_USB : 0
			*/

			return Entry;
		}
	}
}
