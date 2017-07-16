using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CSharpUtils.Streams;

namespace CSharpUtils.Http
{
	public class MultipartDecoder
	{
		Stream InputStream;
		StreamChunker StreamChunker;
		byte[] EndHeadersSequence;
		byte[] BoundarySequence1;
		byte[] BoundarySequence2;
		String Boundary;

		public class Part
		{
			public String Name;
			public bool IsFile;
			public String TempFilePath;
			public String FileName;
			public String ContentType;
			public String Content;
			public HttpHeaderList Headers;
			public Stream Stream;

			public override string ToString()
			{
				return String.Format(
					"MultipartDecoder.Part(Name='{0}'; Content-Type='{1}', IsFile='{2}', TempFilePath='{3}')",
					Name,
					ContentType,
					IsFile,
					TempFilePath
				);
			}
		}

		public MultipartDecoder(Stream InputStream, String Boundary)
		{
			this.InputStream = InputStream;
			this.StreamChunker = new StreamChunker(InputStream, 4096);
			this.Boundary = Boundary;
			this.BoundarySequence1 = Encoding.ASCII.GetBytes(Boundary);
			this.BoundarySequence2 = Encoding.ASCII.GetBytes("\r\n" + Boundary);
			this.EndHeadersSequence = Encoding.ASCII.GetBytes("\r\n\r\n");
		}

		public List<Part> Parse(Action<Part> InitializeFromHeaders = null, Action<Part> Finalize = null)
		{
			var Parts = new List<Part>();

			if (InitializeFromHeaders == null)
			{
				InitializeFromHeaders = delegate(Part HandlePart)
				{
					if (HandlePart.IsFile)
					{
						HandlePart.TempFilePath = Path.GetTempFileName();
						HandlePart.Stream = File.OpenWrite(HandlePart.TempFilePath);
					}
					else
					{
						HandlePart.Stream = new MemoryStream();
					}
				};
			}

			if (Finalize == null)
			{
				Finalize = delegate(Part HandlePart)
				{
					if (HandlePart.IsFile)
					{
						HandlePart.Stream.Close();
						HandlePart.Stream = File.OpenRead(HandlePart.TempFilePath);
					}
					else
					{
						HandlePart.Content = Encoding.UTF8.GetString(HandlePart.Stream.ReadAll());
					}
				};
			}

			this.StreamChunker.SkipUpToSequence(BoundarySequence1);
			if (Encoding.ASCII.GetString(this.StreamChunker.PeekBytes(2)) != "\r\n")
			{
				throw (new Exception("MultipartDecoder (1)"));
			}

			bool Eof = false;

			while (!Eof && !this.StreamChunker.Eof)
			{
				var Part = new Part();
				Part.Headers = new HttpHeaderList(Encoding.UTF8.GetString(this.StreamChunker.GetUpToSequence(EndHeadersSequence, 1 * 1024 * 1024)));
				Part.Headers.Set("Content-Type", "text/plain", Overwrite: false);

				Part.ContentType = Part.Headers.GetOne("Content-Type").Value;
				var Parsed = Part.Headers.GetOne("Content-Disposition").ParseValue("type");
				if (Parsed["type"] == "form-data")
				{
					Part.Name = Parsed["name"];
					Part.IsFile = Parsed.TryGetValue("filename", out Part.FileName);
					InitializeFromHeaders(Part);
					this.StreamChunker.CopyUpToSequence(Part.Stream, BoundarySequence2);
					string PeekedString = Encoding.ASCII.GetString(this.StreamChunker.PeekBytes(2));
					switch (PeekedString)
					{
						case "\r\n":
							// More files
							this.StreamChunker.SkipBytes(2);
							break;
						case "--": case "":
							// Last file.
							Eof = true;
							break;
						default: throw (new Exception("MultipartDecoder (2) : '" + PeekedString + "'"));
					}
					Finalize(Part);
					Parts.Add(Part);
				}
				else
				{
					throw(new Exception("Unknown Type: " + Parsed["type"]));
				}
			}

			return Parts;
		}
	}
}
