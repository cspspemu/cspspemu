using System;
using System.Threading;

namespace CSPspEmu.Core.Audio
{
	public abstract class PspAudioImpl : PspPluginImpl
	{
		/// <summary>
		/// Called periodically on a thread.
		/// </summary>
		public abstract void Update(Action<short[]> ReadStream);

		/// <summary>
		/// 
		/// </summary>
		public abstract void StopSynchronized();

		/// <summary>
		/// 
		/// </summary>
		//public void __TestAudio()
		//{
		//	int m = 0;
		//	Action<short[]> Generator = (Data) =>
		//	{
		//		//Console.WriteLine("aaaa");
		//		for (int n = 0; n < Data.Length; n++)
		//		{
		//			Data[n] = (short)(Math.Cos(((double)m) / 100) * short.MaxValue);
		//			m++;
		//			//Console.WriteLine(Data[n]);
		//		}
		//	};
		//	byte[] GcTestData;
		//	while (true)
		//	{
		//		GcTestData = new byte[4 * 1024 * 1024];
		//		this.Update(Generator);
		//		Thread.Sleep(2);
		//		GC.Collect();
		//	}
		//}
	}
}
