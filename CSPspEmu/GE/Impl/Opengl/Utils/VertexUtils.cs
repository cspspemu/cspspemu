using CSharpPlatform;

namespace CSPspEmu.Core.Gpu.Impl.Opengl.Utils
{
    public class VertexUtils
    {
        public static void GenerateTriangleStripFromSpriteVertices(ref VertexInfo v0, out VertexInfo v1,
            out VertexInfo v2, ref VertexInfo v3)
        {
            var color = v3.Color;
            var tz = v0.Texture.Z;
            var pz = v0.Position.Z;
            var nz = v0.Normal.Z;

            v1 = new VertexInfo
            {
                Texture = new Vector4f(v3.Texture.X, v0.Texture.Y, tz, 0),
                Position = new Vector4f(v3.Position.X, v0.Position.Y, pz, 0),
                Normal = new Vector4f(v3.Normal.X, v0.Normal.Y, nz, 0),
            };

            v2 = new VertexInfo
            {
                Texture = new Vector4f(v0.Texture.X, v3.Texture.Y, tz, 0),
                Position = new Vector4f(v0.Position.X, v3.Position.Y, pz, 0),
                Normal = new Vector4f(v0.Normal.X, v3.Normal.Y, nz, 0),
            };

            v3.Color = v2.Color = v1.Color = v0.Color = color;
            v3.Position.Z = v2.Position.Z = v1.Position.Z = v0.Position.Z = pz;
            v3.Normal.Z = v2.Normal.Z = v1.Normal.Z = v0.Normal.Z = nz;
            v3.Texture.Z = v2.Texture.Z = v1.Texture.Z = v0.Texture.Z = nz;
        }
    }
}