using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;
using CSharpUtils;

namespace CSPspEmu.Hle
{
    public unsafe partial class HleModuleHost : HleModule
    {
        public static readonly HleModuleHost Methods = new HleModuleHost();

        private Dictionary<uint, HleFunctionEntry> _EntriesByNID = null;
        private Dictionary<string, HleFunctionEntry> _EntriesByName = null;

        public string ModuleLocation;

        public Dictionary<uint, HleFunctionEntry> EntriesByNID
        {
            get
            {
                InitializeOnce();
                return _EntriesByNID;
            }
        }

        public Dictionary<string, HleFunctionEntry> EntriesByName
        {
            get
            {
                InitializeOnce();
                return _EntriesByName;
            }
        }

        public string Name
        {
            get { return this.GetType().Name; }
        }

        [Inject] protected PspMemory Memory;

        [Inject] protected InjectContext InjectContext;

        protected HleModuleHost()
        {
            this.ModuleLocation = "flash0:/kd/" + this.GetType().Namespace.Split('.').Last() + ".prx";
            //Initialize();
        }

        private void InitializeOnce()
        {
            if (_EntriesByNID == null) _Initialize();
            ModuleInitializeOnce();
        }

        protected virtual void ModuleInitializeOnce()
        {
        }

        protected virtual void ModuleInitialize()
        {
        }

        protected virtual void ModuleDeinitialize()
        {
        }

        private void _Initialize()
        {
            _EntriesByNID = new Dictionary<uint, HleFunctionEntry>();
            _EntriesByName = new Dictionary<string, HleFunctionEntry>();

            //this.PspEmulatorContext = PspEmulatorContext;
            //PspEmulatorContext.InjectDependencesTo(this);

            //Console.WriteLine(this.ModuleLocation);
            //Console.ReadKey();

            //try
            {
                foreach (
                    var MethodInfo in
                    new MethodInfo[0]
                        .Concat(this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance))
                    //.Concat(this.GetType().GetMethods(BindingFlags.NonPublic))
                    //.Concat(this.GetType().GetMethods(BindingFlags.Public))
                )
                {
                    var Attributes = MethodInfo.GetCustomAttributes(typeof(HlePspFunctionAttribute), true)
                        .Cast<HlePspFunctionAttribute>();
                    if (Attributes.Any())
                    {
                        if (!MethodInfo.IsPublic)
                        {
                            throw(new InvalidProgramException("Method " + MethodInfo + " is not public"));
                        }
                        var Delegate = CreateDelegateForMethodInfo(MethodInfo, Attributes.First());
                        foreach (var Attribute in Attributes)
                        {
                            _EntriesByNID[Attribute.NID] = new HleFunctionEntry()
                            {
                                NID = Attribute.NID,
                                Name = MethodInfo.Name,
                                Description = "",
                                Delegate = Delegate,
                                Module = this,
                                ModuleName = this.Name,
                            };
                        }
                        _EntriesByName[MethodInfo.Name] = _EntriesByNID[Attributes.First().NID];
                    }
                    else
                    {
                        //Console.WriteLine("HleModuleHost: NO: {0}", MethodInfo.Name);
                    }
                }
            }
            //catch (Exception Exception)
            //{
            //	Console.WriteLine(Exception);
            //	throw (Exception);
            //}

            ModuleInitialize();
        }

        public static string StringFromAddress(CpuThreadState CpuThreadState, uint Address)
        {
            if (Address == 0) return null;
            return PointerUtils.PtrToString((byte*) CpuThreadState.GetMemoryPtr(Address), Encoding.UTF8);
        }

        private struct ParamInfo
        {
            public enum RegisterTypeEnum
            {
                Gpr,
                Fpr,
            }

            public RegisterTypeEnum RegisterType;
            public int RegisterIndex;
            public ParameterInfo ParameterInfo;
        }
    }
}