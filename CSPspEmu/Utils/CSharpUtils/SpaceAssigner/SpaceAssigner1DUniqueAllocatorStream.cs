using System.IO;
using CSharpUtils.Extensions;
using CSharpUtils.Streams;

namespace CSharpUtils.Ext.SpaceAssigner
{
    /// <summary>
    /// 
    /// </summary>
    public class SpaceAssigner1DUniqueAllocatorStream : SpaceAssigner1DUniqueAllocator
    {
        /// <summary>
        /// 
        /// </summary>
        public Stream Stream;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spaceAssigner"></param>
        /// <param name="stream"></param>
        public SpaceAssigner1DUniqueAllocatorStream(SpaceAssigner1D spaceAssigner, Stream stream)
            : base(spaceAssigner)
        {
            Stream = stream;
            OnAllocate += SpaceAssigner1DUniqueAllocatorStream_OnAllocate;
        }

        /// <summary>
        /// 
        /// </summary>
        public void FillSpacesWithZeroes()
        {
            foreach (var space in SpaceAssigner.GetAvailableSpaces())
            {
                Stream.SliceWithBounds(space.Min, space.Max).FillStreamWithByte(0x00);
            }
        }

        private void SpaceAssigner1DUniqueAllocatorStream_OnAllocate(byte[] bytes, SpaceAssigner1D.Space space)
        {
            SliceStream.CreateWithBounds(Stream, space.Min, space.Max).WriteBytes(bytes);
        }
    }
}