using System;
using System.IO;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSharpUtils.Extensions;
using CSPspEmu.Hle.Formats;

namespace CSPspEmu.Hle.Vfs.Iso
{
    public unsafe class HleIoDriverIso : IHleIoDriver
    {
        public IsoFile Iso { get; protected set; }

        public class IsoFileArgument : IDisposable
        {
            public IsoNode IsoNode;
            public long StartSector;
            public long Size;
            public Stream Stream;

            public void Dispose()
            {
                if (IsoNode != null) IsoNode.Dispose();
                if (Stream != null) Stream.Dispose();
            }
        }

        public HleIoDriverIso(IsoFile Iso)
        {
            this.Iso = Iso;
        }

        public unsafe int IoInit()
        {
            return 0;
        }

        public unsafe int IoExit()
        {
            return 0;
        }

        public unsafe int IoOpen(HleIoDrvFileArg HleIoDrvFileArg, string FileName, HleIoFlags Flags, SceMode Mode)
        {
            // Ignore the filename in "umd0:xxx".
            // Using umd0: is always opening the whole UMD in sector block mode,
            // ignoring the file name specified after the colon.
            if (HleIoDrvFileArg.DriverName.ToLower().StartsWith("umd"))
            {
                FileName = "";
            }

            // disc0:/sce_lbn0x5fa0_size0x1428
            //Console.WriteLine(":::::::::" + FileName);
            if (FileName == "")
            {
                HleIoDrvFileArg.FileArgument = new IsoFileArgument()
                {
                    IsoNode = null,
                    StartSector = 0,
                    Size = Iso.Stream.Length,
                    Stream = Iso.Stream,
                };
                return 0;
            }

            if (FileName.StartsWith("/sce_"))
            {
                int Sector = 0, Size = 0;
                var Parts = FileName.Substring(5).Split('_');
                foreach (var Part in Parts)
                {
                    if (Part.StartsWith("lbn"))
                    {
                        var Number = Part.Substring(3);
                        Sector = NumberUtils.ParseIntegerConstant(Number, 16);
                    }
                    else if (Part.StartsWith("size"))
                    {
                        var Number = Part.Substring(4);
                        Size = NumberUtils.ParseIntegerConstant(Number, 16);
                    }
                    else
                    {
                        throw(new NotImplementedException("Can't handle special filename '" + FileName + "' part '" +
                                                          Part + "'"));
                    }
                    //Console.WriteLine(Part);
                }
                //Console.WriteLine("SPECIAL({0}, {1})", lbn, size);
                //Console.WriteLine("SPECIAL!!!!!!!!!!!!!!!!!!!!");
                HleIoDrvFileArg.FileArgument = new IsoFileArgument()
                {
                    IsoNode = null,
                    StartSector = Sector,
                    Size = Size,
                    Stream = Iso.Stream.SliceWithLength(Sector * IsoFile.SectorSize, Size),
                };
                return 0;
            }

            //Console.WriteLine(FileName);
            var IsoNode = Iso.Root.Locate(FileName);
            ;
            HleIoDrvFileArg.FileArgument = new IsoFileArgument()
            {
                IsoNode = IsoNode,
                StartSector = IsoNode.DirectoryRecord.Extent,
                Size = IsoNode.DirectoryRecord.Size,
                Stream = IsoNode.Open(),
            };
            return 0;
        }

        public unsafe int IoClose(HleIoDrvFileArg HleIoDrvFileArg)
        {
            var IsoFileArgument = ((IsoFileArgument) HleIoDrvFileArg.FileArgument);
            IsoFileArgument.Stream.Close();
            return 0;
            //throw new NotImplementedException();
        }

        public unsafe int IoRead(HleIoDrvFileArg HleIoDrvFileArg, byte* OutputPointer, int OutputLength)
        {
            var IsoFileArgument = ((IsoFileArgument) HleIoDrvFileArg.FileArgument);
            var OutputData = new byte[OutputLength];
            int Readed = IsoFileArgument.Stream.Read(OutputData, 0, OutputLength);
            PointerUtils.Memcpy(OutputPointer, OutputData, OutputLength);
            return Readed;
            //throw new NotImplementedException();
        }

        public unsafe int IoWrite(HleIoDrvFileArg HleIoDrvFileArg, byte* InputPointer, int InputLength)
        {
            throw new NotImplementedException();
        }

        public unsafe long IoLseek(HleIoDrvFileArg HleIoDrvFileArg, long Offset, SeekAnchor Whence)
        {
            var IsoFileArgument = ((IsoFileArgument) HleIoDrvFileArg.FileArgument);
            //Stream.Seek(
            return IsoFileArgument.Stream.Seek(Offset, (SeekOrigin) Whence);
        }

