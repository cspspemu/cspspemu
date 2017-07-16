using System.IO;

namespace CSharpUtils.Fastcgi
{
    public class FascgiRequestInputStream : MemoryStream
    {
        public bool Finalized = false;
    }
}
