using System.Reflection;
using System.Runtime;
using SafeILGenerator.Ast.Nodes;

namespace SafeILGenerator.Utils
{
    public class IlInstanceHolderPoolItem<TType>
    {
        private readonly IlInstanceHolderPoolItem _item;
        public int Index => _item.Index;
        public FieldInfo FieldInfo => _item.FieldInfo;

        public IlInstanceHolderPoolItem(IlInstanceHolderPoolItem item) => _item = item;

        public TType Value
        {
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            set { _item.Value = value; }
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get { return (TType) _item.Value; }
        }

        public void Free() => _item.Free();

        public AstNodeExprStaticFieldAccess AstFieldAccess => _item.GetAstFieldAccess();

        //public AstNodeExprStaticFieldAccess GetAstFieldAccess()
        //{
        //	return Item.GetAstFieldAccess();
        //}
    }
}