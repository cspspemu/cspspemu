using System;

namespace CSPspEmu.Core
{
	public interface IGuiExternalInterface
	{
		InjectContext InjectContext { get; }

		// Load Methods
		void LoadFile(string FileName);

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
