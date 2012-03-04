using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CSPspEmu.Core.Audio
{
	abstract public class PspAudioImpl : PspEmulatorComponent
	{
		/// <summary>
		/// Called periodically on a thread.
		/// </summary>
		abstract public void Update(Action<short[]> ReadStream);

		/// <summary>
		/// 
		/// </summary>
		abstract public void StopSynchronized();

		/// <summary>
		/// 
		/// </summary>
		abstract public PluginInfo PluginInfo { get; }

		abstract public bool IsWorking { get; }

		/// <summary>
		/// 
		/// </summary>
		public void __TestAudio()
		{
			int m = 0;
			Action<short[]> Generator = (Data) =>
			{
				//Console.WriteLine("aaaa");
				for (int n = 0; n < Data.Length; n++)
				{
					Data[n] = (short)(Math.Cos(((double)m) / 100) * short.MaxValue);
					m++;
					//Console.WriteLine(Data[n]);
				}
			};
			this.InitializeComponent();
			byte[] GcTestData;
			while (true)
			{
				GcTestData = new byte[4 * 1024 * 1024];
				this.Update(Generator);
				Thread.Sleep(2);
				GC.Collect();
			}
		}
	}
}
