using System;

namespace CSharpUtils.Threading
{
    /// <summary>
    /// 
    /// </summary>
    public class GreenThreadException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="innerException"></param>
        public GreenThreadException(string name, Exception innerException) : base(name, innerException)
        {
        }
    }
}