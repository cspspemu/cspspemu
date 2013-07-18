using System;
using System.Text;
using CSharpUtils;
using CSPspEmu.Core;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.registry
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
	public unsafe class sceReg : HleModuleHost
	{
		[Inject]
		HleRegistryManager HleRegistryManager;

		[Inject]
		HleConfig HleConfig;

		/// <summary>
		/// Open the registry
		/// </summary>
		/// <param name="RegParam">A filled in ::RegParam structure</param>
		/// <param name="Mode">Open mode (set to 1)</param>
		/// <param name="RegHandle">Pointer to a REGHANDLE to receive the registry handle</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x92E41280, FirmwareVersion = 150)]
		public int sceRegOpenRegistry(RegParam* RegParam, OpenRegistryMode Mode, out RegHandle RegHandle)
		{
			var HleRegistryNode = new HleRegistryNode(RegParam[0].Name);
			RegHandle = (RegHandle)HleRegistryManager.RegHandles.Create(HleRegistryNode);
			return 0;
		}

		/// <summary>
		/// Flush the registry to disk
		/// </summary>
		/// <param name="RegHandle">The open registry handle</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x39461B4D, FirmwareVersion = 150)]
		public int sceRegFlushRegistry(RegHandle RegHandle)
		{
			var HleRegistryNode = HleRegistryManager.RegHandles.Get((int)RegHandle);
			HleRegistryNode.Flush();
			return 0;
		}

		/// <summary>
		/// Close the registry 
		/// </summary>
		/// <param name="RegHandle">The open registry handle</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xFA8A5739, FirmwareVersion = 150)]
		public int sceRegCloseRegistry(RegHandle RegHandle)
		{
			HleRegistryManager.RegHandles.Remove((int)RegHandle);
			return 0;
		}

		public enum OpenRegistryMode : uint
		{
			Read = 1,
			Write = 2,
		}

		/// <summary>
		/// Open a registry directory
		/// </summary>
		/// <param name="RegHandle">The open registry handle</param>
		/// <param name="Name">The path to the dir to open (e.g. /CONFIG/SYSTEM)</param>
		/// <param name="Mode">Open mode (can be 1 or 2, probably read or read/write</param>
		/// <param name="RegCategoryHandle">Pointer to a REGHANDLE to receive the registry dir handle</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x1D8A762E, FirmwareVersion = 150)]
		public int sceRegOpenCategory(RegHandle RegHandle, string Name, OpenRegistryMode Mode, out RegCategoryHandle RegCategoryHandle)
		{
			var HleRegistryNode = HleRegistryManager.RegHandles.Get((int)RegHandle);
			var HleRegistryCategoryNode = new HleRegistryCategoryNode(HleConfig, HleRegistryNode, Name);
			RegCategoryHandle = (RegCategoryHandle)HleRegistryManager.RegCategoryHandles.Create(HleRegistryCategoryNode);

			return 0;
		}


		/// <summary>
		/// Flush the registry directory to disk
		/// </summary>
		/// <param name="RegCategoryHandle">The open registry dir handle</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x0D69BF40, FirmwareVersion = 150)]
		public int sceRegFlushCategory(RegCategoryHandle RegCategoryHandle)
		{
			var HleRegistryCategoryNode = HleRegistryManager.RegCategoryHandles.Get((int)RegCategoryHandle);
			HleRegistryCategoryNode.Flush();
			return 0;
		}

		/// <summary>
		/// Close the registry directory
		/// </summary>
		/// <param name="RegCategoryHandle">The open registry dir handle</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x0CAE832B, FirmwareVersion = 150)]
		public int sceRegCloseCategory(RegCategoryHandle RegCategoryHandle)
		{
			HleRegistryManager.RegCategoryHandles.Remove((int)RegCategoryHandle);
			return 0;
		}

		/// <summary>
		/// Get a key's information
		/// </summary>
		/// <param name="RegCategoryHandle">The open registry dir handle</param>
		/// <param name="Name">Name of the key</param>
		/// <param name="RegKeyHandle">Pointer to a REGHANDLE to get registry key handle</param>
		/// <param name="Type">Type of the key, on of ::RegKeyTypes</param>
		/// <param name="Size">The size of the key's value in bytes</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xD4475AA8, FirmwareVersion = 150)]
		public int sceRegGetKeyInfo(RegCategoryHandle RegCategoryHandle, string Name, RegKeyHandle* RegKeyHandle, RegKeyTypes* Type, uint* Size)
		{
			var HleRegistryCategoryNode = HleRegistryManager.RegCategoryHandles.Get((int)RegCategoryHandle);
			var KeyNode = HleRegistryCategoryNode.GetKeyByName(Name);
			if (RegKeyHandle != null) *RegKeyHandle = KeyNode.Id;
			if (Type != null) *Type = KeyNode.Type;
			if (Size != null) *Size = KeyNode.Size;

			return 0;
		}

		/// <summary>
		/// Set a key's value
		/// </summary>
		/// <param name="RegCategoryHandle">The open registry dir handle</param>
		/// <param name="Name">The key name</param>
		/// <param name="Buffer">Buffer to hold the value</param>
		/// <param name="Size">The size of the buffer</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x17768E14, FirmwareVersion = 150)]
		public int sceRegSetKeyValue(RegCategoryHandle RegCategoryHandle, string Name, void* Buffer, uint Size)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Get a key's value
		/// </summary>
		/// <param name="RegCategoryHandle">The open registry dir handle</param>
		/// <param name="RegKeyHandle">The open registry key handler (from ::sceRegGetKeyInfo)</param>
		/// <param name="Buffer">Buffer to hold the value</param>
		/// <param name="Size">The size of the buffer</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x28A8E98A, FirmwareVersion = 150)]
		public int sceRegGetKeyValue(RegCategoryHandle RegCategoryHandle, RegKeyHandle RegKeyHandle, void* Buffer, uint Size)
		{
			var HleRegistryCategoryNode = HleRegistryManager.RegCategoryHandles.Get((int)RegCategoryHandle);
			var KeyNode = HleRegistryCategoryNode.GetKeyNodeById(RegKeyHandle);
			KeyNode.Write(Buffer, Size);
			return 0;
		}
	}

	/// <summary>
	/// Struct used to open a registry
	/// </summary>
	public unsafe struct RegParam
	{
		/// <summary>
		/// Set to 1 only for system
		/// </summary>
		public uint RegType; // 0x0

		/// <summary>
		/// Seemingly never used, set to ::SYSTEM_REGISTRY
		/// </summary>
		public fixed byte NameRaw[256]; // 0x4-0x104

		/// <summary>
		/// Length of the name
		/// </summary>
		public int NameLength; // 0x104
		
		/// <summary>
		/// Unknown, set to 1
		/// </summary>
		public uint Unknown2; // 0x108
		
		/// <summary>
		/// Unknown, set to 1
		/// </summary>
		public uint Unknown3; // 0x10C

		public string Name
		{
			get
			{
				fixed (byte* Name = NameRaw)
				{
					return PointerUtils.PtrToString(Name, NameLength, Encoding.UTF8);
				}
			}
		}
	}

	public struct RegistryEntry
	{

	}
}
