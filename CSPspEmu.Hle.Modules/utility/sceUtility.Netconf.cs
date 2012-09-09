using System.Text;
using CSharpUtils;

namespace CSPspEmu.Hle.Modules.utility
{
	public unsafe partial class sceUtility
	{
		/// <summary>
		/// Get the status of a running Network Configuration Dialog
		/// </summary>
		/// <returns>one of pspUtilityDialogState on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x6332AA39, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public pspUtilityDialogState sceUtilityNetconfGetStatus()
		{
			return pspUtilityDialogState.Finished;
		}

		/// <summary>
		/// Check existance of a Net Configuration
		/// </summary>
		/// <param name="id">id of net Configuration (1 to n)</param>
		/// <returns>0 on success, </returns>
		[HlePspFunction(NID = 0x5EEE6548, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceUtilityCheckNetParam(int id)
		{
			//throw (new NotImplementedException());
			return 0;
		}

		/**
		 * Datatype for sceUtilityGetNetParam
		 * since it can return a u32 or a string
		 * we use a union to avoid ugly casting
		 */
		public struct netData
		{
			public fixed byte asString[128];

			public uint asUint
			{
				get
				{
					fixed (byte* _asString = asString)
					{
						return *(uint*)_asString;
					}
				}
			}
		}

		/// <summary>
		/// Get Net Configuration Parameter
		/// </summary>
		/// <param name="conf">Net Configuration number (1 to n) (0 returns valid but seems to be a copy of the last config requested)</param>
		/// <param name="param">which parameter to get</param>
		/// <param name="data">parameter data</param>
		/// <returns>0 on success, </returns>
		[HlePspFunction(NID = 0x434D4B3A, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceUtilityGetNetParam(int conf, int param, netData* data)
		{
			PointerUtils.StoreStringOnPtr("Temp", Encoding.UTF8, data[0].asString, 128);
			//throw(new NotImplementedException());
			return 0;
		}
	}
}
