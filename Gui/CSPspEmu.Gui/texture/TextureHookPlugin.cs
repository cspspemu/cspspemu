using CSharpUtils;
using CSPspEmu.Core;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Core.Types;
using CSPspEmu.Inject;
using HQ2x;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Gui.texture
{
	public class TextureHookPlugin : IInjectInitialize, IDisposable
	{
		[Inject]
		MessageBus MessageBus;

		[Inject]
		PspStoredConfig PspStoredConfig;

		string LinkedTextureMapFile;

		void IInjectInitialize.Initialize()
		{
			MessageBus.Register<TextureHookInfo>(Hook);
			MessageBus.Register<LoadFileMessage>((LoadFileMessage) =>
			{
				LinkedTextureMapFile = LoadFileMessage.FileName + ".texmap";
				LoadTexMapFile();
			});
		}

		private void LoadTexMapFile()
		{
			if (File.Exists(LinkedTextureMapFile))
			{
				TexMap.Clear();
				foreach (var Line in File.ReadAllLines(LinkedTextureMapFile))
				{
					var Parts = Line.Split(new[] { " ", "\t" }, 2, StringSplitOptions.RemoveEmptyEntries);
					if (Parts.Length > 0)
					{
						var CacheHash = Convert.ToUInt64(Parts[0], 16);
						TexMap[CacheHash] = Parts[1].Trim();
					}
				}
			}
		}

		private void SaveTexMapFile()
		{
			var Lines = new List<string>();
			foreach (var Part in TexMap)
			{
				Lines.Add(String.Format("{0:X16} {1}", Part.Key, Part.Value));
			}
			File.WriteAllLines(LinkedTextureMapFile, Lines.ToArray());
		}

		Dictionary<ulong, string> TexMap = new Dictionary<ulong, string>();

		public void Dispose()
		{
			MessageBus.Unregister<TextureHookInfo>(Hook);
		}

		public void AddMapping(ulong CacheHash, string FileName)
		{
			TexMap[CacheHash] = FileName;
			SaveTexMapFile();
		}

		public void Hook(TextureHookInfo TextureInfo)
		{
			Bitmap OutBitmap = null;

			if (TexMap.ContainsKey(TextureInfo.TextureCacheKey.TextureHash))
			{
				OutBitmap = new Bitmap(Image.FromFile(TexMap[TextureInfo.TextureCacheKey.TextureHash]));
			}
			else
			{
				if (PspStoredConfig.ScaleTextures)
				{
					var InBitmap = new Bitmap(TextureInfo.Width, TextureInfo.Height).SetChannelsDataInterleaved(TextureInfo.Data.CastToStructArray<OutputPixel, byte>(), BitmapChannelList.RGBA);
					OutBitmap = (new Engine(new ColorAlphaLerp(), new ColorAlphaThreshold(32, 32, 32, 32))).Process(InBitmap);
				}
			}

			if (OutBitmap != null)
			{
				TextureInfo.Data = OutBitmap.GetChannelsDataInterleaved(BitmapChannelList.RGBA).CastToStructArray<byte, OutputPixel>();
				TextureInfo.Width = OutBitmap.Width;
				TextureInfo.Height = OutBitmap.Height;
			}
		}
	}
}
