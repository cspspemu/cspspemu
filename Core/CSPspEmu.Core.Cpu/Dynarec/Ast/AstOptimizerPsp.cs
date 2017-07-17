#define ENABLE_OPTIMIZE_LWL_LWR

using CSPspEmu.Core.Cpu.Emitter;
using CSPspEmu.Core.Cpu.Table;
using CSPspEmu.Core.Memory;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast.Optimizers;
using System.Collections.Generic;

namespace CSPspEmu.Core.Cpu.Dynarec.Ast
{
    public class AstOptimizerPsp : AstOptimizer
    {
        private static AstMipsGenerator ast = AstMipsGenerator.Instance;
        public PspMemory Memory;

        public static AstNodeStm GlobalOptimize(CpuProcessor processor, AstNodeStm astNodeStm)
        {
            if (processor == null || processor.CpuConfig.EnableAstOptimizations)
            {
                return (AstNodeStm) (new AstOptimizerPsp(processor != null ? processor.Memory : null)).Optimize(
                    ast.Statements(astNodeStm, ast.Return()));
            }
            return astNodeStm;
        }

        private AstOptimizerPsp(PspMemory memory)
        {
            Memory = memory;
        }

        public class LwlLwrState
        {
            public int LwlListIndex;
            public int LwlRtRegister;
            public int LwlRsRegister;
            public int LwlImm;
            public uint LwlPc;
        }

        private List<AstNodeStm> OptimizeLwlLwr(List<AstNodeStm> containerNodes)
        {
            var lwlLwrStates = new Dictionary<int, LwlLwrState>();

            //Console.WriteLine("StartList");

            for (var n = 0; n < containerNodes.Count; n++)
            {
                // Empty node
                if (containerNodes[n] == null)
                    continue;

                // A label breaks the optimization, because we don't know if the register is being to be modified.
                if (containerNodes[n] is AstNodeStmLabel)
                {
                    //Console.WriteLine("clear[0]");
                    lwlLwrStates.Clear();
                    continue;
                }

                var pspInstructionNode = containerNodes[n] as AstNodeStmPspInstruction;
                if (pspInstructionNode == null) continue;
                var disassembledResult = pspInstructionNode.DisassembledResult;
                var instructionInfo = disassembledResult.InstructionInfo;
                var instruction = disassembledResult.Instruction;
                var pc = disassembledResult.InstructionPc;

                // A branch instruction. It breaks the optimization.
                if ((instructionInfo.InstructionType &
                     (InstructionType.B | InstructionType.Jump | InstructionType.Syscall)) != 0)
                {
                    //Console.WriteLine("clear[1]");
                    lwlLwrStates.Clear();
                    continue;
                }

                // lw(l/r) rt, x(rs)
                switch (instructionInfo.Name)
                {
                    case "lwl":
                        lwlLwrStates[instruction.RT] = new LwlLwrState()
                        {
                            LwlListIndex = n,
                            LwlRtRegister = instruction.RT,
                            LwlRsRegister = instruction.RS,
                            LwlImm = instruction.IMM,
                            LwlPc = pc,
                        };
                        //Console.WriteLine("lwl");
                        //  GPR_u(RS), IMM_s(), GPR_u(RT)
                        break;
                    case "lwr":
                        //Console.WriteLine("lwr");
                        //Console.WriteLine(LwlLwrStates.Count);
                        if (lwlLwrStates.ContainsKey(instruction.RT))
                        {
                            var lwlLwrState = lwlLwrStates[instruction.RT];
                            if (
                                (lwlLwrState.LwlRsRegister == instruction.RS) &&
                                (lwlLwrState.LwlRtRegister == instruction.RT) &&
                                (lwlLwrState.LwlImm == instruction.IMM + 3)
                            )
                            {
                                containerNodes[lwlLwrState.LwlListIndex] = null;
                                containerNodes[n] = ast.Statements(
                                    ast.Comment($"{lwlLwrState.LwlPc:X8}+{pc:X8} lwl+lwr"),
                                    ast.AssignGpr(
                                        instruction.RT,
                                        ast.MemoryGetValue<int>(
                                            Memory,
                                            ast.Cast<uint>(ast.Binary(ast.GPR_s(instruction.RS), "+", instruction.IMM))
                                        )
                                    )
                                );
                                //Console.WriteLine("Valid match!");
                            }
                        }
                        break;
                }
            }
            return containerNodes;
        }

        protected AstNode _Optimize(AstNodeStmPspInstruction pspInstruction) => pspInstruction;

        protected override AstNode _Optimize(AstNodeStmContainer container)
        {
            var node = base._Optimize(container);
            if (!(node is AstNodeStmContainer)) return node;
            var container2 = node as AstNodeStmContainer;
#if ENABLE_OPTIMIZE_LWL_LWR
            container2.Nodes = OptimizeLwlLwr(container2.Nodes);
#endif
            //foreach (var _Node in Container.Nodes)
            //{
            //	if (_Node is AstNodeStmPspInstruction)
            //	{
            //		var PspNode = (_Node as AstNodeStmPspInstruction);
            //		if (PspNode.DisassembledResult.InstructionInfo.InstructionType == InstructionType.Psp)
            //		{
            //
            //			Console.WriteLine(PspNode.DisassembledResult);
            //		}
            //	}
            //}
            return base._Optimize(container2);
        }
    }
}