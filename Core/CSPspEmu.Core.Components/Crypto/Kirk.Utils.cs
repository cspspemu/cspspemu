using System;
using System.IO;
using System.Security.Cryptography;
using CSharpUtils;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Components.Crypto
{
    public unsafe partial class Kirk
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static byte[] DecryptAes(byte[] input, byte[] key, byte[] iv = null)
        {
            if (iv == null) iv = new byte[16];

            Logger.Notice("DecryptAes({0}, {1}, {2})", input.Length, key.Length, iv.Length);

            using (var aes = CreateAes())
            {
                aes.Padding = PaddingMode.Zeros;
                var decryptor = aes.CreateDecryptor(key, iv);

                var dataSize = input.Length;

                if ((dataSize % 16) != 0)
                {
                    var input2 = new byte[MathUtils.NextAligned(input.Length, 16)];
                    Array.Copy(input, input2, input.Length);
                    input = input2;
                }

                return new CryptoStream(new MemoryStream(input), decryptor, CryptoStreamMode.Read).ReadBytes(dataSize);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static byte[] EncryptAes(byte[] input, byte[] key, byte[] iv = null)
        {
            if (iv == null) iv = new byte[16];

            using (var aes = CreateAes())
            {
                aes.Padding = PaddingMode.Zeros;
                var encryptor = aes.CreateEncryptor(key, iv);

                return new CryptoStream(new MemoryStream(input), encryptor, CryptoStreamMode.Read).ReadAll(
                    dispose: true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="size"></param>
        public static void DecryptAes(byte[] key, byte* input, byte* output, int size)
        {
            var inputArray = PointerUtils.PointerToByteArray(input, size);
            var outputArray = DecryptAes(inputArray, key);
            PointerUtils.ByteArrayToPointer(outputArray, output);
        }

        private static Aes CreateAes() => Aes.Create() ?? throw new Exception("Can't find AES");
    }
}