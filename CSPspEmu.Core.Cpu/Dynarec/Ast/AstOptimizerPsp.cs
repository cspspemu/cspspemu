#define ENABLE_OPTIMIZE_LWL_LWR

using CSPspEmu.Core.Cpu.Emitter;
using CSPspEmu.Core.Cpu.Table;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast.Optimizers;
using System;
using System.Collections.Generic;

namespace CSPspEmu.Core.Cpu.Dynarec.Ast
{
	public class AstOptimizerPsp : AstOptimizer
	{
		private static AstGenerator ast = AstGenerator.Instance;
		public CpuProcessor Processor;

		public static AstNodeStm GlobalOptimize(CpuProcessor Processor, AstNodeStm AstNodeStm)
		{
			if (Processor.PspConfig.StoredConfig.EnableAstOptimizations)
			{
				return (AstNodeStm)(new AstOptimizerPsp(Processor)).Optimize(ast.Statements(AstNodeStm, ast.Return()));
			} else {
				return AstNodeStm;
			}
		}

		private AstOptimizerPsp(CpuProcessor Processor)
		{
			this.Processor = Processor;
		}

		public class LwlLwrState
		{
			public int LwlListIndex;
			public int LwlRtRegister;
			public int LwlRsRegister;
			public int LwlImm;
			public uint LwlPC;
		}

		private List<AstNodeStm> OptimizeLwlLwr(List<AstNodeStm> ContainerNodes)
		{
			var LwlLwrStates = new Dictionary<int, LwlLwrState>();

			//Console.WriteLine("StartList");

			for (int n = 0; n < ContainerNodes.Count; n++)
			{
				// Empty node
				if (ContainerNodes[n] == null)
				{
					continue;
				}

				// A label breaks the optimization, because we don't know if the register is being to be modified.
				if (ContainerNodes[n] is AstNodeStmLabel)
				{
					//Console.WriteLine("clear[0]");
					LwlLwrStates.Clear();
					continue;
				}

				var PspInstructionNode = ContainerNodes[n] as AstNodeStmPspInstruction;
				if (PspInstructionNode != null)
				{
					var DisassembledResult = PspInstructionNode.DisassembledResult;
					var InstructionInfo = DisassembledResult.InstructionInfo;
					var Instruction = DisassembledResult.Instruction;
					var PC = DisassembledResult.InstructionPC;

					// A branch instruction. It breaks the optimization.
					if ((InstructionInfo.InstructionType & (InstructionType.B | InstructionType.Jump | InstructionType.Syscall)) != 0)
					{
						//Console.WriteLine("clear[1]");
						LwlLwrStates.Clear();
						continue;
					}

					// lw(l/r) rt, x(rs)
					if (InstructionInfo.Name == "lwl")
					{
						LwlLwrStates[Instruction.RT] = new LwlLwrState()
						{
							LwlListIndex = n,
							LwlRtRegister = Instruction.RT,
							LwlRsRegister = Instruction.RS,
							LwlImm = Instruction.IMM,
							LwlPC = PC,
						};
						//Console.WriteLine("lwl");
						//  GPR_u(RS), IMM_s(), GPR_u(RT)
					}
					else if (InstructionInfo.Name == "lwr")
					{
						//Console.WriteLine("lwr");
						//Console.WriteLine(LwlLwrStates.Count);
						if (LwlLwrStates.ContainsKey(Instruction.RT))
						{
							var LwlLwrState = LwlLwrStates[Instruction.RT];
							if (
								(LwlLwrState.LwlRsRegister == Instruction.RS) &&
								(LwlLwrState.LwlRtRegister == Instruction.RT) &&
								(LwlLwrState.LwlImm == Instruction.IMM + 3)
							)
							{
								ContainerNodes[LwlLwrState.LwlListIndex] = null;
								ContainerNodes[n] = ast.Statements(
									ast.Comment(String.Format("{0:X8}+{1:X8} lwl+lwr", LwlLwrState.LwlPC, PC)),
									CpuEmitter.AssignGPR(
										Instruction.RT,
										CpuEmitter.AstMemoryGetValue<int>(
											Processor.Memory,
											ast.Cast<uint>(ast.Binary(CpuEmitter.GPR_s(Instruction.RS), "+", Instruction.IMM))
										)
									)
								);
								//Console.WriteLine("Valid match!");
							}
						}
					}
				}
			}
			return ContainerNodes;
		}

		protected override AstNode _Optimize(AstNodeStmContainer _Container)
		{
			var Node = base._Optimize(_Container);
			if (Node is AstNodeStmContainer)
			{
				var Container = Node as AstNodeStmContainer;
#if ENABLE_OPTIMIZE_LWL_LWR
				Container.Nodes = OptimizeLwlLwr(Container.Nodes);
#endif
				return base._Optimize(Container);
			}
			return Node;
		}
	}
}
