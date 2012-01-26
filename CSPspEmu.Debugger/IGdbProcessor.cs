using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Debugger
{
	public interface IGdbProcessor {
		void registerOnSigval(Action<Sigval> callback);

		uint getRegister(uint index);
		void setRegister(uint index, uint value);

		int  getMemoryRange(byte[] buffer);
		int  setMemoryRange(byte[] buffer);

		void run();	
		void stepInto();
		void stepOver();
		void pause();
		void stop();
		bool isRunning { get; set; }
	}
}
