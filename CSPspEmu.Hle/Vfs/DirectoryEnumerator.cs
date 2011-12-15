using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Vfs
{
	internal class DirectoryEnumerator<TType>
	{
		int Index = -1;
		TType[] List;

		public DirectoryEnumerator(TType[] List)
		{
			this.List = List;
		}

		public bool MoveNext()
		{
			Index++;
			return (GetLeft() > 0);
		}

		public TType Current
		{
			get
			{
				return List[Index];
			}
		}

		public int GetLeft()
		{
			return List.Length - Index;
		}
	}
}
