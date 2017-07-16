using System;
namespace CSharpUtils
{
	public class LanguageUtils
	{
		/// <summary>
		/// Swaps the value of two references.
		/// </summary>
		/// <typeparam name="TType"></typeparam>
		/// <param name="Left"></param>
		/// <param name="Right"></param>
		public static void Swap<TType>(ref TType Left, ref TType Right)
		{
			TType Temp = Left;
			Left = Right;
			Right = Temp;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TType"></typeparam>
		/// <param name="Left"></param>
		/// <param name="Right"></param>
		/// <param name="CopyToLeft"></param>
		public static void Transfer<TType>(ref TType Left, ref TType Right, bool CopyToLeft)
		{
			if (CopyToLeft)
			{
				Left = Right;
			}
			else
			{
				Right = Left;
			}
		}

		/// <summary>
		/// Changes the value of a reference just while the execution of the LocalScope delegate.
		/// </summary>
		/// <typeparam name="TType"></typeparam>
		/// <param name="Variable"></param>
		/// <param name="LocalValue"></param>
		/// <param name="LocalScope"></param>
		public static void LocalSet<TType>(ref TType Variable, TType LocalValue, Action LocalScope)
		{
			var OldValue = Variable;
			Variable = LocalValue;
			try
			{
				LocalScope();
			}
			finally
			{
				Variable = OldValue;
			}
		}

		public static void PropertyLocalSet(object Object, string PropertyName, object LocalValue, Action LocalScope)
		{
			var Property = Object.GetType().GetProperty(PropertyName);
			var OldValue = Property.GetValue(Object);
			Property.SetValue(Object, LocalValue);
			try
			{
				LocalScope();
			}
			finally
			{
				Property.SetValue(Object, OldValue);
			}
		}
	}
}
