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
		private DynarecFunction _DynarecFunction;

		/// <summary>
		/// 
		/// </summary>
		private Action<CpuThreadState> FunctionDelegate;

		public DynarecFunction DynarecFunction
		{
			get
			{
				return _DynarecFunction;
			}
		}

		public void SetDynarecFunction(DynarecFunction DynarecFunction)
		{
			this._DynarecFunction = DynarecFunction;
			this.FunctionDelegate = DynarecFunction.Delegate;
			this.StaticField.Value = DynarecFunction.Delegate;
		}

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

		private MethodCacheInfo()
		{
		}

		public MethodCacheInfo(MethodCache MethodCache, Action<CpuThreadState> DelegateGeneratorForPC, uint PC)
		{
			this.MethodCache = MethodCache;
			this.FunctionDelegate = DelegateGeneratorForPC;
			this.StaticField = ILInstanceHolder.TAlloc<Action<CpuThreadState>>(DelegateGeneratorForPC);
			this.PC = PC;
		}

		/// <summary>
		/// EntryPoint setted first.
		/// </summary>
		public uint PC;

		/// <summary>
		/// EntryPoint for this function.
		/// </summary>
		public uint EntryPC { get { return DynarecFunction.EntryPC; } }

		/// <summary>
		/// Address of the start of the function. Usually is equal to EntryPC but not always.
		/// </summary>
		public uint MinPC { get { return DynarecFunction.MinPC; } }

		/// <summary>
		/// Last address with code for this function.
		/// </summary>
		public uint MaxPC { get { return DynarecFunction.MaxPC; } }

		/// <summary>
		/// 
		/// </summary>
		public uint TotalInstructions { get { return (DynarecFunction.MaxPC - DynarecFunction.MinPC) / 7; } }

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
			FunctionDelegate(CpuThreadState);
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
