using System;
using System.Collections.Generic;
using System.Reflection;
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

						Logger.Notice("GetInstance<{0}>: Miss!", Type);

						var ElapsedTime = Logger.Measure(() =>
						{
							var RealType = TypesByType.ContainsKey(Type) ? TypesByType[Type] : Type;

							if (RealType.IsAbstract) throw (new Exception(String.Format("Can't instantiate class '{0}', because it is abstract", RealType)));
							Instance = (PspEmulatorComponent)_SetInstance(Type, (PspEmulatorComponent)Activator.CreateInstance(RealType));

							Instance._InitializeComponent(this);
							Instance.InitializeComponent();
						});

						Logger.Notice("GetInstance<{0}>: Miss! : LoadTime({1})", Type, ElapsedTime.TotalSeconds);

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

		public TType NewInstance<TType>() where TType : PspEmulatorComponent
		{
			ObjectsByType.Remove(typeof(TType));
			return GetInstance<TType>();
		}

		public void InjectDependencesTo(object Object)
		{
			var GetBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

			foreach (var Member in Object.GetType().GetMembers(GetBindingFlags))
			{
				var Field = Member as FieldInfo;
				var Property = Member as PropertyInfo;
				Type MemberType = null;
				if (Member.MemberType == MemberTypes.Field) MemberType = Field.FieldType;
				if (Member.MemberType == MemberTypes.Property) MemberType = Property.PropertyType;

				var InjectAttributeList = Member.GetCustomAttributes(typeof(InjectAttribute), true);

				if (typeof(PspEmulatorComponent).IsAssignableFrom(MemberType) && InjectAttributeList != null && InjectAttributeList.Length > 0)
				{
					switch (Member.MemberType)
					{
						case MemberTypes.Field: Field.SetValue(Object, this.GetInstance(MemberType)); break;
						case MemberTypes.Property: Property.SetValue(Object, this.GetInstance(MemberType), null); break;
					}
					Logger.Notice("Inject {0} to {1}", MemberType, Object);
				}
			}

			/*
			foreach (var Field in Object.GetType().GetFields(GetBindingFlags))
			{
			}

			foreach (var Property in Object.GetType().GetProperties(GetBindingFlags))
			{
				//Console.WriteLine(Property);
			}
			*/
		}

		public void Dispose()
		{
			foreach (var Pair in ObjectsByType) Pair.Value.Dispose();
			ObjectsByType = new Dictionary<Type, PspEmulatorComponent>();
		}
	}
}
