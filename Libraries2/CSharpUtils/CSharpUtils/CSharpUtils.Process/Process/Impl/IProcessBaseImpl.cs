using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpUtils.Process.Impl
{
	public interface IProcessBaseImpl
	{
		/**
		 * Initializes the Process with the selected delegate.
		 */
		void Init(RunDelegate Delegate);
		void SwitchTo();
		void Remove();
		void Yield();
	}
}
