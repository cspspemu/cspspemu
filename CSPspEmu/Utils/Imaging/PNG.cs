using System;
using System.IO;
using CSharpUtils.Extensions;

namespace CSPspEmu.Utils.Imaging
{
    public class PNG
    {
        static public byte[] Encode(Bitmap32 bitmap)
        {
            var stream = new MemoryStream();
            stream.WriteString("test");
            throw new Exception("WIP");
        }
    }
}