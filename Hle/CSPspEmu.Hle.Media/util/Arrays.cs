namespace cscodec.util
{
	public class Arrays
	{
		public static T[][] ConvertDimensional<T>(T[,] In)
		{
			int Length1 = In.GetLength(0);
			int Length2 = In.GetLength(1);
			var Out = Create<T>(Length1, Length2);
			for (int a = 0; a < Length1; a++)
			for (int b = 0; b < Length2; b++)
			{
				Out[a][b] = In[a, b];
			}
			return Out;
		}

		public static T[][][] ConvertDimensional<T>(T[,,] In)
		{
			int Length1 = In.GetLength(0);
			int Length2 = In.GetLength(1);
			int Length3 = In.GetLength(2);
			var Out = Create<T>(Length1, Length2, Length3);
			for (int a = 0; a < Length1; a++)
			for (int b = 0; b < Length2; b++)
			for (int c = 0; c < Length3; c++)
			{
				Out[a][b][c] = In[a, b, c];
			}
			return Out;
		}

		public static T[] Create<T>(int Rank1)
		{
			return new T[Rank1];
		}

		public static T[][] Create<T>(int Rank1, int Rank2)
		{
			var Ret = new T[Rank1][];
			for (int n = 0; n < Rank1; n++) Ret[n] = Create<T>(Rank2);
			return Ret;
		}

		public static T[][][] Create<T>(int Rank1, int Rank2, int Rank3)
		{
			var Ret = new T[Rank1][][];
			for (int n = 0; n < Rank1; n++) Ret[n] = Create<T>(Rank2, Rank3);
			return Ret;
		}

		public static T[][][][] Create<T>(int Rank1, int Rank2, int Rank3, int Rank4)
		{
			var Ret = new T[Rank1][][][];
			for (int n = 0; n < Rank1; n++) Ret[n] = Create<T>(Rank2, Rank3, Rank4);
			return Ret;
		}

		public static bool Equals<T>(T[] Array1, T[] Array2)
		{
			if (Array1.Length != Array2.Length) return false;
			for (int n = 0; n < Array1.Length; n++) if (Array1 != Array2) return false;
			return true;
		}

		public static void Fill<T>(T[] Array, int IndexStart, int IndexEnd, T Value)
		{
			for (int n = IndexStart; n < IndexEnd; n++) Array[n] = Value;
		}

		public static void Fill<T>(T[] Array, T Value)
		{
			for (int n = 0; n < Array.Length; n++) Array[n] = Value;
		}
	}
}