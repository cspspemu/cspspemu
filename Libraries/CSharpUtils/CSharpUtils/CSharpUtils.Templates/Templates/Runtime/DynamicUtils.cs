using System;
using System.Collections.Generic;

namespace CSharpUtils.Templates.Runtime
{
	/// <summary>
	/// Class to perform operations on dynamic types. Without worrying about exceptions or errors.
	/// </summary>
	public class DynamicUtils
	{
		public static bool ConvertToBool(dynamic Value)
		{
			try
			{
				if (Value == null) return false;
				if (Value is double) return Value != 0.0;
				if (Value is float) return Value != 0.0;
				if (Value is bool) return Value;
				if (Value is int) return Value != 0;
				if (Value is Int64) return Value != 0;
				if (Value is object) return true;
				return (bool)Value;
			}
			catch (Exception)
			{
				//Console.WriteLine("VALUE: {0}", Value);
				//Console.Error.WriteLine(Exception);
				return false;
			}
		}

		public static dynamic Access(dynamic _Value, dynamic _Key)
		{
			try
			{
				if (_Value == null) return null;

				String Key = (String)_Key;
				Object Value = (Object)_Value;
				Type ValueType = Value.GetType();

				var MethodKey = ValueType.GetMethod(Key);
				if (MethodKey != null)
				{
					return MethodKey.Invoke(Value, new object[] { });
				}

				var FieldKey = ValueType.GetField(Key);
				if (FieldKey != null)
				{
					return FieldKey.GetValue(Value);
				}

				var PropertyKey = ValueType.GetProperty(Key);
				if (PropertyKey != null)
				{
					return PropertyKey.GetValue(Value, new object[] { });
				}
			}
			catch (Exception)
			{
			}

			try
			{
				return _Value[_Key];
			}
			//catch (Exception Exception)
			catch (Exception)
			{
				//Console.WriteLine(Exception);
				return null;
			}
		}

		public static dynamic BinaryOperation_Add(dynamic Left, dynamic Right)
		{
			if (Left == null && Right == null) return null;
			return Left + Right;
		}

		public static dynamic BinaryOperation_Sub(dynamic Left, dynamic Right)
		{
			return Left - Right;
		}

		public static dynamic BinaryOperation_Mul(dynamic Left, dynamic Right)
		{
			return Left * Right;
		}

		public static dynamic BinaryOperation_Div(dynamic Left, dynamic Right)
		{
			return Left / Right;
		}

		public static int BinaryOperation_DivInt(dynamic Left, dynamic Right)
		{
			return (int)(Left / Right);
		}

		public static dynamic BinaryOperation_Mod(dynamic Left, dynamic Right)
		{
			return Left % Right;
		}

		public static bool BinaryOperation_AndBool(dynamic Left, dynamic Right)
		{
			return ConvertToBool(Left) && ConvertToBool(Right);
		}

		public static bool BinaryOperation_OrBool(dynamic Left, dynamic Right)
		{
			return ConvertToBool(Left) || ConvertToBool(Right);
		}

		public static double BinaryOperation_Pow(dynamic Left, dynamic Right)
		{
			return Math.Pow(Left, Right);
		}

		public static dynamic BinaryOperation_Range(dynamic Start, dynamic End)
		{
			var List = new List<dynamic>();
			for (dynamic N = Start; N <= End; N++)
			{
				List.Add(N);
			}
			return List;
		}


		public static String GetOpName(string Operation)
		{
			switch (Operation)
			{
				case "+": return "Add";
				case "-": return "Sub";
				case "*": return "Mul";
				case "/": return "Div";
				case "%": return "Mod";
				case "&&": return "AndBool";
				case "||": return "OrBool";
				case "//": return "DivInt";
				case "**": return "Pow";
				case "..": return "Range";
				default: throw(new Exception(String.Format("Unknown operator '{0}'", Operation)));
			}
		}

		public static int CountArray(dynamic Value)
		{
			if (Value == null) return 0;
			if (Value is String) return 0;
			//Console.WriteLine(Value);
			Type ValueType = Value.GetType();
			try
			{
				return Value.Length;
			}
			catch
			{
				try
				{
					return ValueType.GetProperty("Count").GetValue(Value, null);
				}
				catch
				{
					try
					{
						return Value.Count();
					}
					catch
					{
						return 0;
					}
				}
			}
		}

		public static dynamic ConvertToIEnumerable(dynamic List)
		{
			if (List != null)
			{
				try
				{
					var OutputList = new List<dynamic>();

					foreach (var Item in List)
					{
						if (Item.GetType().Name == typeof(KeyValuePair<dynamic, dynamic>).Name)
						{
							//Console.WriteLine(Item.GetType().Name);

							OutputList.Add(Item.Value);
							//OutputList.Add(Item);
							continue;
						}

						OutputList.Add(Item);
					}

					return OutputList;
				}
				catch
				{
				}
			}

			return new object[] { };
		}

		public static String ConvertToString(dynamic Value)
		{
			return String.Format("{0}", Value);
		}

		//internal static dynamic Call(Delegate Delegate, params dynamic[] Params)
		public static dynamic Call(Type Type, string Method, params dynamic[] Params)
		{
			/*
			var ConvertedParams = new List<Object>();
			int ExtractedParamsIndex = 0;

			foreach (var Parameter in Delegate.Method.GetParameters())
			{
				if (Parameter.ParameterType.Name.Substr(-2) == "[]")
				{
					var ExtractedParams = Params.Slice(ExtractedParamsIndex);
					ConvertedParams.Add(ExtractedParams);
					Console.WriteLine(ExtractedParams.Length);
					Console.WriteLine(ExtractedParams[0]);
				}
				else
				{
					ConvertedParams.Add(Params[ExtractedParamsIndex++]);
				}
			}
			foreach (var Param in ConvertedParams)
			{
				Console.WriteLine(Param);
			}
			return Delegate.DynamicInvoke(ConvertedParams);
			 * */

			return Type.InvokeMember(Method, System.Reflection.BindingFlags.InvokeMethod, null, null, Params);

			//return null;
		}
	}
}
