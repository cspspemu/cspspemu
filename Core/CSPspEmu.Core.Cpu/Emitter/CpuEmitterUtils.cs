using CSharpUtils;
using CSPspEmu.Core.Cpu.VFpu;
using System;
using System.Runtime;

namespace CSPspEmu.Core.Cpu.Emitter
{
    public unsafe class CpuEmitterUtils
    {
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static uint _rotr_impl(uint value, int offset) => (value >> offset) | (value << (32 - offset));

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int _max_impl(int left, int right) => (left > right) ? left : right;

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int _min_impl(int left, int right) => (left < right) ? left : right;

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static uint _bitrev_impl(uint v)
        {
            v = ((v >> 1) & 0x55555555) | ((v & 0x55555555) << 1); // swap odd and even bits
            v = ((v >> 2) & 0x33333333) | ((v & 0x33333333) << 2); // swap consecutive pairs
            v = ((v >> 4) & 0x0F0F0F0F) | ((v & 0x0F0F0F0F) << 4); // swap nibbles ... 
            v = ((v >> 8) & 0x00FF00FF) | ((v & 0x00FF00FF) << 8); // swap bytes
            v = (v >> 16) | (v << 16); // swap 2-byte long pairs
            return v;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void _div_impl(CpuThreadState cpuThreadState, int left, int right)
        {
            if (right == 0)
            {
                cpuThreadState.Lo = right;
                cpuThreadState.Hi = left;
            }
            else if (left == int.MinValue && right == -1)
            {
                cpuThreadState.Lo = int.MinValue;
                cpuThreadState.Hi = 0;
            }
            else
            {
                cpuThreadState.Lo = unchecked(left / right);
                cpuThreadState.Hi = unchecked(left % right);
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void _divu_impl(CpuThreadState cpuThreadState, uint left, uint right)
        {
            if (right == 0)
            {
                cpuThreadState.Lo = (int) right;
                cpuThreadState.Hi = (int) left;
            }
            else
            {
                cpuThreadState.Lo = unchecked((int) (left / right));
                cpuThreadState.Hi = unchecked((int) (left % right));
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static uint _ext_impl(uint data, int pos, int size) => BitUtils.Extract(data, pos, size);

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static uint _ins_impl(uint initialData, uint data, int pos, int size) => BitUtils.Insert(initialData, pos, size, data);

        // http://aggregate.org/MAGIC/
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static uint _clo_impl(uint x)
        {
            uint ret = 0;
            while ((x & 0x80000000) != 0)
            {
                x <<= 1;
                ret++;
            }
            return ret;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static uint _clz_impl(uint x) => _clo_impl(~x);

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static uint _wsbh_impl(uint v) => ((v & 0xFF00FF00) >> 8) | ((v & 0x00FF00FF) << 8);

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static uint _wsbw_impl(uint v) => (
            ((v & 0xFF000000) >> 24) |
            ((v & 0x00FF0000) >> 8) |
            ((v & 0x0000FF00) << 8) |
            ((v & 0x000000FF) << 24)
        );

        public static int _cvt_w_s_impl(CpuThreadState cpuThreadState, float fs)
        {
            //Console.WriteLine("_cvt_w_s_impl: {0}", CpuThreadState.FPR[FS]);
            switch (cpuThreadState.Fcr31.Rm)
            {
                case CpuThreadState.Fcr31Struct.TypeEnum.Rint: return (int) MathFloat.Rint(fs);
                case CpuThreadState.Fcr31Struct.TypeEnum.Cast: return (int) MathFloat.Cast(fs);
                case CpuThreadState.Fcr31Struct.TypeEnum.Ceil: return (int) MathFloat.Ceil(fs);
                case CpuThreadState.Fcr31Struct.TypeEnum.Floor: return (int) MathFloat.Floor(fs);
            }

            throw(new InvalidCastException("RM has an invalid value!!"));
            //case CpuThreadState.FCR31.TypeEnum.Floor: CpuThreadState.FPR_I[FD] = (int)MathFloat.Floor(CpuThreadState.FPR[FS]); break;
        }

        public static void _cfc1_impl(CpuThreadState cpuThreadState, int rd, int rt)
        {
            switch (rd)
            {
                case 0: // readonly?
                    throw (new NotImplementedException("_cfc1_impl.RD=0"));
                case 31:
                    cpuThreadState.Gpr[rt] = (int) cpuThreadState.Fcr31.Value;
                    break;
                default: throw (new Exception($"Unsupported CFC1({rd})"));
            }
        }

        public static void _ctc1_impl(CpuThreadState cpuThreadState, int rd, int rt)
        {
            switch (rd)
            {
                case 31:
                    cpuThreadState.Fcr31.Value = (uint) cpuThreadState.Gpr[rt];
                    break;
                default: throw (new Exception($"Unsupported CFC1({rd})"));
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void _comp_impl(CpuThreadState cpuThreadState, float s, float t, bool fcUnordererd,
            bool fcEqual, bool fcLess, bool fcInvQnan)
        {
            if (float.IsNaN(s) || float.IsNaN(t))
            {
                cpuThreadState.Fcr31.Cc = fcUnordererd;
            }
            else
            {
                //bool cc = false;
                //if (fc_equal) cc = cc || (s == t);
                //if (fc_less) cc = cc || (s < t);
                //return cc;
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                var equal = fcEqual && s == t;
                var less = fcLess && s < t;

                cpuThreadState.Fcr31.Cc = (less || equal);
            }
        }

        public static void _break_impl(CpuThreadState cpuThreadState, uint pc, uint value)
        {
            cpuThreadState.Pc = pc;
            Console.Error.WriteLine("-------------------------------------------------------------------");
            Console.Error.WriteLine("-- BREAK  ---------------------------------------------------------");
            Console.Error.WriteLine("-------------------------------------------------------------------");
            throw (new PspBreakException("Break!"));
        }

        public static void _cache_impl(CpuThreadState cpuThreadState, uint pc, uint value)
        {
            cpuThreadState.Pc = pc;
            //Console.Error.WriteLine("cache! : 0x{0:X}", Value);
            //CpuThreadState.CpuProcessor.sceKernelIcacheInvalidateAll();
        }

        public static void _sync_impl(CpuThreadState cpuThreadState, uint pc, uint value)
        {
            cpuThreadState.Pc = pc;
            //Console.WriteLine("Not implemented 'sync' instruction at 0x{0:X8} with value 0x{1:X8}", PC, Value);
        }

        private static readonly uint[] LwrMask = new uint[] {0x00000000, 0xFF000000, 0xFFFF0000, 0xFFFFFF00};
        private static readonly int[] LwrShift = new int[] {0, 8, 16, 24};

        private static readonly uint[] LwlMask = new uint[] {0x00FFFFFF, 0x0000FFFF, 0x000000FF, 0x00000000};
        private static readonly int[] LwlShift = new int[] {24, 16, 8, 0};

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static uint _lwl_exec(CpuThreadState cpuThreadState, uint rs, int offset, uint rt)
        {
            //Console.WriteLine("_lwl_exec");
            var address = (uint) (rs + offset);
            var addressAlign = (uint) address & 3;
            var value = *(uint*) cpuThreadState.GetMemoryPtr(address & unchecked((uint) ~3));
            return (uint) ((value << LwlShift[addressAlign]) | (rt & LwlMask[addressAlign]));
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static uint _lwr_exec(CpuThreadState cpuThreadState, uint rs, int offset, uint rt)
        {
            //Console.WriteLine("_lwr_exec");
            var address = (uint) (rs + offset);
            var addressAlign = (uint) address & 3;
            var value = *(uint*) cpuThreadState.GetMemoryPtr(address & unchecked((uint) ~3));
            return (uint) ((value >> LwrShift[addressAlign]) | (rt & LwrMask[addressAlign]));
        }

        private static readonly uint[] SwlMask = {0xFFFFFF00, 0xFFFF0000, 0xFF000000, 0x00000000};
        private static readonly int[] SwlShift = {24, 16, 8, 0};

        private static readonly uint[] SwrMask = {0x00000000, 0x000000FF, 0x0000FFFF, 0x00FFFFFF};
        private static readonly int[] SwrShift = {0, 8, 16, 24};

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void _swl_exec(CpuThreadState cpuThreadState, uint rs, int offset, uint rt)
        {
            var address = (uint) (rs + offset);
            var addressAlign = (uint) address & 3;
            var addressPointer = (uint*) cpuThreadState.GetMemoryPtr(address & 0xFFFFFFFC);

            *addressPointer = (rt >> SwlShift[addressAlign]) | (*addressPointer & SwlMask[addressAlign]);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void _swr_exec(CpuThreadState cpuThreadState, uint rs, int offset, uint rt)
        {
            //Console.WriteLine("_swr_exec");
            var address = (uint) (rs + offset);
            var addressAlign = (uint) address & 3;
            var addressPointer = (uint*) cpuThreadState.GetMemoryPtr(address & 0xFFFFFFFC);

            *addressPointer = (rt << SwrShift[addressAlign]) | (*addressPointer & SwrMask[addressAlign]);
        }

        public static void _lvl_svl_q(CpuThreadState cpuThreadState, bool save, float* r0, float* r1, float* r2,
            float* r3, uint address)
        {
            //Console.Error.WriteLine("+LLLLLLLLLLLLL {0:X8}", Address);

            var k = (int) (3 - ((address >> 2) & 3));
            address &= unchecked((uint) ~0xF);

            var r = stackalloc float*[4];
            r[0] = r0;
            r[1] = r1;
            r[2] = r2;
            r[3] = r3;

            fixed (float* vfpr = &cpuThreadState.Vfr0)
            {
                for (var j = k; j < 4; j++, address += 4)
                {
                    var ptr = r[j];
                    var memoryAddress = address;
                    var memory = (float*) cpuThreadState.GetMemoryPtr(memoryAddress);

                    //Console.Error.WriteLine("_lvl_svl_q({0}): {1:X8}: Reg({2:X8}) {3} Mem({4:X8})", j, memory_address, *(int*)ptr, Save ? "->" : "<-", *(int*)memory);

                    LanguageUtils.Transfer(ref *memory, ref *ptr, save);

                    //Console.Error.WriteLine("_lvl_svl_q({0}): {1:X8}: Reg({2:X8}) {3} Mem({4:X8})", j, memory_address, *(int*)ptr, Save ? "->" : "<-", *(int*)memory);
                }
            }

            //Console.Error.WriteLine("--------------");
        }

        public static void _lvr_svr_q(CpuThreadState cpuThreadState, bool save, float* r0, float* r1, float* r2,
            float* r3, uint address)
        {
            //Console.Error.WriteLine("+RRRRRRRRRRRRR {0:X8}", Address);

            int k = (int) (4 - ((address >> 2) & 3));
            //Address &= unchecked((uint)~0xF);

            var r = stackalloc float*[4];
            r[0] = r0;
            r[1] = r1;
            r[2] = r2;
            r[3] = r3;

            fixed (float* vfpr = &cpuThreadState.Vfr0)
            {
                for (var j = 0; j < k; j++, address += 4)
                {
                    var ptr = r[j];
                    var memoryAddress = address;
                    var memory = (float*) cpuThreadState.GetMemoryPtr(memoryAddress);

                    //Console.Error.WriteLine("_lvl_svr_q({0}): {1:X8}: Reg({2:X8}) {3} Mem({4:X8})", j, memory_address, *(int*)ptr, Save ? "->" : "<-", *(int*)memory);

                    LanguageUtils.Transfer(ref *memory, ref *ptr, save);

                    //Console.Error.WriteLine("_lvl_svr_q({0}): {1:X8}: Reg({2:X8}) {3} Mem({4:X8})", j, memory_address, *(int*)ptr, Save ? "->" : "<-", *(int*)memory);
                }
            }

            //Console.Error.WriteLine("--------------");
        }

        public static float _vslt_impl(float a, float b) => float.IsNaN(a) || float.IsNaN(b) ? 0f : ((a < b) ? 1f : 0f);
        public static float _vsge_impl(float a, float b) => float.IsNaN(a) || float.IsNaN(b) ? 0f : ((a >= b) ? 1f : 0f);

        public static void _vrnds(CpuThreadState cpuThreadState, int seed) => cpuThreadState.Random = new Random(seed);

        public static int _vrndi(CpuThreadState cpuThreadState)
        {
            var data = new byte[4];
            cpuThreadState.Random.NextBytes(data);
            return BitConverter.ToInt32(data, 0);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static float _vrndf1(CpuThreadState cpuThreadState) => (float) (cpuThreadState.Random.NextDouble() * 2.0f);
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static float _vrndf2(CpuThreadState cpuThreadState) => (float) (cpuThreadState.Random.NextDouble() * 4.0f);
        public static void _vpfxd_impl(CpuThreadState cpuThreadState, uint value) => cpuThreadState.PrefixDestination.Value = value;
        public static void _vpfxs_impl(CpuThreadState cpuThreadState, uint value) => cpuThreadState.PrefixSource.Value = value;
        public static void _vpfxt_impl(CpuThreadState cpuThreadState, uint value) => cpuThreadState.PrefixTarget.Value = value;

        public static uint _vi2uc_impl(int x, int y, int z, int w) => 0
                                                                      | (uint) ((x < 0) ? 0 : ((x >> 23) << 0))
                                                                      | (uint) ((y < 0) ? 0 : ((y >> 23) << 8))
                                                                      | (uint) ((z < 0) ? 0 : ((z >> 23) << 16))
                                                                      | (uint) ((w < 0) ? 0 : ((w >> 23) << 24));

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static uint _vi2c_impl(uint x, uint y, uint z, uint w) => ((x >> 24) << 0) | ((y >> 24) << 8) | ((z >> 24) << 16) | ((w >> 24) << 24) | 0;

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int _vf2iz(float value, int imm5)
        {
            var scalabValue = MathFloat.Scalb(value, imm5);
            var doubleValue = (value >= 0) ? MathFloat.Floor(scalabValue) : MathFloat.Ceil(scalabValue);
            return double.IsNaN(doubleValue) ? 0x7FFFFFFF : doubleValue;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static uint _vi2s_impl(uint v1, uint v2) => (
            ((v1 >> 16) << 0) |
            ((v2 >> 16) << 16)
        );

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static float _vh2f_0(uint a) => HalfFloat.ToFloat((int) BitUtils.Extract(a, 0, 16));

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static float _vh2f_1(uint a) => HalfFloat.ToFloat((int) BitUtils.Extract(a, 16, 16));

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static uint _vf2h_impl(float a, float b) => (uint) ((HalfFloat.FloatToHalfFloat(b) << 16) | (HalfFloat.FloatToHalfFloat(a) << 0));

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int _vi2us_impl(int x, int y) => (
            ((x < 0) ? 0 : ((x >> 15) << 0)) |
            ((y < 0) ? 0 : ((y >> 15) << 16))
        );

        public static uint _mfvc_impl(CpuThreadState cpuThreadState, VfpuControlRegistersEnum vfpuControlRegister)
        {
            Console.Error.WriteLine("Warning: _mfvc_impl");
            switch (vfpuControlRegister)
            {
                case VfpuControlRegistersEnum.VfpuPfxs: return cpuThreadState.PrefixSource.Value;
                case VfpuControlRegistersEnum.VfpuPfxt: return cpuThreadState.PrefixTarget.Value;
                case VfpuControlRegistersEnum.VfpuPfxd: return cpuThreadState.PrefixDestination.Value;
                case VfpuControlRegistersEnum.VfpuCc: return cpuThreadState.VfrCcValue;
                case VfpuControlRegistersEnum.VfpuRcx0:
                    return (uint) MathFloat.ReinterpretFloatAsInt((float) (new Random().NextDouble()));
                case VfpuControlRegistersEnum.VfpuRcx1:
                case VfpuControlRegistersEnum.VfpuRcx2:
                case VfpuControlRegistersEnum.VfpuRcx3:
                case VfpuControlRegistersEnum.VfpuRcx4:
                case VfpuControlRegistersEnum.VfpuRcx5:
                case VfpuControlRegistersEnum.VfpuRcx6:
                case VfpuControlRegistersEnum.VfpuRcx7:
                    return (uint) MathFloat.ReinterpretFloatAsInt(1.0f);
                default:
                    throw (new NotImplementedException("_mfvc_impl: " + vfpuControlRegister));
            }
        }

        public static void _mtvc_impl(CpuThreadState cpuThreadState, VfpuControlRegistersEnum vfpuControlRegister,
            uint value)
        {
            Console.Error.WriteLine("Warning: _mtvc_impl");
            switch (vfpuControlRegister)
            {
                case VfpuControlRegistersEnum.VfpuPfxs:
                    cpuThreadState.PrefixSource.Value = value;
                    return;
                case VfpuControlRegistersEnum.VfpuPfxt:
                    cpuThreadState.PrefixTarget.Value = value;
                    return;
                case VfpuControlRegistersEnum.VfpuPfxd:
                    cpuThreadState.PrefixDestination.Value = value;
                    return;
                case VfpuControlRegistersEnum.VfpuCc:
                    cpuThreadState.VfrCcValue = value;
                    return;
                case VfpuControlRegistersEnum.VfpuRcx0:
                    new Random((int) value);
                    return;
                case VfpuControlRegistersEnum.VfpuRcx1:
                case VfpuControlRegistersEnum.VfpuRcx2:
                case VfpuControlRegistersEnum.VfpuRcx3:
                case VfpuControlRegistersEnum.VfpuRcx4:
                case VfpuControlRegistersEnum.VfpuRcx5:
                case VfpuControlRegistersEnum.VfpuRcx6:
                case VfpuControlRegistersEnum.VfpuRcx7:
                    //(uint)MathFloat.ReinterpretFloatAsInt(1.0f) = Value;
                    return;
                default:
                    throw (new NotImplementedException("_mtvc_impl: " + vfpuControlRegister));
            }
        }
    }
}