using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu
{
	/// <summary>
	/// alias void function(int id, void *arg) PspGeCallback;
	/// </summary>
	public enum PspGeCallback : uint { }

	/// <summary>
	/// 
	/// </summary>
	public struct PspGeCallbackData
	{
		/// <summary>
		/// GE callback for the signal interrupt
		/// alias void function(int id, void *arg) PspGeCallback;
		/// </summary>
		public uint SignalFunction;

		/// <summary>
		/// GE callback argument for signal interrupt
		/// </summary>
		public uint SignalArgument;

		/// <summary>
		/// GE callback for the finish interrupt
		/// alias void function(int id, void *arg) PspGeCallback;
		/// </summary>
		public uint FinishFunction;

		/// <summary>
		/// GE callback argument for finish interrupt
		/// </summary>
		public uint FinishArgument;

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.ToStringDefault();
		}
	}
}
