using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpUtils.Getopt
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Getopt
    {
        /// <summary>
        /// 
        /// </summary>
        private Queue<string> Items;

        /// <summary>
        /// 
        /// </summary>
        private Dictionary<string, Action<string, string>> Actions;

        /// <summary>
        /// 
        /// </summary>
        private Action _defaultAction;

        /// <summary>
        /// 
        /// </summary>
        public string[] SopportedSwitches = {"/", "-", "--"};

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        public Getopt(IEnumerable<string> items)
        {
            Items = new Queue<string>(items);
            Actions = new Dictionary<string, Action<string, string>>();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool HasMore => Items.Count > 0;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string DequeueNext() => Items.Dequeue();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string[] DequeueAllNext() => Items.ToArray();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void AddDefaultRule(Action action) => _defaultAction = action;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public unsafe void AddRule(string name, ref bool value)
        {
            fixed (bool* ptr = &value)
            {
                var ptr2 = ptr;
                AddRule(name, (bool vv) => { *ptr2 = vv; });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public unsafe void AddRule(string name, ref int value)
        {
            fixed (int* ptr = &value)
            {
                var ptr2 = ptr;
                AddRule(name, (int vv) => { *ptr2 = vv; });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void AddRule<T>(string name, Action<T> action)
        {
            AddRule(new[] {name}, action);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void AddRule(string name, Action action)
        {
            AddRule(new[] {name}, action);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="names"></param>
        /// <param name="action"></param>
        public void AddRule(string[] names, Action action)
        {
            void Action(string current, string arg)
            {
                action();
            }

            foreach (var name in names)
            {
                Actions.Add(name, Action);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void AddRule(Action<string> action)
        {
            Actions.Add("", (current, arg) => { action(current); });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static TType CheckArgument<TType>(string name, Func<TType> action)
        {
            try
            {
                return action();
            }
            catch (Exception)
            {
                throw (new Exception(String.Format("Argument {0} requires a {1}", name, typeof(TType))));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="names"></param>
        /// <param name="action"></param>
        public void AddRule<T>(string[] names, Action<T> action)
        {
            Action<string, string> formalAction;
            var type = typeof(T);

            if (type == typeof(bool))
            {
                formalAction = (current, arg) =>
                {
                    // ReSharper disable once PossibleNullReferenceException
                    (action as Action<bool>)(true);
                };
            }
            else if (type == typeof(int))
            {
                formalAction = (current, arg) =>
                {
                    var argument = CheckArgument(current, () => int.Parse(arg != null ? arg : DequeueNext()));
                    // ReSharper disable once PossibleNullReferenceException
                    (action as Action<int>)(argument);
                };
            }
            else if (type == typeof(float))
            {
                formalAction = (current, arg) =>
                {
                    var argument = CheckArgument(current, () => float.Parse(arg != null ? arg : DequeueNext()));
                    // ReSharper disable once PossibleNullReferenceException
                    (action as Action<float>)(argument);
                };
            }
            else if (type == typeof(double))
            {
                formalAction = (current, arg) =>
                {
                    var argument = CheckArgument(current, () => double.Parse(arg != null ? arg : DequeueNext()));
                    // ReSharper disable once PossibleNullReferenceException
                    (action as Action<double>)(argument);
                };
            }
            else if (type == typeof(string))
            {
                formalAction = (current, arg) =>
                {
                    var argument = CheckArgument(current, () => arg != null ? arg : DequeueNext());
                    // ReSharper disable once PossibleNullReferenceException
                    (action as Action<string>)(argument);
                };
            }
            else
            {
                throw new Exception("Unknown Type : " + typeof(T).Name);
            }
            foreach (var name in names)
            {
                Actions.Add(name, formalAction);
            }
            //foreach ()
        }

        /// <summary>
        /// 
        /// </summary>
        public void Process()
        {
            var executedAnyAction = false;

            while (HasMore)
            {
                var currentRaw = DequeueNext();
                string arg = null;
                var current = currentRaw;

                var equalsOffset = currentRaw.IndexOf('=');
                if (equalsOffset >= 0)
                {
                    current = currentRaw.Substring(0, equalsOffset);
                    arg = currentRaw.Substring(equalsOffset + 1);
                }

                if (SopportedSwitches.Any((v) => current.StartsWith(v)))
                {
                    if (Actions.ContainsKey(current))
                    {
                        Actions[current](current, arg);
                        executedAnyAction = true;
                    }
                    else
                    {
                        throw (new Exception("Unknown parameter '" + current + "'"));
                    }
                }
                else
                {
                    if (Actions.ContainsKey(""))
                    {
                        Actions[""](current, arg);
                        executedAnyAction = true;
                    }
                }
            }

            if (!executedAnyAction)
            {
                _defaultAction?.Invoke();
            }
        }
    }
}