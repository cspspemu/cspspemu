using CSPspEmu.Core;
using CSPspEmu.Core.Gpu.Impl.Opengl;
using CSPspEmu.Core.Types;
using CSPspEmu.Inject;
using HQ2x;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using CSharpUtils.Drawing;
using CSharpUtils.Drawing.Extensions;

namespace CSPspEmu.Gui.texture
{
    public class TextureHookPlugin : IInjectInitialize, IDisposable
    {
        [Inject] MessageBus MessageBus;

        [Inject] PspStoredConfig PspStoredConfig;

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
                    var Parts = Line.Split(new[] {" ", "\t"}, 2, StringSplitOptions.RemoveEmptyEntries);
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
                Lines.Add($"{Part.Key:X16} {Part.Value}");
            }
            File.WriteAllLines(LinkedTextureMapFile, Lines.ToArray());
        }

        Dictionary<ulong, string> TexMap = new Dictionary<ulong, string>();

        public void Dispose()
        {
            MessageBus.Unregister<TextureHookInfo>(Hook);
        }

        public void AddMapping(ulong cacheHash, string fileName)
        {
            TexMap[cacheHash] = fileName;
            SaveTexMapFile();
        }

        public void Hook(TextureHookInfo textureInfo)
        {
            Bitmap OutBitmap = null;

            if (TexMap.ContainsKey(textureInfo.TextureCacheKey.TextureHash))
            {
                OutBitmap = new Bitmap(Image.FromFile(TexMap[textureInfo.TextureCacheKey.TextureHash]));
            }
            else
            {
                if (PspStoredConfig.ScaleTextures)
                {
                    var inBitmap =
                        new Bitmap(textureInfo.Width, textureInfo.Height).SetChannelsDataInterleaved(
                            textureInfo.Data.CastToStructArray<OutputPixel, byte>(), BitmapChannelList.Rgba);
                    OutBitmap =
                        (new Engine(new ColorAlphaLerp(), new ColorAlphaThreshold(32, 32, 32, 32))).Process(inBitmap);
                }
            }

            if (OutBitmap != null)
            {
                textureInfo.Data = OutBitmap.GetChannelsDataInterleaved(BitmapChannelList.Rgba)
                    .CastToStructArray<byte, OutputPixel>();
                textureInfo.Width = OutBitmap.Width;
                textureInfo.Height = OutBitmap.Height;
            }
        }
    }
}