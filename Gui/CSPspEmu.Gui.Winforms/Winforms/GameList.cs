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
using System.Drawing.Imaging;
using CSharpUtils.Extensions;

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

                foreach (var File in Directory.EnumerateFiles(Folder, "*", SearchOption.AllDirectories))
                {
                    switch (Path.GetExtension(File).ToLowerInvariant())
                    {
                        case ".iso":
                        case ".cso":
                        case ".dax":
                        case ".pbp":
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
                        Progress?.Invoke(IsoFile, Current, Total);

                        //Serializer.Serialize(Console.Out, Entry);

                        var Hash = GetHash(IsoFile);
                        try
                        {
                            Directory.CreateDirectory(CacheFolder + "/cspspemu_iso_cache");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        var CacheFile = CacheFolder + "/cspspemu_iso_cache/cspspemu_iso_cache_" + Hash + ".xml";

                        GameEntry Entry;
                        bool Cached = false;

                        if (File.Exists(CacheFile))
                        {
                            Entry = (GameEntry) Serializer.Deserialize(File.OpenRead(CacheFile));
                            Cached = true;
                        }
                        else
                        {
                            Entry = HandleIso(IsoFile);
                            if (Entry == null) return;

                            using (var CacheFileStream = File.OpenWrite(CacheFile))
                            {
                                Serializer.Serialize(CacheFileStream, Entry);
                            }
                        }

                        lock (Entries)
                        {
                            Entries.Add(Entry);

                            if (EntryAdded != null && Entry != null)
                            {
                                EntryAdded(Entry, Cached);
                            }
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

        public static string GetHash(string IsoFile)
        {
            var IsoFileInfo = new FileInfo(IsoFile);
            return BitConverter
                .ToString(SHA1.Create()
                    .ComputeHash(Encoding.UTF8.GetBytes(IsoFileInfo.FullName + "_" + IsoFileInfo.Length)))
                .Replace("-", "");
        }

        public static byte[] DefaultIcon;

        public static GameEntry HandleIso(string IsoFile)
        {
            var IsoFileInfo = new FileInfo(IsoFile);
            Psf ParamSfo;
            var Entry = new GameEntry();
            byte[] Icon0Png;
            string UmdData = string.Empty;

            if (DefaultIcon == null)
            {
                var TempMemoryStream = new MemoryStream();
                Properties.Resources.icon0.Save(TempMemoryStream, ImageFormat.Png);
                DefaultIcon = TempMemoryStream.ToArray();
            }

            using (var IsoStream = File.OpenRead(IsoFile))
            {
                switch (new FormatDetector().DetectSubType(IsoStream))
                {
                    case FormatDetector.SubType.Pbp:
                        var PBP = new Pbp().Load(File.OpenRead(IsoFile));
                        ParamSfo = new Psf(PBP[Pbp.Types.ParamSfo]);

                        Icon0Png = PBP.ContainsKey(Pbp.Types.Icon0Png)
                            ? PBP[Pbp.Types.Icon0Png].ReadAll()
                            : DefaultIcon;
                        UmdData = "---";

                        break;
                    case FormatDetector.SubType.Iso:
                    case FormatDetector.SubType.Cso:
                    case FormatDetector.SubType.Dax:
                        using (var Iso = IsoLoader.GetIso(IsoFile))
                        {
                            var FileSystem = new HleIoDriverIso(Iso);

                            if (!FileSystem.FileExists("/PSP_GAME/PARAM.SFO"))
                            {
                                throw (new Exception($"Not a PSP ISO '{IsoFile}'"));
                            }

                            ParamSfo = new Psf(new MemoryStream(FileSystem.OpenRead("/PSP_GAME/PARAM.SFO").ReadAll()));

                            if (FileSystem.FileExists("/UMD_DATA.BIN"))
                                UmdData = FileSystem.OpenRead("/UMD_DATA.BIN").ReadAllContentsAsString();
                            Icon0Png = FileSystem.FileExists("/PSP_GAME/ICON0.PNG")
                                ? FileSystem.OpenRead("/PSP_GAME/ICON0.PNG").ReadAll()
                                : DefaultIcon;
                            Entry.PatchedWithPrometheus =
                                FileSystem.FileExists("/PSP_GAME/SYSDIR/prometheus.prx") ||
                                FileSystem.FileExists("/PSP_GAME/SYSDIR/EBOOT.OLD");
                        }
                        break;
                    default: return null;
                }
            }

            FillGameEntryFromSfo(Entry, ParamSfo);
            Entry.IsoSize = IsoFileInfo.Length;
            Entry.Hash = GetHash(IsoFile);
            Entry.IsoFile = IsoFile;
            Entry.DiscId0 = UmdData.Split('|')[0];
            Entry.Icon0Png = Icon0Png;
            return Entry;
        }

        private static void FillGameEntryFromSfo(GameEntry Entry, Psf ParamSfo)
        {
            var Entries = ParamSfo.EntryDictionary;
            Entry.APP_VER = (string) Entries.GetOrDefault("APP_VER", "01.00");
            Entry.BOOTABLE = (int) Entries.GetOrDefault("BOOTABLE", 1) != 0;
            Entry.CATEGORY = (string) Entries.GetOrDefault("CATEGORY", "UG");
            Entry.DISC_ID = (string) Entries.GetOrDefault("DISC_ID", "XXXX99999");
            if (string.IsNullOrWhiteSpace(Entry.DiscId0))
                Entry.DiscId0 = Entry.DISC_ID.Substring(0, 4) + "-" + Entry.DISC_ID.Substring(4);
            Entry.DISC_NUMBER = (int) Entries.GetOrDefault("DISC_NUMBER", 1);
            Entry.DISC_TOTAL = (int) Entries.GetOrDefault("DISC_TOTAL", 1);
            Entry.DISC_VERSION = (string) Entries.GetOrDefault("DISC_VERSION", "1.00");
            Entry.DRIVER_PATH = (string) Entries.GetOrDefault("DRIVER_PATH", "");
            Entry.GAMEDATA_ID = (string) Entries.GetOrDefault("GAMEDATA_ID", "XXXX99999");
            Entry.HRKGMP_VER = (int) Entries.GetOrDefault("HRKGMP_VER", 19);
            Entry.PARENTAL_LEVEL = (int) Entries.GetOrDefault("PARENTAL_LEVEL", 5);
            Entry.PSP_SYSTEM_VER = (string) Entries.GetOrDefault("PSP_SYSTEM_VER", "1.00");
            Entry.REGION = (int) Entries.GetOrDefault("REGION", 32768);
            Entry.TITLE = (string) Entries.GetOrDefault("TITLE", "Unknown Title");
            Entry.USE_USB = ((int) Entries.GetOrDefault("USE_USB", 0)) != 0;
        }
    }
}