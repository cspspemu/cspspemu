//#define DEBUG_ANALYZER

using System.IO;
using CSharpUtils.Ext.SpaceAssigner;
using CSharpUtils.Streams;

namespace CSharpUtils.Ext.Streams
{
    /// <summary>
    /// 
    /// </summary>
    public class ProxyStreamReadWriteAnalyzer : ProxyStream
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly SpaceAssigner1D _readUsage = new SpaceAssigner1D();

        /// <summary>
        /// 
        /// </summary>
        private readonly SpaceAssigner1D _writeUsage = new SpaceAssigner1D();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseStream"></param>
        public ProxyStreamReadWriteAnalyzer(Stream baseStream)
            : base(baseStream)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public SpaceAssigner1D.Space[] ReadUsage => _readUsage.GetAvailableSpaces();

        /// <summary>
        /// 
        /// </summary>
        public SpaceAssigner1D.Space[] WriteUsage => _writeUsage.GetAvailableSpaces();

        /// <summary>
        /// 
        /// </summary>
        public override long Position
        {
            get { return base.Position; }
            set
            {
#if DEBUG_ANALYZER //Console.WriteLine("Change position to {0}", value);
#endif
                base.Position = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var start = Position;

            try
            {
#if DEBUG_ANALYZER
				Console.WriteLine("Read {0} bytes at {1}", count, Start);
#endif
                return base.Read(buffer, offset, count);
            }
            finally
            {
                _readUsage.AddAvailableWithBounds(start, Position);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            var start = Position;

            try
            {
#if DEBUG_ANALYZER
				Console.WriteLine("Write {0} bytes at {1}", count, Start);
#endif
                base.Write(buffer, offset, count);
            }
            finally
            {
                _writeUsage.AddAvailableWithBounds(start, Position);
            }
        }
    }
}