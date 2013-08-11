using CSharpPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Gpu.Impl.Opengl.Utils
{
	public class VertexUtils
	{
		static public void GenerateTriangleStripFromSpriteVertices(VertexInfo V0, out VertexInfo V1, out VertexInfo V2, VertexInfo V3)
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

			V2.Color = V3.Color = V1.Color = V0.Color = Color;
			V2.Position.Z = V3.Position.Z = V1.Position.Z = V0.Position.Z = PZ;
			V2.Normal.Z = V3.Normal.Z = V1.Normal.Z = V0.Normal.Z = NZ;
			V2.Texture.Z = V3.Texture.Z = V1.Texture.Z = V0.Texture.Z = NZ;
		}
	}
}
