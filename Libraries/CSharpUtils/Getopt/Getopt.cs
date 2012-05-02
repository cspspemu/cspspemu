using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpUtils.Getopt
{
	public class Getopt
	{
		protected Queue<string> Items;
		protected Dictionary<string, Action<string, string>> Actions;

		public Getopt(string[] _Items)
		{
			this.Items = new Queue<string>(_Items);
			this.Actions = new Dictionary<string, Action<string, string>>();
		}

		public bool HasMore
		{
			get
			{
				return Items.Count > 0;
			}
		}

		/*
		public string Next
		{
			get
			{
				return DequeueNext();
			}
		}
		*/

		public string DequeueNext()
		{
			return this.Items.Dequeue();
		}

		unsafe public void AddRule(string Name, ref bool Value)
		{
			fixed (bool* ptr = &Value)
			{
				bool* ptr2 = ptr;
				AddRule<bool>(Name, (bool _Value) =>
				{
					*ptr2 = _Value;
				});
			}
		}

		unsafe public void AddRule(string Name, ref int Value)
		{
			fixed (int* ptr = &Value)
			{
				int* ptr2 = ptr;
				AddRule<int>(Name, (int _Value) =>
				{
					*ptr2 = _Value;
				});
			}
		}

		public void AddRule<T>(string Name, Action<T> Action)
		{
			AddRule<T>(new string[] { Name }, Action);
		}

		public void AddRule(string Name, Action Action)
		{
			AddRule(new string[] { Name }, Action);
		}

		public void AddRule(string[] Names, Action Action)
		{
			Action<string, string> FormalAction;
			FormalAction = (Current, Arg) => { (Action as Action)(); };

			foreach (var Name in Names)
			{
				this.Actions.Add(Name, FormalAction);
			}
		}

		public void AddRule(Action<string> Action)
		{
			Action<string, string> FormalAction;
			FormalAction = (Current, Arg) => { Action(Current); };
			this.Actions.Add("", FormalAction);
		}

		static public TType CheckArgument<TType>(string Name, Func<TType> Action)
		{
			try
			{
				return Action();
			}
			catch (Exception)
			{
				throw (new Exception(String.Format("Argument {0} requires a {1}", Name, typeof(TType))));
			}
		}

		public void AddRule<T>(string[] Names, Action<T> Action)
		{
			Action<string, string> FormalAction;
			var Type = typeof(T);

			if (Type == typeof(bool))
			{
				FormalAction = (Current, Arg) =>
				{
					(Action as Action<Boolean>)(true);
				};
			}
			else if (Type == typeof(int))
			{
				FormalAction = (Current, Arg) =>
				{
					var Argument = CheckArgument(Current, () => int.Parse(Arg != null ? Arg : DequeueNext()));
					(Action as Action<int>)(Argument);
				};
			}
			else if (Type == typeof(float))
			{
				FormalAction = (Current, Arg) =>
				{
					var Argument = CheckArgument(Current, () => float.Parse(Arg != null ? Arg : DequeueNext()));
					(Action as Action<float>)(Argument);
				};
			}
			else if (Type == typeof(double))
			{
				FormalAction = (Current, Arg) =>
				{
					var Argument = CheckArgument(Current, () => double.Parse(Arg != null ? Arg : DequeueNext()));
					(Action as Action<double>)(Argument);
				};
			}
			else if (Type == typeof(string))
			{
				FormalAction = (Current, Arg) =>
				{
					var Argument = CheckArgument(Current, () => Arg != null ? Arg : DequeueNext());
					(Action as Action<string>)(Argument);
				};
			}
			else
			{
				throw (new Exception("Unknown Type : " + typeof(T).Name));
			}
			foreach (var Name in Names)
			{
				this.Actions.Add(Name, FormalAction);
			}
			//foreach ()
		}

		public void Process()
		{
			while (HasMore)
			{
				var CurrentRaw = DequeueNext();
				string Arg = null;
				var Current = CurrentRaw;

				int EqualsOffset = CurrentRaw.IndexOf('=');
				if (EqualsOffset >= 0)
				{
					Current = CurrentRaw.Substring(0, EqualsOffset);
					Arg = CurrentRaw.Substring(EqualsOffset + 1);
				}

				if (Current.Length > 0 && Current[0] == '/')
				{
					if (this.Actions.ContainsKey(Current))
					{
						this.Actions[Current](Current, Arg);
					}
					else
					{
						throw (new Exception("Unknown parameter '" + Current + "'"));
					}
				}
				else
				{
					this.Actions[""](Current, Arg);
				}
			}
		}
	}
}