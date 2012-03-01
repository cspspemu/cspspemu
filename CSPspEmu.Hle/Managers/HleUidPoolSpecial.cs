using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Managers
{
	public class HleUidPoolSpecial<TType, TKey>
	{
		protected TKey LastId = default(TKey);
		protected Dictionary<TKey, TType> Items = new Dictionary<TKey, TType>();
		public SceKernelErrors OnKeyNotFoundError = (SceKernelErrors)(-1);

		public HleUidPoolSpecial()
		{
#if true
			this.LastId = (TKey)(dynamic)1;
#else
			this.LastId = (TKey)(dynamic)0;
#endif
		}

		public HleUidPoolSpecial(TKey FirstId)
		{
			this.LastId = FirstId;
		}

		public TType Set(TKey Id, TType Value)
		{
			Items[(dynamic)Id] = Value;
			if ((dynamic)(LastId) < (dynamic)Id + 1) LastId = (dynamic)Id + 1;
			return Value;
		}

		public TType Get(TKey Id)
		{
			if (!Items.ContainsKey(Id))
			{
				throw (new SceKernelException(OnKeyNotFoundError));
			}
			return Items[Id];
		}

		public TKey Create(TType Item)
		{
			var Id = LastId;
			LastId = (dynamic)LastId + 1;
			Items[(dynamic)Id] = Item;
			return Id;
		}

		public void Remove(TKey Id)
		{
			if (!Items.ContainsKey(Id))
			{
				throw (new SceKernelException(OnKeyNotFoundError));
			}
			Items.Remove(Id);
		}

		public void RemoveAll()
		{
			foreach (var Item in Items.ToArray()) Remove(Item.Key);
		}
	}
}
