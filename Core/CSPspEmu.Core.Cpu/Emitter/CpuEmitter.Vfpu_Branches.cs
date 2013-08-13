using System;
using SafeILGenerator;
using SafeILGenerator.Ast.Nodes;
using CSharpUtils;
using SafeILGenerator.Ast;
using System.Collections.Generic;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		public enum ConditionEnum
		{
			VC_FL = 0,
			VC_EQ = 1,
			VC_LT = 2,
			VC_LE = 3,
			VC_TR = 4,
			VC_NE = 5,
			VC_GE = 6,
			VC_GT = 7,
			VC_EZ = 8,
			VC_EN = 9,
			VC_EI = 10,
			VC_ES = 11,
			VC_NZ = 12,
			VC_NN = 13,
			VC_NI = 14,
			VC_NS = 15,
		}

		public AstNodeStm vcmp()
		{
			var VectorSize = Instruction.ONE_TWO;
			var Cond = Instruction.IMM4;
			var Cond2 = (ConditionEnum)Cond;
			//bool NormalFlag = (Cond & 8) == 0;
			//bool NotFlag = (Cond & 4) != 0;
			//uint TypeFlag = (Cond & 3);

			var Local_CC_TEMP = ast.Local(AstLocal.Create<bool>());
			var Local_CC_OR = ast.Local(AstLocal.Create<bool>());
			var Local_CC_AND = ast.Local(AstLocal.Create<bool>());
			
			return ast.Statements(
				ast.Assign(Local_CC_OR, false),
				ast.Assign(Local_CC_AND, true),
				
				// TODO: CHECK THIS!
				ast.AssignVCC(0, true),
				ast.AssignVCC(1, true),
				ast.AssignVCC(2, true),
				ast.AssignVCC(3, true),

				_List(VectorSize, (Index) =>
				{
					AstNodeExpr Expr;
					//bool UsedForAggregate;

					var Left = VEC_VS[Index];
					var Right = VEC_VT[Index];
					switch (Cond2)
					{
						case ConditionEnum.VC_FL: Expr = ast.Immediate(false); break;
						case ConditionEnum.VC_EQ: Expr = ast.Binary(Left, "==", Right); break;
						case ConditionEnum.VC_LT: Expr = ast.Binary(Left, "<", Right); break;
						//case ConditionEnum.VC_LE: Expr = ast.Binary(Left, "<=", Right); break;
						case ConditionEnum.VC_LE: Expr = ast.CallStatic((Func<float, float, bool>)MathFloat.IsLessOrEqualsThan, Left, Right); break;

						case ConditionEnum.VC_TR: Expr = ast.Immediate(true); break;
						case ConditionEnum.VC_NE: Expr = ast.Binary(Left, "!=", Right); break;
						//case ConditionEnum.VC_GE: Expr = ast.Binary(Left, ">=", Right); break;
						case ConditionEnum.VC_GE: Expr = ast.CallStatic((Func<float, float, bool>)MathFloat.IsGreatOrEqualsThan, Left, Right); break;
						case ConditionEnum.VC_GT: Expr = ast.Binary(Left, ">", Right); break;

						case ConditionEnum.VC_EZ: Expr = ast.Binary(ast.Binary(Left, "==", 0.0f), "||", ast.Binary(Left, "==", -0.0f)); break;
						case ConditionEnum.VC_EN: Expr = ast.CallStatic((Func<float, bool>)MathFloat.IsNan, Left); break;
						case ConditionEnum.VC_EI: Expr = ast.CallStatic((Func<float, bool>)MathFloat.IsInfinity, Left); break;
						case ConditionEnum.VC_ES: Expr = ast.CallStatic((Func<float, bool>)MathFloat.IsNanOrInfinity, Left); break;   // Tekken Dark Resurrection

						case ConditionEnum.VC_NZ: Expr = ast.Binary(Left, "!=", 0f); break;
						case ConditionEnum.VC_NN: Expr = ast.Unary("!", ast.CallStatic((Func<float, bool>)MathFloat.IsNan, Left)); break;
						case ConditionEnum.VC_NI: Expr = ast.Unary("!", ast.CallStatic((Func<float, bool>)MathFloat.IsInfinity, Left)); break;
						case ConditionEnum.VC_NS: Expr = ast.Unary("!", ast.CallStatic((Func<float, bool>)MathFloat.IsNanOrInfinity, Left)); break;

						default: throw (new InvalidOperationException());
					}

					var Statements = new List<AstNodeStm>();
					Statements.Add(ast.Assign(Local_CC_TEMP, Expr));
					Statements.Add(ast.AssignVCC(Index, Local_CC_TEMP));
					Statements.Add(ast.Assign(Local_CC_OR, ast.Binary(Local_CC_OR, "||", Local_CC_TEMP)));
					Statements.Add(ast.Assign(Local_CC_AND, ast.Binary(Local_CC_AND, "&&", Local_CC_TEMP)));

					return ast.Statements(Statements);
				}),
				ast.AssignVCC(4, Local_CC_OR),
				ast.AssignVCC(5, Local_CC_AND)
			);
		}

		public AstNodeStm vslt() { return VEC_VD.SetVector(Index => ast.CallStatic((Func<float, float, float>)CpuEmitterUtils._vslt_impl, VEC_VS[Index], VEC_VT[Index]), PC); }
		public AstNodeStm vsge() { return VEC_VD.SetVector(Index => ast.CallStatic((Func<float, float, float>)CpuEmitterUtils._vsge_impl, VEC_VS[Index], VEC_VT[Index]), PC); }
		public AstNodeStm vscmp() { return VEC_VD.SetVector(Index => ast.CallStatic((Func<float, float, float>)MathFloat.Sign2, VEC_VS[Index], VEC_VT[Index]), PC); }

		public AstNodeStm _vcmovtf(bool True)
		{
			var Register = (int)Instruction.IMM3;

			Func<int, AstNodeExpr> _VCC = (Index) =>
			{
				AstNodeExpr Ret = ast.VCC(Index);
				if (!True) Ret = ast.Unary("!", Ret);
				return Ret;
			};

			if (Register < 6)
			{
				// TODO: CHECK THIS!
				return ast.IfElse(
					_VCC(Register),
					VEC_VD.SetVector(Index => VEC_VS[Index], PC),
					ast.Statements(
						ast.Assign(ast.PrefixSourceEnabled(), false),
						//ast.If(ast.PrefixDestinationEnabled(), VEC_VD.SetVector(Index => VEC_VD[Index], PC))
						ast.If(ast.PrefixDestinationEnabled(), VEC_VD.SetVector(Index => VEC_VD[Index], PC))
					)
				);
			}

			if (Register == 6)
			{
				return VEC_VD.SetVector(Index => ast.Ternary(_VCC(Index), VEC_VS[Index], VEC_VD[Index]), PC);
			}

			// Register == 7

			// Never copy (checked on a PSP)
			return ast.Statement();
		}

		public AstNodeStm vcmovf() { return _vcmovtf(false); }
		public AstNodeStm vcmovt() { return _vcmovtf(true); }

		private AstNodeStm _bvtf(bool True)
		{
			var Register = (int)Instruction.IMM3;
			AstNodeExpr BranchExpr = ast.VCC(Register);
			if (!True) BranchExpr = ast.Unary("!", BranchExpr);
			return AssignBranchFlag(BranchExpr);
		}

		public AstNodeStm bvf() { return _bvtf(false); }
		public AstNodeStm bvfl() { return bvf(); }
		public AstNodeStm bvt() { return _bvtf(true); }
		public AstNodeStm bvtl() { return bvt(); }
	}
}
