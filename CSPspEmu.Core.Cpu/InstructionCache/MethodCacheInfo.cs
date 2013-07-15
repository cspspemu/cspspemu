using CSPspEmu.Core.Cpu.Dynarec;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu.InstructionCache
{
	public sealed class MethodCacheInfo
	{
		static public readonly MethodCacheInfo Methods = new MethodCacheInfo();

		/// <summary>
		/// 
		/// </summary>
		public DynarecFunction DynarecFunction;

		public bool HasSpecialName
		{
			get
			{
				return (DynarecFunction != null) && !String.IsNullOrEmpty(DynarecFunction.Name);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string Name
		{
			get
			{
				if (HasSpecialName) return DynarecFunction.Name;
				return String.Format("0x{0:X8}", EntryPC);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public MethodCache MethodCache;

		/// <summary>
		/// Functions that are calling to this one.
		/// And that should be uncached when this function
		/// </summary>
		//public List<MethodCacheInfo> FunctionsUsingThis = new List<MethodCacheInfo>();

		/// <summary>
		/// Static Field that will hold the Delegate
		/// </summary>
		public ILInstanceHolderPoolItem<Action<CpuThreadState>> StaticField;

		/// <summary>
		/// 
		/// </summary>
		public bool FollowPspCallingConventions;

		/// <summary>
		/// EntryPoint for this function.
		/// </summary>
		public uint EntryPC;

		/// <summary>
		/// Address of the start of the function. Usually is equal to EntryPC but not always.
		/// </summary>
		public uint MinPC;

		/// <summary>
		/// Last address with code for this function.
		/// </summary>
		public uint MaxPC;

		/// <summary>
		/// Ast for this function.
		/// </summary>
		public AstNodeStm AstTree { get { return DynarecFunction != null ? DynarecFunction.AstNode : null; } } 

		/// <summary>
		/// 
		/// </summary>
		/// <param name="CpuThreadState"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CallDelegate(CpuThreadState CpuThreadState)
		{
			//if (StaticField.Value == null) throw(new Exception(String.Format("Delegate not set! at 0x{0:X8}", EntryPC)));
			StaticField.Value(CpuThreadState);
		}

		/// <summary>
		/// 
		/// </summary>
		public void Free()
		{
			MethodCache.Free(this);
		}
	}
}
