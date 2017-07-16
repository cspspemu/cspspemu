using System.Collections.Generic;
using System.Linq;

namespace CSharpUtils.Process
{
	public abstract class ProcessBase : ProcessBaseCore
	{
		protected static ProcessBase CurrentExecutingProcess = null;

		public int Priority = 0;
		public float X = 0, Y = 0, Z = 0;
		public float Angle = 0, ScaleX = 1, ScaleY = 1;
		public float Scale = 1;
		public float Alpha = 1;

		protected ProcessBase _Parent = null;
		protected LinkedList<ProcessBase> Childs;

		private static LinkedList<ProcessBase> _AllProcesses = new LinkedList<ProcessBase>();

		public static LinkedList<ProcessBase> AllProcesses
		{
			get
			{
				return new LinkedList<ProcessBase>(_AllProcesses);
			}
		}

		public static void _RemoveOld()
		{
			foreach (var Process in AllProcesses)
			{
				if (Process.State == State.Ended) Process._Remove();
			}
		}

		private void ExecuteTreeBefore()
		{
			foreach (var process in Childs.Where(process => process.Priority < 0).OrderBy(process => process.Priority)) process.ExecuteTree();
		}

		private void ExecuteTreeAfter()
		{
			foreach (var process in Childs.Where(process => process.Priority >= 0).OrderBy(process => process.Priority)) process.ExecuteTree();
		}

		public void ExecuteTree()
		{
			if (State == State.Ended) return;

			CurrentExecutingProcess = this;

			this.ExecuteTreeBefore();
			//Console.WriteLine("<Execute " + this + ">");
			this.SwitchTo();
			//Console.WriteLine("</Execute " + this + ">");
			this.ExecuteTreeAfter();
		}

		/*
		private void DrawTreeBefore(object _Context)
		{
			foreach (var Process in Childs.Where(process => process.Z < 0).OrderBy(process => process.Z)) Process.DrawTree(_Context);
		}

		private void DrawTreeAfter(object _Context)
		{
			foreach (var process in Childs.Where(process => process.Z >= 0).OrderBy(process => process.Z)) process.DrawTree(_Context);
		}
		*/

		public virtual void DrawTree(object _Context, int Level = 0)
		{
			foreach (var Item in Childs.Concat(new ProcessBase[] { this }).OrderBy(process => process.Z))
			{
				//Console.WriteLine("DrawItem: " + Item + " : " + Item.Z);
				if (Item == this)
				{
					Item.DrawItem(_Context);
				}
				else
				{
					Item.DrawTree(_Context, Level + 1);
				}
			}
			//this.DrawTreeBefore(_Context);
			//this.DrawItem(_Context);
			//this.DrawTreeAfter(_Context);
		}

		protected virtual void DrawItem(object _Context)
		{
			//Console.WriteLine(this);
		}

		public ProcessBase Parent
		{
			get
			{
				return _Parent;
			}
			set
			{
				if (_Parent != null)
				{
					_Parent.Childs.Remove(this);
				}
				{
					_Parent = value;
				}
				if (_Parent != null)
				{
					_Parent.Childs.AddLast(this);
				}
			}
		}

		public ProcessBase() : base()
		{
			_AllProcesses.AddLast(this);
			Childs = new LinkedList<ProcessBase>();
			Parent = CurrentExecutingProcess;
		}

		~ProcessBase()
		{
			_Remove();
		}

		protected override void _Remove()
		{
			Parent = null;
			_AllProcesses.Remove(this);
			base._Remove();
		}
	}
}
