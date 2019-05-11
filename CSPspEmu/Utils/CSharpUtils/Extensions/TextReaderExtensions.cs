using System.IO;

namespace CSharpUtils.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class TextReaderExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="textReader"></param>
        /// <returns></returns>
        public static bool HasMore(this TextReader textReader)
        {
            return textReader.Peek() >= 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textReader"></param>
        /// <returns></returns>
        public static char ReadChar(this TextReader textReader)
        {
            return (char) textReader.Read();
        }
    }
}