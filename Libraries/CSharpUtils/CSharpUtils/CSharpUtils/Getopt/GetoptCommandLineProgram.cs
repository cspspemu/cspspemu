using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CSharpUtils.Extensions;

namespace CSharpUtils.Getopt
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class GetoptCommandLineProgram
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
            /// <param name="aliases"></param>
            public CommandAttribute(params string[] aliases)
            {
                Aliases = aliases;
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
            public object[] Values { get; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="values"></param>
            public ValuesAttribute(params object[] values)
            {
                Values = values;
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
            /// <param name="description"></param>
            public DescriptionAttribute(string description)
            {
                Description = description;
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
            /// <param name="example"></param>
            public ExampleAttribute(string example)
            {
                Example = example;
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
            /// <param name="switches"></param>
            public CommandDefaultAttribute(params string[] switches)
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
        protected virtual void ShowHelp()
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            var versionInfo = FileVersionInfo.GetVersionInfo(currentAssembly.Location);

            Console.WriteLine(
                "{0} - {1} - {2} - {3} - {4}",
                versionInfo.FileDescription,
                String.Join(".", versionInfo.FileVersion.Split('.').Take(2)),
                versionInfo.Comments,
                versionInfo.CompanyName,
                versionInfo.LegalCopyright
            );

            Console.WriteLine();

            Console.WriteLine("Commands:");
            foreach (var commandEntry in CommandEntries)
            {
                Console.Write("   ");
                Console.Write("{0}", commandEntry.Aliases.Take(1).ToStringArray(", "));
                if (commandEntry.MethodInfo != null)
                {
                    if (commandEntry.MethodInfo.GetParameters().Length > 0)
                    {
                        Console.Write(" <{0}>", commandEntry.MethodInfo.GetParameters().Select(item =>
                        {
                            var ret = item.Name;
                            if (item.IsOptional) ret = "[" + ret + "]";
                            return ret;
                        }).ToStringArray(", "));
                    }
                }
                if (commandEntry.Values.Length > 0)
                {
                    Console.Write(" [{0}]", commandEntry.Values.ToStringArray("|"));
                }

                Console.Write(" - ");
                Console.Write("{0}", commandEntry.Description);
                Console.WriteLine();
            }
            Console.WriteLine();

            Console.WriteLine("Examples:");
            foreach (var commandAttribute in CommandEntries)
            {
                foreach (var example in commandAttribute.Examples)
                {
                    Console.WriteLine("   {0}.exe {1}", currentAssembly.GetName().Name, example);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="switchDefaultAttribute"></param>
        /// <param name="method"></param>
        private void BindDefaultMethod(CommandDefaultAttribute switchDefaultAttribute, MethodInfo method)
        {
            Getopt.AddDefaultRule(() => { method.Invoke(this, new object[] { }); });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandEntry"></param>
        private void BindMethod(CommandEntry commandEntry)
        {
            var method = commandEntry.MethodInfo;

            Getopt.AddRule(commandEntry.Aliases, () =>
            {
                var parameters = method.GetParameters();
                var parametersData = new List<object>();

                foreach (var parameter in parameters)
                {
                    if (Getopt.HasMore)
                    {
                        var parameterData = Getopt.DequeueNext();
                        if (parameter.ParameterType == typeof(string))
                        {
                            parametersData.Add(parameterData);
                        }
                        else if (parameter.ParameterType == typeof(int))
                        {
                            parametersData.Add(int.Parse(parameterData));
                        }
                        else
                        {
                            throw new NotImplementedException(
                                "Not supported parameter type " + parameter.ParameterType);
                        }
                    }
                    else
                    {
                        parametersData.Add(null);
                    }
                }

                method.Invoke(this, parametersData.ToArray());
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandEntry"></param>
        private void BindField(CommandEntry commandEntry)
        {
            var field = commandEntry.FieldInfo;

            Getopt.AddRule(commandEntry.Aliases, () =>
            {
                if (field.FieldType == typeof(bool))
                {
                    field.SetValue(this, true);
                }
                else
                {
                    throw (new NotImplementedException("Not supported field type " + field.FieldType));
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

            foreach (var member in GetType()
                .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var descriptionAttribute = member.GetSingleAttribute<DescriptionAttribute>();
                var valuesAttribute = member.GetSingleAttribute<ValuesAttribute>();

                var commandDefaultAttribute = member.GetSingleAttribute<CommandDefaultAttribute>();
                if (commandDefaultAttribute != null)
                {
                    if (member is MethodInfo)
                    {
                        BindDefaultMethod(commandDefaultAttribute, member as MethodInfo);
                    }
                }

                var commandAttribute = member.GetSingleAttribute<CommandAttribute>();
                if (commandAttribute == null) continue;

                var commandEntry = new CommandEntry()
                {
                    Aliases = commandAttribute.Aliases,
                    MemberInfo = member,
                    Examples = member.GetAttribute<ExampleAttribute>().Select(item => item.Example).ToArray(),
                    Description = (descriptionAttribute != null) ? descriptionAttribute.Description : "",
                    Values = (valuesAttribute != null) ? valuesAttribute.Values : new object[0],
                };

                CommandEntries.Add(commandEntry);

                if (member is MethodInfo)
                {
                    BindMethod(commandEntry);
                }
                else if (member is FieldInfo)
                {
                    BindField(commandEntry);
                }
                else
                {
                    throw(new NotImplementedException("Don't know how to handle type " + member.GetType()));
                }
            }

            try
            {
                Getopt.Process();
            }
            catch (TargetInvocationException targetInvocationException)
            {
                Console.Error.WriteLine(targetInvocationException.InnerException);
                Environment.Exit(-1);
            }
            catch (Exception exception)
            {
                //Console.Error.WriteLine(Exception.Message);
                Console.Error.WriteLine(exception);
                Environment.Exit(-2);
            }

            if (Debugger.IsAttached)
            {
                Console.ReadKey();
            }
        }
    }
}