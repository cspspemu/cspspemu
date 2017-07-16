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
using CSPspEmu.Inject;
using CSPspEmu.Core.Gpu.Impl.Opengl;

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

		public Texture<TGpuImpl> Init(TGpuImpl gpuImpl)
		{
			GpuImpl = gpuImpl;
			Init();
			return this;
		}

		protected virtual void Init()
		{
		}

		public void Save(string file)
		{
			var bitmap = new Bitmap(this.Width, this.Height);
			fixed (OutputPixel* dataPtr = Data)
			{
				BitmapUtils.TransferChannelsDataInterleaved(
					bitmap.GetFullRectangle(),
					bitmap,
					(byte*)dataPtr,
					BitmapUtils.Direction.FromDataToBitmap,
					BitmapChannel.Red,
					BitmapChannel.Green,
					BitmapChannel.Blue,
					BitmapChannel.Alpha
				);
			}
			bitmap.Save(file);
		}

		public Texture<TGpuImpl> Load(string fileName)
		{
			var bitmap = new Bitmap(Image.FromFile(fileName));
			SetData(bitmap.GetChannelsDataInterleaved(BitmapChannelList.ARGB).CastToStructArray<OutputPixel>(), bitmap.Width, bitmap.Height);
			return this;
		}

		public abstract bool SetData(OutputPixel[] pixels, int textureWidth, int textureHeight);
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
		private PspMemory PspMemory;
		public readonly Dictionary<ulong, TTexture> Cache = new Dictionary<ulong, TTexture>();
		public TGpuImpl GpuImpl;

		private byte[] SwizzlingBuffer = new byte[4 * 1024 * 1024];
		private OutputPixel[] DecodedTextureBuffer = new OutputPixel[1024 * 1024];

		MessageBus MessageBus;

		public TextureCache(PspMemory pspMemory, TGpuImpl gpuImpl, InjectContext injectContext)
		{
			PspMemory = pspMemory;
			GpuImpl = gpuImpl;
			MessageBus = injectContext.GetInstance<MessageBus>();
		}

		TTexture _invalidTexture;

		public TTexture Get(GpuStateStruct* gpuState)
		{
			var textureMappingState = &gpuState->TextureMappingState;
			var clutState = &textureMappingState->ClutState;
			var textureState = &textureMappingState->TextureState;

			TTexture texture;
			//GC.Collect();
			bool swizzled = textureState->Swizzled;
			uint textureAddress = textureState->Mipmap0.Address;
			uint clutAddress = clutState->Address;
			var clutFormat = clutState->PixelFormat;
			var clutStart = clutState->Start;
			var clutDataStart = PixelFormatDecoder.GetPixelsSize(clutFormat, clutStart);

			ulong hash1 = textureAddress | (ulong)((clutAddress + clutDataStart) << 32);
			bool recheck = false;
			if (Cache.TryGetValue(hash1, out texture))
			{
				if (texture.RecheckTimestamp != _recheckTimestamp)
				{
					recheck = true;
				}
			}
			else
			{
				recheck = true;
			}

			if (recheck)
			{
				//Console.Write(".");

				//Console.WriteLine("{0:X}", ClutAddress);

				var textureFormat = textureState->PixelFormat;
				//var Width = TextureState->Mipmap0.TextureWidth;

				int bufferWidth = textureState->Mipmap0.BufferWidth;

				// FAKE!
				//BufferWidth = TextureState->Mipmap0.TextureWidth;

				var height = textureState->Mipmap0.TextureHeight;
				var textureDataSize = PixelFormatDecoder.GetPixelsSize(textureFormat, bufferWidth * height);
				if (clutState->NumberOfColors > 256)
				{
					clutState->NumberOfColors = 256;
				}
				var clutDataSize = PixelFormatDecoder.GetPixelsSize(clutFormat, clutState->NumberOfColors);
				var clutCount = clutState->NumberOfColors;
				var clutShift = clutState->Shift;
				var clutMask = clutState->Mask;

				//Console.WriteLine(TextureFormat);

				// INVALID TEXTURE
				if (!PspMemory.IsRangeValid(textureAddress, textureDataSize) || textureDataSize > 2048 * 2048 * 4)
				{
					Console.Error.WriteLineColored(ConsoleColor.DarkRed, "UPDATE_TEXTURE(TEX={0},CLUT={1}:{2}:{3}:{4}:0x{5:X},SIZE={6}x{7},{8},Swizzled={9})", textureFormat, clutFormat, clutCount, clutStart, clutShift, clutMask, bufferWidth, height, bufferWidth, swizzled);
					Console.Error.WriteLineColored(ConsoleColor.DarkRed, "Invalid TEXTURE! TextureAddress=0x{0:X}, TextureDataSize={1}", textureAddress, textureDataSize);
					if (_invalidTexture == null)
					{
						_invalidTexture = new TTexture();
						_invalidTexture.Init(GpuImpl);

						int invalidTextureWidth = 2, InvalidTextureHeight = 2;
						int invalidTextureSize = invalidTextureWidth * InvalidTextureHeight;
						var data = new OutputPixel[invalidTextureSize];
						fixed (OutputPixel* dataPtr = data)
						{
							var color1 = OutputPixel.FromRGBA(0xFF, 0x00, 0x00, 0xFF);
							var color2 = OutputPixel.FromRGBA(0x00, 0x00, 0xFF, 0xFF);
							for (var n = 0; n < invalidTextureSize; n++)
							{
								dataPtr[n] = ((n & 1) != 0) ? color1 : color2;
							}
							_invalidTexture.SetData(data, invalidTextureWidth, InvalidTextureHeight);
						}
					}
					return _invalidTexture;
				}

				//Console.WriteLine("TextureAddress=0x{0:X}, TextureDataSize=0x{1:X}", TextureAddress, TextureDataSize);

				var texturePointer = (byte*)PspMemory.PspAddressToPointerSafe(textureAddress);
				var clutPointer = (byte*)PspMemory.PspAddressToPointerSafe(clutAddress);

				TextureCacheKey textureCacheKey = new TextureCacheKey()
				{
					TextureAddress = textureAddress,
					TextureFormat = textureFormat,
					TextureHash = FastHash(texturePointer, textureDataSize),

					ClutHash = FastHash(&(clutPointer[clutDataStart]), clutDataSize),
					ClutAddress = clutAddress,
					ClutFormat = clutFormat,
					ClutStart = clutStart,
					ClutShift = clutShift,
					ClutMask = clutMask,
					Swizzled = swizzled,

					ColorTestEnabled = gpuState->ColorTestState.Enabled,
					ColorTestRef = gpuState->ColorTestState.Ref,
					ColorTestMask = gpuState->ColorTestState.Mask,
					ColorTestFunction = gpuState->ColorTestState.Function,
				};

				if (texture == null || (!texture.TextureCacheKey.Equals(textureCacheKey)))
				{
#if DEBUG_TEXTURE_CACHE
					var textureName = "texture_" + textureCacheKey.TextureHash + "_" + textureCacheKey.ClutHash + "_" + textureFormat + "_" + clutFormat + "_" + bufferWidth + "x" + height + "_" + swizzled;
					Console.Error.WriteLine("UPDATE_TEXTURE(TEX={0},CLUT={1}:{2}:{3}:{4}:0x{5:X},SIZE={6}x{7},{8},Swizzled={9})", TextureFormat, ClutFormat, ClutCount, ClutStart, ClutShift, ClutMask, BufferWidth, Height, BufferWidth, Swizzled);
#endif
					texture = new TTexture();
					texture.Init(GpuImpl);
					texture.TextureCacheKey = textureCacheKey;
					//Texture.Hash = Hash1;

					{
						//int TextureWidth = Math.Max(BufferWidth, Height);
						//int TextureHeight = Math.Max(BufferWidth, Height);
						int textureWidth = bufferWidth;
						int textureHeight = height;
						int textureWidthHeight = textureWidth * textureHeight;

						fixed (OutputPixel* texturePixelsPointer = DecodedTextureBuffer)
						{
							if (swizzled)
							{
								fixed (byte* swizzlingBufferPointer = SwizzlingBuffer)
								{
									PointerUtils.Memcpy(SwizzlingBuffer, texturePointer, textureDataSize);
									PixelFormatDecoder.UnswizzleInline(textureFormat, swizzlingBufferPointer, bufferWidth, height);
									PixelFormatDecoder.Decode(
										textureFormat, swizzlingBufferPointer, texturePixelsPointer, bufferWidth, height,
										clutPointer, clutFormat, clutCount, clutStart, clutShift, clutMask, PixelFormatDecoder.GetPixelsSize(textureFormat, textureWidth)
									);
								}
							}
							else
							{
								PixelFormatDecoder.Decode(
									textureFormat, texturePointer, texturePixelsPointer, bufferWidth, height,
									clutPointer, clutFormat, clutCount, clutStart, clutShift, clutMask, PixelFormatDecoder.GetPixelsSize(textureFormat, textureWidth)
								);
							}

							if (textureCacheKey.ColorTestEnabled)
							{
								byte equalValue, notEqualValue;

								switch (textureCacheKey.ColorTestFunction)
								{
									case ColorTestFunctionEnum.GU_ALWAYS: equalValue = 0xFF; notEqualValue = 0xFF; break;
									case ColorTestFunctionEnum.GU_NEVER: equalValue = 0x00; notEqualValue = 0x00; break;
									case ColorTestFunctionEnum.GU_EQUAL: equalValue = 0xFF; notEqualValue = 0x00; break;
									case ColorTestFunctionEnum.GU_NOTEQUAL: equalValue = 0x00; notEqualValue = 0xFF; break;
									default: throw(new NotImplementedException());
								}

								ConsoleUtils.SaveRestoreConsoleState(() =>
								{
									Console.BackgroundColor = ConsoleColor.Red;
									Console.ForegroundColor = ConsoleColor.Yellow;
									Console.Error.WriteLine("{0} : {1}, {2} : ref:{3} : mask:{4}", textureCacheKey.ColorTestFunction, equalValue, notEqualValue, textureCacheKey.ColorTestRef, textureCacheKey.ColorTestMask);
								});

								for (int n = 0; n < textureWidthHeight; n++)
								{
									if ((texturePixelsPointer[n] & textureCacheKey.ColorTestMask).Equals((textureCacheKey.ColorTestRef & textureCacheKey.ColorTestMask)))
									{
										texturePixelsPointer[n].A = equalValue;
									}
									else
									{
										texturePixelsPointer[n].A = notEqualValue;
									}
									if (texturePixelsPointer[n].A == 0)
									{
										//Console.Write("yup!");
									}
								}
							}

							var textureInfo = new TextureHookInfo() { TextureCacheKey = textureCacheKey, Data = DecodedTextureBuffer, Width = textureWidth, Height = textureHeight };
							MessageBus.Dispatch(textureInfo);

							var result = texture.SetData(textureInfo.Data, textureInfo.Width, textureInfo.Height);
						}
					}
					if (Cache.ContainsKey(hash1))
					{
						Cache[hash1].Dispose();
					}
					Cache[hash1] = texture;
				}
			}

			texture.RecheckTimestamp = _recheckTimestamp;

			return texture;
		}

		private DateTime _recheckTimestamp = DateTime.MinValue;

		public void RecheckAll()
		{
			_recheckTimestamp = DateTime.UtcNow;
		}

		public static ulong FastHash(byte* pointer, int count, ulong startHash = 0)
		{
			return Hashing.FastHash(pointer, count, startHash);
		}
	}
}
