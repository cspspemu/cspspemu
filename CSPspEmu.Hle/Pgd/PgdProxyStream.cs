using System.IO;
using CSharpUtils.Streams;

namespace CSPspEmu.Hle.Pgd
{
	public class PgdProxyStream : ProxyStream
	{
		public PgdProxyStream(Stream BaseStream)
			: base(BaseStream)
		{
		}
	}
}
