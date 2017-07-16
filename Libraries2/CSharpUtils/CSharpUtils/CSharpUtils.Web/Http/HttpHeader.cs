using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CSharpUtils.Http
{
	public class HttpHeader
	{
		public readonly String NormalizedName;
		public readonly String Name;
		public readonly String Value;

		public HttpHeader(String Name, String Value)
		{
			this.Name = Name.Trim();
			this.Value = Value.Trim();
			this.NormalizedName = GetNormalizedName(Name);
		}

		public static String GetNormalizedName(String Name)
		{
			return Name.Trim().ToLower();
		}

		public Dictionary<String, String> ParseValue(String FirstKeyName = "Type")
		{
			var Values = new Dictionary<String, String>();
			var TwoParts=  Value.Split(new char[] { ';' }, 2);
			Values[FirstKeyName] = TwoParts[0];
			//Console.WriteLine(TwoParts[0]);
			if (TwoParts.Length == 2)
			{
				var ParseLeft = new Regex("\\s*(\\w+)=(\"|)([^\"]*)\\2(;|$)", RegexOptions.Compiled);
				foreach (Match Match in ParseLeft.Matches(TwoParts[1]))
				{
					string MatchKey = Match.Groups[1].Value;
					string MatchValue = Match.Groups[3].Value;
					Values[MatchKey.ToLower()] = MatchValue;
					//Console.WriteLine(Match.Groups[1]);
				}
			}

			return Values;
		}
	}
}
