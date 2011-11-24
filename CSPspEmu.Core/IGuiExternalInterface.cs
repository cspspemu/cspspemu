using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Display;
using CSPspEmu.Core.Controller;

namespace CSPspEmu.Core
{
	public interface IGuiExternalInterface
	{
		// Get Object Methods
		PspMemory GetMemory();
		PspDisplay GetDisplay();
		PspController GetController();
		PspConfig GetConfig();

		// Load Methods
		void LoadFile(String FileName);

		// Running Methods
		void Pause();
		void Resume();
		void PauseResume(Action Action);
		bool IsPaused();
	}
}
