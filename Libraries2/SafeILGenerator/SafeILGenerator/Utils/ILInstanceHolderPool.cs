using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Utils
{
	public class ILInstanceHolderPoolItem<TType>
	{
		private ILInstanceHolderPoolItem Item;
		public int Index { get { return Item.Index; } }
		public FieldInfo FieldInfo { get { return Item.FieldInfo; } }

		public ILInstanceHolderPoolItem(ILInstanceHolderPoolItem Item)
		{
			this.Item = Item;
		}

		public TType Value
		{
			//[MethodImpl(MethodImplOptions.AggressiveInlining)]
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			set
			{
				Item.Value = value;
			}
			//[MethodImpl(MethodImplOptions.AggressiveInlining)]
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				return (TType)Item.Value;
			}
		}

		public void Free()
		{
			Item.Free();
		}

		public AstNodeExprStaticFieldAccess AstFieldAccess
		{
			get
			{
				return Item.GetAstFieldAccess();
			}
		}

		//public AstNodeExprStaticFieldAccess GetAstFieldAccess()
		//{
		//	return Item.GetAstFieldAccess();
		//}
	}

	public class ILInstanceHolderPoolItem : IDisposable
	{
		private readonly ILInstanceHolderPool Parent;
		public readonly int Index;
		internal bool Allocated;
		public readonly FieldInfo FieldInfo;

		public ILInstanceHolderPoolItem(ILInstanceHolderPool Parent, int Index, FieldInfo FieldInfo)
		{
			this.Parent = Parent;
			this.Index = Index;
			this.FieldInfo = FieldInfo;
		}

		public object Value
		{
			//[MethodImpl(MethodImplOptions.AggressiveInlining)]
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			set
			{
				FieldInfo.SetValue(null, value);
			}
			//[MethodImpl(MethodImplOptions.AggressiveInlining)]
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				return FieldInfo.GetValue(null);
			}
		}

		public void Free()
		{
			if (Allocated)
			{
				Allocated = false;
				Parent.Free(this);
			}
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

	public class ILInstanceHolderPool
	{
		private static AstGenerator ast = AstGenerator.Instance;

		public readonly Type ItemType;
		private ILInstanceHolderPoolItem[] FieldInfos;
		private LinkedList<int> FreeItems = new LinkedList<int>();
		private Type HolderType;
		private static int Autoincrement = 0;
		public readonly int CapacityCount;

		public int FreeCount
		{
			get
			{
				return FreeItems.Count;
			}
		}

		public bool HasAvailable
		{
			get
			{
				return FreeCount > 0;
			}
		}

		public ILInstanceHolderPoolItem Alloc()
		{
			var Item = FieldInfos[FreeItems.First.Value];
			FreeItems.RemoveFirst();
			Item.Allocated = true;
			Item.Value = null;
			return Item;
		}

		internal void Free(ILInstanceHolderPoolItem Item)
		{
			FreeItems.AddLast(Item.Index);
		}

		private static string DllName = "Temp.dll";
		private static AssemblyBuilder AssemblyBuilder;
		private static ModuleBuilder ModuleBuilder;

		static ILInstanceHolderPool()
		{
			AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
				new AssemblyName("DynamicAssembly" + Autoincrement++),
				AssemblyBuilderAccess.RunAndCollect,
				DllName
			);
			ModuleBuilder = AssemblyBuilder.DefineDynamicModule(AssemblyBuilder.GetName().Name, DllName, false);
		}

		public ILInstanceHolderPool(Type ItemType, int Count, string TypeName = null)
		{
			this.ItemType = ItemType;
			this.CapacityCount = Count;
			if (TypeName == null) TypeName = "DynamicType" + Autoincrement++;
			var TypeBuilder = ModuleBuilder.DefineType(TypeName, TypeAttributes.Sealed | TypeAttributes.Public | TypeAttributes.Class);
			FieldInfos = new ILInstanceHolderPoolItem[Count];
			var Names = new string[Count];
			for (int n = 0; n < Count; n++)
			{
				Names[n] = String.Format("V{0}", n);
				TypeBuilder.DefineField(Names[n], ItemType, FieldAttributes.Public | FieldAttributes.Static);
			}

			HolderType = TypeBuilder.CreateType();

			var Fields = HolderType.GetFields();
			for (int n = 0; n < Count; n++)
			{
				FieldInfos[n] = new ILInstanceHolderPoolItem(this, n, Fields[n]);
				FreeItems.AddLast(n);
			}
		}
	}
}
