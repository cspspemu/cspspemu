using CSharpUtils;
using System;
using System.Collections.Generic;

namespace CSPspEmu.Core.Cpu.VFpu
{
    public interface IVfpuPrefixCommon
    {
        void EnableAndSetValueAndPc(uint value, uint pc);
    }

    public class VfpuPrefix : IVfpuPrefixCommon
    {
        public uint DeclaredPc;
        public uint UsedPc;
        public uint Value;
        public bool Enabled;
        public int UsedCount;

        internal void Consume()
        {
            Enabled = false;
        }

        public void CheckPrefixUsage(uint pc)
        {
            // Disable the prefix once it have been used.
            if (!Enabled) return;
            if (UsedCount > 0 && UsedPc != pc)
            {
                Enabled = false;
                //Console.WriteLine("VfpuPrefix.Enabled = false | {0:X8} : {1:X8} | {2:X8}", Value, DeclaredPC, PC);
            }

            UsedPc = pc;
            UsedCount++;
        }

        public static implicit operator uint(VfpuPrefix value) => value.Value;

        public static implicit operator VfpuPrefix(uint value) => new VfpuPrefix() {Value = value};

        // swz(xyzw)
        //assert(i >= 0 && i < 4);
        //return (value >> (0 + i * 2)) & 3;
        public uint SourceIndex(int i)
        {
            return BitUtils.Extract(Value, 0 + i * 2, 2);
        }

        public void SourceIndex(int i, uint valueToInsert) => BitUtils.Insert(ref Value, 0 + i * 2, 2, valueToInsert);

        // abs(xyzw)
        //assert(i >= 0 && i < 4);
        //return (value >> (8 + i * 1)) & 1;
        public bool SourceAbsolute(int i) => BitUtils.Extract(Value, 8 + i * 1, 1) != 0;

        public void SourceAbsolute(int i, bool valueToInsert) => BitUtils.Insert(ref Value, 8 + i * 1, 1, valueToInsert ? 1U : 0U);

        // cst(xyzw)
        //assert(i >= 0 && i < 4);
        //return (value >> (12 + i * 1)) & 1;
        public bool SourceConstant(int i) => BitUtils.Extract(Value, 12 + i * 1, 1) != 0;

        public void SourceConstant(int i, bool valueToInsert) => BitUtils.Insert(ref Value, 12 + i * 1, 1, valueToInsert ? 1U : 0U);

        // neg(xyzw)
        //assert(i >= 0 && i < 4);
        //return (value >> (16 + i * 1)) & 1;
        public bool SourceNegate(int i) => BitUtils.Extract(Value, 16 + i * 1, 1) != 0;

        public void SourceNegate(int i, bool valueToInsert) => BitUtils.Insert(ref Value, 16 + i * 1, 1, valueToInsert ? 1U : 0U);

        public void EnableAndSetValueAndPc(uint value, uint pc)
        {
            Enabled = true;
            Value = value;
            DeclaredPc = pc;
            UsedCount = 0;
            //Console.WriteLine("VfpuPrefix.Enabled = true | {0:X8} : {1:X8} | {2:X8}", Value, DeclaredPC, PC);
        }

        public string Format
        {
            get
            {
                var parts = new List<string>();
                for (var index = 0; index < 4; index++)
                {
                    string part;
                    if (SourceConstant(index))
                    {
                        switch (SourceIndex(index))
                        {
                            case 0:
                                part = SourceAbsolute(index) ? "3" : "0";
                                break;
                            case 1:
                                part = SourceAbsolute(index) ? "1/3" : "1";
                                break;
                            case 2:
                                part = SourceAbsolute(index) ? "1/4" : "2";
                                break;
                            case 3:
                                part = SourceAbsolute(index) ? "1/6" : "1/2";
                                break;
                            default: throw (new InvalidOperationException());
                        }
                    }
                    else
                    {
                        part = ComponentNames[SourceIndex(index)];
                        if (SourceAbsolute(index)) part = "|" + part + "|";
                        if (SourceNegate(index)) part = "-" + part;
                    }

                    parts.Add(part);
                }
                return "[" + string.Join(", ", parts) + "]";
            }
        }


        public static readonly string[] ComponentNames = {"x", "y", "z", "w"};

        public override string ToString() =>
            $"VfpuPrefix(Enabled={Enabled}, UsedPC=0x{UsedPc:X}, DeclaredPC=0x{DeclaredPc:X})({Format})";

        public bool IsValidIndex(int index) => (index >= 0) && (index < 4);
    }

    public class VfpuDestinationPrefix : IVfpuPrefixCommon
    {
        public uint DeclaredPc;
        public uint UsedPc;
        public uint Value;
        public bool Enabled;
        public int UsedCount;

        public void CheckPrefixUsage(uint pc)
        {
            //Console.WriteLine("VfpuDestinationPrefix.CheckPrefixUsage | {0:X8} : {1:X8} | {2:X8} | {3} | {4}", Value, DeclaredPC, PC, Enabled, UsedCount);
            // Disable the prefix once it have been used.
            if (!Enabled) return;
            if (UsedCount > 0 && UsedPc != pc)
            {
                Enabled = false;
                //Console.WriteLine("VfpuDestinationPrefix.Enabled = false | {0:X8} : {1:X8} | {2:X8}", Value, DeclaredPC, PC);
            }

            UsedPc = pc;
            UsedCount++;
        }

        public static implicit operator uint(VfpuDestinationPrefix value) => value.Value;

        public static implicit operator VfpuDestinationPrefix(uint value) => new VfpuDestinationPrefix() {Value = value};

        // sat(xyzw)
        //assert(i >= 0 && i < 4);
        //return (value >> (0 + i * 2)) & 3;
        public uint DestinationSaturation(int i) => BitUtils.Extract(Value, 0 + i * 2, 2);

        public void DestinationSaturation(int i, uint valueToInsert) => BitUtils.Insert(ref Value, 0 + i * 2, 2, valueToInsert);

        // msk(xyzw)
        //assert(i >= 0 && i < 4);
        //return (value >> (8 + i * 1)) & 1;
        public bool DestinationMask(int i) => BitUtils.Extract(Value, 8 + i * 1, 1) != 0;

        public void DestinationMask(int i, bool valueToInsert) => BitUtils.Insert(ref Value, 8 + i * 1, 1, valueToInsert ? 1U : 0U);

        public void EnableAndSetValueAndPc(uint value, uint pc)
        {
            Enabled = true;
            Value = value;
            DeclaredPc = pc;
            UsedCount = 0;
            //Console.WriteLine("VfpuDestinationPrefix.Enabled = true | {0:X8} : {1:X8} | {2:X8}", Value, DeclaredPC, PC);
        }

        public string Format
        {
            get
            {
                var parts = new List<string>();
                for (var index = 0; index < 4; index++)
                {
                    string part;
                    if (!DestinationMask(index))
                    {
                        switch (DestinationSaturation(index))
                        {
                            case 1:
                                part = "0:1";
                                break;
                            case 3:
                                part = "-1:1";
                                break;
                            default: throw (new InvalidOperationException());
                        }
                    }
                    else
                    {
                        part = "M";
                    }

                    parts.Add(part);
                }
                return "[" + string.Join(", ", parts) + "]";
            }
        }

        public override string ToString() => $"VfpuDestinationPrefix(Enabled={Enabled}, UsedPC=0x{UsedPc:X}, DeclaredPC=0x{DeclaredPc:X})({Format})";

        public bool IsValidIndex(int index) => (index >= 0) && (index < 4);
    }
}