using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;

namespace CSharpUtils.SpaceAssigner
{
	public class SpaceAssigner1DUniqueAllocator
	{
		SpaceAssigner1D SpaceAssigner;
		Dictionary<byte[], SpaceAssigner1D.Space> AllocatedSpaces;
		public event Action<byte[], SpaceAssigner1D.Space> OnAllocate;

		public SpaceAssigner1DUniqueAllocator(SpaceAssigner1D SpaceAssigner)
		{
			this.SpaceAssigner = SpaceAssigner;
			this.AllocatedSpaces = new Dictionary<byte[], SpaceAssigner1D.Space>(new ArrayEqualityComparer<byte>());
		}

		public SpaceAssigner1D.Space AllocateUnique(byte[] data)
		{
			if (!AllocatedSpaces.ContainsKey(data))
			{
				var AllocatedSpace = SpaceAssigner.Allocate(data.Length);
				if (OnAllocate != null) OnAllocate(data, AllocatedSpace);
				AllocatedSpaces[data] = AllocatedSpace;
			}
			return AllocatedSpaces[data];
		}

		public SpaceAssigner1D.Space[] AllocateUnique(byte[][] data)
		{
			/// @TODO Has to use data.Distinct() and the Allocate[] function in order
			///       to be able to avoid a greedy behaviour.

			var Spaces = new SpaceAssigner1D.Space[data.Length];
			for (int n = 0; n < data.Length; n++)
			{
				Spaces[n] = AllocateUnique(data[n]);
			}
			return Spaces;
		}

		public Encoding Encoding;

		public SpaceAssigner1D.Space AllocateUnique(String String, Encoding Encoding = null)
		{
			if (Encoding == null) Encoding = this.Encoding;
			return AllocateUnique(String.GetStringzBytes(Encoding));
		}
	}
}
