using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpUtils
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <see cref="Lazy&lt;T&gt;"/>
	public class LazyHolder<T>
	{
		public Func<T> Getter;
		protected bool _Setted = false;
		protected T _Value;
		public T Value
		{
			get
			{
				if (!_Setted)
				{
					_Value = Getter();
					_Setted = true;
				}
				return _Value;
			}
		}

		public void Invalidate()
		{
			_Setted = false;
			//_Value = default(T);
		}

		public LazyHolder(Func<T> Getter)
		{
			this.Getter = Getter;
		}

		static public LazyHolder<T> Create(Func<T> Getter)
		{
			return new LazyHolder<T>(Getter);
		}

		public static implicit operator T(LazyHolder<T> LazyHolder) 
		{
			return LazyHolder.Value;
		}
	}
}
