using System;
using System.Collections.Generic;

namespace CSharpUtils.Ext.SpaceAssigner
{
    /// <summary>
    /// 
    /// </summary>
    public class SpaceAssigner2D
    {
        /// <summary>
        /// 
        /// </summary>
        public class Space
        {
            /// <summary>
            /// 
            /// </summary>
            public long X1, Y1, X2, Y2;

            /// <summary>
            /// 
            /// </summary>
            public long Width => X2 - X1;

            /// <summary>
            /// 
            /// </summary>
            public long Height => Y2 - Y1;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="x1"></param>
            /// <param name="y1"></param>
            /// <param name="x2"></param>
            /// <param name="y2"></param>
            public Space(long x1, long y1, long x2, long y2)
            {
                X1 = x1;
                Y1 = y1;
                X2 = x2;
                Y2 = y2;
            }
        }

        protected LinkedList<Space> AvailableChunks;

        /// <summary>
        /// 
        /// </summary>
        public SpaceAssigner2D()
        {
            AvailableChunks = new LinkedList<Space>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="space"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void AddAvailable(Space space)
        {
            throw new NotImplementedException();
            //AvailableChunks.AddLast(Space);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Space Allocate(long length)
        {
            throw new NotImplementedException();
        }
    }
}