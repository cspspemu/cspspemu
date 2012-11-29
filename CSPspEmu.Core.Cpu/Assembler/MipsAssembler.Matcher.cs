using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CSPspEmu.Core.Cpu.Assembler
{
	public unsafe partial class MipsAssembler
	{
		public class MatcherResult 
		{
			public Regex Regex;
			public string RegexString;
			public string[] GroupNames;
		}

		static public string MatchFormatRegex(string Format)
		{
			switch (Format)
			{
				case "%vr": return @"\[[c|s|\s|0|\-|,]*\]";
				case "%vi": case "%i": return @"[+\-]?[\w_]+";
				case "%Y": return @"(?:\w+\+)?\w+";
				case "%vp4": case "%vp5": case "%vp6": case "%vp7": return @"(?:0:1|-1:1|M)";
				case "%vp0": case "%vp1": case "%vp2": case "%vp3": return @"\|?\-?[xyzw\d/]+\|?";
				default: return @"\w+";
			}
		}

		static private MatcherResult _MatcherToRegexUncached(string Pattern)
		{
			Regex Regex1 = new Regex(@"(\s+|%\w+)", RegexOptions.Compiled | RegexOptions.ECMAScript);
			var GroupNames = new List<string>();
			Pattern = Regex.Escape(Pattern);
			Pattern = Pattern.Replace(@"\ ", " ");
			Pattern = Regex1.Replace(Pattern, (Match) =>
			{
				if (Match.Value[0] == '%')
				{
					GroupNames.Add(Match.Value);
					return @"\s*(" + MatchFormatRegex(Match.Value) + @")\s*";
				}
				else
				{
					return @"\s*";
				}
			});
			Pattern = @"^\s*" + Pattern + @"\s*$";
			return new MatcherResult()
			{
				Regex = new Regex(Pattern),
				RegexString = Pattern,
				GroupNames = GroupNames.ToArray(),
			};
		}

		static public MatcherResult MatcherToRegex(string Pattern)
		{
			if (!MatcherToRegexCache.ContainsKey(Pattern))
			{
				MatcherToRegexCache[Pattern] = _MatcherToRegexUncached(Pattern);
			}
			return MatcherToRegexCache[Pattern];
		}

		static Dictionary<string, MatcherResult> MatcherToRegexCache = new Dictionary<string, MatcherResult>();

		static public SortedDictionary<string, string> Matcher(string Pattern, string Text)
		{
			var Info = MatcherToRegex(Pattern);
			var Match = Info.Regex.Match(Text);
			if (Match == Match.Empty) throw(new Exception(String.Format("Pattern '{0}';'{1}' doesn't match '{2}'", Pattern, Info.RegexString, Text)));
			var Output = new SortedDictionary<string, string>();
			for (int n = 0; n < Info.GroupNames.Length; n++)
			{
				Output.Add(Info.GroupNames[n], Match.Groups[n + 1].Value);
				//Console.WriteLine("{0} -> {1}", Info.GroupNames[n], Match.Groups[n + 1].Value);
			}
			return Output;
		}
	}
}
