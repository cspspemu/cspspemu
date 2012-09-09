using System;
using CSPspEmu.Core.Display;
using CSPspEmu.Core.Controller;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Core
{
	public interface IGuiExternalInterface
	{
		// Get Object Methods
		PspMemory GetMemory();
		PspDisplay GetDisplay();
		PspController GetController();
		PspConfig GetConfig();
		bool IsInitialized();

		// Load Methods
		void LoadFile(String FileName);

		// Running Methods
		void Pause();
		void Resume();
		void PauseResume(Action Action);
		bool IsPaused();

		// Debug
		void ShowDebugInformation();

		PluginInfo GetAudioPluginInfo();
		PluginInfo GetGpuPluginInfo();

		void CaptureGpuFrame();
	}
}
