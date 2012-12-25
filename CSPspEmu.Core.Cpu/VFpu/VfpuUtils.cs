using System;
using System.Collections.Generic;
using System.Linq;

namespace CSPspEmu.Core.Cpu.VFpu
{
	public class VfpuUtils
	{
		public enum RegisterType
		{
			Cell, Vector, Matrix
		}

		public struct VfpuRegisterInfo
		{
			public int VfpuSize;
			public char Type;
			public int Matrix;
			public int Column;
			public int Row;
			public RegisterType RegisterType
			{
				get
				{
					switch (Type)
					{
						case 'S': return VfpuUtils.RegisterType.Cell;
						case 'R': case 'C': return VfpuUtils.RegisterType.Vector;
						case 'M': return VfpuUtils.RegisterType.Matrix;
						default: throw(new NotImplementedException());
					}
				}
			}

			public static VfpuRegisterInfo Parse(int VfpuSize, string Name)
			{
				Name = Name.ToUpperInvariant();
				return new VfpuRegisterInfo()
				{
					VfpuSize = VfpuSize,
					Type = Name[0],
					Matrix = int.Parse(Name.Substr(1, 1)),
					Column = int.Parse(Name.Substr(2, 1)),
					Row = int.Parse(Name.Substr(3, 1))
				};
			}

			public int Index
			{
				get
				{
					switch (Type)
					{
						case 'R':
						case 'C':
							{
								//return (int)VfpuUtils.GetCellIndex(Register.Matrix, Register.Row, Register.Column);
								int Line = (Type == 'R') ? Row : Column;
								int Offset = (Type == 'R') ? Column : Row;
								int RowColumn = (Type == 'R') ? 1 : 0;

								if (VfpuSize == 2) Offset /= 2;

								return new VfpuRegisterInt()
								{
									RC_LINE = Line,
									RC_MATRIX = Matrix,
									RC_ROW_COLUMN = RowColumn,
									RC_OFFSET = Offset,
								};
							}
						case 'M':
						case 'E':
							{
								int _Row = (Type == 'M') ? Row : Column;
								int _Column = (Type == 'M') ? Column : Row;
								int Transposed = (Type == 'M') ? 0 : 1;

								if (VfpuSize == 2) { Row /= 2; Column /= 2; }

								return new VfpuRegisterInt()
								{
									M_MATRIX = Matrix,
									M_ROW = _Row,
									M_COLUMN = _Column,
									M_TRANSPOSED = Transposed,
								};
							}
						case 'S':
							return new VfpuRegisterInt()
							{
								S_COLUMN = Column,
								S_ROW = Row,
								S_MATRIX = Matrix,
							};
						default:
							throw (new NotImplementedException(String.Format("Not implemented VfpuUtils.ParsedRegister.Index Type:'{0}'", Type)));

					}
				}
			}
		}


