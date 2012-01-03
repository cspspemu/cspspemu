//#define DEBUG_TEXTURE_CACHE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CSharpUtils;
using CSPspEmu.Core.Gpu.State.SubStates;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Utils;
using OpenTK.Graphics.OpenGL;
using CSharpUtils.Extensions;
using System.Drawing;
using System.Runtime.ExceptionServices;

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	sealed unsafe public class Texture : IDisposable
	{
		private int TextureId;

		public DateTime RecheckTimestamp;
		public TextureCacheKey TextureCacheKey;

		public Texture()
		{
			TextureId = GL.GenTexture();
		}

		public void SetData(PixelFormatDecoder.OutputPixel *Pixels, int Width, int Height)
		{
			Bind();
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedInt8888Reversed, new IntPtr(Pixels));

			//glTexEnvf(GL_TEXTURE_ENV, GL_RGB_SCALE, 1.0); // 2.0 in scale_2x
			//GL.TexEnv(TextureEnvTarget.TextureEnv, GL_TEXTURE_ENV_MODE, TextureEnvModeTranslate[state.texture.effect]);

		}

		public void Bind()
		{
			GL.BindTexture(TextureTarget.Texture2D, TextureId);
		}

		public void Dispose()
		{
			if (TextureId != 0)
			{
				GL.DeleteTexture(TextureId);
				TextureId = 0;
			}
		}
	}

	public struct TextureCacheKey
	{
		public uint TextureAddress;
		public uint TextureHash;
		public GuPixelFormats TextureFormat;

		public uint ClutAddress;
		public uint ClutHash;
		public GuPixelFormats ClutFormat;
		public int ClutStart;
		public int ClutShift;
		public int ClutMask;
		public bool Swizzled;
	}

	sealed unsafe public class TextureCache : PspEmulatorComponent
	{
		PspMemory PspMemory;
		//Dictionary<TextureCacheKey, Texture> Cache = new Dictionary<TextureCacheKey, Texture>();
		Dictionary<ulong, Texture> Cache = new Dictionary<ulong, Texture>();

		byte[] SwizzlingBuffer = new byte[1024 * 1024 * 4];
		PixelFormatDecoder.OutputPixel[] DecodedTextureBuffer = new PixelFormatDecoder.OutputPixel[1024 * 1024];

		public override void InitializeComponent()
		{
			PspMemory = PspEmulatorContext.GetInstance<PspMemory>();
		}

		public Texture Get(TextureStateStruct* TextureState, ClutStateStruct* ClutState)
		{
			Texture Texture;
			//GC.Collect();
			bool Swizzled = TextureState->Swizzled;
			uint TextureAddress = TextureState->Mipmap0.Address;
			uint ClutAddress = ClutState->Address;
			var ClutFormat = ClutState->PixelFormat;
			var ClutStart = ClutState->Start;
			var ClutDataStart = PixelFormatDecoder.GetPixelsSize(ClutFormat, ClutStart);

			ulong Hash1 = TextureAddress | (ulong)((ClutAddress + ClutDataStart) << 32);
			bool Recheck = false;
			if (Cache.TryGetValue(Hash1, out Texture))
			{
				if (Texture.RecheckTimestamp != RecheckTimestamp)
				{
					Recheck = true;
				}
			}
			else
			{
				Recheck = true;
			}

			if (Recheck)
			{
				//Console.Write(".");

				//Console.WriteLine("{0:X}", ClutAddress);
				var TexturePointer = (byte*)PspMemory.PspAddressToPointerSafe(TextureAddress);
				var ClutPointer = (byte *)PspMemory.PspAddressToPointerSafe(ClutAddress);
				var TextureFormat = TextureState->PixelFormat;
				//var Width = TextureState->Mipmap0.TextureWidth;
				int BufferWidth = TextureState->Mipmap0.BufferWidth;
				var Height = TextureState->Mipmap0.TextureHeight;
				var TextureDataSize = PixelFormatDecoder.GetPixelsSize(TextureFormat, BufferWidth * Height);
				if (ClutState->NumberOfColors > 256)
				{
					ClutState->NumberOfColors = 256;
				}
				var ClutDataSize = PixelFormatDecoder.GetPixelsSize(ClutFormat, ClutState->NumberOfColors);
				var ClutCount = ClutState->NumberOfColors;
				var ClutShift = ClutState->Shift;
				var ClutMask = ClutState->Mask;

				//Console.WriteLine(TextureFormat);

				if (TextureDataSize > 2048 * 2048 * 4)
				{
					Console.Error.WriteLine("UPDATE_TEXTURE(TEX={0},CLUT={1}:{2}:{3}:{4}:0x{5:X},SIZE={6}x{7},{8},Swizzled={9})", TextureFormat, ClutFormat, ClutCount, ClutStart, ClutShift, ClutMask, BufferWidth, Height, BufferWidth, Swizzled);
					Console.Error.WriteLine("Invalid TEXTURE!");
					return new Texture();
				}

				TextureCacheKey TextureCacheKey = new TextureCacheKey()
				{
					TextureAddress = TextureAddress,
					TextureFormat = TextureFormat,
					TextureHash = FastHash((uint*)TexturePointer, TextureDataSize),

					ClutHash = FastHash((uint*)&(ClutPointer[ClutDataStart]), ClutDataSize),
					ClutAddress = ClutAddress,
					ClutFormat = ClutFormat,
					ClutStart = ClutStart,
					ClutShift = ClutShift,
					ClutMask = ClutMask,
					Swizzled = Swizzled,
				};

				if (Texture == null || (!Texture.TextureCacheKey.Equals(TextureCacheKey)))
				{
#if DEBUG_TEXTURE_CACHE
					string TextureName = "texture_" + TextureCacheKey.TextureHash + "_" + TextureCacheKey.ClutHash + "_" + TextureFormat + "_" + ClutFormat + "_" + BufferWidth + "x" + Height;

					Console.Error.WriteLine("UPDATE_TEXTURE(TEX={0},CLUT={1}:{2}:{3}:{4}:0x{5:X},SIZE={6}x{7},{8},Swizzled={9})", TextureFormat, ClutFormat, ClutCount, ClutStart, ClutShift, ClutMask, BufferWidth, Height, BufferWidth, Swizzled);
#endif
					Texture = new Texture();
					Texture.TextureCacheKey = TextureCacheKey;
					{
						fixed (PixelFormatDecoder.OutputPixel* TexturePixelsPointer = DecodedTextureBuffer)
						{
							if (Swizzled)
							{
								fixed (byte* SwizzlingBufferPointer = SwizzlingBuffer)
								{
									Marshal.Copy(new IntPtr(TexturePointer), SwizzlingBuffer, 0, TextureDataSize);
									PixelFormatDecoder.UnswizzleInline(TextureFormat, (void*)SwizzlingBufferPointer, BufferWidth, Height);
									PixelFormatDecoder.Decode(
										TextureFormat, (void*)SwizzlingBufferPointer, TexturePixelsPointer, BufferWidth, Height,
										ClutPointer, ClutFormat, ClutCount, ClutStart, ClutShift, ClutMask
									);
								}
							}
							else
							{
								PixelFormatDecoder.Decode(
									TextureFormat, (void*)TexturePointer, TexturePixelsPointer, BufferWidth, Height,
									ClutPointer, ClutFormat, ClutCount, ClutStart, ClutShift, ClutMask
								);
							}
							
#if DEBUG_TEXTURE_CACHE
							var Bitmap = new Bitmap(BufferWidth, Height);
							BitmapUtils.TransferChannelsDataInterleaved(
								Bitmap.GetFullRectangle(),
								Bitmap,
								(byte*)TexturePixelsPointer,
								BitmapUtils.Direction.FromDataToBitmap,
								BitmapChannel.Red,
								BitmapChannel.Green,
								BitmapChannel.Blue,
								BitmapChannel.Alpha
							);
							Bitmap.Save(TextureName + ".png");
#endif

							Texture.SetData(TexturePixelsPointer, BufferWidth, Height);
						}
					}
					if (Cache.ContainsKey(Hash1))
					{
						Cache[Hash1].Dispose();
					}
					Cache[Hash1] = Texture;
				}
			}

			Texture.RecheckTimestamp = RecheckTimestamp;

			return Texture;
		}

		private DateTime RecheckTimestamp = DateTime.MinValue;

		public void RecheckAll()
		{
			RecheckTimestamp = DateTime.Now;
		}

		static public uint FastHash(uint* Pointer, int Count, uint StartHash = 0)
		{
			return Hashing.FastHash(Pointer, Count, StartHash);
		}
	}
}
