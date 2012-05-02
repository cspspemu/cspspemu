using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpUtils.Factory
{
	public class Factory
	{
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		virtual public T New<T>() where T : new()
		{
			return new T();
		}
	}
}
