using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLES;
using System.Runtime.ExceptionServices;
using CSPspEmu.Core.Gpu.State;
using CSharpUtils;
using CSPspEmu.Core.Memory;
using System.Runtime.InteropServices;
using System.Threading;
using System.Reflection;
using CSPspEmu.Core.Gpu.State.SubStates;
using CSharpPlatform;

namespace CSPspEmu.Core.Gpu.Impl.OpenglEs
{
	public sealed unsafe partial class GpuImplOpenglEs : GpuImpl, IInjectInitialize
    {
		OffscreenContext GraphicsContext;
		ShaderProgram ShaderProgram;

		Uniform projectionMatrix;
		Uniform viewMatrix;
		Uniform worldMatrix;
		Uniform textureMatrix;

		Uniform fColor;
		Uniform u_has_vertex_color;
		Uniform u_transform_2d;

		Uniform u_has_texture;
		Uniform u_texture;
		Uniform u_texture_effect;

		VertexAttribLocation aColorLocation;
		VertexAttribLocation aPositionLocation;
		VertexAttribLocation aTexCoord;

		TextureCacheOpengles TextureCache;
		TextureOpengles CurrentTexture;
		VertexTypeStruct VertexType;

		[Inject]
		PspMemory Memory;

		private GpuImplOpenglEs()
		{
		}

		void IInjectInitialize.Initialize()
		{
			this.TextureCache = new TextureCacheOpengles(Memory, this);
		}

		private string glVENDOR;
		private string glRENDERER;
		private string glVERSION;
		private string glSLVERSION;
		private string glEXTENSIONS;

		VertexInfo[] VertexInfoBuffer = new VertexInfo[1024 * 1024];
		VertexReader VertexReader = new VertexReader();

