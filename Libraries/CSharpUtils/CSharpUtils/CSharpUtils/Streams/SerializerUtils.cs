using System;
using System.IO;

namespace CSharpUtils.Streams
{
    public static class SerializerUtils
    {
        public static MemoryStream SerializeToMemoryStream(Action<Stream> Serializer)
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