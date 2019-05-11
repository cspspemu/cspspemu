using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpUtils.Ext.SpaceAssigner
{
    /// <summary>
    /// 
    /// </summary>
    public class SpaceAssigner1DUniqueAllocator
    {
        protected SpaceAssigner1D SpaceAssigner;
        protected Dictionary<byte[], SpaceAssigner1D.Space> AllocatedSpaces;

        /// <summary>
        /// 
        /// </summary>
        public event Action<byte[], SpaceAssigner1D.Space> OnAllocate;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spaceAssigner"></param>
        public SpaceAssigner1DUniqueAllocator(SpaceAssigner1D spaceAssigner)
        {
            SpaceAssigner = spaceAssigner;
            AllocatedSpaces = new Dictionary<byte[], SpaceAssigner1D.Space>(new ArrayEqualityComparer<byte>());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public SpaceAssigner1D.Space AllocateUnique(byte[] data)
        {
            if (!AllocatedSpaces.ContainsKey(data))
            {
                var allocatedSpace = SpaceAssigner.Allocate(data.Length);
                OnAllocate?.Invoke(data, allocatedSpace);
                AllocatedSpaces[data] = allocatedSpace;
            }
            return AllocatedSpaces[data];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public SpaceAssigner1D.Space[] AllocateUnique(byte[][] data)
        {
            // @TODO Has to use data.Distinct() and the Allocate[] function in order
            //       to be able to avoid a greedy behaviour.

            var spaces = new SpaceAssigner1D.Space[data.Length];
            for (var n = 0; n < data.Length; n++)
            {
                spaces[n] = AllocateUnique(data[n]);
            }
            return spaces;
        }

        /// <summary>
        /// 
        /// </summary>
        public Encoding Encoding;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="String"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public SpaceAssigner1D.Space AllocateUnique(string String, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding;
            return AllocateUnique(String.GetStringzBytes(encoding));
        }
    }
}