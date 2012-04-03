using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using CSharpUtils;

namespace CSPspEmu.Core.Crypto
{
	unsafe public partial class Kirk
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
		public int KirkSha1(byte* OutputBuffer, byte* InputBuffer, int InputSize)
		{
			if (!IsKirkInitialized) return KIRK_NOT_INITIALIZED;

			var Header = (KIRK_SHA1_HEADER*)InputBuffer;
			if (InputSize == 0 || Header->DataSize == 0) return KIRK_DATA_SIZE_ZERO;

			//Size <<= 4;
			//Size >>= 4;
			InputSize &= 0x0FFFFFFF;
			InputSize = (InputSize < Header->DataSize) ? InputSize : Header->DataSize;

			var Sha1Hash = Sha1(
				PointerUtils.PointerToByteArray(InputBuffer + 4, InputSize)
			);

			Marshal.Copy(Sha1Hash, 0, new IntPtr(OutputBuffer), Sha1Hash.Length);

			return KIRK_OPERATION_SUCCESS;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Input"></param>
		/// <returns></returns>
		static public byte[] Sha1(byte[] Input)
		{
			return (new SHA1CryptoServiceProvider()).ComputeHash(Input);
		}
	}
}
