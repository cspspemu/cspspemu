using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.utility
{
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

		public enum PSP_AV_MODULE
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
			PSP_UTILITY_SAVEDATA_AUTOLOAD       = 0,
			PSP_UTILITY_SAVEDATA_AUTOSAVE       = 1,
			PSP_UTILITY_SAVEDATA_LOAD           = 2,
			PSP_UTILITY_SAVEDATA_SAVE           = 3,
			PSP_UTILITY_SAVEDATA_LISTLOAD       = 4,
			PSP_UTILITY_SAVEDATA_LISTSAVE       = 5,
			PSP_UTILITY_SAVEDATA_LISTDELETE     = 6,
			PSP_UTILITY_SAVEDATA_DELETE         = 7,
			PSP_UTILITY_SAVEDATA_SIZES          = 8,
			PSP_UTILITY_SAVEDATA_AUTODELETE     = 9,
			PSP_UTILITY_SAVEDATA_SINGLEDELETE   = 10,
			PSP_UTILITY_SAVEDATA_LIST           = 11,
			PSP_UTILITY_SAVEDATA_FILES          = 12,
			PSP_UTILITY_SAVEDATA_MAKEDATASECURE = 13,
			PSP_UTILITY_SAVEDATA_MAKEDATA       = 14,
			PSP_UTILITY_SAVEDATA_READSECURE     = 15,
			PSP_UTILITY_SAVEDATA_READ           = 16,
			PSP_UTILITY_SAVEDATA_WRITESECURE    = 17,
			PSP_UTILITY_SAVEDATA_WRITE          = 18,
			PSP_UTILITY_SAVEDATA_ERASESECURE    = 19,
			PSP_UTILITY_SAVEDATA_ERASE          = 20,
			PSP_UTILITY_SAVEDATA_DELETEDATA     = 21,
			PSP_UTILITY_SAVEDATA_GETSIZE        = 22,
		}


		public enum pspUtilityMsgDialogMode
		{
			PSP_UTILITY_MSGDIALOG_MODE_ERROR = 0, // Error message
			PSP_UTILITY_MSGDIALOG_MODE_TEXT  = 1, // String message
		}

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

		public enum PspUtilitySavedataFocus
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

		unsafe public struct PspUtilitySavedataSFOParam
		{
			public fixed byte title[0x80];
			public fixed byte savedataTitle[0x80];
			public fixed byte detail[0x400];
			public byte parentalLevel;
			public fixed byte unknown[3];
		}

		public struct PspUtilitySavedataFileData
		{
			uint bufPointer;
			uint bufSize;
			uint size;	/* ??? - why are there two sizes? */
			int unknown;
		}

		public struct PspUtilitySavedataListSaveNewData
		{
			PspUtilitySavedataFileData icon0;
			uint titleCharPointer;
		}

		/// <summary>
		/// Structure to hold the parameters for a message dialog
		/// </summary>
		public struct pspUtilityMsgDialogParams
		{
			/// <summary>
			/// 
			/// </summary>
			public pspUtilityDialogCommon Base;

			/// <summary>
			/// 
			/// </summary>
			public int unknown;

			/// <summary>
			/// 
			/// </summary>
			public pspUtilityMsgDialogMode mode;

			/// <summary>
			/// 
			/// </summary>
			public uint errorValue;
			
			/// <summary>
			/// The message to display (may contain embedded linefeeds)
			/// </summary>
			public fixed byte message[512];

			/// <summary>
			/// 
			/// </summary>
			public pspUtilityMsgDialogOption options;

			/// <summary>
			/// 
			/// </summary>
			public pspUtilityMsgDialogPressed buttonPressed;
		}

		/// <summary>
		/// 
		/// </summary>
		public struct pspUtilityDialogCommon
		{
			/// <summary>
			/// Size of the structure
			/// </summary>
			public uint size;
			
			/// <summary>
			/// Language
			/// </summary>
			public int  language;
			
			/// <summary>
			/// Set to 1 for X/O button swap
			/// </summary>
			public int  buttonSwap;
			
			/// <summary>
			/// Graphics thread priority
			/// </summary>
			public int  graphicsThread;
			
			/// <summary>
			/// Access/fileio thread priority (SceJobThread)
			/// </summary>
			public int  accessThread;
			
			/// <summary>
			/// Font thread priority (ScePafThread)
			/// </summary>
			public int  fontThread;
			
			/// <summary>
			/// Sound thread priority
			/// </summary>
			public int  soundThread;
			
			/// <summary>
			/// Result
			/// </summary>
			public int  result;

			/// <summary>
			/// Set to 0
			/// </summary>
			public fixed int  reserved[4];
		}

		public struct SceUtilitySavedataParam
		{
			/// <summary>
			/// 
			/// </summary>
			public pspUtilityDialogCommon Base;

			/// <summary>
			/// 
			/// </summary>
			public PspUtilitySavedataMode Mode;
	
			/// <summary>
			/// 
			/// </summary>
			public int unknown1;
	
			/// <summary>
			/// 
			/// </summary>
			public int overwrite;

			/// <summary>
			/// gameName: name used from the game for saves, equal for all saves
			/// </summary>
			public fixed byte gameName[13];

			/// <summary>
			/// 
			/// </summary>
			public fixed byte reserved[3];

			/// <summary>
			/// saveName: name of the particular save, normally a number
			/// </summary>
			public fixed byte saveName[20];

			/// <summary>
			/// saveNameList: used by multiple modes
			/// </summary>
			public uint saveNameListPointer; // char[20]

			/// <summary>
			/// fileName: name of the data file of the game for example DATA.BIN
			/// </summary>
			public fixed byte fileName[13];

			/// <summary>
			/// 
			/// </summary>
			public fixed byte reserved1[3];
	
			/// <summary>
			/// pointer to a buffer that will contain data file unencrypted data
			/// </summary>
			public uint dataBufPointer;
			
			/// <summary>
			/// size of allocated space to dataBuf
			/// </summary>
			public uint dataBufSize;

			/// <summary>
			/// 
			/// </summary>
			public uint dataSize;

			/// <summary>
			/// 
			/// </summary>
			public PspUtilitySavedataSFOParam sfoParam;

			/// <summary>
			/// 
			/// </summary>
			public PspUtilitySavedataFileData icon0FileData;

			/// <summary>
			/// 
			/// </summary>
			public PspUtilitySavedataFileData icon1FileData;

			/// <summary>
			/// 
			/// </summary>
			public PspUtilitySavedataFileData pic1FileData;

			/// <summary>
			/// 
			/// </summary>
			public PspUtilitySavedataFileData snd0FileData;

			/// <summary>
			/// Pointer to an PspUtilitySavedataListSaveNewData structure
			/// </summary>
			//public PspUtilitySavedataListSaveNewData *newData;
			public uint newDataPointer;

			/// <summary>
			/// Initial focus for lists
			/// </summary>
			public PspUtilitySavedataFocus focus;

			/// <summary>
			/// unknown2: ?
			/// </summary>
			public fixed int unknown2[4];

		//#if _PSP_FW_VERSION >= 200

			/// <summary>
			/// key: encrypt/decrypt key for save with firmware >= 2.00
			/// </summary>
			public fixed byte key[16];

			/// <summary>
			/// unknown3: ?
			/// </summary>
			public fixed byte unknown3[20];

		//#endif

		}

		public enum pspUtilityNetconfActions
		{
			PSP_NETCONF_ACTION_CONNECTAP,
			PSP_NETCONF_ACTION_DISPLAYSTATUS,
			PSP_NETCONF_ACTION_CONNECT_ADHOC,
			PSP_NETCONF_ACTION_CONNECTAP_LASTUSED,
		}

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

		struct pspUtilityNetconfData
		{
			/// <summary>
			/// 
			/// </summary>
			public pspUtilityDialogCommon Base;
			
			/// <summary>
			/// One of pspUtilityNetconfActions
			/// </summary>
			public int action;

			/// <summary>
			/// Adhoc connection params
			/// </summary>
			//pspUtilityNetconfAdhoc * adhocparam;
			public uint adhocparamPointer;
			
			/// <summary>
			/// Set to 1 to allow connections with the 'Internet Browser' option set to 'Start' (ie. hotspot connection)
			/// </summary>
			public int hotspot;

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
			PSP_UTILITY_OSK_RESULT_UNCHANGED = 0,
			PSP_UTILITY_OSK_RESULT_CANCELLED = 1,
			PSP_UTILITY_OSK_RESULT_CHANGED   = 2,
		};

		/// <summary>
		/// Enumeration for input types (these are limited by initial choice of language)
		/// </summary>
		public enum SceUtilityOskInputType
		{
			PSP_UTILITY_OSK_INPUTTYPE_ALL                    = 0x00000000,
			PSP_UTILITY_OSK_INPUTTYPE_LATIN_DIGIT            = 0x00000001,
			PSP_UTILITY_OSK_INPUTTYPE_LATIN_SYMBOL           = 0x00000002,
			PSP_UTILITY_OSK_INPUTTYPE_LATIN_LOWERCASE        = 0x00000004,
			PSP_UTILITY_OSK_INPUTTYPE_LATIN_UPPERCASE        = 0x00000008,
			PSP_UTILITY_OSK_INPUTTYPE_JAPANESE_DIGIT         = 0x00000100,
			PSP_UTILITY_OSK_INPUTTYPE_JAPANESE_SYMBOL        = 0x00000200,
			PSP_UTILITY_OSK_INPUTTYPE_JAPANESE_LOWERCASE     = 0x00000400,
			PSP_UTILITY_OSK_INPUTTYPE_JAPANESE_UPPERCASE     = 0x00000800,
			// http://en.wikipedia.org/wiki/Hiragana
			PSP_UTILITY_OSK_INPUTTYPE_JAPANESE_HIRAGANA      = 0x00001000,
			// http://en.wikipedia.org/wiki/Katakana
			// Half-width Katakana
			PSP_UTILITY_OSK_INPUTTYPE_JAPANESE_HALF_KATAKANA = 0x00002000,
			PSP_UTILITY_OSK_INPUTTYPE_JAPANESE_KATAKANA      = 0x00004000,
			// http://en.wikipedia.org/wiki/Kanji
			PSP_UTILITY_OSK_INPUTTYPE_JAPANESE_KANJI         = 0x00008000,
			PSP_UTILITY_OSK_INPUTTYPE_RUSSIAN_LOWERCASE      = 0x00010000,
			PSP_UTILITY_OSK_INPUTTYPE_RUSSIAN_UPPERCASE      = 0x00020000,
			PSP_UTILITY_OSK_INPUTTYPE_KOREAN                 = 0x00040000,
			PSP_UTILITY_OSK_INPUTTYPE_URL                    = 0x00080000,
		};

		/// <summary>
		/// OSK Field data
		/// </summary>
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
			public uint descPointer;

			/// <summary>
			/// Initial text
			/// </summary>
			//public ushort* intext;
			public uint intextPointer;  

			/// <summary>
			/// Length of output text
			/// </summary>
			public int outtextlength;     
  
			/// <summary>
			/// Pointer to the output text
			/// </summary>
			//public ushort* outtext;           
			public uint outtextPointer;           

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
			public uint dataPointer;    

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
		public int sceUtilityLoadAvModule(int _module)
		{
			return 0;
		}

		/**
		 * Load a network module (PRX) from user mode.
		 * Load PSP_NET_MODULE_COMMON and PSP_NET_MODULE_INET
		 * to use infrastructure WifI (via an access point).
		 * Available on firmware 2.00 and higher only.
		 *
		 * @param module - module number to load (PSP_NET_MODULE_xxx)
		 * @return 0 on success, < 0 on error
		 */
		[HlePspFunction(NID = 0x1579A159, FirmwareVersion = 200)]
		[HlePspNotImplemented]
		public int sceUtilityLoadNetModule(int _module)
		{
			//unimplemented();
			//unimplemented_notice();
			//Logger.log(Logger.Level.WARNING, "sceUtility", "sceUtilityLoadNetModule not implemented!");
			return -1;
		}

		/// <summary>
		/// Load a module (PRX) from user mode.
		/// </summary>
		/// <param name="_module">module to load (PSP_MODULE_xxx)</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0x2A2B3DE0, FirmwareVersion = 150)]
		public int sceUtilityLoadModule(PspModule _module)
		{
			throw (new NotImplementedException());
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
		public int sceUtilityUnloadModule(PspModule _module)
		{
			throw (new NotImplementedException());
		}
	}
}
