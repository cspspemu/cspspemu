using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.ge
{
	unsafe public partial class sceGe_user
	{
		/// <summary>
		/// Context.
		/// </summary>
		public struct PspGeContext
		{
			public fixed uint Data[512];
		}

		/// <summary>
		/// Save the GE's current state.Save the GE's current state.
		/// </summary>
		/// <param name="contextAddr">Pointer to a ::PspGeContext.</param>
		/// <returns>???</returns>
		[HlePspFunction(NID = 0x438A385A, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceGeSaveContext(PspGeContext* contextAddr)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Restore a previously saved GE context.
		/// </summary>
		/// <param name="contextAddr">Pointer to a ::PspGeContext.</param>
		/// <returns>???</returns>
		[HlePspFunction(NID = 0x0BF608FB, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceGeRestoreContext(PspGeContext* contextAddr)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Retrive the current value of a GE command.
		/// </summary>
		/// <param name="cmd">The GE command register to retrieve.</param>
		/// <returns>The value of the GE command.</returns>
		[HlePspFunction(NID = 0xDC93CFEF, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceGeGetCmd(int cmd)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Retrieve a matrix of the given type.
		/// </summary>
		/// <param name="MatrixType">One of ::PspGeMatrixTypes.</param>
		/// <param name="MatrixAddress">Pointer to a variable to store the matrix.</param>
		/// <returns>???</returns>
		[HlePspFunction(NID = 0x57C8945B, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceGeGetMtx(PspGeMatrixTypes MatrixType, uint* MatrixAddress)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Matrix types that can be retrieved.
		/// </summary>
		public enum PspGeMatrixTypes
		{
			Bone0 = 0,
			Bone1 = 1,
			Bone2 = 2,
			Bone3 = 3,
			Bone4 = 4,
			Bone5 = 5,
			Bone6 = 6,
			Bone7 = 7,
			World = 8,
			View = 9,
			Projection = 10,
			Texture = 11,
		}
	}
}
