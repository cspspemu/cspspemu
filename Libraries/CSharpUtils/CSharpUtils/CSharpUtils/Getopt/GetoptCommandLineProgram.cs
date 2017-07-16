using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using CSharpUtils.Getopt;

namespace CSharpUtils.Getopt
{
    /// <summary>
    /// 
    /// </summary>
    abstract public class GetoptCommandLineProgram
    {
        /// <summary>
        /// 
        /// </summary>
        protected class CommandEntry
        {
            /// <summary>
            /// 
            /// </summary>
            public string[] Aliases;

            /// <summary>
            /// 
            /// </summary>
            public object[] Values;

            /// <summary>
            /// 
            /// </summary>
            public string Description { get; internal set; }

            /// <summary>
            /// 
            /// </summary>
            public string[] Examples { get; internal set; }

            /// <summary>
            /// 
            /// </summary>
            public MemberInfo MemberInfo { get; internal set; }

            /// <summary>
            /// 
            /// </summary>
            public MethodInfo MethodInfo
            {
                get { return MemberInfo as MethodInfo; }
            }

            /// <summary>
            /// 
            /// </summary>
            public PropertyInfo PropertyInfo
            {
                get { return MemberInfo as PropertyInfo; }
            }

            /// <summary>
            /// 
            /// </summary>
            public FieldInfo FieldInfo
            {
                get { return MemberInfo as FieldInfo; }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple =
            true)]
        protected class CommandAttribute : Attribute
        {
            /// <summary>
            /// 
            /// </summary>
            public string[] Aliases { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="Aliases"></param>
            public CommandAttribute(params string[] Aliases)
            {
                this.Aliases = Aliases;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
        protected class ValuesAttribute : Attribute
        {
            /// <summary>
            /// 
            /// </summary>
            public object[] Values { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="Values"></param>
            public ValuesAttribute(params object[] Values)
            {
                this.Values = Values;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple =
            false)]
        protected class DescriptionAttribute : Attribute
        {
            /// <summary>
            /// 
            /// </summary>
            public string Description { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="Description"></param>
            public DescriptionAttribute(string Description)
            {
                this.Description = Description;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        protected class ExampleAttribute : Attribute
        {
            /// <summary>
            /// 
            /// </summary>
            public string Example { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="Example"></param>
            public ExampleAttribute(string Example)
            {
                this.Example = Example;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        protected class CommandDefaultAttribute : Attribute
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="Switches"></param>
            public CommandDefaultAttribute(params string[] Switches)
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected List<CommandEntry> CommandEntries;

        /// <summary>
        /// 
        /// </summary>
        [Command("-?", "-h", "--help", "/?", "/h")]
        [CommandDefaultAttribute]
        [Description("Shows this help")]
        virtual protected void ShowHelp()
        {
            var CurrentAssembly = Assembly.GetEntryAssembly();
            var VersionInfo = FileVersionInfo.GetVersionInfo(CurrentAssembly.Location);

            Console.WriteLine(
                "{0} - {1} - {2} - {3} - {4}",
                VersionInfo.FileDescription,
                String.Join(".", VersionInfo.FileVersion.Split('.').Take(2)),
                VersionInfo.Comments,
                VersionInfo.CompanyName,
                VersionInfo.LegalCopyright
            );

            Console.WriteLine();

            Console.WriteLine("Commands:");
            foreach (var CommandEntry in CommandEntries)
            {
                Console.Write("   ");
                Console.Write("{0}", CommandEntry.Aliases.Take(1).ToStringArray(", "));
                if (CommandEntry.MethodInfo != null)
                {
                    if (CommandEntry.MethodInfo.GetParameters().Length > 0)
                    {
                        Console.Write(" <{0}>", CommandEntry.MethodInfo.GetParameters().Select(Item =>
                        {
                            string Ret = Item.Name;
                            if (Item.IsOptional) Ret = "[" + Ret + "]";
                            return Ret;
                        }).ToStringArray(", "));
                    }
                }
                if (CommandEntry.Values.Length > 0)
                {
                    Console.Write(" [{0}]", CommandEntry.Values.ToStringArray("|"));
                }

                Console.Write(" - ");
                Console.Write("{0}", CommandEntry.Description);
                Console.WriteLine();
            }
            Console.WriteLine();

            Console.WriteLine("Examples:");
            foreach (var CommandAttribute in CommandEntries)
            {
                foreach (var Example in CommandAttribute.Examples)
                {
                    Console.WriteLine("   {0}.exe {1}", CurrentAssembly.GetName().Name, Example);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SwitchDefaultAttribute"></param>
        /// <param name="Method"></param>
        private void BindDefaultMethod(CommandDefaultAttribute SwitchDefaultAttribute, MethodInfo Method)
        {
            Getopt.AddDefaultRule(() => { Method.Invoke(this, new object[] { }); });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CommandEntry"></param>
        private void BindMethod(CommandEntry CommandEntry)
        {
            var Method = CommandEntry.MethodInfo;

            Getopt.AddRule(CommandEntry.Aliases, () =>
            {
                var Parameters = Method.GetParameters();
                var ParametersData = new List<object>();

                foreach (var Parameter in Parameters)
                {
                    if (Getopt.HasMore)
                    {
                        var ParameterData = Getopt.DequeueNext();
                        if (Parameter.ParameterType == typeof(string))
                        {
                            ParametersData.Add((string) ParameterData);
                        }
                        else if (Parameter.ParameterType == typeof(int))
                        {
                            ParametersData.Add(int.Parse(ParameterData));
                        }
                        else
                        {
                            throw(new NotImplementedException("Not supported parameter type " + Parameter.ParameterType)
                            );
                        }
                    }
                    else
                    {
                        ParametersData.Add(null);
                    }
                }

                Method.Invoke(this, ParametersData.ToArray());
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CommandEntry"></param>
        private void BindField(CommandEntry CommandEntry)
        {
            var Field = CommandEntry.FieldInfo;

            Getopt.AddRule(CommandEntry.Aliases, () =>
            {
                if (Field.FieldType == typeof(bool))
                {
                    Field.SetValue(this, true);
                }
                else
                {
                    throw (new NotImplementedException("Not supported field type " + Field.FieldType));
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        protected Getopt Getopt;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public void Run(string[] args)
        {
            Getopt = new Getopt(args);

            CommandEntries = new List<CommandEntry>();

            foreach (var Member in this.GetType()
                .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var DescriptionAttribute = Member.GetSingleAttribute<DescriptionAttribute>();
                var ValuesAttribute = Member.GetSingleAttribute<ValuesAttribute>();

                var CommandDefaultAttribute = Member.GetSingleAttribute<CommandDefaultAttribute>();
                if (CommandDefaultAttribute != null)
                {
                    if (Member is MethodInfo)
                    {
                        BindDefaultMethod(CommandDefaultAttribute, Member as MethodInfo);
                    }
                }

                var CommandAttribute = Member.GetSingleAttribute<CommandAttribute>();
                if (CommandAttribute != null)
                {
                    var CommandEntry = new CommandEntry()
                    {
                        Aliases = CommandAttribute.Aliases,
                        MemberInfo = Member,
                        Examples = Member.GetAttribute<ExampleAttribute>().Select(Item => Item.Example).ToArray(),
                        Description = (DescriptionAttribute != null) ? DescriptionAttribute.Description : "",
                        Values = (ValuesAttribute != null) ? ValuesAttribute.Values : new object[0],
                    };

                    CommandEntries.Add(CommandEntry);

                    if (Member is MethodInfo)
                    {
                        BindMethod(CommandEntry);
                    }
                    else if (Member is FieldInfo)
                    {
                        BindField(CommandEntry);
                    }
                    else
                    {
                        throw(new NotImplementedException("Don't know how to handle type " + Member.GetType()));
                    }
                }
            }

            try
            {
                Getopt.Process();
            }
            catch (TargetInvocationException TargetInvocationException)
            {
                Console.Error.WriteLine(TargetInvocationException.InnerException);
                Environment.Exit(-1);
            }
            catch (Exception Exception)
            {
                //Console.Error.WriteLine(Exception.Message);
                Console.Error.WriteLine(Exception);
                Environment.Exit(-2);
            }

            if (Debugger.IsAttached)
            {
                Console.ReadKey();
            }
        }
    }
}