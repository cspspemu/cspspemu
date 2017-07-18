using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using CSharpUtils;
using System.Collections.Concurrent;
using CSharpUtils.Extensions;

/// <summary>
/// 
/// </summary>
public sealed class InjectContext : IDisposable
{
    /// <summary>
    /// 
    /// </summary>
    static Logger Logger = Logger.GetLogger("InjectContext");

    /// <summary>
    /// 
    /// </summary>
    public InjectContext()
    {
        SetInstance<InjectContext>(this);
    }

    /// <summary>
    /// Instances 
    /// </summary>
    private readonly ConcurrentDictionary<Type, object> ObjectsByType = new ConcurrentDictionary<Type, object>();

    /// <summary>
    /// 
    /// </summary>
    private readonly ConcurrentDictionary<Type, Type> TypesByType = new ConcurrentDictionary<Type, Type>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Type"></param>
    /// <returns></returns>
    public object GetInstance(Type Type)
    {
        if (!ObjectsByType.ContainsKey(Type))
        {
            var Instance = default(object);

            Logger.Notice("GetInstance<{0}>: Miss!", Type);

            var ElapsedTime = Logger.Measure(() =>
            {
                var RealType = TypesByType.ContainsKey(Type) ? TypesByType[Type] : Type;

                if (RealType.IsAbstract)
                    throw (new Exception(string.Format("Can't instantiate class '{0}', because it is abstract",
                        RealType)));

                try
                {
                    //object Instance2 = null;
                    //var ElapsedTime2 = Logger.Measure(() =>
                    //{
                    //	Instance2 = Activator.CreateInstance(RealType, true);
                    //});
                    //Console.Error.WriteLine("{0} : {1} : {2}", ElapsedTime2, Type, RealType);
                    Instance = _SetInstance(Type, Activator.CreateInstance(RealType, true));
                }
                catch (MissingMethodException)
                {
                    throw(new Exception("No constructor for type '" + Type.Name + "'"));
                }

                InjectDependencesTo(Instance);
                //Instance._InitializeComponent(this);
                //Instance.InitializeComponent();
            });

            //Console.Out.WriteLineColored((ElapsedTime.TotalSeconds >= 0.05) ? ConsoleColor.Red : ConsoleColor.Gray, "GetInstance<{0}>: Miss! : LoadTime({1})", Type, ElapsedTime.TotalSeconds);
            Logger.Notice("GetInstance<{0}>: Miss! : LoadTime({1})", Type, ElapsedTime.TotalSeconds);

            return Instance;
        }

        return ObjectsByType[Type];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    /// <returns></returns>
    public TType GetInstance<TType>() // where TType : IInjectComponent
    {
        return (TType) GetInstance(typeof(TType));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    /// <param name="Instance"></param>
    /// <returns></returns>
    public TType SetInstance<TType>(object Instance) // where TType : IInjectComponent
    {
        Logger.Info("PspEmulatorContext.SetInstance<{0}>", typeof(TType));
        return _SetInstance<TType>(Instance);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Type"></param>
    /// <param name="Instance"></param>
    /// <returns></returns>
    protected object _SetInstance(Type Type, object Instance)
    {
        if (ObjectsByType.ContainsKey(Type))
        {
            throw (new InvalidOperationException());
        }
        ObjectsByType[Type] = Instance;
        return Instance;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    /// <param name="Instance"></param>
    /// <returns></returns>
    protected TType _SetInstance<TType>(object Instance) // where TType : IInjectComponent
    {
        return (TType) _SetInstance(typeof(TType), Instance);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType1"></typeparam>
    /// <param name="Type2"></param>
    public void SetInstanceType<TType1>(Type Type2) // where TType1 : IInjectComponent
    {
        SetInstanceType(typeof(TType1), Type2);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType1"></typeparam>
    /// <typeparam name="TType2"></typeparam>
    public void SetInstanceType<TType1, TType2>() // where TType1 : IInjectComponent
    {
        SetInstanceType<TType1>(typeof(TType2));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Type1"></param>
    /// <param name="Type2"></param>
    public void SetInstanceType(Type Type1, Type Type2) // where TType1 : IInjectComponent
    {
        TypesByType[Type1] = Type2;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    /// <returns></returns>
    public TType NewInstance<TType>() // where TType : IInjectComponent
    {
        RemoveInstance(typeof(TType));
        return GetInstance<TType>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Type"></param>
    private void RemoveInstance(Type Type)
    {
        object Removed;
        while (ObjectsByType.TryRemove(Type, out Removed))
        {
            if (!ObjectsByType.ContainsKey(Type)) break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Object"></param>
    public void InjectDependencesTo(object Object)
    {
        var GetBindingFlags = (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        //Console.WriteLine("aaaaaaaaaaaaaaaaa");

        // Initialize all [Inject]
        foreach (var Member in Object.GetType().GetMembers(GetBindingFlags))
        {
            //Console.WriteLine("{0}", Member);
            var Field = Member as FieldInfo;
            var Property = Member as PropertyInfo;
            Type MemberType = null;
            if (Member.MemberType == MemberTypes.Field) MemberType = Field.FieldType;
            if (Member.MemberType == MemberTypes.Property) MemberType = Property.PropertyType;

            var InjectAttributeList = Member.GetCustomAttributes(typeof(InjectAttribute), true);

            if (InjectAttributeList.Length > 0)
            {
                switch (Member.MemberType)
                {
                    case MemberTypes.Field:
                        Field.SetValue(Object, this.GetInstance(MemberType));
                        break;
                    case MemberTypes.Property:
                        Property.SetValue(Object, this.GetInstance(MemberType), null);
                        break;
                }
                Logger.Notice("Inject {0} to {1}", MemberType, Object.GetType());
            }
        }

        // Call Initialization
        if (Object.GetType().GetInterfaces().Contains(typeof(IInjectInitialize)))
        {
            ((IInjectInitialize) Object).Initialize();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Dispose()
    {
        foreach (var Item in ObjectsByType.Values)
        {
            if (Item == this) continue;

            if (Item.GetType().GetInterfaces().Contains(typeof(IDisposable)))
            {
                ((IDisposable) Item).Dispose();
            }
        }
        ObjectsByType.Clear();
    }

    public static InjectContext Bootstrap(object Bootstrap, Dictionary<Type, Type> PairTypes = null)
    {
        var InjectContext = new InjectContext();
        InjectContext.MapFromClassAttributes(Bootstrap);
        InjectContext.MapFromPairTypes(PairTypes);
        InjectContext.InjectDependencesTo(Bootstrap);
        return InjectContext;
    }

    public void MapFromPairTypes(Dictionary<Type, Type> PairTypes)
    {
        if (PairTypes != null)
        {
            foreach (var Pair in PairTypes)
            {
                this.SetInstanceType(Pair.Key, Pair.Value);
            }
        }
    }

    public void MapFromClassAttributes(object Bootstrap)
    {
        foreach (var InjectMap in Bootstrap.GetType().GetCustomAttributes<InjectMapAttribute>(true))
        {
            this.SetInstanceType(InjectMap.From, InjectMap.To);
        }
    }
}