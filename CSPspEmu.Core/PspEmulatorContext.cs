using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using CSharpUtils;

namespace CSPspEmu.Core
{
	public class PspEmulatorContext : IDisposable
	{
		static Logger Logger = Logger.GetLogger("PspEmulatorContext");

		public PspConfig PspConfig;

		//public event Action ApplicationExit;

		public PspEmulatorContext(PspConfig PspConfig)
		{
			this.PspConfig = PspConfig;
			this.PspConfig.PspEmulatorContext = this;
		}

		protected Dictionary<Type, PspEmulatorComponent> ObjectsByType = new Dictionary<Type, PspEmulatorComponent>();
		protected Dictionary<Type, Type> TypesByType = new Dictionary<Type, Type>();

		public object GetInstance(Type Type)
		{
			lock (this)
			{
				try
				{
					if (!ObjectsByType.ContainsKey(Type))
					{
						var Instance = default(PspEmulatorComponent);

						Logger.Info("GetInstance<{0}>: Miss!", Type);

						var ElapsedTime = Logger.Measure(() =>
						{
							if (TypesByType.ContainsKey(Type))
							{
								Instance = (PspEmulatorComponent)_SetInstance(Type, (PspEmulatorComponent)Activator.CreateInstance(TypesByType[Type]));
							}
							else
							{
								Instance = (PspEmulatorComponent)_SetInstance(Type, (PspEmulatorComponent)Activator.CreateInstance(Type));
							}
							Instance._InitializeComponent(this);
							Instance.InitializeComponent();
						});

						Logger.Info("GetInstance<{0}>: Miss! : LoadTime({1})", Type, ElapsedTime.TotalSeconds);

						return Instance;
					}

					return ObjectsByType[Type];
				}
				catch (TargetInvocationException TargetInvocationException)
				{
					Logger.Error("Error obtaining instance '{0}'", Type);
					StackTraceUtils.PreserveStackTrace(TargetInvocationException.InnerException);
					throw (TargetInvocationException.InnerException);
				}
			}
		}

		public TType GetInstance<TType>() where TType : PspEmulatorComponent
		{
			return (TType)GetInstance(typeof(TType));
		}

		public TType SetInstance<TType>(PspEmulatorComponent Instance) where TType : PspEmulatorComponent
		{
			lock (this)
			{
				Logger.Info("PspEmulatorContext.SetInstance<{0}>", typeof(TType));
				return _SetInstance<TType>(Instance);
			}
		}

		protected object _SetInstance(Type Type, PspEmulatorComponent Instance)
		{
			if (ObjectsByType.ContainsKey(Type))
			{
				throw (new InvalidOperationException());
			}
			ObjectsByType[Type] = Instance;
			return Instance;
		}

		protected TType _SetInstance<TType>(PspEmulatorComponent Instance) where TType : PspEmulatorComponent
		{
			return (TType)_SetInstance(typeof(TType), Instance);
		}

		public void SetInstanceType<TType1>(Type Type2) where TType1 : PspEmulatorComponent
		{
			lock (this)
			{
				TypesByType[typeof(TType1)] = Type2;
			}
		}

		public void SetInstanceType<TType1, TType2>() where TType1 : PspEmulatorComponent
		{
			SetInstanceType<TType1>(typeof(TType2));
		}

		/*
		public TType NewInstance<TType>() where TType : PspEmulatorComponent
		{
			ObjectsByType.Remove(typeof(TType));
			return GetInstance<TType>();
		}
		*/

		public void InjectDependencesTo(object Object)
		{
			var GetBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

			foreach (var Field in Object.GetType().GetFields(GetBindingFlags))
			{
				var FieldType = Field.FieldType;
				if (typeof(PspEmulatorComponent).IsAssignableFrom(FieldType))
				{
					Field.SetValue(Object, this.GetInstance(FieldType));
					Logger.Info("Inject {0} to {1}", FieldType, Object);
				}
			}

			foreach (var Property in Object.GetType().GetProperties(GetBindingFlags))
			{
				Console.WriteLine(Property);
				var PropertyType = Property.PropertyType;
				if (typeof(PspEmulatorComponent).IsAssignableFrom(PropertyType))
				{
					Property.SetValue(Object, this.GetInstance(PropertyType), null);
					Logger.Info("Inject {0} to {1}", PropertyType, Object);
				}
			}
		}

		public void Dispose()
		{
			foreach (var Pair in ObjectsByType) Pair.Value.Dispose();
			ObjectsByType = new Dictionary<Type, PspEmulatorComponent>();
		}
	}
}
