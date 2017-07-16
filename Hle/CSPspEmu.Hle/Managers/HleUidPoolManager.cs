using CSPspEmu.Hle.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace CSPspEmu.Hle
{
    public class HleUidPoolClassAttribute : Attribute
    {
        public SceKernelErrors NotFoundError = (SceKernelErrors) (-1);
        public int FirstItem = 1;
        public bool ReuseIds = false;
    }

    public interface IHleUidPoolClass : IDisposable
    {
    }

    static public class IHleUidPoolClassExtensions
    {
        static public T AllocateUid<T>(this T IHleUidPoolClass, InjectContext InjectContext) where T : IHleUidPoolClass
        {
            InjectContext.GetInstance<HleUidPoolManager>().Alloc(IHleUidPoolClass.GetType(), IHleUidPoolClass);
            return IHleUidPoolClass;
        }

        static public void RemoveUid(this IHleUidPoolClass IHleUidPoolClass, InjectContext InjectContext)
        {
            InjectContext.GetInstance<HleUidPoolManager>().RemoveItem(IHleUidPoolClass.GetType(), IHleUidPoolClass);
        }

        static public int GetUidIndex(this IHleUidPoolClass IHleUidPoolClass, InjectContext InjectContext)
        {
            return InjectContext.GetInstance<HleUidPoolManager>()
                .GetOrAllocIndex(IHleUidPoolClass.GetType(), IHleUidPoolClass);
        }
    }
}

namespace CSPspEmu.Hle.Managers
{
    public class HleUidPoolManager
    {
        class TypePool
        {
            private int LastId = 0;
            private Type Type;
            private HleUidPoolClassAttribute Info;
            private readonly HashSet<int> FreedIds = new HashSet<int>();
            private readonly Dictionary<int, IHleUidPoolClass> Items = new Dictionary<int, IHleUidPoolClass>();
            private readonly Dictionary<IHleUidPoolClass, int> RevItems = new Dictionary<IHleUidPoolClass, int>();

            public TypePool(Type Type)
            {
                this.Type = Type;
                this.Info = Type.GetAttribute<HleUidPoolClassAttribute>().FirstOrDefault();
                if (this.Info != null)
                {
                    LastId = this.Info.FirstItem;
                }
            }

            public bool ReuseIds
            {
                get { return (this.Info != null) ? this.Info.ReuseIds : false; }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public int Alloc(IHleUidPoolClass Item)
            {
                if (Item.GetType() != this.Type)
                    throw(new InvalidOperationException("Trying to insert invalid object type"));
                int Index = -1;

                if (ReuseIds)
                {
                    //Console.Error.WriteLine("******************************************");
                    if (FreedIds.Count > 0)
                    {
                        Index = FreedIds.Min();
                        FreedIds.Remove(Index);
                    }
                }

                if (Index == -1) Index = LastId++;

                Items[Index] = Item;
                RevItems[Item] = Index;
                return Index;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public bool Contains(int Index)
            {
                return this.Items.ContainsKey(Index);
            }

            //[MethodImpl(MethodImplOptions.Synchronized)]
            public void RemoveItem(IHleUidPoolClass Item)
            {
                Item.Dispose();
                if (ReuseIds) FreedIds.Add(RevItems[Item]);
                Items.Remove(RevItems[Item]);
                RevItems.Remove(Item);
            }

            //[MethodImpl(MethodImplOptions.Synchronized)]
            public void Remove(int Index)
            {
                RemoveItem(Items[Index]);
            }

            public void RemoveAll()
            {
                foreach (var Item in List().ToArray())
                {
                    RemoveItem(Item);
                }
            }

            private void ThrowNotFound()
            {
                if (Info != null)
                {
                    throw (new SceKernelException(Info.NotFoundError));
                }
                else
                {
                    throw (new SceKernelException((SceKernelErrors) (-1)));
                }
            }

            //[MethodImpl(MethodImplOptions.Synchronized)]
            public IHleUidPoolClass Get(int Index, bool CanReturnNull = false)
            {
                if (!Items.ContainsKey(Index))
                {
                    if (CanReturnNull) return null;
                    ThrowNotFound();
                }
                return Items[Index];
            }

            public int GetOrAllocIndex(IHleUidPoolClass Item)
            {
                if (!RevItems.ContainsKey(Item)) return Alloc(Item);
                return GetIndex(Item);
            }

            public int GetIndex(IHleUidPoolClass Item)
            {
                if (!RevItems.ContainsKey(Item)) ThrowNotFound();
                return RevItems[Item];
            }

            public int Count
            {
                get { return Items.Count; }
            }

            public IEnumerable<IHleUidPoolClass> List()
            {
                foreach (var Value in Items.Values) yield return Value;
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

        public int Count(Type Type)
        {
            return _GetTypePool(Type).Count;
        }

        public IEnumerable<TType> List<TType>()
        {
            return _GetTypePool(typeof(TType)).List().Select(Item => (TType) Item);
        }

        public void Remove(Type Type, int Index)
        {
            _GetTypePool(Type).Remove(Index);
        }

        public void RemoveItem(Type Type, IHleUidPoolClass Item)
        {
            _GetTypePool(Type).RemoveItem(Item);
        }

        public IHleUidPoolClass Get(Type Type, int Index, bool CanReturnNull = false)
        {
            return _GetTypePool(Type).Get(Index, CanReturnNull);
        }

        public int GetOrAllocIndex(Type Type, IHleUidPoolClass Item)
        {
            return _GetTypePool(Type).GetOrAllocIndex(Item);
        }

        public int GetIndex(Type Type, IHleUidPoolClass Item)
        {
            return _GetTypePool(Type).GetIndex(Item);
        }

        public void RemoveAll<TType>()
        {
            _GetTypePool(typeof(TType)).RemoveAll();
        }
    }
}