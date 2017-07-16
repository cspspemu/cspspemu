using System.IO;

namespace CSharpUtils.Fastcgi
{
    public class FastcgiPipeStream : IFastcgiPipe
    {
        Stream Stream;

        public FastcgiPipeStream(Stream Stream)
        {
            this.Stream = Stream;
        }

        public void Write(byte[] Data, int Offset, int Length)
        {
            Stream.Write(Data, Offset, Length);
        }

        public int Read(byte[] Data, int Offset, int Length)
        {
            return Stream.Read(Data, Offset, Length);
        }

        public void Close()
        {
            Stream.Close();
        }
    }
}
