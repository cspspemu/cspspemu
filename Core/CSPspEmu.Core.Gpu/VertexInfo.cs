using System;
using System.Runtime.InteropServices;
//using OpenTK;
using CSharpPlatform;

namespace CSPspEmu.Core.Gpu
{
    public static class Vector4FRawExtensions
    {
        public static Vector4f Normalize(this Vector4f vector) =>
            vector * (1.0f / (float) Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z));

        public static Vector4f ToVector3(this Vector4f vector) => new Vector4f(vector.X, vector.Y, vector.Z, 0f);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexInfoVector3F
    {
        public float X, Y, Z;

        public VertexInfoVector3F(Vector4f vector4F)
        {
            X = vector4F.X;
            Y = vector4F.Y;
            Z = vector4F.Z;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexInfoColor
    {
        public float R, G, B, A;

        public VertexInfoColor(Vector4f vector4F)
        {
            R = vector4F.X;
            G = vector4F.Y;
            B = vector4F.Z;
            A = vector4F.W;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct VertexInfoWeights
    {
        public fixed float W[8];

        public VertexInfoWeights(VertexInfo vertexInfo)
        {
            fixed (float* wPtr = W)
            {
                for (var n = 0; n < 8; n++) wPtr[n] = vertexInfo.Weights[n];
            }
        }
    }

    /// <summary>Information about a vertex.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct VertexInfo
    {
        public Vector4f Color;
        public Vector4f Position;
        public Vector4f Normal;
        public Vector4f Texture;
        public fixed float Weights[8];

        public override string ToString()
        {
            return string.Format(
                "VertexInfo(Position={0}, Normal={1}, UV={2}, COLOR={3})",
                Position, Normal, Texture, Color
            );
        }
    }
}