//#define DEBUG_ANALYZER

using System.IO;
using CSharpUtils.SpaceAssigner;

namespace CSharpUtils.Streams
{
	/// <summary>
	/// 
	/// </summary>
    public class ProxyStreamReadWriteAnalyzer : ProxyStream
    {
		/// <summary>
		/// 
		/// </summary>
        protected SpaceAssigner1D _ReadUsage = new SpaceAssigner1D();

		/// <summary>
		/// 
		/// </summary>
        protected SpaceAssigner1D _WriteUsage = new SpaceAssigner1D();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="BaseStream"></param>
        public ProxyStreamReadWriteAnalyzer(Stream BaseStream)
            : base(BaseStream)
        {
        }

		/// <summary>
		/// 
		/// </summary>
        public SpaceAssigner1D.Space[] ReadUsage
        {
            get
            {
                return _ReadUsage.GetAvailableSpaces();
            }
        }

		/// <summary>
		/// 
		/// </summary>
        public SpaceAssigner1D.Space[] WriteUsage
        {
            get
            {
                return _WriteUsage.GetAvailableSpaces();
            }
        }

		/// <summary>
		/// 
		/// </summary>
		public override long Position
		{
			get
			{
				return base.Position;
			}
			set
			{
#if DEBUG_ANALYZER
				//Console.WriteLine("Change position to {0}", value);
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
            var Start = Position;

            try
            {
#if DEBUG_ANALYZER
				Console.WriteLine("Read {0} bytes at {1}", count, Start);
#endif
                return base.Read(buffer, offset, count);
            }
            finally
            {
                _ReadUsage.AddAvailableWithBounds(Start, Position);
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
            var Start = Position;

            try
            {
#if DEBUG_ANALYZER
				Console.WriteLine("Write {0} bytes at {1}", count, Start);
#endif
				base.Write(buffer, offset, count);
            }
            finally
            {
                _WriteUsage.AddAvailableWithBounds(Start, Position);
            }
        }
    }
}
