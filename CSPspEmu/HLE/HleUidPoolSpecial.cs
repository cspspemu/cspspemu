using System;
using System.Collections.Generic;
using System.Linq;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle.Managers
{
    public class HleUidPoolSpecial<TType, TKey> where TType : IDisposable
    {
        protected TKey LastId = default(TKey);
        protected Dictionary<TKey, TType> Items = new Dictionary<TKey, TType>();
        protected SceKernelErrors OnKeyNotFoundError = unchecked((SceKernelErrors) 0xDEAD0017);

        public HleUidPoolSpecial()
        {
            //if (!typeof(TType).Implements(typeof(IHleUidPoolClass)))
            //{
            //	throw (new Exception(String.Format("HleUidPoolSpecial: {0} not implements {1}", typeof(TType), typeof(IHleUidPoolClass))));
            //}

            var Attribute = typeof(TType).GetAttribute<HleUidPoolClassAttribute>().FirstOrDefault();
            if (Attribute != null)
            {
                this.OnKeyNotFoundError = Attribute.NotFoundError;
            }
            this.LastId = (TKey) (object) 1;
        }

        public HleUidPoolSpecial(TKey FirstId)
        {
            this.LastId = FirstId;
        }

        static Type TKeyType = typeof(TKey);

        private long TKeyToLong(TKey Value)
        {
            return Convert.ToInt64(Value);
        }

        private TKey LongToTKey(long Value)
        {
            if (TKeyType.IsEnum)
            {
                return (TKey) Enum.ToObject(typeof(TKey), Value);
            }
            if (TKeyType == typeof(int)) return (TKey) (object) (int) Value;
            return (TKey) (object) Value;
        }

        public TType Set(TKey Id, TType Value)
        {
            Items[Id] = Value;
            if (TKeyToLong(LastId) < TKeyToLong(Id) + 1) LastId = LongToTKey(TKeyToLong(Id) + 1);
            return Value;
        }

        /*
        public TKey? Find(TType Value)
        {
            foreach (var Pair in Items) if (Pair.Value.Equals(Value)) return Pair.Key;
            return null;
        }

        public TKey GetOrCreate(TType Item)
        {
            var Result = Find(Item);
            if (Result == null)
            {
                Result = Create(Item);
            }
            return Result.Value;
        }
        */

        public bool Contains(TKey Id)
        {
            return Items.ContainsKey(Id);
        }

        public TType Get(TKey Id)
        {
            if (!Items.ContainsKey(Id))
            {
                throw new SceKernelException(OnKeyNotFoundError);
            }
            return Items[Id];
        }

        public TKey Create(TType Item)
        {
            var Id = LastId;
            LastId = LongToTKey(TKeyToLong(LastId) + 1);
            Items[Id] = Item;
            return Id;
        }

        public void Remove(TKey Id)
        {
            if (!Items.ContainsKey(Id))
            {
                throw new SceKernelException(OnKeyNotFoundError);
            }
            var Item = Items[Id];
            Items.Remove(Id);
            Item.Dispose();
        }

        public void RemoveAll()
        {
            foreach (var Item in Items.ToArray())
            {
                Remove(Item.Key);
                Item.Value.Dispose();
            }
        }
    }
}