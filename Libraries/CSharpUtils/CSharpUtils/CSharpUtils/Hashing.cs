using System;
using System.IO;
using System.Security.Cryptography;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    public class Hashing
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="progressAction"></param>
        /// <returns></returns>
        public static string GetMd5Hash(Stream stream, Action<long, long> progressAction = null)
        {
            if (progressAction == null) progressAction = (current, total) => { };

            const int minBufferSize = 1 * 1024; // 1 KB
            const int maxBufferSize = 1 * 1024 * 1024; // 1 MB

            var temp = new byte[Math.Max(minBufferSize, Math.Min(maxBufferSize, stream.Length))];
            var md5 = MD5.Create();

            while (!stream.Eof())
            {
                var readed = stream.Read(temp, 0, temp.Length);
                md5.TransformBlock(temp, 0, readed, temp, 0);
                progressAction(stream.Position, stream.Length);
            }

            md5.TransformFinalBlock(temp, 0, 0);
            //Md5.TransformBlock
            //return String.Join("", MD5.Create().ComputeHash(Stream).Select(Byte => Byte.ToString("x2").ToLower()));
            return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="progressAction"></param>
        /// <returns></returns>
        public static string GetMd5Hash(string path, Action<long, long> progressAction = null)
        {
            using (var stream = File.OpenRead(path))
            {
                return GetMd5Hash(stream, progressAction);
            }
        }
    }
}