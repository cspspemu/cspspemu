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
                    InstructionTable.ALL, throwOnUnexistent: false
                ),
                new DynarecBranchAnalyzer()
            );

        public Instruction Instruction;

        [Flags]
        public enum JumpFlags
        {
            NormalInstruction = (1 << 0),
            BranchOrJumpInstruction = (1 << 1),
            SyscallInstruction = (1 << 2),
            JumpInstruction = (1 << 3),
            FpuInstruction = (1 << 4),
            VFpuInstruction = (1 << 5),

            JumpAlways = (1 << 10),
            Likely = (1 << 11),
            AndLink = (1 << 12),

            FixedJump = 0,
            DynamicJump = (1 << 20),
        }

        public JumpFlags bvf() => JumpFlags.BranchOrJumpInstruction | JumpFlags.VFpuInstruction;
        public JumpFlags bvt() => JumpFlags.BranchOrJumpInstruction | JumpFlags.VFpuInstruction;
        public JumpFlags bvfl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.VFpuInstruction | JumpFlags.Likely;
        public JumpFlags bvtl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.VFpuInstruction | JumpFlags.Likely;

        public JumpFlags beq() => JumpFlags.BranchOrJumpInstruction |
                                  ((Instruction.RS == Instruction.RT) ? JumpFlags.JumpAlways : 0);

        public JumpFlags bne() => JumpFlags.BranchOrJumpInstruction;
        public JumpFlags beql() => JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely;
        public JumpFlags bnel() => JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely;
        public JumpFlags bltz() => JumpFlags.BranchOrJumpInstruction;
        public JumpFlags bltzal() => JumpFlags.BranchOrJumpInstruction | JumpFlags.AndLink;
        public JumpFlags bltzl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely;
        public JumpFlags bltzall() => JumpFlags.BranchOrJumpInstruction | JumpFlags.AndLink | JumpFlags.Likely;
        public JumpFlags blez() => JumpFlags.BranchOrJumpInstruction;
        public JumpFlags blezl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely;
        public JumpFlags bgtz() => JumpFlags.BranchOrJumpInstruction;
        public JumpFlags bgez() => JumpFlags.BranchOrJumpInstruction;
        public JumpFlags bgtzl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely;
        public JumpFlags bgezl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely;
        public JumpFlags bgezal() => JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction | JumpFlags.AndLink;

        public JumpFlags bgezall() => JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction |
                                      JumpFlags.AndLink | JumpFlags.Likely;

        public JumpFlags j() => JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction | JumpFlags.JumpAlways;

        public JumpFlags jr() => JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction | JumpFlags.JumpAlways |
                                 JumpFlags.DynamicJump;

        public JumpFlags jalr() => JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction |
                                   JumpFlags.JumpAlways |
                                   JumpFlags.AndLink | JumpFlags.DynamicJump;

        public JumpFlags jal() => JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction | JumpFlags.JumpAlways |
                                  JumpFlags.AndLink;

        public JumpFlags bc1f() => JumpFlags.BranchOrJumpInstruction | JumpFlags.FpuInstruction;
        public JumpFlags bc1t() => JumpFlags.BranchOrJumpInstruction | JumpFlags.FpuInstruction;
        public JumpFlags bc1fl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.FpuInstruction | JumpFlags.Likely;
        public JumpFlags bc1tl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.FpuInstruction | JumpFlags.Likely;
        public JumpFlags syscall() => JumpFlags.SyscallInstruction;
        public JumpFlags unhandled() => JumpFlags.NormalInstruction;
        public JumpFlags unknown() => JumpFlags.NormalInstruction;
    }
}