		/// <summary>
		///  vcst.[s | p | t | q] vd, VFPU_CST
		///  vd = vfpu_constant[VFPU_CST], where VFPU_CST is one of:
		///    VFPU_HUGE      infinity
		///    VFPU_SQRT2     sqrt(2)
		///    VFPU_SQRT1_2   sqrt(1/2)
		///    VFPU_2_SQRTPI  2/sqrt(pi)
		///    VFPU_PI        pi
		///    VFPU_2_PI      2/pi
		///    VFPU_1_PI      1/pi
		///    VFPU_PI_4      pi/4
		///    VFPU_PI_2      pi/2
		///    VFPU_E         e
		///    VFPU_LOG2E     log2(e)
		///    VFPU_LOG10E    log10(e)
		///    VFPU_LN2       ln(2)
		///    VFPU_LN10      ln(10)
		///    VFPU_2PI       2*pi
		///    VFPU_PI_6      pi/6
		///    VFPU_LOG10TWO  log10(2)
		///    VFPU_LOG2TEN   log2(10)
		///    VFPU_SQRT3_2   sqrt(3)/2
		/// </summary>
		public static readonly float[] VfpuConstantsValues = new float[] {
			(float)0.0f,                        // VFPU_ZERO     - 0
			(float)float.PositiveInfinity,      // VFPU_HUGE     - infinity
			(float)(Math.Sqrt(2.0)),            // VFPU_SQRT2    - sqrt(2)
			(float)(Math.Sqrt(1.0 / 2.0)),      // VFPU_SQRT1_2  - sqrt(1 / 2)
			(float)(2.0 / Math.Sqrt(Math.PI)),  // VFPU_2_SQRTPI - 2 / sqrt(pi)
			(float)(2.0 / Math.PI),             // VFPU_2_PI     - 2 / pi
			(float)(1.0 / Math.PI),             // VFPU_1_PI     - 1 / pi
			(float)(Math.PI / 4.0),             // VFPU_PI_4     - pi / 4
			(float)(Math.PI / 2.0),             // VFPU_PI_2     - pi / 2
			(float)(Math.PI),                   // VFPU_PI       - pi
			(float)(Math.E),                    // VFPU_E        - e
			(float)(Math.Log(Math.E, 2)),       // VFPU_LOG2E    - log2(E) = log(E) / log(2)
			(float)(Math.Log10(Math.E)),        // VFPU_LOG10E   - log10(E)
			(float)(Math.Log(2)),               // VFPU_LN2      - ln(2)
			(float)(Math.Log(10)),              // VFPU_LN10     - ln(10)
			(float)(2.0 * Math.PI),             // VFPU_2PI      - 2 * pi
			(float)(Math.PI / 6.0),             // VFPU_PI_6     - pi / 6
			(float)(Math.Log10(2.0)),           // VFPU_LOG10TWO - log10(2)
			(float)(Math.Log(10.0, 2)),         // VFPU_LOG2TEN  - log2(10) = log(10) / log(2)
			(float)(Math.Sqrt(3.0) / 2.0)       // VFPU_SQRT3_2  - sqrt(3) / 2
		};

		public static string[] ConstantNames
		{
			get
			{
				return VfpuConstantsIndices.Keys.ToArray();
			}
		}

		public static float GetVfpuConstantsValue(int Index)
		{
			if (Index < 0 || Index >= VfpuConstantsValues.Length) return 0f;
			return VfpuConstantsValues[Index];
		}

		public static float GetConstantValueByName(string Name)
		{
			return VfpuConstantsValues[VfpuConstantsIndices[Name]];
		}

		public static readonly Dictionary<string, int> VfpuConstantsIndices = new Dictionary<string, int>() {
			{ "VFPU_ZERO", 0 },
			{ "VFPU_HUGE", 1 },
			{ "VFPU_SQRT2", 2 },
			{ "VFPU_SQRT1_2", 3 },
			{ "VFPU_2_SQRTPI", 4 },
			{ "VFPU_2_PI", 5 },
			{ "VFPU_1_PI", 6 },
			{ "VFPU_PI_4", 7 },
			{ "VFPU_PI_2", 8 },
			{ "VFPU_PI", 9 },
			{ "VFPU_E", 10 },
			{ "VFPU_LOG2E", 11 },
			{ "VFPU_LOG10E", 12 },
			{ "VFPU_LN2", 13 },
			{ "VFPU_LN10", 14 },
			{ "VFPU_2PI", 15 },
			{ "VFPU_PI_6", 16 },
			{ "VFPU_LOG10TWO", 17 },
			{ "VFPU_LOG2TEN", 18 },
			{ "VFPU_SQRT3_2", 19 },
		};

		public static int GetIndexCell(int Matrix, int Column, int Row)
		{
			return Matrix * 16 + Column * 4 + Row;
		}

		public static int[] GetIndices(int Size, RegisterType RegisterType, VfpuRegisterInt Register, string Name = null)
		{
			return _GetIndices(Size, RegisterType, Register, Name).ToArray();
		}

