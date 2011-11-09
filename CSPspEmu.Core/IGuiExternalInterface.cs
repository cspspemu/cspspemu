using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core
{
	public interface IGuiExternalInterface
	{
		void LoadFile(String FileName);
		PspMemory GetMemory();
		PspDisplay GetDisplay();
		PspController GetController();
		void Pause();
		void Resume();
		void PauseResume(Action Action);
	}
}
