using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using CSharpUtils;

namespace CSPspEmu.Core
{
	unsafe public sealed class Hashing
	{
		static Logger Logger = Logger.GetLogger("Hashing");

		[HandleProcessCorruptedStateExceptions()]
		static public uint FastHash(uint* Pointer, int Count, uint StartHash = 0)
		{
			if (Pointer == null)
			{
				return StartHash;
			}

			//Console.WriteLine("{0:X}", new IntPtr(Pointer));

			var CountInBlocks = Count / 4;
			var Hash = StartHash;

			try
			{
				if (CountInBlocks > 2048 * 2048)
				{
					Logger.Error("FastHash too big count!");
				}
				else
				{
					for (int n = 0; n < CountInBlocks; n++)
					{
						Hash += (uint)(Pointer[n] + (n << 17));
					}
				}
			}
			catch (AccessViolationException AccessViolationException)
			{
				Logger.Error(AccessViolationException);
			}

			return Hash;
		}
	}
}
