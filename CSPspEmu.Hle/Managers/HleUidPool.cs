using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Managers
{
	public class HleUidPool<TType>
	{
		protected int LastId = 1;
		protected Dictionary<int, TType> Items = new Dictionary<int, TType>();

		public HleUidPool(int FirstId = 1)
		{
			this.LastId = FirstId;
		}

		public TType Set(int Id, TType Value)
		{
			Items[Id] = Value;
			if (LastId < Id + 1) LastId = Id + 1;
			return Value;
		}

		public TType Get(int Id)
		{
			return Items[Id];
		}

		public int Create(TType Item)
		{
			var Id = LastId++;
			Items[Id] = Item;
			return Id;
		}

		public void Remove(int Id)
		{
			Items.Remove(Id);
		}
	}
}
