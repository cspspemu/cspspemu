using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CSharpUtils.Streams
{
	static public class SerializerUtils
	{
		static public MemoryStream SerializeToMemoryStream(Action<Stream> Serializer)
		{
			var Stream = new MemoryStream();
			Stream.PreservePositionAndLock(() =>
			{
				Serializer(Stream);
				Stream.Flush();
			});
			return Stream;
		}
	}
}
