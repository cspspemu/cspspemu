using System;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    public static class NumberUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultBase"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static int ParseIntegerConstant(string value, int defaultBase = 10)
        {
            try
            {
                value = value.Replace("_", "");
                if (value.Substr(0, 1) == "-") return -ParseIntegerConstant(value.Substr(1));
                if (value.Substr(0, 1) == "+") return +ParseIntegerConstant(value.Substr(1));
                if (value.Substr(0, 2) == "0x") return Convert.ToInt32(value.Substr(2), 16);
                if (value.Substr(0, 2) == "0b") return Convert.ToInt32(value.Substr(2), 2);
                return Convert.ToInt32(value, defaultBase);
            }
            catch (FormatException formatException)
            {
                throw new FormatException("Can't parse the string '" + value + "'", formatException);
            }
        }
    }
}