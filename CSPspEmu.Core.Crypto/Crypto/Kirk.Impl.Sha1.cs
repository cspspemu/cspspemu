using System.Security.Cryptography;
using CSharpUtils;

namespace CSPspEmu.Core.Crypto
{
	public unsafe partial class Kirk
	{
		/// <summary>
		/// SIZE: 0004
		/// </summary>
		public struct KIRK_SHA1_HEADER
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
		/// <param name="OutputBuffer"></param>
		/// <param name="InputBuffer"></param>
		/// <param name="InputSize"></param>
		/// <returns></returns>
		public void KirkSha1(byte* OutputBuffer, byte* InputBuffer, int InputSize)
		{
			CheckInitialized();

			var Header = (KIRK_SHA1_HEADER*)InputBuffer;
			if (InputSize == 0 || Header->DataSize == 0)
			{
				throw(new KirkException(ResultEnum.PSP_KIRK_DATA_SIZE_IS_ZERO));
			}

			//Size <<= 4;
			//Size >>= 4;
			InputSize &= 0x0FFFFFFF;
			InputSize = (InputSize < Header->DataSize) ? InputSize : Header->DataSize;

			var Sha1Hash = Sha1(
				PointerUtils.PointerToByteArray(InputBuffer + 4, InputSize)
			);

			PointerUtils.Memcpy(OutputBuffer, Sha1Hash, Sha1Hash.Length);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Input"></param>
		/// <returns></returns>
		public static byte[] Sha1(byte[] Input)
		{
			return (new SHA1CryptoServiceProvider()).ComputeHash(Input);
		}
	}
}
