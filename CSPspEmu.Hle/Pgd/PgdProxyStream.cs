using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Streams;
using System.IO;

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
