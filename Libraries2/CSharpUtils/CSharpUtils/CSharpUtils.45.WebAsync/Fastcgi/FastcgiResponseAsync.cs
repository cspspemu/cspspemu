using System.IO;

namespace CSharpUtils.Web._45.Fastcgi
{
	public class FastcgiResponseAsync
	{
		public FastcgiOutputStream StdoutStream;
		public FastcgiOutputStream StderrStream;
		public FastcgiHeaders Headers;

		public StreamWriter StdoutWriter;
		public StreamWriter StderrWriter;
	}
}
