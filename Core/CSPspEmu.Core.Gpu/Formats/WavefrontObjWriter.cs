using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpPlatform;
using CSharpPlatform.GL.Utils;

namespace CSPspEmu.Core.Gpu.Formats
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="http://en.wikipedia.org/wiki/Wavefront_.obj_file"/>
	public class WavefrontObjWriter
	{
		private int VertexIndex = 1;
		private Stream Stream;
		private StreamWriter StreamWriter;
		private StringWriter VertexWriter;
		private StringWriter ContentWriter;

		public WavefrontObjWriter(string FileName)
		{
			Stream = File.Open(FileName, FileMode.Create, FileAccess.Write);
			StreamWriter = new StreamWriter(Stream);
			VertexWriter = new StringWriter();
			ContentWriter = new StringWriter();
		}

		public void End()
		{
			_EndVertices();
			StreamWriter.Write(VertexWriter.ToString());
			StreamWriter.Write(ContentWriter.ToString());
			StreamWriter.Flush();
			Stream.Flush();
			Stream.Dispose();
			Stream = null;
			StreamWriter = null;
			//throw new NotImplementedException();
		}

		private void WriteLine(string Line)
		{
			ContentWriter.WriteLine(Line);
		}

		private void WriteVerticeLine(string Line)
		{
			VertexWriter.WriteLine(Line);
		}

		List<Vector4f> Vertices = new List<Vector4f>();
		Dictionary<Vector4f, int> VerticesIndices = new Dictionary<Vector4f, int>();

		private void _EndVertices()
		{
			float Normalize = 0.0f;
			foreach (var Vertex in Vertices)
			{
				if (!float.IsNaN(Vertex.X) && !float.IsNaN(Vertex.Y) && !float.IsNaN(Vertex.Z))
				{
					Normalize = Math.Max(Normalize, Math.Abs(Vertex.X));
					Normalize = Math.Max(Normalize, Math.Abs(Vertex.Y));
					Normalize = Math.Max(Normalize, Math.Abs(Vertex.Z));
				}
			}

			Normalize /= 64;

			var NormalizeRecp = (1.0f / Normalize);

			foreach (var Vertex in Vertices)
			{
				var NormalizedVertex = Vertex * NormalizeRecp;
				WriteVerticeLine("v " + NormalizedVertex.X + " " + NormalizedVertex.Y + " " + NormalizedVertex.Z);
			}
		}

		public int AddVertex(Vector4f Position)
		{
			WriteLine("# v " + Position.X + " " + Position.Y + " " + Position.Z + " ");

			if (!VerticesIndices.ContainsKey(Position))
			{
				//WriteVerticeLine("v " + Position.X + " " + Position.Y + " " + Position.Z);
				Vertices.Add(Position);
				VerticesIndices[Position] = VertexIndex;
				return VertexIndex++;
			}
			return VerticesIndices[Position];
		}

		/*
		public int AddVertex(float x, float y, float z)
		{
			WriteLine("v " + x + " " + y + " " + z);
			return VertexIndex++;
		}
		*/

		public void AddFace(params int[] Indices)
		{
			WriteLine("f " + String.Join(" ", Indices));
		}

		public void StartComment(string Text)
		{
			WriteLine("# " + Text);
		}

		public void StartObject(string Name)
		{
			WriteLine("o " + Name);
		}

		public void AddTriangleStrip(VertexInfo[] Vertices)
		{
			List<int> Indices = new List<int>();
			foreach (var Vertex in Vertices)
			{
				Indices.Add(AddVertex(Vertex.Position.ToVector3()));
			}
			AddFace(Indices.ToArray());
		}

		/*
		public void SampleAddCube()
		{
			StartComment("Cube");
			StartObject("cube");
		}
		*/

		public void AddFaces(int NumberOfVerticesPerFace, params int[] VerticesIndices)
		{
			for (int n = 0; n < VerticesIndices.Length; n += NumberOfVerticesPerFace)
			{
				AddFace(VerticesIndices.Skip(n).Take(NumberOfVerticesPerFace).ToArray());
			}
		}

		public void AddFaces(int NumberOfVerticesPerFace, IEnumerable<int> VerticesIndices)
		{
			AddFaces(NumberOfVerticesPerFace, VerticesIndices.ToArray());
		}
	}
}
