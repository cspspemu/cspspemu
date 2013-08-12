using CSharpPlatform;
using CSPspEmu.Core.Gpu.State;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Gpu.Impl.Opengl.Utils
{
	unsafe public class VertexUtils
	{
		static public void GenerateTriangleStripFromSpriteVertices(ref VertexInfo V0, out VertexInfo V1, out VertexInfo V2, ref VertexInfo V3)
		{
			var Color = V3.Color;
			var TZ = V0.Texture.Z;
			var PZ = V0.Position.Z;
			var NZ = V0.Normal.Z;

			V1 = new VertexInfo()
			{
				Texture = new Vector4f(V3.Texture.X, V0.Texture.Y, TZ, 0),
				Position = new Vector4f(V3.Position.X, V0.Position.Y, PZ, 0),
				Normal = new Vector4f(V3.Normal.X, V0.Normal.Y, NZ, 0),
			};

			V2 = new VertexInfo()
			{
				Texture = new Vector4f(V0.Texture.X, V3.Texture.Y, TZ, 0),
				Position = new Vector4f(V0.Position.X, V3.Position.Y, PZ, 0),
				Normal = new Vector4f(V0.Normal.X, V3.Normal.Y, NZ, 0),
			};

			V3.Color = V2.Color = V1.Color = V0.Color = Color;
			V3.Position.Z = V2.Position.Z = V1.Position.Z = V0.Position.Z = PZ;
			V3.Normal.Z = V2.Normal.Z = V1.Normal.Z = V0.Normal.Z = NZ;
			V3.Texture.Z = V2.Texture.Z = V1.Texture.Z = V0.Texture.Z = NZ;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="VertexInfo"></param>
		/// <returns></returns>
		static public VertexInfo PerformSkinning(ref VertexTypeStruct VertexType, GpuStateStruct* GpuState, VertexInfo VertexInfo)
		{
			int SkinningWeightCount = VertexType.RealSkinningWeightCount;
			if (SkinningWeightCount == 0) return VertexInfo;

			var OutputPosition = default(Vector4f);
			var OutputNormal = default(Vector4f);
			var InputPosition = VertexInfo.Position;
			var InputNormal = VertexInfo.Normal;

			var BoneMatrices = &GpuState->SkinningState.BoneMatrix0;

			//Console.WriteLine("------");
			float WeightTotal = 0f;
			for (int m = 0; m < SkinningWeightCount; m++)
			{
				WeightTotal += VertexInfo.Weights[m];
			}
			for (int m = 0; m < SkinningWeightCount; m++)
			{
				var BoneMatrix = BoneMatrices[m];
				var BoneMatrixValues = BoneMatrix.Values;
				float Weight = VertexInfo.Weights[m] / WeightTotal;

				BoneMatrix.SetLastColumn();

				//Console.WriteLine("{0}", Weight);
				//Console.WriteLine("{0}", BoneMatrix);

				if (Weight != 0)
				{
					OutputPosition.X += (InputPosition.X * BoneMatrixValues[0] + InputPosition.Y * BoneMatrixValues[4] + InputPosition.Z * BoneMatrixValues[8] + 1 * BoneMatrixValues[12]) * Weight;
					OutputPosition.Y += (InputPosition.X * BoneMatrixValues[1] + InputPosition.Y * BoneMatrixValues[5] + InputPosition.Z * BoneMatrixValues[9] + 1 * BoneMatrixValues[13]) * Weight;
					OutputPosition.Z += (InputPosition.X * BoneMatrixValues[2] + InputPosition.Y * BoneMatrixValues[6] + InputPosition.Z * BoneMatrixValues[10] + 1 * BoneMatrixValues[14]) * Weight;

					OutputNormal.X += (InputNormal.X * BoneMatrixValues[0] + InputNormal.Y * BoneMatrixValues[4] + InputNormal.Z * BoneMatrixValues[8] + 0 * BoneMatrixValues[12]) * Weight;
					OutputNormal.Y += (InputNormal.X * BoneMatrixValues[1] + InputNormal.Y * BoneMatrixValues[5] + InputNormal.Z * BoneMatrixValues[9] + 0 * BoneMatrixValues[13]) * Weight;
					OutputNormal.Z += (InputNormal.X * BoneMatrixValues[2] + InputNormal.Y * BoneMatrixValues[6] + InputNormal.Z * BoneMatrixValues[10] + 0 * BoneMatrixValues[14]) * Weight;
				}
			}

			VertexInfo.Position = OutputPosition;
			VertexInfo.Normal = OutputNormal;

			return VertexInfo;
		}
	}
}
