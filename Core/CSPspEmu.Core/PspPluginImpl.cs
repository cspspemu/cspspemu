using System;

namespace CSPspEmu.Core
{
    public abstract class PspPluginImpl
    {
        /// <summary>
        /// 
        /// </summary>
        public abstract PluginInfo PluginInfo { get; }

        /// <summary>
        /// 
        /// </summary>
        public abstract bool IsWorking { get; }

        //public abstract void Start();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="AvailablePluginImplementations"></param>
        public static void SelectWorkingPlugin<TType>(InjectContext InjectContext,
            params Type[] AvailablePluginImplementations) where TType : PspPluginImpl
        {
            foreach (var ImplementationType in AvailablePluginImplementations)
            {
                bool IsWorking = false;

                try
                {
                    IsWorking = ((PspPluginImpl) InjectContext.GetInstance(ImplementationType)).IsWorking;
                }
                catch (Exception Exception)
                {
                    Console.Error.WriteLine(Exception);
                }

                if (IsWorking)
                {
                    // Found a working implementation
                    InjectContext.SetInstanceType<TType>(ImplementationType);
                    return;
                }
            }

            throw (new Exception("Can't find working type for '" + AvailablePluginImplementations + "'"));
            //return null;
        }
    }
}