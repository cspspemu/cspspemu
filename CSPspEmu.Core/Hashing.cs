using System;
using System.Runtime.ExceptionServices;
using CSharpUtils;

namespace CSPspEmu.Core
{
	public unsafe sealed class Hashing
	{
		static Logger Logger = Logger.GetLogger("Hashing");

		[HandleProcessCorruptedStateExceptions]
		public static ulong FastHash(byte* Pointer, int Count, ulong StartHash = 0)
		{
			if (Pointer == null)
			{
				return StartHash;
			}

			if (Count > 4 * 2048 * 2048)
			{
				Logger.Error("FastHash too big count!");
				return StartHash;
			}

			try
			{
				return FastHash_64(Pointer, Count, StartHash);
			}
			catch (NullReferenceException NullReferenceException)
			{
				Logger.Error(NullReferenceException);
			}
			catch (AccessViolationException AccessViolationException)
			{
				Logger.Error(AccessViolationException);
			}

			return StartHash;

		}

		[HandleProcessCorruptedStateExceptions]
		private static ulong FastHash_64(byte* Pointer, int Count, ulong StartHash = 0)
		{
			var Hash = StartHash;

			while (Count >= 8)
			{
				Hash += (*(ulong*)Pointer) + (ulong)(Count << 31);
				Pointer += 8;
				Count -= 8;
			}

			while (Count >= 1)
			{
				Hash += *Pointer++;
				Count--;
			}

			return Hash;
		}
	}
}
