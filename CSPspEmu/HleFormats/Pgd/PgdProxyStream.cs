using System.IO;
using CSharpUtils.Streams;

namespace CSPspEmu.Hle.Formats.Pgd
{
    public class PgdProxyStream : ProxyStream
    {
        public PgdProxyStream(Stream baseStream) : base(baseStream)
        {
        }
    }
}