using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle
{
	public interface IArgumentReader
	{
		int LoadInteger();
		float LoadFloat();
		long LoadLong();
		string LoadString();
	}
}
