using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using CSharpUtils;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Vfs;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core;

namespace CSPspEmu.Hle.Modules.utility
{
    public unsafe partial class sceUtility
    {
        [Inject] HleIoManager HleIoManager;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ListRequest
        {
            /// <summary>
            /// 
            /// </summary>
            public uint MaxEntries;

            /// <summary>
            /// 
            /// </summary>
            public uint NumEntriesReaded;

            /// <summary>
            /// 
            /// </summary>
            public PspPointer Entries;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SizeFreeInfo
        {
            /// <summary>
            /// 
            /// </summary>
            public uint SectorSize;

            /// <summary>
            /// 
            /// </summary>
            public uint FreeSectors;

            /// <summary>
            /// 
            /// </summary>
            public uint FreeKb;

            /// <summary>
            /// 
            /// </summary>
            private fixed byte FreeKbStringFixed[8];

            public string FreeKbString
            {
                get
                {
                    fixed (byte* Ptr = FreeKbStringFixed) return PointerUtils.FixedByteGet(8, Ptr);
                }
                set
                {
                    fixed (byte* Ptr = FreeKbStringFixed) PointerUtils.FixedByteSet(8, Ptr, value);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SizeUsedInfo
        {
            /// <summary>
            /// +0000 - 
            /// </summary>
            private fixed byte gameNameFixed[16];

            public string gameName
            {
                get
                {
                    fixed (byte* Ptr = gameNameFixed) return PointerUtils.FixedByteGet(16, Ptr);
                }
                set
                {
                    fixed (byte* Ptr = gameNameFixed) PointerUtils.FixedByteSet(16, Ptr, value);
                }
            }

            /// <summary>
            /// +0010 - 
            /// </summary>
            private fixed byte saveNameFixed[20];

            public string saveName
            {
                get
                {
                    fixed (byte* Ptr = saveNameFixed) return PointerUtils.FixedByteGet(20, Ptr);
                }
                set
                {
                    fixed (byte* Ptr = saveNameFixed) PointerUtils.FixedByteSet(20, Ptr, value);
                }
            }

            /// <summary>
            /// +0024 - 
            /// </summary>
            public uint UsedSectors;

            /// <summary>
            /// +0028 - 
            /// </summary>
            public uint UsedKb;

            /// <summary>
            /// +002C - 
            /// </summary>
            private fixed byte UsedKbStringFixed[8];

            public string UsedKbString
            {
                get
                {
                    fixed (byte* Ptr = UsedKbStringFixed) return PointerUtils.FixedByteGet(8, Ptr);
                }
                set
                {
                    fixed (byte* Ptr = UsedKbStringFixed) PointerUtils.FixedByteSet(8, Ptr, value);
                }
            }

            /// <summary>
            /// +0034 - 
            /// </summary>
            public uint UsedKb32;

            /// <summary>
            /// +0038 - 
            /// </summary>
            private fixed byte UsedKb32StringFixed[8];

            public string UsedKb32String
            {
                get
                {
                    fixed (byte* Ptr = UsedKb32StringFixed) return PointerUtils.FixedByteGet(8, Ptr);
                }
                set
                {
                    fixed (byte* Ptr = UsedKb32StringFixed) PointerUtils.FixedByteSet(8, Ptr, value);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SizeRequiredSpaceInfo
        {
            /// <summary>
            /// 
            /// </summary>
            public uint RequiredSpaceSectors;

            /// <summary>
            /// 
            /// </summary>
            public uint RequiredSpaceKb;

            /// <summary>
            /// 
            /// </summary>
            public fixed byte RequiredSpaceStringFixed[8];

            public string RequiredSpaceString
            {
                get
                {
                    fixed (byte* Ptr = RequiredSpaceStringFixed) return PointerUtils.FixedByteGet(8, Ptr);
                }
                set
                {
                    fixed (byte* Ptr = RequiredSpaceStringFixed) PointerUtils.FixedByteSet(8, Ptr, value);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public uint RequiredSpace32KB;

            /// <summary>
            /// 
            /// </summary>
            public fixed byte RequiredSpace32KBStringFixed[8];

            public string RequiredSpace32KBString
            {
                get
                {
                    fixed (byte* Ptr = RequiredSpace32KBStringFixed) return PointerUtils.FixedByteGet(8, Ptr);
                }
                set
                {
                    fixed (byte* Ptr = RequiredSpace32KBStringFixed) PointerUtils.FixedByteSet(8, Ptr, value);
                }
            }
        }

        /// <summary>
        /// Saves or Load savedata to/from the passed structure
        /// After having called this continue calling sceUtilitySavedataGetStatus to
        /// check if the operation is completed
        /// </summary>
        /// <param name="Params">Savedata parameters</param>
        /// <returns>0 on success</returns>
        [HlePspFunction(NID = 0x50C4CD57, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceUtilitySavedataInitStart(ref SceUtilitySavedataParam Params)
        {
            Console.WriteLine(Params.Mode);
            Console.WriteLine(Params.ToStringDefault());
            //Params.DataBufPointer
            Params.Base.Result = 0;

            var SavePathFolder = "ms0:/PSP/SAVEDATA/" + Params.GameName + Params.SaveName;
            var SaveDataBin = SavePathFolder + "/DATA.BIN";
            var SaveIcon0 = SavePathFolder + "/ICON0.PNG";
            var SavePic1 = SavePathFolder + "/PIC1.PNG";

            Action<PspUtilitySavedataFileData, String> Save =
                (PspUtilitySavedataFileData PspUtilitySavedataFileData, String FileName) =>
                {
                    if (PspUtilitySavedataFileData.Used)
                    {
                        HleIoManager.HleIoWrapper.WriteBytes(
                            FileName,
                            Memory.ReadBytes(PspUtilitySavedataFileData.BufferPointer, PspUtilitySavedataFileData.Size)
                        );
                    }
                };

            try
            {
                switch (Params.Mode)
                {
                    case PspUtilitySavedataMode.Autoload:
                    case PspUtilitySavedataMode.Load:
                    case PspUtilitySavedataMode.ListLoad:
                    {
                        try
                        {
                            Memory.WriteBytes(
                                Params.DataBufPointer,
                                HleIoManager.HleIoWrapper.ReadBytes(SaveDataBin)
                            );
                        }
                        catch (IOException)
                        {
                            throw (new SceKernelException(SceKernelErrors.ERROR_SAVEDATA_LOAD_NO_DATA));
                        }
                        catch (Exception Exception)
                        {
                            Console.WriteLine(Exception);
                            throw (new SceKernelException(SceKernelErrors.ERROR_SAVEDATA_LOAD_ACCESS_ERROR));
                        }
                    }
                        break;
                    case PspUtilitySavedataMode.Autosave:
                    case PspUtilitySavedataMode.Save:
                    case PspUtilitySavedataMode.ListSave:
                    {
                        try
                        {
                            HleIoManager.HleIoWrapper.Mkdir(SavePathFolder, SceMode.All);

                            HleIoManager.HleIoWrapper.WriteBytes(
                                SaveDataBin,
                                Memory.ReadBytes(Params.DataBufPointer, Params.DataSize)
                            );

                            Save(Params.Icon0FileData, SaveIcon0);
                            Save(Params.Pic1FileData, SavePic1);
                            //Save(Params.SfoParam, SavePic1);
                        }
                        catch (Exception Exception)
                        {
                            Console.Error.WriteLine(Exception);
                            throw (new SceKernelException(SceKernelErrors.ERROR_SAVEDATA_SAVE_ACCESS_ERROR));
                        }
                    }
                        break;

                    // "METAL SLUG XX" outputs the following on stdout after calling mode 8 (PspUtilitySavedataMode.Sizes):
                    //
                    // ------ SIZES ------
                    // ---------- savedata result ----------
                    // result = 0x801103c7
                    //
                    // bind : un used(0x0).
                    //
                    // -- dir name --
                    // title id : ULUS10495
                    // user  id : METALSLUGXX
                    //
                    // ms free size
                    //   cluster size(byte) : 32768 byte
                    //   free cluster num   : 32768
                    //   free size(KB)      : 1048576 KB
                    //   free size(string)  : "1 GB"
                    //
                    // ms data size(titleId=ULUS10495, userId=METALSLUGXX)
                    //   cluster num        : 0
                    //   size (KB)          : 0 KB
                    //   size (string)      : "0 KB"
                    //   size (32KB)        : 0 KB
                    //   size (32KB string) : "0 KB"
                    //
                    // utility data size
                    //   cluster num        : 13
                    //   size (KB)          : 416 KB
                    //   size (string)      : "416 KB"
                    //   size (32KB)        : 416 KB
                    //   size (32KB string) : "416 KB"
                    // error: SCE_UTILITY_SAVEDATA_TYPE_SIZES return 801103c7
                    case PspUtilitySavedataMode.Sizes:
                    {
                        SceKernelErrors SceKernelError = SceKernelErrors.ERROR_OK;

                        //Console.Error.WriteLine("Not Implemented: sceUtilitySavedataInitStart.Sizes");

                        uint SectorSize = 1024;
                        uint FreeSize = 32 * 1024 * 1024; // 32 MB
                        uint UsedSize = 0;

                        // MS free size.
                        // Gets the ammount of free space in the Memory Stick. If null,
                        // the size is ignored and no error is returned.
                        {
                            var SizeFreeInfo = (SizeFreeInfo*) Params.msFreeAddr.GetPointer<SizeFreeInfo>(Memory);
                            SizeFreeInfo->SectorSize = SectorSize;
                            SizeFreeInfo->FreeSectors = FreeSize / SectorSize;
                            SizeFreeInfo->FreeKb = FreeSize / 1024;

                            SizeFreeInfo->FreeKbString = (SizeFreeInfo->FreeKb) + "KB";
                        }

                        // MS data size.
                        // Gets the size of the data already saved in the Memory Stick.
                        // If null, the size is ignored and no error is returned.
                        {
                            var SizeUsedInfo = (SizeUsedInfo*) Params.msDataAddr.GetPointer<SizeUsedInfo>(Memory);

                            if (SizeUsedInfo != null)
                            {
#if true
                                if (true)
                                {
                                    Console.WriteLine(SizeUsedInfo->saveName);
                                    Console.WriteLine(SizeUsedInfo->gameName);

                                    SizeUsedInfo->UsedKb = UsedSize / 1024;
                                    SizeUsedInfo->UsedKb32 = UsedSize / (32 * 1024);

                                    SizeUsedInfo->UsedKbString = (SizeUsedInfo->UsedKb) + "KB";
                                    SizeUsedInfo->UsedKb32String = (SizeUsedInfo->UsedKb32) + "KB";
                                }
                                else
                                {
                                    SceKernelError = SceKernelErrors.ERROR_SAVEDATA_SIZES_NO_DATA;
                                }
#else
									SceKernelError = SceKernelErrors.ERROR_SAVEDATA_SIZES_NO_DATA;
#endif
                            }
                        }

                        // Utility data size.
                        // Gets the size of the data to be saved in the Memory Stick.
                        // If null, the size is ignored and no error is returned.
                        {
                            var SizeRequiredSpaceInfo =
                                (SizeRequiredSpaceInfo*) Params.utilityDataAddr.GetPointer<SizeRequiredSpaceInfo>(
                                    Memory);
                            if (SizeRequiredSpaceInfo != null)
                            {
                                long RequiredSize = 0;
                                RequiredSize += Params.Icon0FileData.Size;
                                RequiredSize += Params.Icon1FileData.Size;
                                RequiredSize += Params.Pic1FileData.Size;
                                RequiredSize += Params.Snd0FileData.Size;
                                RequiredSize += Params.DataSize;

                                SizeRequiredSpaceInfo->RequiredSpaceSectors =
                                    (uint) MathUtils.RequiredBlocks(RequiredSize, SectorSize);
                                SizeRequiredSpaceInfo->RequiredSpaceKb =
                                    (uint) MathUtils.RequiredBlocks(RequiredSize, 1024);
                                SizeRequiredSpaceInfo->RequiredSpace32KB =
                                    (uint) MathUtils.RequiredBlocks(RequiredSize, 32 * 1024);

                                SizeRequiredSpaceInfo->RequiredSpaceString =
                                    (SizeRequiredSpaceInfo->RequiredSpaceKb) + "KB";
                                SizeRequiredSpaceInfo->RequiredSpace32KBString =
                                    (SizeRequiredSpaceInfo->RequiredSpace32KB) + "KB";
                            }
                        }

                        if (SceKernelError != SceKernelErrors.ERROR_OK) throw (new SceKernelException(SceKernelError));
                    }
                        break;
                    case PspUtilitySavedataMode.List:
                    {
                        var ListRequest = (ListRequest*) Params.idListAddr.GetPointer<ListRequest>(Memory);
                        ListRequest->NumEntriesReaded = 0;
                    }
                        break;
                    case PspUtilitySavedataMode.GetSize:
                    {
                        //Params.DataSize
                        //throw (new SceKernelException(SceKernelErrors.ERROR_SAVEDATA_RW_NO_MEMSTICK));
                        //throw (new SceKernelException(SceKernelErrors.ERROR_SAVEDATA_RW_NO_DATA));
                        Console.Error.WriteLine("Not Implemented: sceUtilitySavedataInitStart.GetSize");
                    }
                        break;
                    case PspUtilitySavedataMode.Read:
                    case PspUtilitySavedataMode.ReadSecure:
                    {
                        //throw (new SceKernelException(SceKernelErrors.ERROR_SAVEDATA_RW_NO_DATA));
                        Console.Error.WriteLine("Not Implemented: sceUtilitySavedataInitStart.Read");
                    }
                        break;
                    default:
                        Console.Error.WriteLine("sceUtilitySavedataInitStart: Unsupported mode: " + Params.Mode);
                        Debug.Fail("sceUtilitySavedataInitStart: Unsupported mode: " + Params.Mode);
                        throw (new SceKernelException((SceKernelErrors) (-1)));
                    //break;
                }
                //throw(new NotImplementedException());

                Params.Base.Result = SceKernelErrors.ERROR_OK;
            }
            catch (SceKernelException SceKernelException)
            {
                Params.Base.Result = SceKernelException.SceKernelError;
            }
            finally
            {
                CurrentDialogStep = DialogStepEnum.SUCCESS;
            }


            return 0;
        }

        /// <summary>
        /// Shutdown the savedata utility. after calling this continue calling
        /// ::sceUtilitySavedataGetStatus to check when it has shutdown
        /// </summary>
        /// <returns>0 on success</returns>
        [HlePspFunction(NID = 0x9790B33C, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceUtilitySavedataShutdownStart()
        {
            //throw(new NotImplementedException());
            CurrentDialogStep = DialogStepEnum.SHUTDOWN;
            return 0;
        }

        /// <summary>
        /// Refresh status of the savedata function
        /// </summary>
        /// <param name="unknown">unknown, pass 1</param>
        [HlePspFunction(NID = 0xD4B95FFB, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public void sceUtilitySavedataUpdate(int unknown)
        {
            throw(new NotImplementedException());
        }

        /// <summary>
        /// Check the current status of the saving/loading/shutdown process
        /// Continue calling this to check current status of the process
        /// before calling this call also sceUtilitySavedataUpdate
        /// </summary>
        /// <returns>
        ///		2 if the process is still being processed.
        ///		3 on save/load success, then you can call sceUtilitySavedataShutdownStart.
        ///		4 on complete shutdown.
        /// </returns>
        [HlePspFunction(NID = 0x8874DBE0, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public DialogStepEnum sceUtilitySavedataGetStatus()
        {
            try
            {
                return CurrentDialogStep;
            }
            finally
            {
                if (CurrentDialogStep == DialogStepEnum.SHUTDOWN)
                {
                    CurrentDialogStep = DialogStepEnum.NONE;
                }
            }
        }
    }
}