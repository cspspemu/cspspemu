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
       
        [InstructionName("bvf")]
        public JumpFlags bvf() => JumpFlags.BranchOrJumpInstruction | JumpFlags.VFpuInstruction;

        [InstructionName("bvt")]
        public JumpFlags bvt() => JumpFlags.BranchOrJumpInstruction | JumpFlags.VFpuInstruction;

        [InstructionName("bvfl")]
        public JumpFlags bvfl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.VFpuInstruction | JumpFlags.Likely;

        [InstructionName("bvtl")]
        public JumpFlags bvtl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.VFpuInstruction | JumpFlags.Likely;

        [InstructionName("beq")]
        public JumpFlags beq() => JumpFlags.BranchOrJumpInstruction |
                                  ((Instruction.Rs == Instruction.Rt) ? JumpFlags.JumpAlways : 0);

        [InstructionName("bne")]
        public JumpFlags bne() => JumpFlags.BranchOrJumpInstruction;

        [InstructionName("beql")]
        public JumpFlags beql() => JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely;

        [InstructionName("bnel")]
        public JumpFlags bnel() => JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely;

        [InstructionName("bltz")]
        public JumpFlags bltz() => JumpFlags.BranchOrJumpInstruction;

        [InstructionName("bltzal")]
        public JumpFlags bltzal() => JumpFlags.BranchOrJumpInstruction | JumpFlags.AndLink;

        [InstructionName("bltzl")]
        public JumpFlags bltzl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely;

        [InstructionName("bltzall")]
        public JumpFlags bltzall() => JumpFlags.BranchOrJumpInstruction | JumpFlags.AndLink | JumpFlags.Likely;

        [InstructionName("blez")]
        public JumpFlags blez() => JumpFlags.BranchOrJumpInstruction;

        [InstructionName("blezl")]
        public JumpFlags blezl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely;

        [InstructionName("bgtz")]
        public JumpFlags bgtz() => JumpFlags.BranchOrJumpInstruction;

        [InstructionName("bgez")]
        public JumpFlags bgez() => JumpFlags.BranchOrJumpInstruction;

        [InstructionName("bgtzl")]
        public JumpFlags bgtzl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely;

        [InstructionName("bgezl")]
        public JumpFlags bgezl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely;

        [InstructionName("bgezal")]
        public JumpFlags bgezal() => JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction | JumpFlags.AndLink;


        [InstructionName("bgezall")]
        public JumpFlags bgezall() => JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction |
                                      JumpFlags.AndLink | JumpFlags.Likely;


        [InstructionName("j")]
        public JumpFlags j() => JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction | JumpFlags.JumpAlways;

        [InstructionName("jr")]
        public JumpFlags jr() => JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction | JumpFlags.JumpAlways |
                                 JumpFlags.DynamicJump;

        [InstructionName("jalr")]
        public JumpFlags jalr() => JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction |
                                   JumpFlags.JumpAlways |
                                   JumpFlags.AndLink | JumpFlags.DynamicJump;

        [InstructionName("jal")]
        public JumpFlags jal() => JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction | JumpFlags.JumpAlways |
                                  JumpFlags.AndLink;

        [InstructionName("bc1f")]
        public JumpFlags bc1f() => JumpFlags.BranchOrJumpInstruction | JumpFlags.FpuInstruction;

        [InstructionName("bc1t")]
        public JumpFlags bc1t() => JumpFlags.BranchOrJumpInstruction | JumpFlags.FpuInstruction;

        [InstructionName("bc1fl")]
        public JumpFlags bc1fl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.FpuInstruction | JumpFlags.Likely;

        [InstructionName("bc1tl")]
        public JumpFlags bc1tl() => JumpFlags.BranchOrJumpInstruction | JumpFlags.FpuInstruction | JumpFlags.Likely;

        [InstructionName("syscall")]
        public JumpFlags syscall() => JumpFlags.SyscallInstruction;

        [InstructionName("unhandled")]
        public JumpFlags unhandled() => JumpFlags.NormalInstruction;

        [InstructionName("unknown")]
        public JumpFlags unknown() => JumpFlags.NormalInstruction;
    }
}