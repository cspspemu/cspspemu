using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Runner
{
	public interface IRunnableComponent
	{
		/// <summary>
		/// Starts the component and waits until it have been successfully started.
		/// </summary>
		void StartSynchronized();

		/// <summary>
		/// Stops the component and waits until it have been successfully stopped.
		/// </summary>
		void StopSynchronized();

		/// <summary>
		/// Pauses the component and waits until it have been successfully paused.
		/// </summary>
		void PauseSynchronized();

		/// <summary>
		/// Resumes the component and waits until it have been successfully resumed.
		/// </summary>
		void ResumeSynchronized();
	}
}