		public override void Prim(GlobalGpuState GlobalGpuState, GpuStateStruct* GpuState, GuPrimitiveType PrimitiveType, ushort VertexCount)
		{
			VertexType = GpuState->VertexState.Type;
			var TextureState = &GpuState->TextureMappingState.TextureState;

			VertexReader.SetVertexTypeStruct(
				VertexType,
				(byte*)Memory.PspAddressToPointerSafe(GlobalGpuState.GetAddressRelativeToBaseOffset(GpuState->VertexAddress), 0)
			);

			// Set Matrices
			this.ShaderProgram.Use();

			PrepareStateCommon(GpuState);

			if (GpuState->ClearingMode)
			{
				PrepareStateClear(GpuState);
			}
			else
			{
				PrepareStateDraw(GpuState);
			}

			PrepareStateMatrix(GpuState);

			fixed (VertexInfo* VertexInfoBufferPtr = VertexInfoBuffer)
			{
				switch (PrimitiveType)
				{
					case GuPrimitiveType.Sprites:
						int m = 0;
						for (int n = 0; n < VertexCount; n += 2)
						{
							VertexInfo TopLeft, _TopRight, BottomRight, _BottomLeft;

							VertexReader.ReadVertex(n + 0, &TopLeft);
							VertexReader.ReadVertex(n + 1, &BottomRight);
							TopLeft.Color.W = 1.0f;
							BottomRight.Color.W = 1.0f;

							//Console.WriteLine("--------------");
							//Console.WriteLine("({0},{1}) ({2},{3})", TopLeft.Position.X, TopLeft.Position.Y, TopLeft.Texture.X, TopLeft.Texture.Y);
							//Console.WriteLine("({0},{1}) ({2},{3})", BottomRight.Position.X, BottomRight.Position.Y, BottomRight.Texture.X, BottomRight.Texture.Y);

							{
								//if (GpuState->ClearingMode) Console.WriteLine("{0} - {1}", VertexInfoTopLeft, VertexInfoBottomRight);

								var Color = BottomRight.Color;
								var TZ = TopLeft.Texture.Z;
								var PZ = TopLeft.Position.Z;
								var NZ = TopLeft.Normal.Z;

								_TopRight = new VertexInfo()
								{
									Texture = new Vector4fRaw(BottomRight.Texture.X, TopLeft.Texture.Y, TZ, 0),
									Position = new Vector4fRaw(BottomRight.Position.X, TopLeft.Position.Y, PZ, 0),
									Normal = new Vector4fRaw(BottomRight.Normal.X, TopLeft.Normal.Y, NZ, 0),
								};

								_BottomLeft = new VertexInfo()
								{
									Texture = new Vector4fRaw(TopLeft.Texture.X, BottomRight.Texture.Y, TZ, 0),
									Position = new Vector4fRaw(TopLeft.Position.X, BottomRight.Position.Y, PZ, 0),
									Normal = new Vector4fRaw(TopLeft.Normal.X, BottomRight.Normal.Y, NZ, 0),
								};

								_BottomLeft.Color = BottomRight.Color = _TopRight.Color = TopLeft.Color = Color;
								_BottomLeft.Position.Z = BottomRight.Position.Z = _TopRight.Position.Z = TopLeft.Position.Z = PZ;
								_BottomLeft.Normal.Z = BottomRight.Normal.Z = _TopRight.Normal.Z = TopLeft.Normal.Z = NZ;
								_BottomLeft.Texture.Z = BottomRight.Texture.Z = _TopRight.Texture.Z = TopLeft.Texture.Z = NZ;
							}

							//VertexInfoBufferPtr[m++] = _TopRight;
							//VertexInfoBufferPtr[m++] = BottomRight;
							//VertexInfoBufferPtr[m++] = TopLeft;
							//VertexInfoBufferPtr[m++] = _BottomLeft;

							VertexInfoBufferPtr[m++] = _TopRight;
							VertexInfoBufferPtr[m++] = TopLeft;
							VertexInfoBufferPtr[m++] = BottomRight;
							VertexInfoBufferPtr[m++] = _BottomLeft;

							//Console.WriteLine("-----");
							//Console.WriteLine("({0},{1}) ({2},{3})", _TopRight.Position.X, _TopRight.Position.Y, _TopRight.Texture.X, _TopRight.Texture.Y);
							//Console.WriteLine("({0},{1}) ({2},{3})", TopLeft.Position.X, TopLeft.Position.Y, TopLeft.Texture.X, TopLeft.Texture.Y);
							//Console.WriteLine("({0},{1}) ({2},{3})", BottomRight.Position.X, BottomRight.Position.Y, BottomRight.Texture.X, BottomRight.Texture.Y);
							//Console.WriteLine("({0},{1}) ({2},{3})", _BottomLeft.Position.X, _BottomLeft.Position.Y, _BottomLeft.Texture.X, _BottomLeft.Texture.Y);

							//VertexInfoBufferPtr[m++] = _BottomLeft;
							//VertexInfoBufferPtr[m++] = BottomRight;
							//VertexInfoBufferPtr[m++] = _TopRight;
							//VertexInfoBufferPtr[m++] = TopLeft;

							//VertexInfoBufferPtr[m++] = _BottomLeft;
							//VertexInfoBufferPtr[m++] = TopLeft;
							//VertexInfoBufferPtr[m++] = BottomRight;
							//VertexInfoBufferPtr[m++] = _TopRight;

							//VertexInfoBufferPtr[m++] = _BottomLeft;
							//VertexInfoBufferPtr[m++] = TopLeft;
							//VertexInfoBufferPtr[m++] = BottomRight;
							//VertexInfoBufferPtr[m++] = _TopRight;
						}
						VertexCount = (ushort)m;
						break;
					default:
						VertexReader.ReadVertices(0, VertexInfoBufferPtr, VertexCount);
						break;
				}

				//if (PrimitiveType == GuPrimitiveType.LineStrip)
				{
					aPositionLocation.Pointer(3, GL.GL_FLOAT, false, sizeof(VertexInfo), ((byte*)VertexInfoBufferPtr) + (int)Marshal.OffsetOf(typeof(VertexInfo), "Position"));
					aPositionLocation.Enable();

					if (VertexType.ColorSize != 0)
					{
						aColorLocation.Pointer(4, GL.GL_FLOAT, false, sizeof(VertexInfo), ((byte*)VertexInfoBufferPtr) + (int)Marshal.OffsetOf(typeof(VertexInfo), "Color"));
						aColorLocation.Enable();
						u_has_vertex_color.SetBool(true);
					}
					else
					{
						aColorLocation.Disable();
						u_has_vertex_color.SetBool(false);
					}

					u_transform_2d.SetBool(VertexType.Transform2D);

					if (GpuState->TextureMappingState.Enabled)
					{
						aTexCoord.Pointer(3, GL.GL_FLOAT, false, sizeof(VertexInfo), ((byte*)VertexInfoBufferPtr) + (int)Marshal.OffsetOf(typeof(VertexInfo), "Texture"));
						aTexCoord.Enable();
						u_has_texture.SetBool(true);
						u_texture.SetInt(0);
						u_texture_effect.SetInt((int)TextureState->Effect);
					}
					else
					{
						aTexCoord.Disable();
						u_has_texture.SetBool(false);
					}

					GL.glDrawArrays(PrimitiveTypeTranslate[(int)PrimitiveType], 0, VertexCount);
				}

				//TestingRender();
			}
		}

		public override void Finish(GpuStateStruct* GpuState)
		{
		}

		public override void End(GpuStateStruct* GpuState)
		{
			PrepareWrite(GpuState);
		}

		public override void AddedDisplayList()
		{
		}

		public override void SetCurrent()
		{
			this.GraphicsContext.SetCurrent();
		}

		public override void UnsetCurrent()
		{
			this.GraphicsContext.UnsetCurrent();
		}

		readonly byte[] TempBuffer = new byte[512 * 512 * 4];

		public bool SwapBuffers = false;

		public override PluginInfo PluginInfo
		{
			get {
				return new PluginInfo()
				{
					Name = "OpenglEs - " + this.glRENDERER + " - " + this.glVERSION + " - " + this.glSLVERSION + " - " + this.glVENDOR,
					Version = "0.1",
				};
			}
		}

		public override bool IsWorking
		{
			get
			{
				return OffscreenContext.IsWorking;
			}
		}

		public override void Sync(GpuStateStruct* GpuState)
		{
			throw new NotImplementedException();
		}

		public override void BeforeDraw(GpuStateStruct* GpuState)
		{
			throw new NotImplementedException();
		}
	}
}
