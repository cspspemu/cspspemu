using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using CSharpUtils;

namespace CSPspEmu.Core
{
	public class PspEmulatorContext
	{
		public PspConfig PspConfig;

		//public event Action ApplicationExit;

		public PspEmulatorContext(PspConfig PspConfig)
		{
			this.PspConfig = PspConfig;
		}

		protected Dictionary<Type, PspEmulatorComponent> ObjectsByType = new Dictionary<Type, PspEmulatorComponent>();
		protected Dictionary<Type, Type> TypesByType = new Dictionary<Type, Type>();

		public TType GetInstance<TType>() where TType : PspEmulatorComponent
		{
			lock (this)
			{
				try
				{
					if (!ObjectsByType.ContainsKey(typeof(TType)))
					{
						Console.WriteLine("GetInstance<{0}>: Miss!", typeof(TType));
						var Start = DateTime.Now;
						PspEmulatorComponent Instance;
						if (TypesByType.ContainsKey(typeof(TType)))
						{
							Instance = _SetInstance<TType>((PspEmulatorComponent)Activator.CreateInstance(TypesByType[typeof(TType)]));
						}
						else
						{
							Instance = _SetInstance<TType>((PspEmulatorComponent)Activator.CreateInstance(typeof(TType)));
						}
						Instance._InitializeComponent(this);
						Instance.InitializeComponent();
						var End = DateTime.Now;
						Console.WriteLine("GetInstance<{0}>: Miss! : LoadTime({1})", typeof(TType), End - Start);
						return (TType)Instance;
					}

					return (TType)ObjectsByType[typeof(TType)];
				}
				catch (TargetInvocationException TargetInvocationException)
				{
					Console.Error.WriteLine("Error obtaining instance '{0}'", typeof(TType));
					StackTraceUtils.PreserveStackTrace(TargetInvocationException.InnerException);
					throw (TargetInvocationException.InnerException);
				}
			}
		}

		public TType SetInstance<TType>(PspEmulatorComponent Instance) where TType : PspEmulatorComponent
		{
			lock (this)
			{
				ConsoleUtils.SaveRestoreConsoleState(() =>
				{
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("PspEmulatorContext.SetInstance<{0}>", typeof(TType));
					//Console.WriteLine(Environment.StackTrace);
				});
				return _SetInstance<TType>(Instance);
			}
		}

		protected TType _SetInstance<TType>(PspEmulatorComponent Instance) where TType : PspEmulatorComponent
		{
			if (ObjectsByType.ContainsKey(typeof(TType)))
			{
				throw(new InvalidOperationException());
			}
			ObjectsByType[typeof(TType)] = Instance;
			return (TType)Instance;
		}

		public void SetInstanceType<TType1, TType2>() where TType1 : PspEmulatorComponent
		{
			lock (this)
			{
				TypesByType[typeof(TType1)] = typeof(TType2);
			}
		}

		/*
		public TType NewInstance<TType>() where TType : PspEmulatorComponent
		{
			ObjectsByType.Remove(typeof(TType));
			return GetInstance<TType>();
		}
		*/
	}
}
