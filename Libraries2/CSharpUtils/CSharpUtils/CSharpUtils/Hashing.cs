using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CSharpUtils
{
    public class Hashing
    {
        public static string GetMd5Hash(Stream Stream, Action<long, long> ProgressAction = null)
        {
            if (ProgressAction == null) ProgressAction = (Current, Total) => { };

            const int MinBufferSize = 1 * 1024; // 1 KB
            const int MaxBufferSize = 1 * 1024 * 1024; // 1 MB

            byte[] Temp = new byte[Math.Max(MinBufferSize, Math.Min(MaxBufferSize, Stream.Length))];
            var md5 = MD5.Create();

            while (!Stream.Eof())
            {
                int Readed = Stream.Read(Temp, 0, Temp.Length);
                md5.TransformBlock(Temp, 0, Readed, Temp, 0);
                ProgressAction(Stream.Position, Stream.Length);
            }

            md5.TransformFinalBlock(Temp, 0, 0);
            //Md5.TransformBlock
            //return String.Join("", MD5.Create().ComputeHash(Stream).Select(Byte => Byte.ToString("x2").ToLower()));
            return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
        }

        public static string GetMd5Hash(string Path, Action<long, long> ProgressAction = null)
        {
            using (var Stream = File.OpenRead(Path))
            {
                return GetMd5Hash(Stream, ProgressAction);
            }
        }
    }
}
