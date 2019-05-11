using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CSharpUtils.Extensions;

namespace CSharpUtils.Misc.Acme1
{
    /// <summary>
    /// 
    /// </summary>
    public class Acme1File : IEnumerable<Acme1File.Entry>
    {
        /// <summary>
        /// 
        /// </summary>
        public class Entry
        {
            /// <summary>
            /// 
            /// </summary>
            public int Id;

            /// <summary>
            /// 
            /// </summary>
            public string Text;
        }

        readonly Dictionary<int, Entry> _entries = new Dictionary<int, Entry>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        public void Load(Stream stream, Encoding encoding)
        {
            _entries.Clear();
            var allContent = stream.ReadAllContentsAsString(encoding).TrimStart();
            var parts = allContent.Split(new[] {"## POINTER "}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                //Console.WriteLine(Part.EscapeString());

                var subparts = part.Split(new[] {"\r\n", "\r", "\n"}, 2, StringSplitOptions.None);
                var infoMatch = Regex.Match(subparts[0], @"(\d+).*$", RegexOptions.Compiled | RegexOptions.Multiline);
                var text = subparts[1].TrimEnd();
                var id = ConvertEx.FlexibleToInt(infoMatch.Groups[1].Value);

                _entries[id] = new Entry()
                {
                    Id = id,
                    Text = text,
                };
                //Console.WriteLine(Subparts.ToStringArray().EscapeString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public Entry this[int index] => _entries[index];

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Entry> GetEnumerator() => _entries.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _entries.Values.GetEnumerator();
    }
}