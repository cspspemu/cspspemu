using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Hle.Managers;
using System.Runtime.InteropServices;

namespace CSPspEmu.Hle.Modules.utility
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
	unsafe public partial class sceUtility : HleModuleHost
	{
		public enum DialogStepEnum
		{
			/// <summary>
			/// 
			/// </summary>
			NONE = 0,

			/// <summary>
			///
			/// </summary>
			INIT = 1,

			/// <summary>
			/// If the GUI is visible (you need to call sceUtilityMsgDialogGetStatus).
			/// </summary>
			PROCESSING = 2,

			/// <summary>
			/// If the user cancelled the dialog, and you need to call sceUtilityMsgDialogShutdownStart.
			/// </summary>
			SUCCESS = 3,

			/// <summary>
			/// If the dialog has been successfully shut down.
			/// </summary>
			SHUTDOWN = 4,
		}

		/*
		union netData {
			u32 asUint;
			char asString[128];
		}
		*/

		public enum PSP_NET_MODULE
		{
			PSP_NET_MODULE_COMMON    = 1,
			PSP_NET_MODULE_ADHOC     = 2,
			PSP_NET_MODULE_INET      = 3,
			PSP_NET_MODULE_PARSEURI  = 4,
			PSP_NET_MODULE_PARSEHTTP = 5,
			PSP_NET_MODULE_HTTP      = 6,
			PSP_NET_MODULE_SSL       = 7,
		}

		public enum PSP_AV_MODULE : uint
		{
			PSP_AV_MODULE_AVCODEC    = 0,
			PSP_AV_MODULE_SASCORE    = 1,
			PSP_AV_MODULE_ATRAC3PLUS = 2, // Requires PSP_AV_MODULE_AVCODEC loading first
			PSP_AV_MODULE_MPEGBASE   = 3, // Requires PSP_AV_MODULE_AVCODEC loading first
			PSP_AV_MODULE_MP3        = 4,
			PSP_AV_MODULE_VAUDIO     = 5,
			PSP_AV_MODULE_AAC        = 6,
			PSP_AV_MODULE_G729       = 7,
		}

		/// Save data utility modes
		public enum PspUtilitySavedataMode : uint
		{
			/// <summary>
			/// PSP_UTILITY_SAVEDATA_AUTOLOAD = 0
			/// </summary>
			Autoload = 0,

			/// <summary>
			/// PSP_UTILITY_SAVEDATA_AUTOSAVE = 1
			/// </summary>
			Autosave = 1,

			/// <summary>
			/// PSP_UTILITY_SAVEDATA_LOAD = 2
			/// </summary>
			Load = 2,
			
			/// <summary>
			/// PSP_UTILITY_SAVEDATA_SAVE = 3
			/// </summary>
			Save = 3,

			/// <summary>
			/// PSP_UTILITY_SAVEDATA_LISTLOAD = 4
			/// </summary>
			ListLoad = 4,

			/// <summary>
			/// PSP_UTILITY_SAVEDATA_LISTSAVE = 5 
			/// </summary>
			ListSave = 5,

			/// <summary>
			/// PSP_UTILITY_SAVEDATA_LISTDELETE = 6
			/// </summary>
			ListDelete = 6,
			
			/// <summary>
			/// PSP_UTILITY_SAVEDATA_DELETE = 7
			/// </summary>
			Delete = 7,

			/// <summary>
			/// PSP_UTILITY_SAVEDATA_SIZES = 8
			/// </summary>
			Sizes = 8,
			
			/// <summary>
			/// PSP_UTILITY_SAVEDATA_AUTODELETE = 9
			/// </summary>
			AutoDelete = 9,

			/// <summary>
			/// PSP_UTILITY_SAVEDATA_SINGLEDELETE = 10 = 0x0A
			/// </summary>
			SingleDelete = 10,
			
			/// <summary>
			/// PSP_UTILITY_SAVEDATA_LIST = 11 = 0x0B
			/// </summary>
			List = 11,
			
			/// <summary>
			/// PSP_UTILITY_SAVEDATA_FILES = 12 = 0x0C
			/// </summary>
			Files = 12,

			/// <summary>
			/// PSP_UTILITY_SAVEDATA_MAKEDATASECURE = 13 = 0x0D
			/// </summary>
			MakeDataSecure = 13,

			/// <summary>
			/// PSP_UTILITY_SAVEDATA_MAKEDATA = 14 = 0x0E
			/// </summary>
			MakeData = 14,

			/// <summary>
			/// PSP_UTILITY_SAVEDATA_READSECURE = 15 = 0x0F
			/// </summary>
			ReadSecure = 15,

			/// <summary>
			/// PSP_UTILITY_SAVEDATA_READ = 16 = 0x10
			/// </summary>
			Read = 16,

			/// <summary>
			/// PSP_UTILITY_SAVEDATA_WRITESECURE = 17 = 0x11
			/// </summary>
			WriteSecure = 17,

			/// <summary>
			/// PSP_UTILITY_SAVEDATA_WRITE = 18 = 0x12
			/// </summary>
			Write = 18,

			/// <summary>
			/// PSP_UTILITY_SAVEDATA_ERASESECURE = 19 = 0x13
			/// </summary>
			EraseSecure = 19,
			
			/// <summary>
			/// PSP_UTILITY_SAVEDATA_ERASE = 20 = 0x14
			/// </summary>
			Erase = 20,

			/// <summary>
			/// PSP_UTILITY_SAVEDATA_DELETEDATA = 21 = 0x15
			/// </summary>
			DeleteData = 21,
			
			/// <summary>
			/// PSP_UTILITY_SAVEDATA_GETSIZE = 22 = 0x16
			/// </summary>
			GetSize = 22,
		}


		public enum pspUtilityMsgDialogMode
		{
			PSP_UTILITY_MSGDIALOG_MODE_ERROR = 0, // Error message
			PSP_UTILITY_MSGDIALOG_MODE_TEXT  = 1, // String message
		}

		[Flags]
		public enum pspUtilityMsgDialogOption
		{
			PSP_UTILITY_MSGDIALOG_OPTION_ERROR         = 0x00000000, // Error message (why two flags?)
			PSP_UTILITY_MSGDIALOG_OPTION_TEXT          = 0x00000001, // Text message (why two flags?)
			PSP_UTILITY_MSGDIALOG_OPTION_YESNO_BUTTONS = 0x00000010, // Yes/No buttons instead of 'Cancel'
			PSP_UTILITY_MSGDIALOG_OPTION_DEFAULT_NO    = 0x00000100, // Default position 'No', if not set will default to 'Yes'
		}

		public enum pspUtilityMsgDialogPressed
		{
			PSP_UTILITY_MSGDIALOG_RESULT_UNKNOWN1 = 0,
			PSP_UTILITY_MSGDIALOG_RESULT_YES      = 1,
			PSP_UTILITY_MSGDIALOG_RESULT_NO       = 2,
			PSP_UTILITY_MSGDIALOG_RESULT_BACK     = 3,
		}

		public enum PspUtilitySavedataFocus : uint
		{
			PSP_UTILITY_SAVEDATA_FOCUS_UNKNOWN    = 0, // 
			PSP_UTILITY_SAVEDATA_FOCUS_FIRSTLIST  = 1, // First in list
			PSP_UTILITY_SAVEDATA_FOCUS_LASTLIST   = 2, // Last in list
			PSP_UTILITY_SAVEDATA_FOCUS_LATEST     = 3, //  Most recent date
			PSP_UTILITY_SAVEDATA_FOCUS_OLDEST     = 4, // Oldest date
			PSP_UTILITY_SAVEDATA_FOCUS_UNKNOWN2   = 5, //
			PSP_UTILITY_SAVEDATA_FOCUS_UNKNOWN3   = 6, //
			PSP_UTILITY_SAVEDATA_FOCUS_FIRSTEMPTY = 7, // First empty slot
			PSP_UTILITY_SAVEDATA_FOCUS_LASTEMPTY  = 8, // Last empty slot
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		unsafe public struct PspUtilitySavedataSFOParam
		{
			/// <summary>
			/// 0000 -
			/// </summary>
			public fixed byte TitleRaw[0x80];

			public string Title
			{
				get
				{
					fixed (byte* Pointer = TitleRaw)
					{
						return PointerUtils.PtrToString(Pointer, Encoding.UTF8);
					}
				}
			}

			/// <summary>
			/// 0080 -
			/// </summary>
			public fixed byte SavedataTitleRaw[0x80];

			public string SavedataTitle
			{
				get
				{
					fixed (byte* Pointer = SavedataTitleRaw)
					{
						return PointerUtils.PtrToString(Pointer, Encoding.UTF8);
					}
				}
			}

			/// <summary>
			/// 0100 -
			/// </summary>
			public fixed byte Detail[0x400];

			/// <summary>
			/// 0500 -
			/// </summary>
			public byte ParentalLevel;

			/// <summary>
			/// 0501 -
			/// </summary>
			public fixed byte Unknown[3];
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct PspUtilitySavedataFileData
		{
			/// <summary>
			/// 0000 -
			/// </summary>
			public PspPointer BufferPointer;

			/// <summary>
			/// 0004 -
			/// </summary>
			public int BufferSize;

			/// <summary>
			/// 0008 - why are there two sizes?
			/// </summary>
			public int Size;

			/// <summary>
			/// 000C -
			/// </summary>
			public uint Unknown;

			public bool Used
			{
				get
				{
					if (BufferPointer.IsNull) return false;
					//if (BufferSize == 0) return false;
					if (Size == 0) return false;
					return true;
				}
			}
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct PspUtilitySavedataListSaveNewData
		{
			public PspUtilitySavedataFileData icon0;
			public uint titleCharPointer;
		}

		/// <summary>
		/// Structure to hold the parameters for a message dialog
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct pspUtilityMsgDialogParams
		{
			/// <summary>
			/// 
			/// </summary>
			public pspUtilityDialogCommon Base;

			/// <summary>
			/// 
			/// </summary>
			public int Unknown;

			/// <summary>
			/// 
			/// </summary>
			public pspUtilityMsgDialogMode Mode;

			/// <summary>
			/// 
			/// </summary>
			public uint ErrorValue;
			
			/// <summary>
			/// The message to display (may contain embedded linefeeds)
			/// </summary>
			public fixed byte FixedMessageRaw[512];

			/// <summary>
			/// 
			/// </summary>
			public pspUtilityMsgDialogOption Options;

			/// <summary>
			/// 
			/// </summary>
			public pspUtilityMsgDialogPressed ButtonPressed;

			public String Message
			{
				get
				{
					fixed (byte* Pointer = FixedMessageRaw)
					{
						return PointerUtils.PtrToString(Pointer, Encoding.UTF8);
					}
				}
			}
		}

		/*
	                    savedataParams.base.result = SceKernelErrors.ERROR_SAVEDATA_LOAD_NO_DATA;
	                } catch (Exception e) {
	                    savedataParams.base.result = SceKernelErrors.ERROR_SAVEDATA_LOAD_ACCESS_ERROR;
		*/

		/// <summary>
		/// 
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct pspUtilityDialogCommon
		{
			/// <summary>
			/// 0000 - Size of the structure
			/// </summary>
			public uint Size;
			
			/// <summary>
			/// 0004 - Language
			/// </summary>
			public Language Language;
			
			/// <summary>
			/// 0008 - Set to 1 for X/O button swap
			/// </summary>
			public int ButtonSwap;
			
			/// <summary>
			/// 000C - Graphics thread priority
			/// </summary>
			public int GraphicsThread;
			
			/// <summary>
			/// 0010 - Access/fileio thread priority (SceJobThread)
			/// </summary>
			public int AccessThread;
			
			/// <summary>
			/// 0014 - Font thread priority (ScePafThread)
			/// </summary>
			public int FontThread;
			
			/// <summary>
			/// 0018 - Sound thread priority
			/// </summary>
			public int SoundThread;
			
			/// <summary>
			/// 001C - Result
			/// </summary>
			public SceKernelErrors Result;

			/// <summary>
			/// 0020 - Set to 0
			/// </summary>
			public fixed int Reserved[4];
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct SceUtilitySavedataParam
		{
			/// <summary>
			/// 0000 - 
			/// </summary>
			public pspUtilityDialogCommon Base;

			/// <summary>
			/// 0030 - 
			/// </summary>
			public PspUtilitySavedataMode Mode;
	
			/// <summary>
			/// 0034 -
			/// </summary>
			public int Unknown1;
	
			/// <summary>
			/// 0038 -
			/// </summary>
			public int Overwrite;

			/// <summary>
			/// 003C - gameName: name used from the game for saves, equal for all saves
			/// </summary>
			public fixed byte GameNameRaw[16];

			public string GameName
			{
				get
				{
					fixed (byte* Pointer = GameNameRaw)
					{
						return PointerUtils.PtrToString(Pointer, 16, Encoding.UTF8);
					}
				}
			}

			/// <summary>
			/// 004C - saveName: name of the particular save, normally a number
			/// </summary>
			public fixed byte SaveNameRaw[20];

			public string SaveName
			{
				get
				{
					fixed (byte* Pointer = SaveNameRaw)
					{
						return PointerUtils.PtrToString(Pointer, 20, Encoding.UTF8);
					}
				}
			}

			/// <summary>
			/// 0060 - saveNameList: used by multiple modes
			/// </summary>
			public PspPointer SaveNameListPointer; // char[20]

			/// <summary>
			/// 0064 - fileName: name of the data file of the game for example DATA.BIN
			/// </summary>
			public fixed byte FileName[16];

			/// <summary>
			/// 0074 - pointer to a buffer that will contain data file unencrypted data
			/// </summary>
			public uint DataBufPointer;
			
			/// <summary>
			/// 0078 - size of allocated space to dataBuf
			/// </summary>
			public uint DataBufSize;

			/// <summary>
			/// 007C -
			/// </summary>
			public int DataSize;

			//PspUtilitySavedataFileData Data

			/// <summary>
			/// 0080 -
			/// </summary>
			public PspUtilitySavedataSFOParam SfoParam; // 504?

			/// <summary>
			/// 0584 -
			/// </summary>
			public PspUtilitySavedataFileData Icon0FileData; // 16

			/// <summary>
			/// 0594 -
			/// </summary>
			public PspUtilitySavedataFileData Icon1FileData; // 16

			/// <summary>
			/// 05A4 -
			/// </summary>
			public PspUtilitySavedataFileData Pic1FileData; // 16

			/// <summary>
			/// 05B4 -
			/// </summary>
			public PspUtilitySavedataFileData Snd0FileData; // 16

			/// <summary>
			/// 05C4 -Pointer to an PspUtilitySavedataListSaveNewData structure
			/// </summary>
			//public PspUtilitySavedataListSaveNewData *newData;
			public uint NewDataPointer;

			/// <summary>
			/// 05C8 -Initial focus for lists
			/// </summary>
			public PspUtilitySavedataFocus Focus;

			/// <summary>
			/// 05CC -
			/// </summary>
			public uint abortStatus;

			/// <summary>
			/// 05D0 -
			/// </summary>
			public PspPointer msFreeAddr;

			/// <summary>
			/// 05D4 -
			/// </summary>
			public PspPointer msDataAddr;

			/// <summary>
			/// 05D8 -
			/// </summary>
			public PspPointer utilityDataAddr;

		//#if _PSP_FW_VERSION >= 200

			/// <summary>
			/// 05E0 -key: encrypt/decrypt key for save with firmware >= 2.00
			/// </summary>
			public fixed byte Key[16];

			/// <summary>
			/// 05F0 -
			/// </summary>
			public uint secureVersion;
			
			/// <summary>
			/// 05F4 -
			/// </summary>
			public uint multiStatus;
			
			/// <summary>
			/// 05F8 -
			/// </summary>
			public PspPointer idListAddr;
			
			/// <summary>
			/// 05FC -
			/// </summary>
			public PspPointer fileListAddr;
			
			/// <summary>
			/// 0600 -
			/// </summary>
			public PspPointer sizeAddr;

			/// <summary>
			/// 0604 -unknown3: ?
			/// </summary>
			public fixed byte Unknown3[20 - 5];

		//#endif

		}

		public enum pspUtilityNetconfActions
		{
			PSP_NETCONF_ACTION_CONNECTAP,
			PSP_NETCONF_ACTION_DISPLAYSTATUS,
			PSP_NETCONF_ACTION_CONNECT_ADHOC,
			PSP_NETCONF_ACTION_CONNECTAP_LASTUSED,
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct pspUtilityNetconfAdhoc
		{
			/// <summary>
			/// 
			/// </summary>
			public fixed byte name[8];

			/// <summary>
			/// 
			/// </summary>
			public uint timeout;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct pspUtilityNetconfData
		{
			/// <summary>
			/// 
			/// </summary>
			public pspUtilityDialogCommon Base;
			
			/// <summary>
			/// One of pspUtilityNetconfActions
			/// </summary>
			public int Action;

			/// <summary>
			/// Adhoc connection params
			/// </summary>
			//pspUtilityNetconfAdhoc * adhocparam;
			public uint AdhocparamPointer;
			
			/// <summary>
			/// Set to 1 to allow connections with the 'Internet Browser' option set to 'Start' (ie. hotspot connection)
			/// </summary>
			public int Hotspot;

			/// <summary>
			/// Will be set to 1 when connected to a hotspot style connection
			/// </summary>
			public int hotspot_connected;

			/// <summary>
			/// Set to 1 to allow connections to Wifi service providers (WISP)
			/// </summary>
			public int wifisp;
		}

		/// <summary>
		/// Enumeration for input language
		/// </summary>
		public enum SceUtilityOskInputLanguage
		{
			PSP_UTILITY_OSK_LANGUAGE_DEFAULT   = 0x00,
			PSP_UTILITY_OSK_LANGUAGE_JAPANESE  = 0x01,
			PSP_UTILITY_OSK_LANGUAGE_ENGLISH   = 0x02,
			PSP_UTILITY_OSK_LANGUAGE_FRENCH    = 0x03,
			PSP_UTILITY_OSK_LANGUAGE_SPANISH   = 0x04,
			PSP_UTILITY_OSK_LANGUAGE_GERMAN    = 0x05,
			PSP_UTILITY_OSK_LANGUAGE_ITALIAN   = 0x06,
			PSP_UTILITY_OSK_LANGUAGE_DUTCH     = 0x07,
			PSP_UTILITY_OSK_LANGUAGE_PORTUGESE = 0x08,
			PSP_UTILITY_OSK_LANGUAGE_RUSSIAN   = 0x09,
			PSP_UTILITY_OSK_LANGUAGE_KOREAN    = 0x0a,
		};

		/// <summary>
		/// Enumeration for OSK internal state
		/// </summary>
		public enum SceUtilityOskState
		{
			/// <summary>
			/// No OSK is currently active
			/// </summary>
			PSP_UTILITY_OSK_DIALOG_NONE      = 0,
			
			/// <summary>
			/// The OSK is currently being initialized
			/// </summary>
			PSP_UTILITY_OSK_DIALOG_INITING   = 1,
			
			/// <summary>
			/// The OSK is initialised
			/// </summary>
			PSP_UTILITY_OSK_DIALOG_INITED    = 2,
			
			/// <summary>
			/// The OSK is visible and ready for use
			/// </summary>
			PSP_UTILITY_OSK_DIALOG_VISIBLE   = 3,
			
			/// <summary>
			/// The OSK has been cancelled and should be shut down
			/// </summary>
			PSP_UTILITY_OSK_DIALOG_QUIT      = 4,
			
			/// <summary>
			/// The OSK has successfully shut down 
			/// </summary>
			PSP_UTILITY_OSK_DIALOG_FINISHED  = 5,
		};

		/// <summary>
		/// Enumeration for OSK field results
		/// </summary>
		public enum SceUtilityOskResult
		{
			/// <summary>
			/// PSP_UTILITY_OSK_RESULT_UNCHANGED
			/// </summary>
			Unchanged = 0,
			
			/// <summary>
			/// PSP_UTILITY_OSK_RESULT_CANCELLED
			/// </summary>
			Cancelled = 1,

			/// <summary>
			/// PSP_UTILITY_OSK_RESULT_CHANGED
			/// </summary>
			Changed = 2,
		};

		/// <summary>
		/// Enumeration for input types (these are limited by initial choice of language)
		/// </summary>
		[Flags]
		public enum SceUtilityOskInputType
		{
			/// <summary>
			/// 
			/// </summary>
			PSP_UTILITY_OSK_INPUTTYPE_ALL                    = 0x00000000,

			/// <summary>
			/// 
			/// </summary>
			PSP_UTILITY_OSK_INPUTTYPE_LATIN_DIGIT            = 0x00000001,

			/// <summary>
			/// 
			/// </summary>
			PSP_UTILITY_OSK_INPUTTYPE_LATIN_SYMBOL           = 0x00000002,

			/// <summary>
			/// 
			/// </summary>
			PSP_UTILITY_OSK_INPUTTYPE_LATIN_LOWERCASE        = 0x00000004,

			/// <summary>
			/// 
			/// </summary>
			PSP_UTILITY_OSK_INPUTTYPE_LATIN_UPPERCASE        = 0x00000008,

			/// <summary>
			/// 
			/// </summary>
			PSP_UTILITY_OSK_INPUTTYPE_JAPANESE_DIGIT         = 0x00000100,

			/// <summary>
			/// 
			/// </summary>
			PSP_UTILITY_OSK_INPUTTYPE_JAPANESE_SYMBOL        = 0x00000200,

			/// <summary>
			/// 
			/// </summary>
			PSP_UTILITY_OSK_INPUTTYPE_JAPANESE_LOWERCASE     = 0x00000400,

			/// <summary>
			/// 
			/// </summary>
			PSP_UTILITY_OSK_INPUTTYPE_JAPANESE_UPPERCASE     = 0x00000800,
			
			/// <summary>
			/// http://en.wikipedia.org/wiki/Hiragana
			/// </summary>
			PSP_UTILITY_OSK_INPUTTYPE_JAPANESE_HIRAGANA      = 0x00001000,

			/// <summary>
			// Half-width Katakana
			/// </summary>
			PSP_UTILITY_OSK_INPUTTYPE_JAPANESE_HALF_KATAKANA = 0x00002000,

			/// <summary>
			/// http://en.wikipedia.org/wiki/Katakana
			/// </summary>
			PSP_UTILITY_OSK_INPUTTYPE_JAPANESE_KATAKANA      = 0x00004000,
			
			/// <summary>
			/// http://en.wikipedia.org/wiki/Kanji
			/// </summary>
			PSP_UTILITY_OSK_INPUTTYPE_JAPANESE_KANJI         = 0x00008000,

			/// <summary>
			/// 
			/// </summary>
			PSP_UTILITY_OSK_INPUTTYPE_RUSSIAN_LOWERCASE      = 0x00010000,

			/// <summary>
			/// 
			/// </summary>
			PSP_UTILITY_OSK_INPUTTYPE_RUSSIAN_UPPERCASE      = 0x00020000,

			/// <summary>
			/// 
			/// </summary>
			PSP_UTILITY_OSK_INPUTTYPE_KOREAN                 = 0x00040000,

			/// <summary>
			/// 
			/// </summary>
			PSP_UTILITY_OSK_INPUTTYPE_URL                    = 0x00080000,
		};

		/// <summary>
		/// OSK Field data
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct SceUtilityOskData
		{
			/// <summary>
			/// Unknown. Pass 0.
			/// </summary>
			public int unk_00;                
			
			/// <summary>
			/// Unknown. Pass 0.
			/// </summary>
			public int unk_04;
			
			/// <summary>
			/// One of ::SceUtilityOskInputLanguage
			/// </summary>
			public SceUtilityOskInputLanguage language;
			
			/// <summary>
			/// Unknown. Pass 0.
			/// </summary>
			public int unk_12;                  
			
			/// <summary>
			/// One or more of ::SceUtilityOskInputType (types that are selectable by pressing SELECT)
			/// </summary>
			public SceUtilityOskInputType inputtype;

			/// <summary>
			/// Number of lines
			/// </summary>
			public int lines;            
 
			/// <summary>
			/// Unknown. Pass 0.
			/// </summary>
			public int unk_24;           

			/// <summary>
			/// Description text
			/// </summary>
			//public ushort* desc;              
			public PspPointer descPointer;

			/// <summary>
			/// Initial text
			/// </summary>
			//public ushort* intext;
			public PspPointer intextPointer;  

			/// <summary>
			/// Length of output text
			/// </summary>
			public int outtextlength;     
  
			/// <summary>
			/// Pointer to the output text
			/// </summary>
			//public ushort* outtext;           
			public PspPointer outtextPointer;           

			/// <summary>
			/// Result.
			/// </summary>
			public SceUtilityOskResult result;  
   
			/// <summary>
			/// The max text that can be input
			/// </summary>
			public int outtextlimit;        
		}

		/// <summary>
		/// OSK parameters
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct SceUtilityOskParams
		{
			/// <summary>
			/// 
			/// </summary>
			public pspUtilityDialogCommon Base;   

			/// <summary>
			/// Number of input fields
			/// </summary>
			public int datacount;

			/// <summary>
			/// Pointer to the start of the data for the input fields
			/// </summary>
			//SceUtilityOskData*     data;
			public PspPointer dataPointer;    

			/// <summary>
			/// The local OSK state, one of ::SceUtilityOskState
			/// </summary>
			public int state; 
 
			/// <summary>
			/// Unknown. Pass 0	
			/// </summary>
			public int unk_60;
		}


		public DialogStepEnum CurrentDialogStep = DialogStepEnum.NONE;

		/// <summary>
		/// Load an audio/video module (PRX) from user mode.
		/// 
		/// Available on firmware 2.00 and higher only.
		/// </summary>
		/// <param name="_module">module number to load (PSP_AV_MODULE_xxx)</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xC629AF26, FirmwareVersion = 200)]
		[HlePspNotImplemented]
		public int sceUtilityLoadAvModule(PSP_AV_MODULE _module)
		{
			return 0;
		}

		/// <summary>
		/// Unload an audio/video module (PRX) from user mode.
		/// Available on firmware 2.00 and higher only.
		/// </summary>
		/// <param name="_module">module number to be unloaded</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xF7D8D092, FirmwareVersion = 200)]
		[HlePspNotImplemented]
		public int sceUtilityUnloadAvModule(PSP_AV_MODULE _module)
		{
			return 0;
		}

		/// <summary>
		/// Load a network module (PRX) from user mode.
		/// Load PSP_NET_MODULE_COMMON and PSP_NET_MODULE_INET
		/// to use infrastructure WifI (via an access point).
		/// Available on firmware 2.00 and higher only.
		/// </summary>
		/// <param name="_module">module number to load (PSP_NET_MODULE_xxx)</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x1579A159, FirmwareVersion = 200)]
		[HlePspNotImplemented]
		public int sceUtilityLoadNetModule(int _module)
		{
			//unimplemented();
			//unimplemented_notice();
			//Logger.log(Logger.Level.WARNING, "sceUtility", "sceUtilityLoadNetModule not implemented!");
			//return -1;
			return 0;
		}

		/// <summary>
		/// Load a module (PRX) from user mode.
		/// </summary>
		/// <param name="PspModule">module to load (PSP_MODULE_xxx)</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0x2A2B3DE0, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceUtilityLoadModule(PspModule PspModule)
		{
			//throw (new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Unload a module (PRX) from user mode.
		/// </summary>
		/// <param name="_module">module to unload (PSP_MODULE_xxx)</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0xE49BFE92, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceUtilityUnloadModule(PspModule _module)
		{
			//throw (new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Get the current status of game sharing.
		/// 
		/// 2(Visible)  if the GUI is visible (you need to call sceUtilityGameSharingGetStatus).
		/// 3(Quit)     if the user cancelled the dialog, and you need to call sceUtilityGameSharingShutdownStart.
		/// 4(Finished) if the dialog has been successfully shut down.
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x946963F3, FirmwareVersion = 150)]
		public pspUtilityDialogState sceUtilityGameSharingGetStatus()
		{
			return pspUtilityDialogState.Finished;
		}

		/// <summary>
		/// Return-values for the various sceUtility***GetStatus() functions
		/// </summary>
		public enum pspUtilityDialogState : int
		{
			/// <summary>
			/// No dialog is currently active
			/// PSP_UTILITY_DIALOG_NONE
			/// </summary>
			None = 0,

			/// <summary>
			/// The dialog is currently being initialized
			/// PSP_UTILITY_DIALOG_INIT
			/// </summary>
			Init = 1,

			/// <summary>
			/// The dialog is visible and ready for use
			/// PSP_UTILITY_DIALOG_VISIBLE
			/// </summary>
			Visible = 2,

			/// <summary>
			/// The dialog has been canceled and should be shut down
			/// PSP_UTILITY_DIALOG_QUIT
			/// </summary>
			Quit = 3,

			/// <summary>
			/// The dialog has successfully shut down
			/// PSP_UTILITY_DIALOG_FINISHED
			/// </summary>
			Finished = 4,
		}
	}
}
