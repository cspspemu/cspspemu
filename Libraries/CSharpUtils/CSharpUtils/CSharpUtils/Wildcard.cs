using System;
using System.Text.RegularExpressions;

/**
 * @reinux
 * http://www.codeproject.com/KB/recipes/wildcardtoregex.aspx
 */

namespace CSharpUtils
{
	/// <summary>
	/// Represents a wildcard running on the
	/// <see cref="System.Text.RegularExpressions"/> engine.
	/// </summary>
	public class Wildcard : Regex
	{
		string wildcardPattern;

		/// <summary>
		/// Initializes a wildcard with the given search pattern.
		/// </summary>
		/// <param name="pattern">The wildcard pattern to match.</param>

		public Wildcard(string pattern)
			: base(WildcardToRegex(pattern))
		{
			this.wildcardPattern = pattern;
		}

		/// <summary>
		/// Initializes a wildcard with the given search pattern and options.
		/// </summary>
		/// <param name="pattern">The wildcard pattern to match.</param>
		/// <param name="options">A combination of one or more
		/// <see cref="RegexOptions"/>.</param>
		public Wildcard(string pattern, RegexOptions options)
			: base(WildcardToRegex(pattern), options)
		{
			this.wildcardPattern = pattern;
		}

		/// <summary>
		/// Converts a wildcard to a regex.
		/// </summary>
		/// <param name="pattern">The wildcard pattern to convert.</param>
		/// <returns>A regex equivalent of the given wildcard.</returns>
		public static string WildcardToRegex(string pattern)
		{
			return "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
		}

		public static implicit operator Wildcard(String Input)
		{
			return new Wildcard(Input);
		}

		static public implicit operator String(Wildcard Input)
		{
			return Input.wildcardPattern;
		}
	}
}
