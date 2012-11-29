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
			public uint VfpuSize;
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

			public static VfpuRegisterInfo Parse(uint VfpuSize, string Name)
			{
				Name = Name.ToUpperInvariant();
				return new VfpuRegisterInfo()
				{
					VfpuSize = VfpuSize,
					Type = Name[0],
					Matrix = int.Parse(Name.Substring(1, 1)),
					Column = int.Parse(Name.Substring(2, 1)),
					Row = int.Parse(Name.Substring(3, 1))
				};
			}

			public uint Index
			{
				get
				{
					switch (Type)
					{
						case 'R':
						case 'C':
							{
								//return (uint)VfpuUtils.GetCellIndex(Register.Matrix, Register.Row, Register.Column);
								int Line = (Type == 'R') ? Row : Column;
								int Offset = (Type == 'R') ? Column : Row;
								int RowColumn = (Type == 'R') ? 1 : 0;

								return new VfpuRegisterInt()
								{
									RC_LINE = (uint)Line,
									RC_MATRIX = (uint)Matrix,
									RC_ROW_COLUMN = (uint)RowColumn,
									RC_OFFSET = (uint)Offset,
								};
							}
						case 'M':
						case 'E':
							{
								int _Row = (Type == 'M') ? Row : Column;
								int _Column = (Type == 'M') ? Column : Row;
								int Transposed = (Type == 'M') ? 0 : 1;

								return new VfpuRegisterInt()
								{
									M_MATRIX = (uint)Matrix,
									M_ROW = (uint)_Row,
									M_COLUMN = (uint)_Column,
									M_TRANSPOSED = (uint)Transposed,
								};
							}
						case 'S':
							return new VfpuRegisterInt()
							{
								S_COLUMN = (uint)Column,
								S_MATRIX = (uint)Matrix,
								S_ROW = (uint)Row,
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

		public static int GetCellIndex(int Matrix, int Column, int Row)
		{
			return Matrix * 16 + Column * 4 + Row;
		}

		public static uint GetCellIndex(uint Matrix, uint Column, uint Row)
		{
			return (uint)GetCellIndex((int)Matrix, (int)Column, (int)Row);
		}

		public static int[] GetIndices(uint Size, RegisterType RegisterType, VfpuRegisterInt Register, string Name = null)
		{
			return _GetIndices(Size, RegisterType, Register, Name).ToArray();
		}

		public static int[,] GetIndicesMatrix(uint Size, VfpuRegisterInt Register, string Name = null)
		{
			var IndicesMatrix = new int[Size, Size];

			for (uint Row = 0; Row < Size; Row++)
			{
				for (uint Column = 0; Column < Size; Column++)
				{
					if (Register.M_TRANSPOSED != 0)
					{
						IndicesMatrix[Column, Row] = (int)GetCellIndex(Register.M_MATRIX, Register.M_ROW + Row, Register.M_COLUMN + Column);
					}
					else
					{
						IndicesMatrix[Column, Row] = (int)GetCellIndex(Register.M_MATRIX, Register.M_COLUMN + Column, Register.M_ROW + Row);
					}
				}
			}

			return IndicesMatrix;
		}

		private static IEnumerable<int> _GetIndices(uint Size, RegisterType RegisterType, VfpuRegisterInt Register, string Name = null)
		{
			if (Size < 1 || Size > 4) throw (new Exception(String.Format("Invalid Size {0} !€ [0, 4]", Size)));

			if (RegisterType == VfpuUtils.RegisterType.Vector && Size == 1) RegisterType = VfpuUtils.RegisterType.Cell;

			switch (RegisterType)
			{
				case RegisterType.Cell:
					{
						yield return (int)GetCellIndex(Register.S_MATRIX, Register.S_COLUMN, Register.S_ROW);
						yield break;
					}
				case RegisterType.Vector:
					for (uint Index = 0; Index < Size; Index++)
					{
						if (Register.RC_ROW_COLUMN == 1)
						{
							yield return (int)GetCellIndex(Register.RC_MATRIX, Register.RC_OFFSET + Index, Register.RC_LINE);
						}
						else
						{
							yield return (int)GetCellIndex(Register.RC_MATRIX, Register.RC_LINE, Register.RC_OFFSET + Index);
						}
					}
					yield break;
				case RegisterType.Matrix:
					{
						var Matrix = GetIndicesMatrix(Size, Register, Name);
						for (uint Row = 0; Row < Size; Row++)
						{
							for (uint Column = 0; Column < Size; Column++)
							{
								yield return Matrix[Column, Row];
							}
						}
					}
					yield break;
				default: throw (new NotImplementedException(String.Format("Invalid vfpu registry name {0}('{1}')", Register, Name)));
			}
		}

		public static int[] GetIndices(string NameWithSufix)
		{
			uint Size = 0;
			if (NameWithSufix.EndsWith(".q")) Size = 4;
			else if (NameWithSufix.EndsWith(".t")) Size = 3;
			else if (NameWithSufix.EndsWith(".p")) Size = 2;
			else if (NameWithSufix.EndsWith(".s")) Size = 1;
			if (Size == 0) throw(new Exception("Register doesn't have sufix"));
			return GetIndices(Size, NameWithSufix.Substring(0, NameWithSufix.Length - 2));
		}

		public static int[] GetIndices(uint Size, string Name)
		{
			var Register = VfpuRegisterInfo.Parse(Size, Name);
			if ((Register.Type == 'S') && (Size != 1)) throw (new Exception("Invalid"));
			return GetIndices(Size, Register.RegisterType, Register.Index, Name);
		}

		public static IEnumerable<int> XRange(int From, int To)
		{
			for (int n = From; n < To; n++) yield return n;
		}
	}
}
