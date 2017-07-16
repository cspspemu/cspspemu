using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpUtils.Drawing.Distance
{
	/// <summary>
	/// 
	/// </summary>
	public struct DistanceEntry
	{
		/// <summary>
		/// 
		/// </summary>
		public int DistanceX, DistanceY;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="DistanceX"></param>
		/// <param name="DistanceY"></param>
		public DistanceEntry(int DistanceX, int DistanceY)
		{
			this.DistanceX = DistanceX;
			this.DistanceY = DistanceY;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="NewDistance"></param>
		public void SetDistanceIfLower(DistanceEntry NewDistance)
		{
			if (NewDistance < this)
			{
				this.DistanceX = NewDistance.DistanceX;
				this.DistanceY = NewDistance.DistanceY;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Left"></param>
		/// <param name="Right"></param>
		/// <returns></returns>
		static public bool operator <(DistanceEntry Left, DistanceEntry Right)
		{
			return Left.DistanceRank < Right.DistanceRank;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Left"></param>
		/// <param name="Right"></param>
		/// <returns></returns>
		static public bool operator >(DistanceEntry Left, DistanceEntry Right)
		{
			return Left.DistanceRank > Right.DistanceRank;
		}

		/// <summary>
		/// 
		/// </summary>
		public int DistanceRank
		{
			get
			{
				return DistanceX * DistanceX + DistanceY * DistanceY;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public double Distance
		{
			get
			{
				return Math.Sqrt((double)DistanceRank);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		static public string Hex = "0123456789ABCDEF";

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public char GetChar()
		{
			int Pos = (int)Distance;
			return Hex[Math.Min(Pos, Hex.Length - 1)];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Format("D({0},{1})", DistanceX, DistanceY);
		}
	}
}
