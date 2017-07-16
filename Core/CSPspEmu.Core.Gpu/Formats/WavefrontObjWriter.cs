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
        private int _vertexIndex = 1;
        private Stream _stream;
        private StreamWriter _streamWriter;
        private StringWriter _vertexWriter;
        private StringWriter _contentWriter;

        public WavefrontObjWriter(string fileName)
        {
            _stream = File.Open(fileName, FileMode.Create, FileAccess.Write);
            _streamWriter = new StreamWriter(_stream);
            _vertexWriter = new StringWriter();
            _contentWriter = new StringWriter();
        }

        public void End()
        {
            _EndVertices();
            _streamWriter.Write(_vertexWriter.ToString());
            _streamWriter.Write(_contentWriter.ToString());
            _streamWriter.Flush();
            _stream.Flush();
            _stream.Dispose();
            _stream = null;
            _streamWriter = null;
            //throw new NotImplementedException();
        }

        private void WriteLine(string line) => _contentWriter.WriteLine(line);

        private void WriteVerticeLine(string line) => _vertexWriter.WriteLine(line);

        List<Vector4f> Vertices = new List<Vector4f>();
        Dictionary<Vector4f, int> VerticesIndices = new Dictionary<Vector4f, int>();

        private void _EndVertices()
        {
            var normalize = 0.0f;
            foreach (var vertex in Vertices)
            {
                if (float.IsNaN(vertex.X) || float.IsNaN(vertex.Y) || float.IsNaN(vertex.Z)) continue;
                normalize = Math.Max(normalize, Math.Abs(vertex.X));
                normalize = Math.Max(normalize, Math.Abs(vertex.Y));
                normalize = Math.Max(normalize, Math.Abs(vertex.Z));
            }

            normalize /= 64;

            var normalizeRecp = (1.0f / normalize);

            foreach (var vertex in Vertices)
            {
                var normalizedVertex = vertex * normalizeRecp;
                WriteVerticeLine($"v {normalizedVertex.X} {normalizedVertex.Y} {normalizedVertex.Z}");
            }
        }

        public int AddVertex(Vector4f position)
        {
            WriteLine($"# v {position.X} {position.Y} {position.Z} ");

            if (!VerticesIndices.ContainsKey(position))
            {
                //WriteVerticeLine("v " + Position.X + " " + Position.Y + " " + Position.Z);
                Vertices.Add(position);
                VerticesIndices[position] = _vertexIndex;
                return _vertexIndex++;
            }
            return VerticesIndices[position];
        }

        /*
        public int AddVertex(float x, float y, float z)
        {
            WriteLine("v " + x + " " + y + " " + z);
            return VertexIndex++;
        }
        */

        public void AddFace(params int[] indices) => WriteLine($"f {String.Join(" ", indices)}");
        public void StartComment(string text) => WriteLine($"# {text}");
        public void StartObject(string name) => WriteLine("o " + name);

        public void AddTriangleStrip(VertexInfo[] vertices)
        {
            var indices = new List<int>();
            foreach (var vertex in vertices)
                indices.Add(AddVertex(vertex.Position.ToVector3()));
            AddFace(indices.ToArray());
        }

        /*
        public void SampleAddCube()
        {
            StartComment("Cube");
            StartObject("cube");
        }
        */

        public void AddFaces(int numberOfVerticesPerFace, params int[] verticesIndices)
        {
            for (int n = 0; n < verticesIndices.Length; n += numberOfVerticesPerFace)
            {
                AddFace(verticesIndices.Skip(n).Take(numberOfVerticesPerFace).ToArray());
            }
        }

        public void AddFaces(int numberOfVerticesPerFace, IEnumerable<int> verticesIndices) =>
            AddFaces(numberOfVerticesPerFace, verticesIndices.ToArray());
    }
}