using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core
{
	/// <summary>
	// 
	/// </summary>
	/// <see cref="http://silverspring.lan.st/NPSPTD_01.txt"/>
	/// <see cref="http://daxhordes.org/forum/viewtopic.php?f=33&t=808"/>
	public class IplReader
	{
		/// <summary>
		/// 
		/// </summary>
		protected Stream Stream;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="NandReader"></param>
		public IplReader(NandReader NandReader)
		{
			this.Stream = NandReader.SliceWithLength();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public MemoryStream GetIplData()
		{
			var MemoryStream = new MemoryStream();
			foreach (var BlockOffset in GetIplOffsets().ToArray())
			{
				//Console.WriteLine(BlockOffset);
				Stream.Position = NandReader.BytesPerBlock * BlockOffset;
				MemoryStream.WriteBytes(Stream.ReadBytes(NandReader.BytesPerBlock));
			}
			MemoryStream.Position = 0;
			return MemoryStream;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ushort> GetIplOffsets()
		{
			Stream.Position = NandReader.BytesPerBlock * 4;
			while (true)
			{
				var Result = Stream.ReadStruct<ushort>();
				if (Result == 0) break;
				yield return Result;
			}
		}
	}
}
