using System.Security.Cryptography;
using CSharpUtils;

namespace CSPspEmu.Core.Components.Crypto
{
    public unsafe partial class Kirk
    {
        /// <summary>
        /// SIZE: 0004
        /// </summary>
        public struct KirkSha1Header
        {
            /// <summary>
            /// 0000 - Size of the input data source where will be generated the hash from.
            /// </summary>
            public int DataSize;
        }

        /// <summary>
        /// Creates a SHA1 Hash
        /// 
        /// Command: 11, 0xB
        /// </summary>
        /// <param name="outputBuffer"></param>
        /// <param name="inputBuffer"></param>
        /// <param name="inputSize"></param>
        /// <returns></returns>
        public void KirkSha1(byte* outputBuffer, byte* inputBuffer, int inputSize)
        {
            //CheckInitialized();

            var header = (KirkSha1Header*) inputBuffer;
            if (inputSize == 0 || header->DataSize == 0)
            {
                throw(new KirkException(ResultEnum.PspKirkDataSizeIsZero));
            }

            //Size <<= 4;
            //Size >>= 4;
            inputSize &= 0x0FFFFFFF;
            inputSize = (inputSize < header->DataSize) ? inputSize : header->DataSize;

            var sha1Hash = Sha1(
                PointerUtils.PointerToByteArray(inputBuffer + 4, inputSize)
            );

            PointerUtils.Memcpy(outputBuffer, sha1Hash, sha1Hash.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] Sha1(byte[] input)
        {
            return (new SHA1CryptoServiceProvider()).ComputeHash(input);
        }
    }
}