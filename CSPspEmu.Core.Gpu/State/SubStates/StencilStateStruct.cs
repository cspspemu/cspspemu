using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
	public struct StencilStateStruct
	{
		/// <summary>
		/// Stencil Test Enable (GL_STENCIL_TEST)
		/// </summary>
		public bool Enabled;

		/// <summary>
		/// 
		/// </summary>
		public TestFunctionEnum Function;

		/// <summary>
		/// 
		/// </summary>
		public byte FunctionRef;

		/// <summary>
		/// 0xFF
		/// </summary>
		public byte FunctionMask;

		/// <summary>
		/// 
		/// </summary>
		public StencilOperationEnum OperationSFail;

		/// <summary>
		/// 
		/// </summary>
		public StencilOperationEnum OperationDpFail;

		/// <summary>
		/// 
		/// </summary>
		public StencilOperationEnum OperationDpPass;
	}
}
