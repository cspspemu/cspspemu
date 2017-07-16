using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using CSharpUtils.Http;

namespace CSharpUtils.Fastcgi.Http
{
	public abstract class FastcgiHttpServer : FastcgiServer
	{
		sealed protected override void HandleFascgiRequest(FastcgiRequest FastcgiRequest)
		{
			FastcgiRequest.StdinStream.SetPosition(0);
			Dictionary<string, string> Post = new Dictionary<string, string>();
			Dictionary<string, HttpFile> Files = new Dictionary<string, HttpFile>();
			bool HandledPost = false;

			//foreach (var Param in FastcgiRequest.Params) Console.WriteLine(Param);

			HttpHeader ContentType = new HttpHeader("Content-Type", FastcgiRequest.GetParam("CONTENT_TYPE"));

			var ContentTypeParts = ContentType.ParseValue("type");
			if (ContentTypeParts["type"] == "multipart/form-data")
			{
				string Boundary = ContentTypeParts["boundary"];
				//File.WriteAllBytes("Boundary.bin", Encoding.ASCII.GetBytes(Boundary));
				MultipartDecoder MultipartDecoder = new MultipartDecoder(FastcgiRequest.StdinStream, "--" + Boundary);
				var Parts = MultipartDecoder.Parse();

				foreach (var Part in Parts)
				{
					if (Part.IsFile)
					{
						Files.Add(Part.Name, new HttpFile()
						{
							TempFile = new FileInfo(Part.TempFilePath),
							FileName = Part.FileName,
							ContentType = Part.ContentType,
						});
					}
					else
					{
						Post[Part.Name] = Part.Content;
					}

					HandledPost = true;
				}
			}

			if (!HandledPost)
			{
				Post = HttpUtils.ParseUrlEncoded(Encoding.UTF8.GetString(FastcgiRequest.StdinStream.ReadAll()));
			}

			//CONTENT_TYPE: multipart/form-data; boundary=----WebKitFormBoundaryIMw3ByBOPx38V6Bd

			using (var OutputTextWriter = new StringWriter())
			{
				var HttpRequest = new HttpRequest()
				{
					Headers = new HttpHeaderList(),
					Output = OutputTextWriter,
					Enviroment = FastcgiRequest.Params,
					StdinStream = FastcgiRequest.StdinStream,
					Post = Post,
					Files = Files,
					Get = HttpUtils.ParseUrlEncoded(FastcgiRequest.GetParam("QUERY_STRING")),
					Cookies = new Dictionary<String, String>(),
				};

				HttpRequest.Headers.Set("X-Dynamic", "C#");
				HttpRequest.SetContentType("text/html", Encoding.UTF8);

				var Stopwatch = new Stopwatch();
				Stopwatch.Start();
				{
					HandleHttpRequest(HttpRequest);
				}
				Stopwatch.Stop();
				double GenerationTime = (double)Stopwatch.ElapsedTicks / (double)Stopwatch.Frequency;

				HttpRequest.Headers.Set("X-GenerationTime", String.Format("{0}", GenerationTime.ToString("F8")));

				using (var Stdout = new StreamWriter(FastcgiRequest.StdoutStream))
				{
					HttpRequest.Headers.WriteTo(Stdout);
				}

				using (var Stdout = new StreamWriter(FastcgiRequest.StdoutStream, HttpRequest.Encoding))
				{
					Stdout.Write(OutputTextWriter.ToString());
					if (HttpRequest.OutputDebug)
					{
						Stdout.WriteLine("<pre>");
						Stdout.WriteLine("Generation Time: {0}", GenerationTime);
						Stdout.WriteLine("</pre>");
					}
				}
			}
		}

		protected abstract void HandleHttpRequest(HttpRequest HttpRequest);
	}
}
