using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Managers
{
	public class HleUidPool<TType>
	{
		protected uint LastId = 0;
		protected Dictionary<uint, TType> Items = new Dictionary<uint, TType>();

		public HleUidPool()
		{
		}

		public TType Get(uint Id)
		{
			return Items[Id];
		}

		public uint Create(TType Item)
		{
			var Id = LastId++;
			Items[Id] = Item;
			return Id;
		}

		public void Remove(uint Id)
		{
			Items.Remove(Id);
		}
	}
}
