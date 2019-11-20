using System;
using CSPspEmu.Core.Cpu.Switch;
using CSPspEmu.Core.Cpu.Table;

// ReSharper disable InconsistentNaming

namespace CSPspEmu.Core.Cpu.Dynarec
{
    public sealed class DynarecBranchAnalyzer
    {
        public static readonly Func<Instruction, JumpFlags> GetBranchInfo = instruction =>
            GetBranchInfoField(instruction.Value);

        public static readonly Func<uint, JumpFlags> GetBranchInfoField =
            EmitLookupGenerator.GenerateInfoDelegate(
                EmitLookupGenerator.GenerateSwitchDelegateReturn<DynarecBranchAnalyzer, JumpFlags>(
                    "_GetBranchInfo",
                    InstructionTable.All, throwOnUnexistent: false, warnUnmapped: false
                ),
                new DynarecBranchAnalyzer()
            );

        public Instruction Instruction;

        [Flags]
        public enum JumpFlags
        {
            NormalInstruction = 1 << 0,
            BranchOrJumpInstruction = 1 << 1,
            SyscallInstruction = 1 << 2,
            JumpInstruction = 1 << 3,
            FpuInstruction = 1 << 4,
            VFpuInstruction = 1 << 5,

            JumpAlways = 1 << 10,
            Likely = 1 << 11,
            AndLink = 1 << 12,

            FixedJump = 0,
            DynamicJump = 1 << 20,
        }

        [InstructionName(InstructionNames.Bvf)]
        public JumpFlags bvf() => JumpFlags.BranchOrJumpInstruction | JumpFlags.VFpuInstruction;

        [InstructionName(InstructionNames.Bvt)]
        public JumpFlags bvt() => JumpFlags.BranchOrJumpInstruction | JumpFlags.VFpuInstruction;

        [InstructionName(InstructionNames.Bvfl)]
        public JumpFlags bvfl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.VFpuInstruction | JumpFlags.Likely;

        [InstructionName(InstructionNames.Bvtl)]
        public JumpFlags bvtl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.VFpuInstruction | JumpFlags.Likely;

        [InstructionName(InstructionNames.Beq)]
        public JumpFlags beq() => JumpFlags.BranchOrJumpInstruction |
                                  (Instruction.Rs == Instruction.Rt ? JumpFlags.JumpAlways : 0);

        [InstructionName(InstructionNames.Bne)]
        public JumpFlags bne() => JumpFlags.BranchOrJumpInstruction;

        [InstructionName(InstructionNames.Beql)]
        public JumpFlags beql() => JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely;

        [InstructionName(InstructionNames.Bnel)]
        public JumpFlags bnel() => JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely;

        [InstructionName(InstructionNames.Bltz)]
        public JumpFlags bltz() => JumpFlags.BranchOrJumpInstruction;

        [InstructionName(InstructionNames.Bltzal)]
        public JumpFlags bltzal() => JumpFlags.BranchOrJumpInstruction | JumpFlags.AndLink;

        [InstructionName(InstructionNames.Bltzl)]
        public JumpFlags bltzl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely;

        [InstructionName(InstructionNames.Bltzall)]
        public JumpFlags bltzall() => JumpFlags.BranchOrJumpInstruction | JumpFlags.AndLink | JumpFlags.Likely;

        [InstructionName(InstructionNames.Blez)]
        public JumpFlags blez() => JumpFlags.BranchOrJumpInstruction;

        [InstructionName(InstructionNames.Blezl)]
        public JumpFlags blezl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely;

        [InstructionName(InstructionNames.Bgtz)]
        public JumpFlags bgtz() => JumpFlags.BranchOrJumpInstruction;

        [InstructionName(InstructionNames.Bgez)]
        public JumpFlags bgez() => JumpFlags.BranchOrJumpInstruction;

        [InstructionName(InstructionNames.Bgtzl)]
        public JumpFlags bgtzl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely;

        [InstructionName(InstructionNames.Bgezl)]
        public JumpFlags bgezl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely;

        [InstructionName(InstructionNames.Bgezal)]
        public JumpFlags bgezal() => JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction | JumpFlags.AndLink;


        [InstructionName(InstructionNames.Bgezall)]
        public JumpFlags bgezall() => JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction |
                                      JumpFlags.AndLink | JumpFlags.Likely;


        [InstructionName(InstructionNames.J)]
        public JumpFlags j() => JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction | JumpFlags.JumpAlways;

        [InstructionName(InstructionNames.Jr)]
        public JumpFlags jr() => JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction | JumpFlags.JumpAlways |
                                 JumpFlags.DynamicJump;

        [InstructionName(InstructionNames.Jalr)]
        public JumpFlags jalr() => JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction |
                                   JumpFlags.JumpAlways |
                                   JumpFlags.AndLink | JumpFlags.DynamicJump;

        [InstructionName(InstructionNames.Jal)]
        public JumpFlags jal() => JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction | JumpFlags.JumpAlways |
                                  JumpFlags.AndLink;

        [InstructionName(InstructionNames.Bc1F)]
        public JumpFlags bc1f() => JumpFlags.BranchOrJumpInstruction | JumpFlags.FpuInstruction;

        [InstructionName(InstructionNames.Bc1T)]
        public JumpFlags bc1t() => JumpFlags.BranchOrJumpInstruction | JumpFlags.FpuInstruction;

        [InstructionName(InstructionNames.Bc1Fl)]
        public JumpFlags bc1fl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.FpuInstruction | JumpFlags.Likely;

        [InstructionName(InstructionNames.Bc1Tl)]
        public JumpFlags bc1tl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.FpuInstruction | JumpFlags.Likely;

        [InstructionName(InstructionNames.Syscall)]
        public JumpFlags syscall() => JumpFlags.SyscallInstruction;

        [InstructionName(InstructionNames.Unhandled)]
        public JumpFlags unhandled() => JumpFlags.NormalInstruction;

        [InstructionName(InstructionNames.Unknown)]
        public JumpFlags unknown() => JumpFlags.NormalInstruction;
    }
}