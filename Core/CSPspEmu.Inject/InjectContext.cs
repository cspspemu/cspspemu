using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using CSharpUtils;
using System.Collections.Concurrent;

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
    private readonly ConcurrentDictionary<Type, object> _objectsByType = new ConcurrentDictionary<Type, object>();

    /// <summary>
    /// 
    /// </summary>
    private readonly ConcurrentDictionary<Type, Type> _typesByType = new ConcurrentDictionary<Type, Type>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public object GetInstance(Type type)
    {
        if (_objectsByType.ContainsKey(type)) return _objectsByType[type];

        var instance = default(object);

        Logger.Notice("GetInstance<{0}>: Miss!", type);

        var elapsedTime = Logger.Measure(() =>
        {
            var realType = _typesByType.ContainsKey(type) ? _typesByType[type] : type;

            if (realType.IsAbstract)
                throw (new Exception(String.Format("Can't instantiate class '{0}', because it is abstract", realType)));

            try
            {
                //object Instance2 = null;
                //var ElapsedTime2 = Logger.Measure(() =>
                //{
                //	Instance2 = Activator.CreateInstance(RealType, true);
                //});
                //Console.Error.WriteLine("{0} : {1} : {2}", ElapsedTime2, Type, RealType);
                instance = _SetInstance(type, Activator.CreateInstance(realType, true));
            }
            catch (MissingMethodException)
            {
                throw(new Exception("No constructor for type '" + type.Name + "'"));
            }

            InjectDependencesTo(instance);
            //Instance._InitializeComponent(this);
            //Instance.InitializeComponent();
        });

        //Console.Out.WriteLineColored((ElapsedTime.TotalSeconds >= 0.05) ? ConsoleColor.Red : ConsoleColor.Gray, "GetInstance<{0}>: Miss! : LoadTime({1})", Type, ElapsedTime.TotalSeconds);
        Logger.Notice("GetInstance<{0}>: Miss! : LoadTime({1})", type, elapsedTime.TotalSeconds);

        return instance;
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
    /// <param name="instance"></param>
    /// <returns></returns>
    public TType SetInstance<TType>(object instance) // where TType : IInjectComponent
    {
        Logger.Info("PspEmulatorContext.SetInstance<{0}>", typeof(TType));
        return _SetInstance<TType>(instance);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="instance"></param>
    /// <returns></returns>
    protected object _SetInstance(Type type, object instance)
    {
        if (_objectsByType.ContainsKey(type))
        {
            throw (new InvalidOperationException());
        }
        _objectsByType[type] = instance;
        return instance;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    /// <param name="instance"></param>
    /// <returns></returns>
    protected TType _SetInstance<TType>(object instance) // where TType : IInjectComponent
    {
        return (TType) _SetInstance(typeof(TType), instance);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType1"></typeparam>
    /// <param name="type2"></param>
    public void SetInstanceType<TType1>(Type type2) // where TType1 : IInjectComponent
    {
        SetInstanceType(typeof(TType1), type2);
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
    /// <param name="type1"></param>
    /// <param name="type2"></param>
    public void SetInstanceType(Type type1, Type type2) // where TType1 : IInjectComponent
    {
        _typesByType[type1] = type2;
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
    /// <param name="type"></param>
    private void RemoveInstance(Type type)
    {
        object removed;
        while (_objectsByType.TryRemove(type, out removed))
        {
            if (!_objectsByType.ContainsKey(type)) break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Object"></param>
    public void InjectDependencesTo(object Object)
    {
        var getBindingFlags = (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        //Console.WriteLine("aaaaaaaaaaaaaaaaa");

        // Initialize all [Inject]
        foreach (var member in Object.GetType().GetMembers(getBindingFlags))
        {
            //Console.WriteLine("{0}", Member);
            var field = member as FieldInfo;
            var property = member as PropertyInfo;

            Type memberType = null;
            if (member.MemberType == MemberTypes.Field && field != null) memberType = field.FieldType;
            if (member.MemberType == MemberTypes.Property && property != null) memberType = property.PropertyType;

            var injectAttributeList = member.GetCustomAttributes(typeof(InjectAttribute), true);

            if (injectAttributeList.Length > 0)
            {
                switch (member.MemberType)
                {
                    case MemberTypes.Field:
                        if (field != null) field.SetValue(Object, GetInstance(memberType));
                        break;
                    case MemberTypes.Property:
                        if (property != null) property.SetValue(Object, GetInstance(memberType), null);
                        break;
                }
                Logger.Notice("Inject {0} to {1}", memberType, Object.GetType());
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
        foreach (var item in _objectsByType.Values)
        {
            if (item == this) continue;

            if (item.GetType().GetInterfaces().Contains(typeof(IDisposable)))
            {
                ((IDisposable) item).Dispose();
            }
        }
        _objectsByType.Clear();
    }

    public static InjectContext Bootstrap(object bootstrap, Dictionary<Type, Type> pairTypes = null)
    {
        var injectContext = new InjectContext();
        injectContext.MapFromClassAttributes(bootstrap);
        injectContext.MapFromPairTypes(pairTypes);
        injectContext.InjectDependencesTo(bootstrap);
        return injectContext;
    }

    public void MapFromPairTypes(Dictionary<Type, Type> pairTypes)
    {
        if (pairTypes == null) return;

        foreach (var pair in pairTypes)
        {
            SetInstanceType(pair.Key, pair.Value);
        }
    }

    public void MapFromClassAttributes(object bootstrap)
    {
        foreach (var injectMap in bootstrap.GetType().GetCustomAttributes<InjectMapAttribute>(true))
        {
            SetInstanceType(injectMap.From, injectMap.To);
        }
    }
}