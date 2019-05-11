using System.Text;
using System.Text.RegularExpressions;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    public static class ConvertEx
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="String"></param>
        /// <returns></returns>
        public static int FlexibleToInt(string String)
        {
            var regex = new Regex(@"^\d*", RegexOptions.Compiled);
            var selected = regex.Match(String).Groups[0].Value;
            int value;
            int.TryParse(selected, out value);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string GetString(this byte[] bytes, Encoding encoding) => encoding.GetString(bytes);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string GetString(this byte[] bytes) => bytes.GetString(Encoding.ASCII);
    }
}