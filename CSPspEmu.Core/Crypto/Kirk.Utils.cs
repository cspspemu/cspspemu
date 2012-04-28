using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CSharpUtils;

namespace CSPspEmu.Core.Crypto
{
	unsafe public partial class Kirk
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Input"></param>
		/// <param name="Key"></param>
		/// <param name="IV"></param>
		/// <returns></returns>
		static public byte[] DecryptAes(byte[] Input, byte[] Key, byte[] IV = null)
		{
			if (IV == null) IV = new byte[16];

			Logger.Notice("DecryptAes({0}, {1}, {2})", Input.Length, Key.Length, IV.Length);

			using (var AES = Aes.Create())
			{
				AES.Padding = PaddingMode.Zeros;
				var Decryptor = AES.CreateDecryptor(Key, IV);

				int DataSize = Input.Length;

				if ((DataSize % 16) != 0)
				{
					var Input2 = new byte[MathUtils.NextAligned(Input.Length, 16)];
					Array.Copy(Input, Input2, Input.Length);
					Input = Input2;
				}

				return new CryptoStream(new MemoryStream(Input), Decryptor, CryptoStreamMode.Read).ReadBytes(DataSize);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Input"></param>
		/// <param name="Key"></param>
		/// <param name="IV"></param>
		/// <returns></returns>
		static public byte[] EncryptAes(byte[] Input, byte[] Key, byte[] IV = null)
		{
			if (IV == null) IV = new byte[16];

			using (var AES = Aes.Create())
			{
				AES.Padding = PaddingMode.Zeros;
				var Encryptor = AES.CreateEncryptor(Key, IV);

				return new CryptoStream(new MemoryStream(Input), Encryptor, CryptoStreamMode.Read).ReadAll(Dispose: true);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Key"></param>
		/// <param name="Input"></param>
		/// <param name="Output"></param>
		/// <param name="Size"></param>
		static public void DecryptAes(byte[] Key, byte* Input, byte* Output, int Size)
		{
			var InputArray = PointerUtils.PointerToByteArray(Input, Size);
			var OutputArray = DecryptAes(InputArray, Key);
			PointerUtils.ByteArrayToPointer(OutputArray, Output);
		}
	}
}
