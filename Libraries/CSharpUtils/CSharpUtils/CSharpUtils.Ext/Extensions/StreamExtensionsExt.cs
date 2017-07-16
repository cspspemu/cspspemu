using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpUtils.Ext.SpaceAssigner;
using CSharpUtils.Extensions;
using CSharpUtils.Streams;

namespace CSharpUtils.Ext.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class StreamExtensionsExt
    {
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Presumes it is ordered.</remarks>
        /// <param name="spaces"></param>
        /// <param name="thresold"></param>
        /// <returns></returns>
        public static SpaceAssigner1D.Space[] JoinWithThresold(this SpaceAssigner1D.Space[] spaces, int thresold = 32)
        {
            var newSpaces = new Stack<SpaceAssigner1D.Space>();
            newSpaces.Push(spaces[0]);
            for (var n = 1; n < spaces.Length; n++)
            {
                var prevSpace = newSpaces.Pop();
                var currentSpace = spaces[n];

                //Console.WriteLine("{0} - {1}", PrevSpace, CurrentSpace);

                if (currentSpace.Min - prevSpace.Max <= thresold)
                {
                    newSpaces.Push(new SpaceAssigner1D.Space(prevSpace.Min, currentSpace.Max));
                }
                else
                {
                    newSpaces.Push(prevSpace);
                    newSpaces.Push(currentSpace);
                }
            }

            return newSpaces.Reverse().ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="spaces"></param>
        /// <returns></returns>
        public static MapStream ConvertSpacesToMapStream(this Stream stream, SpaceAssigner1D.Space[] spaces)
        {
            var mapStream = new MapStream();

            foreach (var space in spaces)
            {
                mapStream.Map(space.Min, stream.SliceWithBounds(space.Min, space.Max));
            }

            return mapStream;
        }
    }
}