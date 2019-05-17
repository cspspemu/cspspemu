using System;
using System.Numerics;
using System.Runtime.InteropServices;
//using OpenTK;
using CSharpPlatform;

namespace CSPspEmu.Core.Gpu
{
    public static class Vector4FRawExtensions
    {
        public static Vector4 Normalize(this Vector4 vector) =>
            vector * (1.0f / (float) Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z));

        public static Vector4 ToVector3(this Vector4 vector) => new Vector4(vector.X, vector.Y, vector.Z, 0f);
        public static Vector4 ToVector4(this Vector4 vector) => vector;
        //public static Vector4 ToVector4(this Vector4 vector) => new Vector4(vector.X, vector.Y, vector.Z, 1f);
        //public static Vector4 ToVector4(this Vector4 vector) => new Vector4(vector.X, vector.Y, vector.Z, 1f);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexInfoVector3F
    {
        public float X, Y, Z;

        public VertexInfoVector3F(Vector4 vector)
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexInfoColor
    {
        public float R, G, B, A;

        public VertexInfoColor(Vector4 vector)
        {
            R = vector.X;
            G = vector.Y;
            B = vector.Z;
            A = vector.W;
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
        public Vector4 Color;
        public Vector4 Position;
        public Vector4 Normal;
        public Vector4 Texture;
        public fixed float Weights[8];

        public override string ToString() => $"VertexInfo(Position={Position}, Normal={Normal}, UV={Texture}, COLOR={Color})";
    }
}