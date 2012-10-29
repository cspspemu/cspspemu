using System;

namespace CSPspEmu.Core
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class InjectAttribute : Attribute
	{
	}

	public abstract class PspEmulatorComponent : IDisposable
	{
		protected PspEmulatorContext PspEmulatorContext { get; private set; }

		public void _InitializeComponent(PspEmulatorContext PspEmulatorContext)
		{
			//Console.WriteLine("_InitializeComponent : {0}", this.GetType());
			if (this.PspEmulatorContext != null) throw(new Exception("Can't call _InitializeComponent twice."));
			this.PspEmulatorContext = PspEmulatorContext;
			PspEmulatorContext.InjectDependencesTo(this);
		}

		public virtual void InitializeComponent()
		{
		}

		public virtual void Dispose()
		{
		}
	}
}
