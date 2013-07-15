using System;
using SafeILGenerator;
using SafeILGenerator.Ast.Nodes;
using CSharpUtils;
using SafeILGenerator.Ast;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		public AstNodeStm vcmp()
		{
			var VectorSize = Instruction.ONE_TWO;
			var Cond = Instruction.IMM4;
			bool NormalFlag = (Cond & 8) == 0;
			bool NotFlag = (Cond & 4) != 0;
			uint TypeFlag = Cond & 3;

			var Local_CC_TEMP = ast.Local(AstLocal.Create<bool>());
			var Local_CC_OR = ast.Local(AstLocal.Create<bool>());
			var Local_CC_AND = ast.Local(AstLocal.Create<bool>());
			
			return ast.Statements(
				ast.Assign(Local_CC_OR, false),
				ast.Assign(Local_CC_AND, true),
				_List((Index) =>
				{
					AstNodeExpr Expr;
					if (NormalFlag)
					{
						switch (TypeFlag)
						{
							case 0: Expr = ast.Immediate(false); break;
							case 1: Expr = ast.Binary(VEC_VS[Index], "==", VEC_VT[Index]); break;
							case 2: Expr = ast.Binary(VEC_VS[Index], "<" , VEC_VT[Index]); break;
							case 3: Expr = ast.Binary(VEC_VS[Index], "<=", VEC_VT[Index]); break;
							default: throw (new InvalidOperationException());
						}
					}
					else
					{
						switch (TypeFlag)
						{
							case 0: Expr = ast.Binary(VEC_VS[Index], "==", 0f); break;
							case 1: Expr = ast.CallStatic((Func<float, bool>)MathFloat.IsNan, VEC_VS[Index]); break;
							case 2: Expr = ast.CallStatic((Func<float, bool>)MathFloat.IsInfinity, VEC_VS[Index]); break;
							case 3: Expr = ast.CallStatic((Func<float, bool>)MathFloat.IsNanInfinity, VEC_VS[Index]); break;
							default: throw (new InvalidOperationException());
						}
					}

					if (NotFlag) Expr = ast.Unary("!", Expr);

					return ast.Statements(
						ast.Assign(Local_CC_TEMP, Expr),
						ast.AssignVCC(Index, Local_CC_TEMP),
						ast.Assign(Local_CC_OR, ast.Binary(Local_CC_OR, "||", Local_CC_TEMP)),
						ast.Assign(Local_CC_AND, ast.Binary(Local_CC_AND, "&&", Local_CC_TEMP))
					);
				}),
				ast.AssignVCC(4, Local_CC_OR),
				ast.AssignVCC(5, Local_CC_AND)
			);
		}

		static public float _vslt_impl(float a, float b) {
			if (float.IsNaN(a) || float.IsNaN(b)) return 0f;
			return (a < b) ? 1f : 0f;
		}

		static public float _vsge_impl(float a, float b)
		{
			if (float.IsNaN(a) || float.IsNaN(b)) return 0f;
			return (a >= b) ? 1f : 0f;
		}

		public AstNodeStm vslt() { return VEC_VD.SetVector(Index => ast.CallStatic((Func<float, float, float>)_vslt_impl, VEC_VS[Index], VEC_VT[Index])); }
		public AstNodeStm vsge() { return VEC_VD.SetVector(Index => ast.CallStatic((Func<float, float, float>)_vsge_impl, VEC_VS[Index], VEC_VT[Index])); }
		public AstNodeStm vscmp() { return VEC_VD.SetVector(Index => ast.CallStatic((Func<float, float>)MathFloat.Sign, VEC_VS[Index] - VEC_VT[Index])); }


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
				return ast.IfElse(
					_VCC(Register),
					VEC_VD.SetVector(Index => VEC_VS[Index]),
					ast.Statements(
						ast.Assign(ast.PrefixSourceEnabled(), false),
						ast.If(ast.PrefixDestinationEnabled(), VEC_VD.SetVector(Index => VEC_VD[Index]))
					)
				);
			}
			else if (Register == 6)
			{
				return VEC_VD.SetVector(Index => ast.Ternary(_VCC(Index), VEC_VS[Index], VEC_VD[Index]));
			}
			else
			{
				// Never copy (checked on a PSP)
				return ast.Statement();
			}
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
