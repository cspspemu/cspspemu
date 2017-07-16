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

		static public string MatchFormatRegex(string format)
		{
			switch (format)
			{
				case "%vr": return @"\[[c|s|\s|0|\-|,]*\]";
				case "%vi": case "%i": return @"[+\-]?[\w_]+";
				case "%Y": return @"(?:\w+\+)?\w+";
				case "%zp": case "%yp": case "%xp":
				case "%zt": case "%yt": case "%xt":
				case "%zq": case "%yq": case "%xq":
				case "%zm": case "%ym": case "%xm":
				case "%tym": 
				case "%yn":
				case "%Xq":
					return @"[SRCME][0-8][0-4][0-4](?:\.[sptq])?";
				case "%vp4": case "%vp5": case "%vp6": case "%vp7": return @"(?:0:1|-1:1|M|m)";
				case "%vp0": case "%vp1": case "%vp2": case "%vp3": return @"\|?\-?[xyzw\d/]+\|?";
				default: return @"\w+";
			}
		}

		static private MatcherResult _MatcherToRegexUncached(string pattern)
		{
			var regex1 = new Regex(@"(\s+|%\w+)", RegexOptions.Compiled | RegexOptions.ECMAScript);
			var groupNames = new List<string>();
			pattern = Regex.Escape(pattern);
			pattern = pattern.Replace(@"\ ", " ");
			pattern = regex1.Replace(pattern, (match) =>
			{
				if (match.Value[0] == '%')
				{
					groupNames.Add(match.Value);
					return @"\s*(" + MatchFormatRegex(match.Value) + @")\s*";
				}
				else
				{
					return @"\s*";
				}
			});
			pattern = @"^\s*" + pattern + @"\s*$";
			return new MatcherResult()
			{
				Regex = new Regex(pattern),
				RegexString = pattern,
				GroupNames = groupNames.ToArray(),
			};
		}

		static public MatcherResult MatcherToRegex(string pattern)
		{
			if (!MatcherToRegexCache.ContainsKey(pattern))
			{
				MatcherToRegexCache[pattern] = _MatcherToRegexUncached(pattern);
			}
			return MatcherToRegexCache[pattern];
		}

		static Dictionary<string, MatcherResult> MatcherToRegexCache = new Dictionary<string, MatcherResult>();

		static public SortedDictionary<string, string> Matcher(string pattern, string text)
		{
			var info = MatcherToRegex(pattern);
			var match = info.Regex.Match(text);
			if (match == Match.Empty) throw(new Exception($"Pattern '{pattern}';'{info.RegexString}' doesn't match '{text}'"));
			var output = new SortedDictionary<string, string>();
			for (var n = 0; n < info.GroupNames.Length; n++)
			{
				output.Add(info.GroupNames[n], match.Groups[n + 1].Value);
				//Console.WriteLine("{0} -> {1}", Info.GroupNames[n], Match.Groups[n + 1].Value);
			}
			return output;
		}
	}
}
