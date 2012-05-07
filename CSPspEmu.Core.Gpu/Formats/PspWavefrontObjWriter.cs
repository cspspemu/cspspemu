using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Gpu.State;
using OpenTK;

namespace CSPspEmu.Core.Gpu.Formats
{
	unsafe public class PspWavefrontObjWriter
	{
		WavefrontObjWriter WavefrontObjWriter;

		public PspWavefrontObjWriter(WavefrontObjWriter WavefrontObjWriter)
		{
			this.WavefrontObjWriter = WavefrontObjWriter;
		}

		public void End()
		{
			this.WavefrontObjWriter.End();
		}

		State.GuPrimitiveType CurrentPrimitiveType;
		List<int> PrimitiveIndices = new List<int>();
		Matrix4 ModelMatrix;
		GpuStateStruct* GpuState;
		State.VertexTypeStruct VertexType;

		public void StartPrimitive(GpuStateStruct* GpuState, State.GuPrimitiveType PrimitiveType, uint VertexAddress, int VertexCount, ref State.VertexTypeStruct VertexType)
		{
			this.GpuState = GpuState;
			this.VertexType = VertexType;
			var ViewMatrix = GpuState->VertexState.ViewMatrix.Matrix4;
			var WorldMatrix = GpuState->VertexState.WorldMatrix.Matrix4;
			ModelMatrix = Matrix4.Mult(ViewMatrix, WorldMatrix);

			this.CurrentPrimitiveType = PrimitiveType;
			WavefrontObjWriter.StartComment("Start: " + this.CurrentPrimitiveType + " : VertexAddress: 0x" + String.Format("{0:X}", VertexAddress) + " : " + VertexCount + " : " + this.VertexType);
			PrimitiveIndices.Clear();

			//throw new NotImplementedException();
			/*
			Console.WriteLine("ViewMatrix (DEMO):");
			Console.WriteLine("{0}", Matrix4.Translation(new Vector3(0f, 0f, -3.5f)));
			Console.WriteLine("ViewMatrix:");
			Console.WriteLine("{0}", ViewMatrix);
			*/
		}

		public void PutVertex(ref VertexInfo VertexInfo)
		{
			//GpuState.VertexState.ViewMatrix.Matrix
			if (!GpuState->ClearingMode)
			{
				var Vector = VertexInfo.Position.ToVector3();
				if (!VertexType.Transform2D)
				{
					Vector = Vector3.Transform(Vector, ModelMatrix);
				}

				PrimitiveIndices.Add(WavefrontObjWriter.AddVertex(Vector));
			}
			//throw new NotImplementedException();
		}

		public void EndPrimitive()
		{
			switch (this.CurrentPrimitiveType)
			{
				case State.GuPrimitiveType.Sprites:
					WavefrontObjWriter.AddFaces(4, PrimitiveIndices);
					break;
				case State.GuPrimitiveType.Triangles:
					WavefrontObjWriter.AddFaces(3, PrimitiveIndices);
					break;
				case State.GuPrimitiveType.TriangleStrip:
					{
						var Indices = PrimitiveIndices.ToArray();
						int TriangleCount = Indices.Length - 2;
						for (int n = 0; n < TriangleCount; n++)
						{
							WavefrontObjWriter.AddFace(Indices[n + 0], Indices[n + 1], Indices[n + 2]);
						}
					}
					break;
				default:
					WavefrontObjWriter.StartComment("Can't handle primitive type: " + this.CurrentPrimitiveType);
					break;
			}
			WavefrontObjWriter.StartComment("End: " + this.CurrentPrimitiveType);
			//throw new NotImplementedException();
		}
	}
}
