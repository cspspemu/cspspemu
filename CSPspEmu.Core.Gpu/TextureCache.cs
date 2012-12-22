//#define DEBUG_TEXTURE_CACHE

using System;
using System.Collections.Generic;
using System.Drawing;
using CSharpUtils;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Gpu.State.SubStates;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Types;
using CSPspEmu.Core.Utils;

namespace CSPspEmu.Core.Gpu
{
	public unsafe abstract class Texture<TGpuImpl> : IDisposable
	{
		//public int TextureId { get; private set; }
		public ulong TextureHash { get { return TextureCacheKey.TextureHash; } }

		public TGpuImpl GpuImpl;
		public DateTime RecheckTimestamp;
		public TextureCacheKey TextureCacheKey;
		public int Width;
		public int Height;
		protected OutputPixel[] Data;

		public Texture()
		{
		}

		public Texture<TGpuImpl> Init(TGpuImpl GpuImpl)
		{
			this.GpuImpl = GpuImpl;
			Init();
			return this;
		}

		protected virtual void Init()
		{
		}

		public void Save(string File)
		{
			var Bitmap = new Bitmap(this.Width, this.Height);
			fixed (OutputPixel* DataPtr = Data)
			{
				BitmapUtils.TransferChannelsDataInterleaved(
					Bitmap.GetFullRectangle(),
					Bitmap,
					(byte*)DataPtr,
					BitmapUtils.Direction.FromDataToBitmap,
					BitmapChannel.Red,
					BitmapChannel.Green,
					BitmapChannel.Blue,
					BitmapChannel.Alpha
				);
			}
			Bitmap.Save(File);
		}

		public Texture<TGpuImpl> Load(string FileName)
		{
			var Bitmap = new Bitmap(Image.FromFile(FileName));
			Bitmap.LockBitsUnlock(System.Drawing.Imaging.PixelFormat.Format32bppArgb, (BitmapData) =>
			{
				this.SetData((OutputPixel *)BitmapData.Scan0, BitmapData.Width, BitmapData.Height);
			});
			return this;
		}

		public abstract bool SetData(OutputPixel* Pixels, int TextureWidth, int TextureHeight);
		public abstract void Bind();
		public abstract void Dispose();

		public override string ToString()
		{
			return this.ToStringDefault();
		}
	}

	public struct TextureCacheKey
	{
		public uint TextureAddress;
		public ulong TextureHash;
		public GuPixelFormats TextureFormat;

		public uint ClutAddress;
		public ulong ClutHash;
		public GuPixelFormats ClutFormat;
		public int ClutStart;
		public int ClutShift;
		public int ClutMask;
		public bool Swizzled;
		public bool ColorTestEnabled;
		public OutputPixel ColorTestRef;
		public OutputPixel ColorTestMask;
		public ColorTestFunctionEnum ColorTestFunction;

		public override string ToString()
		{
			return this.ToStringDefault();
		}
	}

	public unsafe class TextureCache<TGpuImpl, TTexture> where TTexture : Texture<TGpuImpl>, new()
	{
		PspMemory PspMemory;
		//Dictionary<TextureCacheKey, Texture> Cache = new Dictionary<TextureCacheKey, Texture>();
		Dictionary<ulong, TTexture> Cache = new Dictionary<ulong, TTexture>();
		public TGpuImpl GpuImpl;

		byte[] SwizzlingBuffer = new byte[4 * 1024 * 1024];
		OutputPixel[] DecodedTextureBuffer = new OutputPixel[1024 * 1024];

		public TextureCache(PspMemory PspMemory, TGpuImpl GpuImpl)
		{
			this.PspMemory = PspMemory;
			this.GpuImpl = GpuImpl;
		}

		TTexture InvalidTexture;

