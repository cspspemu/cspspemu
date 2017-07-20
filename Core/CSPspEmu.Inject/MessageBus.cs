using System;
using System.Collections.Generic;

namespace CSPspEmu.Inject
{
    public class MessageBus
    {
        Dictionary<Type, List<Delegate>> Handlers = new Dictionary<Type, List<Delegate>>();

        public void Unregister<T>(Action<T> Handler)
        {
            //if (!Handlers.ContainsKey(typeof(T))) Handlers[typeof(T)] = new List<Delegate>();
            //Handlers[typeof(T)].Add(Handler);
        }

        public void Register<T>(Action<T> Handler)
        {
            if (!Handlers.ContainsKey(typeof(T))) Handlers[typeof(T)] = new List<Delegate>();
            Handlers[typeof(T)].Add(Handler);
        }

        public void Dispatch<T>(T Value)
        {
            if (Handlers.ContainsKey(typeof(T)))
            {
                foreach (var Handler in Handlers[typeof(T)])
                {
                    Handler.DynamicInvoke(Value);
                }
            }
        }
    }
}