        public enum UmdCommandEnum : uint
        {
            /// <summary>
            /// UMD file seek set.
            /// </summary>
            FileSeekSet = 0x01010005,

            /// <summary>
            /// Get UMD Primary Volume Descriptor
            /// </summary>
            GetPrimaryVolumeDescriptor = 0x01020001,

            /// <summary>
            /// Get UMD Path Table
            /// </summary>
            GetPathTable = 0x01020002,

            /// <summary>
            /// Get Sector size
            /// </summary>
            GetSectorSize = 0x01020003,

            /// <summary>
            /// Get UMD file pointer.
            /// </summary>
            GetFilePointer = 0x01020004,

            /// <summary>
            /// Get UMD file start sector.
            /// </summary>
            GetStartSector = 0x01020006,

            /// <summary>
            /// Get UMD file length in bytes.
            /// </summary>
            GetLengthInBytes = 0x01020007,

            /// <summary>
            /// Read UMD file.
            /// </summary>
            ReadFile = 0x01030008,

            /// <summary>
            /// UMD disc read sectors operation.
            /// </summary>
            ReadSectors = 0x01F30003,

            /// <summary>
            /// UMD file seek whence.
            /// </summary>
            FileSeek = 0x01F100A6,

            /// <summary>
            /// Define decryption key (DRM by amctrl.prx).
            /// </summary>
            DefineDecryptionKey = 0x04100001
        }

        public struct FileSeekIn
        {
            public long Offset;
            public uint Unknown;
            public uint Whence;
        }

        unsafe delegate void* ActionIntPVoid(int Value);

        public unsafe int IoIoctl(HleIoDrvFileArg HleIoDrvFileArg, uint Command, byte* InputPointer, int InputLength,
            byte* OutputPointer, int OutputLength)
        {
            var IsoFileArgument = ((IsoFileArgument) HleIoDrvFileArg.FileArgument);

            ActionIntPVoid ExpectedOutputSize = (int MinimumSize) =>
            {
                if (OutputLength < MinimumSize || OutputPointer == null)
                    throw (new SceKernelException(SceKernelErrors.ERROR_INVALID_ARGUMENT));
                return OutputPointer;
            };

            ActionIntPVoid ExpectedInputSize = (int MinimumSize) =>
            {
                if (InputLength < MinimumSize || InputPointer == null)
                    throw (new SceKernelException(SceKernelErrors.ERROR_INVALID_ARGUMENT));
                return InputPointer;
            };

            switch ((UmdCommandEnum) Command)
            {
#if false
				case UmdCommandEnum.DefineDecryptionKey:
					{
						return 0;
					}
#endif
                case UmdCommandEnum.FileSeekSet:
                {
                    var In = (uint*) ExpectedInputSize(sizeof(uint));
                    IsoFileArgument.Stream.Position = *In;
                    return 0;
                }
                case UmdCommandEnum.ReadSectors:
                {
                    var In = (uint*) ExpectedInputSize(sizeof(uint));
                    var NumberOfSectors = *In;
                    var CopySize = (int) (IsoFile.SectorSize * NumberOfSectors);
                    var Out = (byte*) ExpectedOutputSize(CopySize);
                    var BytesReaded = IsoFileArgument.Stream.ReadBytes(CopySize);
                    PointerUtils.Memcpy(Out, BytesReaded, BytesReaded.Length);
                    return 0;
                }
                case UmdCommandEnum.FileSeek:
                {
                    var In = (FileSeekIn*) ExpectedInputSize(sizeof(FileSeekIn));
                    IsoFileArgument.Stream.Seek(In->Offset, (SeekOrigin) In->Whence);
                    return 0;
                }
                case UmdCommandEnum.GetFilePointer:
                {
                    var Out = (uint*) ExpectedOutputSize(sizeof(uint));
                    *Out = (uint) IsoFileArgument.Stream.Position;
                    return 0;
                }
                case UmdCommandEnum.GetStartSector:
                {
                    var Out = (uint*) ExpectedOutputSize(sizeof(uint));
                    *Out = (uint) IsoFileArgument.StartSector;
                    return 0;
                }
                case UmdCommandEnum.GetSectorSize:
                {
                    var Out = (uint*) ExpectedOutputSize(sizeof(uint));
                    *Out = IsoFile.SectorSize;
                    return 0;
                }
                case UmdCommandEnum.GetLengthInBytes:
                {
                    var Out = (ulong*) ExpectedOutputSize(sizeof(ulong));
                    *Out = (uint) IsoFileArgument.Size;
                    return 0;
                }
                case UmdCommandEnum.GetPrimaryVolumeDescriptor:
                {
                    var Out = (PrimaryVolumeDescriptor*) ExpectedOutputSize(sizeof(PrimaryVolumeDescriptor));
                    *Out = Iso.PrimaryVolumeDescriptor;
                    return 0;
                }
                default:
                    throw new NotImplementedException(String.Format("Not implemented command 0x{0:X} : {1}", Command,
                        (UmdCommandEnum) Command));
            }
        }

