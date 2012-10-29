using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Vfs
{
	/*
	/// <summary>
	/// Structure passed to the init and exit functions of the io driver system
	/// </summary>
	public struct PspIoDrvArg
	{
		/// <summary>
		/// Pointer to the original driver which was added (PspIoDrv *)
		/// </summary>
		public uint DriverPointer;
		
		/// <summary>
		/// Pointer to a user defined argument (if written by the driver will preseve across calls
		/// </summary>
		public uint ArgumentPointer;
	}

	/// <summary>
	/// Structure passed to the file functions of the io driver system
	/// </summary>
	public unsafe struct PspIoDrvFileArg
	{
		/// <summary>
		/// Unknown
		/// </summary>
		public uint unk1;

		/// <summary>
		/// The file system number, e.g. if a file is opened as host5:/myfile.txt this field will be 5
		/// </summary>
		public uint fs_num;

		/// <summary>
		/// Pointer to the driver structure
		/// </summary>
		public PspIoDrvArg* drv;

		/// <summary>
		/// Unknown, again
		/// </summary>
		public uint unk2;

		/// <summary>
		/// Pointer to a user defined argument, this is preserved on a per file basis
		/// </summary>
		public void* arg;
	}

	/// <summary>
	/// Structure to maintain the file driver pointers
	/// </summary>
	public struct PspIoDrvFuncs
	{
		/// <summary>
		/// int (*IoInit)(PspIoDrvArg* arg);
		/// </summary>
		public uint IoInit_Pointer;
		
		/// <summary>
		/// int (*IoExit)(PspIoDrvArg* arg); 
		/// </summary>
		public uint IoExit_Pointer;
		
		/// <summary>
		/// int (*IoOpen)(PspIoDrvFileArg *arg, char *file, int flags, SceMode mode); 
		/// </summary>
		public uint IoOpen_Pointer;

		/// <summary>
		/// int (*IoClose)(PspIoDrvFileArg *arg); 
		/// </summary>
		public uint IoClose_Pointer;

		/// <summary>
		/// int (*IoRead)(PspIoDrvFileArg *arg, char *data, int len); 
		/// </summary>
		public uint IoRead_Pointer;

		/// <summary>
		/// int (*IoWrite)(PspIoDrvFileArg *arg, const char *data, int len); 
		/// </summary>
		public uint IoWrite_Pointer;
		
		/// <summary>
		/// SceOff (*IoLseek)(PspIoDrvFileArg *arg, SceOff ofs, int whence); 
		/// </summary>
		public uint IoLseek_Pointer;
		
		/// <summary>
		/// int (*IoIoctl)(PspIoDrvFileArg *arg, unsigned int cmd, void *indata, int inlen, void *outdata, int outlen);
		/// </summary>
		public uint IoIoctl_Pointer;
		
		/// <summary>
		/// int (*IoRemove)(PspIoDrvFileArg *arg, const char *name); 
		/// </summary>
		public uint IoRemove_Pointer;

		/// <summary>
		/// int (*IoMkdir)(PspIoDrvFileArg *arg, const char *name, SceMode mode); 
		/// </summary>
		public uint IoMkdir_Pointer;
		
		/// <summary>
		/// int (*IoRmdir)(PspIoDrvFileArg *arg, const char *name);
		/// </summary>
		public uint IoRmdir_Pointer;

		/// <summary>
		/// int (*IoDopen)(PspIoDrvFileArg *arg, const char *dirname); 
		/// </summary>
		public uint IoDopen_Pointer;
		
		/// <summary>
		/// int (*IoDclose)(PspIoDrvFileArg *arg);
		/// </summary>
		public uint IoDclose_Pointer;
		
		/// <summary>
		/// int (*IoDread)(PspIoDrvFileArg *arg, SceIoDirent *dir);
		/// </summary>
		public uint IoDread_Pointer;
		
		/// <summary>
		/// int (*IoGetstat)(PspIoDrvFileArg *arg, const char *file, SceIoStat *stat);
		/// </summary>
		public uint IoGetstat_Pointer;
		
		/// <summary>
		/// int (*IoChstat)(PspIoDrvFileArg *arg, const char *file, SceIoStat *stat, int bits);
		/// </summary>
		public uint IoChstat_Pointer;

		/// <summary>
		/// int (*IoRename)(PspIoDrvFileArg *arg, const char *oldname, const char *newname); 
		/// </summary>
		public uint IoRename_Pointer;

		/// <summary>
		/// int (*IoChdir)(PspIoDrvFileArg *arg, const char *dir); 
		/// </summary>
		public uint IoChdir_Pointer;
		
		/// <summary>
		/// int (*IoMount)(PspIoDrvFileArg *arg); 
		/// </summary>
		public uint IoMount_Pointer;
		
		/// <summary>
		/// int (*IoUmount)(PspIoDrvFileArg *arg); 
		/// </summary>
		public uint IoUmount_Pointer;

		/// <summary>
		/// int (*IoDevctl)(PspIoDrvFileArg *arg, const char *devname, unsigned int cmd, void *indata, int inlen, void *outdata, int outlen); 
		/// </summary>
		public uint IoDevctl_Pointer;
		
		/// <summary>
		/// int (*IoUnk21)(PspIoDrvFileArg *arg); 
		/// </summary>
		public uint IoUnk21_Pointer;
	}
	*/
}
