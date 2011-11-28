using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Gpu.State.SubStates;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Utils;
using OpenTK.Graphics.OpenGL;

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	sealed unsafe public class Texture : IDisposable
	{
		protected int TextureId;

		public bool Recheck;
		public TextureCacheKey TextureCacheKey;

		public Texture()
		{
			TextureId = GL.GenTexture();
		}

		~Texture()
		{
			Dispose();
		}

		public void SetData(PixelFormatDecoder.OutputPixel *Pixels, int Width, int Height)
		{
			Bind();
			//GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedInt8888, new IntPtr(Pixels));
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedInt8888Reversed, new IntPtr(Pixels));


			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

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
				//GL.DeleteTexture(TextureId);
				TextureId = 0;
			}
		}
	}

	public struct TextureCacheKey
	{
		public uint TextureAddress;
		public uint TextureHash;
		public PixelFormats TextureFormat;

		public uint ClutAddress;
		public uint ClutHash;
		public PixelFormats ClutFormat;
		public int ClutStart;
		public int ClutShift;
		public int ClutMask;
	}

	sealed unsafe public class TextureCache : PspEmulatorComponent
	{
		PspMemory PspMemory;
		//Dictionary<TextureCacheKey, Texture> Cache = new Dictionary<TextureCacheKey, Texture>();
		Dictionary<ulong, Texture> Cache = new Dictionary<ulong, Texture>();

		PixelFormatDecoder.OutputPixel[] TempBuffer = new PixelFormatDecoder.OutputPixel[1024 * 1024];

		public TextureCache(PspEmulatorContext PspEmulatorContext) : base(PspEmulatorContext)
		{
			PspMemory = PspEmulatorContext.GetInstance<PspMemory>();
		}

		public Texture Get(TextureStateStruct* TextureState, ClutStateStruct* ClutState)
		{
			Texture Texture;
			//GC.Collect();
			uint TextureAddress = TextureState[0].Mipmap0.Address;
			uint ClutAddress = ClutState[0].Address;
			var ClutFormat = ClutState[0].PixelFormat;
			var PaletteStart = ClutState[0].Start;
			var ClutDataStart = PixelFormatDecoder.GetPixelsSize(ClutFormat, PaletteStart);

			ulong Hash1 = TextureAddress | (ulong)((ClutAddress + ClutDataStart) << 32);
			bool Recheck = false;
			if (Cache.TryGetValue(Hash1, out Texture))
			{
				if (Texture.Recheck)
				{
					Recheck = Texture.Recheck;
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
				var TextureFormat = TextureState[0].PixelFormat;
				var Width = TextureState[0].Mipmap0.Width;
				var Height = TextureState[0].Mipmap0.Height;
				var TextureDataSize = PixelFormatDecoder.GetPixelsSize(TextureFormat, Width * Height);
				var ClutDataSize = PixelFormatDecoder.GetPixelsSize(ClutFormat, ClutState[0].NumberOfColors);
				var ClutCount = ClutState[0].NumberOfColors;
				var ClutShift = ClutState[0].Shift;
				var ClutMask = ClutState[0].Mask;

				TextureCacheKey TextureCacheKey = new TextureCacheKey()
				{
					TextureAddress = TextureAddress,
					TextureFormat = TextureFormat,
					TextureHash = FastHash((uint*)TexturePointer, TextureDataSize),

					ClutHash = FastHash((uint*)&(ClutPointer[ClutDataStart]), ClutDataSize),
					ClutAddress = ClutAddress,
					ClutFormat = ClutFormat,
					ClutStart = PaletteStart,
					ClutShift = ClutShift,
					ClutMask = ClutMask,
				};

				if (Texture == null || (!Texture.TextureCacheKey.Equals(TextureCacheKey)))
				{
					//Console.Write("{0},", ClutFormat);
					Texture = new Texture();
					Texture.TextureCacheKey = TextureCacheKey;
					{
						fixed (PixelFormatDecoder.OutputPixel* TexturePixelsPointer = TempBuffer)
						{
							PixelFormatDecoder.Decode(
								TextureFormat, (void*)TexturePointer, TexturePixelsPointer, Width * Height, Width,
								ClutPointer, ClutFormat, ClutCount, PaletteStart, ClutShift, ClutMask
							);
							Texture.SetData(TexturePixelsPointer, Width, Height);
						}
					}
					Cache[Hash1] = Texture;
				}
			}

			Texture.Recheck = false;

			return Texture;
		}

		public void RecheckAll()
		{
			foreach (var Texture in Cache.Values)
			{
				Texture.Recheck = true;
			}
		}

		static public uint FastHash(uint* Pointer, int Count, uint StartHash = 0)
		{
			Count /= 4;
			uint Hash = StartHash;
			for (int n = 0; n < Count; n++)
			{
				Hash ^= (uint)(Pointer[n] + (n << 16));
			}
			return Hash;
		}
	}
}