		public static int GetIndexCell(VfpuRegisterInt Register, string Name = null)
		{
			return GetIndexCell(Register.S_MATRIX, Register.S_COLUMN, Register.S_ROW);
		}

		public static int[] GetIndicesVector(int Size, VfpuRegisterInt Register, string Name = null)
		{
			var IndicesVector = new int[Size];
			int OffsetMultiplier = (Size == 2) ? 2 : 1;
			for (int Index = 0; Index < Size; Index++)
			{
				if (Register.RC_ROW_COLUMN == 1)
				{
					IndicesVector[Index] = (int)GetIndexCell(Register.RC_MATRIX, Register.RC_OFFSET * OffsetMultiplier + Index, Register.RC_LINE);
				}
				else
				{
					IndicesVector[Index] = (int)GetIndexCell(Register.RC_MATRIX, Register.RC_LINE, Register.RC_OFFSET * OffsetMultiplier + Index);
				}
			}
			return IndicesVector;
		}

		public static int[,] GetIndicesMatrix(int Size, VfpuRegisterInt Register, string Name = null)
		{
			var IndicesMatrix = new int[Size, Size];
			int OffsetMultiplier = (Size == 2) ? 2 : 1;

			for (int Row = 0; Row < Size; Row++)
			{
				for (int Column = 0; Column < Size; Column++)
				{
					if (Register.M_TRANSPOSED != 0)
					{
						IndicesMatrix[Column, Row] = (int)GetIndexCell(Register.M_MATRIX, Register.M_ROW * OffsetMultiplier + Row, Register.M_COLUMN * OffsetMultiplier + Column);
					}
					else
					{
						IndicesMatrix[Column, Row] = (int)GetIndexCell(Register.M_MATRIX, Register.M_COLUMN * OffsetMultiplier + Column, Register.M_ROW * OffsetMultiplier + Row);
					}
				}
			}

			return IndicesMatrix;
		}

		private static IEnumerable<int> _GetIndices(int Size, RegisterType RegisterType, VfpuRegisterInt Register, string Name = null)
		{
			if (Size < 1 || Size > 4) throw (new Exception(String.Format("Invalid Size {0} !€ [0, 4]", Size)));

			if (RegisterType == VfpuUtils.RegisterType.Vector && Size == 1) RegisterType = VfpuUtils.RegisterType.Cell;

			switch (RegisterType)
			{
				case RegisterType.Cell:
					{
						yield return (int)GetIndexCell(Register, Name);
						yield break;
					}
				case RegisterType.Vector:
					{
						var Vector = GetIndicesVector(Size, Register, Name);
						for (int Index = 0; Index < Size; Index++)
						{
							yield return (int)Vector[Index];
						}
						yield break;
					}
				case RegisterType.Matrix:
					{
						var Matrix = GetIndicesMatrix(Size, Register, Name);
						for (int Row = 0; Row < Size; Row++)
						{
							for (int Column = 0; Column < Size; Column++)
							{
								yield return Matrix[Column, Row];
							}
						}
						yield break;
					}
				default: throw (new NotImplementedException(String.Format("Invalid vfpu registry name {0}('{1}')", Register, Name)));
			}
		}

		public static int[] GetIndices(string NameWithSufix)
		{
			int Size = 0;
			if (NameWithSufix.EndsWith(".q")) Size = 4;
			else if (NameWithSufix.EndsWith(".t")) Size = 3;
			else if (NameWithSufix.EndsWith(".p")) Size = 2;
			else if (NameWithSufix.EndsWith(".s")) Size = 1;
			if (Size == 0) throw(new Exception("Register doesn't have sufix"));
			return GetIndices(Size, NameWithSufix.Substr(0, -2));
		}

		public static int[] GetIndices(int Size, string Name)
		{
			var Register = VfpuRegisterInfo.Parse(Size, Name);
			if ((Register.Type == 'S') && (Size != 1)) throw (new Exception("Invalid"));
			return GetIndices(Size, Register.RegisterType, Register.Index, Name);
		}
	}
}
