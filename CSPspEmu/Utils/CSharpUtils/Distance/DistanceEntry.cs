using System;

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
        /// <param name="distanceX"></param>
        /// <param name="distanceY"></param>
        public DistanceEntry(int distanceX, int distanceY)
        {
            DistanceX = distanceX;
            DistanceY = distanceY;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newDistance"></param>
        public void SetDistanceIfLower(DistanceEntry newDistance)
        {
            if (newDistance < this)
            {
                DistanceX = newDistance.DistanceX;
                DistanceY = newDistance.DistanceY;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator <(DistanceEntry left, DistanceEntry right)
        {
            return left.DistanceRank < right.DistanceRank;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >(DistanceEntry left, DistanceEntry right)
        {
            return left.DistanceRank > right.DistanceRank;
        }

        /// <summary>
        /// 
        /// </summary>
        public int DistanceRank => DistanceX * DistanceX + DistanceY * DistanceY;

        /// <summary>
        /// 
        /// </summary>
        public double Distance => Math.Sqrt(DistanceRank);

        /// <summary>
        /// 
        /// </summary>
        public static string Hex = "0123456789ABCDEF";

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public char GetChar()
        {
            var pos = (int) Distance;
            return Hex[Math.Min(pos, Hex.Length - 1)];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"D({DistanceX},{DistanceY})";
    }
}