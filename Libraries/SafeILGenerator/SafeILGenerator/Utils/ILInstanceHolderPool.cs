using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime;

namespace SafeILGenerator.Utils
{
    public class ILInstanceHolderPoolItem<TType>
    {
        private IlInstanceHolderPoolItem Item;
        public int Index => Item.Index;
        public FieldInfo FieldInfo => Item.FieldInfo;

        public ILInstanceHolderPoolItem(IlInstanceHolderPoolItem item)
        {
            Item = item;
        }

        public TType Value
        {
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            set { Item.Value = value; }
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get { return (TType) Item.Value; }
        }

        public void Free() => Item.Free();

        public AstNodeExprStaticFieldAccess AstFieldAccess => Item.GetAstFieldAccess();

        //public AstNodeExprStaticFieldAccess GetAstFieldAccess()
        //{
        //	return Item.GetAstFieldAccess();
        //}
    }

    public class IlInstanceHolderPoolItem : IDisposable
    {
        private readonly IlInstanceHolderPool _parent;
        public readonly int Index;
        internal bool Allocated;
        public readonly FieldInfo FieldInfo;

        public IlInstanceHolderPoolItem(IlInstanceHolderPool parent, int index, FieldInfo fieldInfo)
        {
            _parent = parent;
            Index = index;
            FieldInfo = fieldInfo;
        }

        public object Value
        {
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            set { FieldInfo.SetValue(null, value); }
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get { return FieldInfo.GetValue(null); }
        }

        public void Free()
        {
            if (!Allocated) return;
            Allocated = false;
            _parent.Free(this);
        }

        public AstNodeExprStaticFieldAccess GetAstFieldAccess()
        {
            if (FieldInfo == null) throw (new Exception("FieldInfo == null"));
            return new AstNodeExprStaticFieldAccess(FieldInfo);
        }

        void IDisposable.Dispose()
        {
            Free();
        }
    }

    public class IlInstanceHolderPool
    {
        //private static AstGenerator _ast = AstGenerator.Instance;

        public readonly Type ItemType;
        private IlInstanceHolderPoolItem[] FieldInfos;
        private LinkedList<int> FreeItems = new LinkedList<int>();
        private static int _autoincrement;
        public readonly int CapacityCount;

        public int FreeCount => FreeItems.Count;
        public bool HasAvailable => FreeCount > 0;

        public IlInstanceHolderPoolItem Alloc()
        {
            var item = FieldInfos[FreeItems.First.Value];
            FreeItems.RemoveFirst();
            item.Allocated = true;
            item.Value = null;
            return item;
        }

        internal void Free(IlInstanceHolderPoolItem item) => FreeItems.AddLast(item.Index);

        private static string DllName = "Temp.dll";
        private static readonly ModuleBuilder ModuleBuilder;

        static IlInstanceHolderPool()
        {
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("DynamicAssembly" + _autoincrement++),
                AssemblyBuilderAccess.RunAndCollect,
                DllName
            );
            ModuleBuilder = assemblyBuilder.DefineDynamicModule(assemblyBuilder.GetName().Name, DllName, false);
        }

        public IlInstanceHolderPool(Type itemType, int count, string typeName = null)
        {
            ItemType = itemType;
            CapacityCount = count;
            if (typeName == null) typeName = "DynamicType" + _autoincrement++;
            var typeBuilder = ModuleBuilder.DefineType(typeName,
                TypeAttributes.Sealed | TypeAttributes.Public | TypeAttributes.Class);
            FieldInfos = new IlInstanceHolderPoolItem[count];
            var names = new string[count];
            for (var n = 0; n < count; n++)
            {
                names[n] = $"V{n}";
                typeBuilder.DefineField(names[n], itemType, FieldAttributes.Public | FieldAttributes.Static);
            }

            var holderType = typeBuilder.CreateType();

            var fields = holderType.GetFields();
            for (var n = 0; n < count; n++)
            {
                FieldInfos[n] = new IlInstanceHolderPoolItem(this, n, fields[n]);
                FreeItems.AddLast(n);
            }
        }
    }
}