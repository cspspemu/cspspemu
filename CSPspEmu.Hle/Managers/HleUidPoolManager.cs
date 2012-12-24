using CSPspEmu.Hle.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle
{
	public class HleUidPoolClassAttribute : Attribute
	{
		public SceKernelErrors NotFoundError = (SceKernelErrors)(-1);
		public int FirstItem = 1;
	}

	public interface IHleUidPoolClass
	{
	}

	static public class IHleUidPoolClassExtensions
	{
		static public void RemoveUid(this IHleUidPoolClass IHleUidPoolClass, InjectContext InjectContext)
		{
			InjectContext.GetInstance<HleUidPoolManager>().RemoveItem(IHleUidPoolClass.GetType(), IHleUidPoolClass);
		}

		static public int GetUidIndex(this IHleUidPoolClass IHleUidPoolClass, InjectContext InjectContext)
		{
			return InjectContext.GetInstance<HleUidPoolManager>().GetIndex(IHleUidPoolClass.GetType(), IHleUidPoolClass);
		}
	}
}

namespace CSPspEmu.Hle.Managers
{
	public class HleUidPoolManager
	{
		class TypePool
		{
			int LastId = 0;
			Type Type;
			HleUidPoolClassAttribute Info;
			Dictionary<int, IHleUidPoolClass> Items = new Dictionary<int, IHleUidPoolClass>();
			Dictionary<IHleUidPoolClass, int> RevItems = new Dictionary<IHleUidPoolClass, int>();

			public TypePool(Type Type)
			{
				this.Type = Type;
				this.Info = Type.GetAttribute<HleUidPoolClassAttribute>().FirstOrDefault();
				if (this.Info != null)
				{
					LastId = this.Info.FirstItem;
				}
			}

			public int Alloc(IHleUidPoolClass Item)
			{
				if (Item.GetType() != this.Type) throw(new InvalidOperationException("Trying to insert invalid object type"));
				int Index = LastId++;
				Items[Index] = Item;
				RevItems[Item] = Index;
				return Index;
			}

			public bool Contains(int Index)
			{
				return this.Items.ContainsKey(Index);
			}

			public void Remove(int Index)
			{
				RevItems.Remove(Items[Index]);
				Items.Remove(Index);
			}

			public void RemoveItem(IHleUidPoolClass Item)
			{
				Items.Remove(RevItems[Item]);
				RevItems.Remove(Item);
			}

			private void ThrowNotFound()
			{
				if (Info != null)
				{
					throw (new SceKernelException(Info.NotFoundError));
				}
				else
				{
					throw (new SceKernelException((SceKernelErrors)(-1)));
				}
			}

			public IHleUidPoolClass Get(int Index)
			{
				if (!Items.ContainsKey(Index)) ThrowNotFound();
				return Items[Index];
			}

			public int GetIndex(IHleUidPoolClass Item)
			{
				if (!RevItems.ContainsKey(Item)) ThrowNotFound();
				return RevItems[Item];
			}
		}

		Dictionary<Type, TypePool> Items = new Dictionary<Type, TypePool>();

		public HleUidPoolManager()
		{
		}

		private TypePool _GetTypePool(Type Type)
		{
			if (!Items.ContainsKey(Type)) Items[Type] = new TypePool(Type);
			return Items[Type];
		}

		public int Alloc(Type Type, IHleUidPoolClass Item)
		{
			return _GetTypePool(Type).Alloc(Item);
		}

		public bool Contains(Type Type, int Index)
		{
			return _GetTypePool(Type).Contains(Index);
		}

		public void Remove(Type Type, int Index)
		{
			_GetTypePool(Type).Remove(Index);
		}

		public void RemoveItem(Type Type, IHleUidPoolClass Item)
		{
			_GetTypePool(Type).RemoveItem(Item);
		}

		public IHleUidPoolClass Get(Type Type, int Index)
		{
			return _GetTypePool(Type).Get(Index);
		}

		public int GetIndex(Type Type, IHleUidPoolClass Item)
		{
			return _GetTypePool(Type).GetIndex(Item);
		}
	}
}
