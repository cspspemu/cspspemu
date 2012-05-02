using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpUtils.SpaceAssigner
{
	public class SpaceAssigner2D
	{
		public class Space
		{
			public long x1, y1, x2, y2;

			public long width
			{
				get
				{
					return x2 - x1;
				}
			}

			public long height
			{
				get
				{
					return y2 - y1;
				}
			}

			public Space(long x1, long y1, long x2, long y2)
			{
				this.x1 = x1;
				this.y1 = y1;
				this.x2 = x2;
				this.y2 = y2;
			}
		}

		protected LinkedList<Space> AvailableChunks;

		public SpaceAssigner2D()
		{
			AvailableChunks = new LinkedList<Space>();
		}

		public void AddAvailable(Space Space)
		{
			throw (new NotImplementedException());
			//AvailableChunks.AddLast(Space);
		}

		public Space Allocate(long Length)
		{
			throw (new NotImplementedException());
		}
	}
}