        public unsafe int IoRemove(HleIoDrvFileArg HleIoDrvFileArg, string Name)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoMkdir(HleIoDrvFileArg HleIoDrvFileArg, string Name, SceMode Mode)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoRmdir(HleIoDrvFileArg HleIoDrvFileArg, string Name)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoDopen(HleIoDrvFileArg HleIoDrvFileArg, string Name)
        {
            //throw new NotImplementedException();
            var IsoNode = Iso.Root.Locate(Name);
            //HleIoDrvFileArg.FileArgument = new DisposableDummy<DirectoryEnumerator<IsoNode>>(new DirectoryEnumerator<IsoNode>(IsoNode.Childs.ToArray()));
            HleIoDrvFileArg.FileArgument = new DirectoryEnumerator<IsoNode>(IsoNode.Childs.ToArray());
            return 0;
        }

        public unsafe int IoDclose(HleIoDrvFileArg HleIoDrvFileArg)
        {
            //throw new NotImplementedException();
            return 0;
        }

        public unsafe int IoDread(HleIoDrvFileArg HleIoDrvFileArg, HleIoDirent* IoDirent)
        {
            //var Enumerator = (DirectoryEnumerator<IsoNode>)(DisposableDummy<DirectoryEnumerator<IsoNode>>)HleIoDrvFileArg.FileArgument;
            var Enumerator = (DirectoryEnumerator<IsoNode>) HleIoDrvFileArg.FileArgument;

            // More items.
            if (Enumerator.MoveNext())
            {
                //Console.Error.WriteLine("'{0}'", Enumerator.Current.ToString());
                var IsoNode = Enumerator.Current;
                {
                    IoDirent->Name = IsoNode.Name;
                    _IoGetstat(IsoNode, &IoDirent->Stat);
                }
            }
            // No more items.
            else
            {
            }

            return Enumerator.GetLeft();
        }

        public unsafe void _IoGetstat(IsoNode IsoNode, SceIoStat* Stat)
        {
            //IsoNode.DirectoryRecord.Date
            Stat->Mode = 0;
            Stat->Mode |= SceMode.UserCanRead | SceMode.UserCanWrite | SceMode.UserCanExecute;
            Stat->Mode |= SceMode.GroupCanRead | SceMode.GroupCanWrite | SceMode.GroupCanExecute;
            Stat->Mode |= SceMode.OtherCanRead | SceMode.OtherCanWrite | SceMode.OtherCanExecute;

            if (IsoNode.IsDirectory)
            {
                Stat->Mode = SceMode.Directory;
                Stat->Attributes = IOFileModes.Directory;
            }
            else
            {
                Stat->Mode = SceMode.File;
                Stat->Attributes = IOFileModes.File | IOFileModes.CanRead | IOFileModes.CanWrite |
                                   IOFileModes.CanExecute;
            }
            Stat->Size = IsoNode.DirectoryRecord.Size;
            Stat->TimeCreation = ScePspDateTime.FromDateTime(IsoNode.DirectoryRecord.Date);
            Stat->TimeLastAccess = ScePspDateTime.FromDateTime(IsoNode.DirectoryRecord.Date);
            Stat->TimeLastModification = ScePspDateTime.FromDateTime(IsoNode.DirectoryRecord.Date);
            Stat->DeviceDependentData0 = IsoNode.DirectoryRecord.Extent;

            //Stat[0].DeviceDependentData
            //throw new NotImplementedException();
        }

        public unsafe int IoGetstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* Stat)
        {
            //Console.WriteLine(FileName);
            _IoGetstat(Iso.Root.Locate(FileName), Stat);

            return 0;
        }

        public unsafe int IoChstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* stat, int bits)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoRename(HleIoDrvFileArg HleIoDrvFileArg, string OldFileName, string NewFileName)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoChdir(HleIoDrvFileArg HleIoDrvFileArg, string DirectoryName)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoMount(HleIoDrvFileArg HleIoDrvFileArg)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoUmount(HleIoDrvFileArg HleIoDrvFileArg)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoDevctl(HleIoDrvFileArg HleIoDrvFileArg, string DeviceName, uint Command, byte* InputPointer,
            int InputLength, byte* OutputPointer, int OutputLength)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoUnk21(HleIoDrvFileArg HleIoDrvFileArg)
        {
            throw new NotImplementedException();
        }
    }
}