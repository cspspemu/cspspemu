using System;
using CSharpUtils;

namespace CSPspEmu.Hle.Formats
{
    public unsafe class ElfPsp
    {
        [Flags]
        public enum ModuleFlags : ushort
        {
            User = 0x0000,
            Kernel = 0x1000,
        }

        [Flags]
        public enum LibFlags : ushort
        {
            DirectJump = 0x0001,
            Syscall = 0x4000,
            SysLib = 0x8000,
        }

        public enum ModuleNids : uint
        {
            ModuleInfo = 0xF01D73A7,
            ModuleBootstart = 0xD3744BE0,
            ModuleRebootBefore = 0x2F064FA6,
            ModuleStart = 0xD632ACDB,
            ModuleStartThreadParameter = 0x0F7C276C,
            ModuleStop = 0xCEE8593C,
            ModuleStopThreadParameter = 0xCF0CC697,
        }

        public struct ModuleExport
        {
            /// <summary>
            /// Address to a stringz with the module.
            /// Relative to ?
            /// </summary>
            public uint Name;

            /// <summary>
            /// ?
            /// </summary>
            public ushort Version;

            /// <summary>
            /// ?
            /// </summary>
            public ushort Flags;

            /// <summary>
            /// ?
            /// </summary>
            public byte EntrySize;

            /// <summary>
            /// ?
            /// </summary>
            public byte VariableCount;

            /// <summary>
            /// ?
            /// </summary>
            public ushort FunctionCount;

            /// <summary>
            /// ?
            /// </summary>
            public uint Exports;
        }

        public struct ModuleImport
        {
            /// <summary>
            /// Address to a stringz with the module.
            /// </summary>
            public uint Name;

            /// <summary>
            /// Version of the module?
            /// </summary>
            public ushort Version;

            /// <summary>
            /// Flags for the module.
            /// </summary>
            public ushort Flags;

            /// <summary>
            /// ?
            /// </summary>
            public byte EntrySize;

            /// <summary>
            /// Number of imported variables.
            /// </summary>
            public byte VariableCount;

            /// <summary>
            /// Number of imported functions.
            /// </summary>
            public ushort FunctionCount;

            /// <summary>
            /// Address to the nid pointer. (Read)
            /// </summary>
            public uint NidAddress;

            /// <summary>
            /// Address to the function table. (Write 16 bits. jump/syscall)
            /// </summary>
            public uint CallAddress;
        }

        // http://hitmen.c02.at/files/yapspd/psp_doc/chap26.html
        // 26.2.2.8
        public struct ModuleInfo
        {
            public enum AtributesEnum : ushort
            {
                UserMode = 0x0000,
                KernelMode = 0x100,
            }

            /// <summary>
            /// Module Attributes
            /// </summary>
            public AtributesEnum ModuleAtributes;

            /// <summary>
            /// Module Version
            /// </summary>
            public ushort ModuleVersion;

            /// <summary>
            /// Module Name (0 terminated)
            /// </summary>
#pragma warning disable 649
            private fixed byte _nameRaw[28];
#pragma warning restore 649

            /// <summary>
            /// 
            /// </summary>
            public string Name
            {
                get
                {
                    fixed (byte* nameRawPtr = _nameRaw) return PointerUtils.PtrToStringUtf8(nameRawPtr);
                }
            }

            /// <summary>
            /// Initial value for GP (Global Pointer).
            /// </summary>
            public uint Gp;

            /// <summary>
            /// Address of section .lib.ent
            /// </summary>
            public uint ExportsStart;

            /// <summary>
            /// Address of section .lib.ent.btm
            /// </summary>
            public uint ExportsEnd;

            /// <summary>
            /// Address of section .lib.stub
            /// </summary>
            public uint ImportsStart;

            /// <summary>
            /// Address of section .lib.stub.btm
            /// </summary>
            public uint ImportsEnd;
        }
    }
}