		public TTexture Get(GpuStateStruct* GpuState)
		{
			var TextureMappingState = &GpuState->TextureMappingState;
			var ClutState = &TextureMappingState->ClutState;
			var TextureState = &TextureMappingState->TextureState;

			TTexture Texture;
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

				// FAKE!
				//BufferWidth = TextureState->Mipmap0.TextureWidth;

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


				if (!PspMemory.IsRangeValid(TextureAddress, TextureDataSize) || TextureDataSize > 2048 * 2048 * 4)
				{
					Console.Error.WriteLine("UPDATE_TEXTURE(TEX={0},CLUT={1}:{2}:{3}:{4}:0x{5:X},SIZE={6}x{7},{8},Swizzled={9})", TextureFormat, ClutFormat, ClutCount, ClutStart, ClutShift, ClutMask, BufferWidth, Height, BufferWidth, Swizzled);
					Console.Error.WriteLine("Invalid TEXTURE! TextureAddress=0x{0:X}, TextureDataSize={1}", TextureAddress, TextureDataSize);
					Console.Error.WriteLine("Invalid TEXTURE!");
					if (InvalidTexture == null)
					{
						InvalidTexture = new TTexture();
						InvalidTexture.Init(GpuImpl);

						int ITWidth = 2, ITHeight = 2;
						int ITWidthHeight = ITWidth * ITHeight;
						fixed (OutputPixel* Data = new OutputPixel[ITWidthHeight])
						{
							var Color1 = OutputPixel.FromRGBA(0xFF, 0x00, 0x00, 0xFF);
							var Color2 = OutputPixel.FromRGBA(0x00, 0x00, 0xFF, 0xFF);
							for (int n = 0; n < ITWidthHeight; n++)
							{
								Data[n] = ((n & 1) != 0) ? Color1 : Color2;
							}
							InvalidTexture.SetData(Data, ITWidth, ITHeight);
						}
					}
					return InvalidTexture;
				}

				//Console.WriteLine("TextureAddress=0x{0:X}, TextureDataSize=0x{1:X}", TextureAddress, TextureDataSize);

				TextureCacheKey TextureCacheKey = new TextureCacheKey()
				{
					TextureAddress = TextureAddress,
					TextureFormat = TextureFormat,
					TextureHash = FastHash(TexturePointer, TextureDataSize),

					ClutHash = FastHash(&(ClutPointer[ClutDataStart]), ClutDataSize),
					ClutAddress = ClutAddress,
					ClutFormat = ClutFormat,
					ClutStart = ClutStart,
					ClutShift = ClutShift,
					ClutMask = ClutMask,
					Swizzled = Swizzled,

					ColorTestEnabled = GpuState->ColorTestState.Enabled,
					ColorTestRef = GpuState->ColorTestState.Ref,
					ColorTestMask = GpuState->ColorTestState.Mask,
					ColorTestFunction = GpuState->ColorTestState.Function,
				};

				if (Texture == null || (!Texture.TextureCacheKey.Equals(TextureCacheKey)))
				{
					string TextureName = "texture_" + TextureCacheKey.TextureHash + "_" + TextureCacheKey.ClutHash + "_" + TextureFormat + "_" + ClutFormat + "_" + BufferWidth + "x" + Height + "_" + Swizzled;
#if DEBUG_TEXTURE_CACHE

					Console.Error.WriteLine("UPDATE_TEXTURE(TEX={0},CLUT={1}:{2}:{3}:{4}:0x{5:X},SIZE={6}x{7},{8},Swizzled={9})", TextureFormat, ClutFormat, ClutCount, ClutStart, ClutShift, ClutMask, BufferWidth, Height, BufferWidth, Swizzled);
#endif
					Texture = new TTexture();
					Texture.Init(GpuImpl);
					Texture.TextureCacheKey = TextureCacheKey;
					{
						//int TextureWidth = Math.Max(BufferWidth, Height);
						//int TextureHeight = Math.Max(BufferWidth, Height);
						int TextureWidth = BufferWidth;
						int TextureHeight = Height;
						int TextureWidthHeight = TextureWidth * TextureHeight;

						fixed (OutputPixel* TexturePixelsPointer = DecodedTextureBuffer)
						{
							if (Swizzled)
							{
								fixed (byte* SwizzlingBufferPointer = SwizzlingBuffer)
								{
									PointerUtils.Memcpy(SwizzlingBuffer, TexturePointer, TextureDataSize);
									PixelFormatDecoder.UnswizzleInline(TextureFormat, (void*)SwizzlingBufferPointer, BufferWidth, Height);
									PixelFormatDecoder.Decode(
										TextureFormat, (void*)SwizzlingBufferPointer, TexturePixelsPointer, BufferWidth, Height,
										ClutPointer, ClutFormat, ClutCount, ClutStart, ClutShift, ClutMask, StrideWidth: PixelFormatDecoder.GetPixelsSize(TextureFormat, TextureWidth)
									);
								}
							}
							else
							{
								PixelFormatDecoder.Decode(
									TextureFormat, (void*)TexturePointer, TexturePixelsPointer, BufferWidth, Height,
									ClutPointer, ClutFormat, ClutCount, ClutStart, ClutShift, ClutMask, StrideWidth: PixelFormatDecoder.GetPixelsSize(TextureFormat, TextureWidth)
								);
							}

							if (TextureCacheKey.ColorTestEnabled)
							{
								byte EqualValue, NotEqualValue;

								switch (TextureCacheKey.ColorTestFunction)
								{
									case ColorTestFunctionEnum.GU_ALWAYS: EqualValue = 0xFF; NotEqualValue = 0xFF; break;
									case ColorTestFunctionEnum.GU_NEVER: EqualValue = 0x00; NotEqualValue = 0x00; break;
									case ColorTestFunctionEnum.GU_EQUAL: EqualValue = 0xFF; NotEqualValue = 0x00; break;
									case ColorTestFunctionEnum.GU_NOTEQUAL: EqualValue = 0x00; NotEqualValue = 0xFF; break;
									default: throw(new NotImplementedException());
								}

								ConsoleUtils.SaveRestoreConsoleState(() =>
								{
									Console.BackgroundColor = ConsoleColor.Red;
									Console.ForegroundColor = ConsoleColor.Yellow;
									Console.Error.WriteLine("{0} : {1}, {2} : ref:{3} : mask:{4}", TextureCacheKey.ColorTestFunction, EqualValue, NotEqualValue, TextureCacheKey.ColorTestRef, TextureCacheKey.ColorTestMask);
								});

								for (int n = 0; n < TextureWidthHeight; n++)
								{
									if ((TexturePixelsPointer[n] & TextureCacheKey.ColorTestMask).Equals((TextureCacheKey.ColorTestRef & TextureCacheKey.ColorTestMask)))
									{
										TexturePixelsPointer[n].A = EqualValue;
									}
									else
									{
										TexturePixelsPointer[n].A = NotEqualValue;
									}
									if (TexturePixelsPointer[n].A == 0)
									{
										//Console.Write("yup!");
									}
								}
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

							var Result = Texture.SetData(TexturePixelsPointer, TextureWidth, TextureHeight);
#if DEBUG_TEXTURE_CACHE
							if (!Result || Texture.TextureId == 0)
							{
								var Bitmap2 = new Bitmap(BufferWidth, Height);
								BitmapUtils.TransferChannelsDataInterleaved(
									Bitmap2.GetFullRectangle(),
									Bitmap2,
									(byte*)TexturePixelsPointer,
									BitmapUtils.Direction.FromDataToBitmap,
									BitmapChannel.Red,
									BitmapChannel.Green,
									BitmapChannel.Blue,
									BitmapChannel.Alpha
								);
								string TextureName2 = @"C:\projects\csharp\cspspemu\__invalid_texture_" + TextureCacheKey.TextureHash + "_" + TextureCacheKey.ClutHash + "_" + TextureFormat + "_" + ClutFormat + "_" + BufferWidth + "x" + Height;
								Bitmap.Save(TextureName2 + ".png");
							}
#endif
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
			RecheckTimestamp = DateTime.UtcNow;
		}

		public static ulong FastHash(byte* Pointer, int Count, ulong StartHash = 0)
		{
			return Hashing.FastHash(Pointer, Count, StartHash);
		}
	}
}
