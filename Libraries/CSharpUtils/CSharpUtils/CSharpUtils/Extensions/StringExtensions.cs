using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CSharpUtils;

public static class StringExtensions
{
    public static string JoinToString<T>(this IEnumerable<T> str, string separator) => String.Join(separator, str);

    public static string ToCamelCase(this string str)
    {
        return str.Substr(0, 1).ToUpper() + str.Substr(1).ToLower();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="String"></param>
    /// <param name="RegexString"></param>
    /// <param name="ActionMatch"></param>
    /// <returns></returns>
    public static string RegexReplace(this string String, string RegexString, Func<GroupCollection, string> ActionMatch)
    {
        var Regex = new Regex(RegexString, RegexOptions.None);
        return Regex.Replace(String, Match => { return ActionMatch(Match.Groups); });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="String"></param>
    /// <param name="RegexString"></param>
    /// <returns></returns>
    public static bool RegexIsMatch(this string String, string RegexString)
    {
        var Regex = new Regex(RegexString, RegexOptions.None);
        return Regex.IsMatch(String);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="String"></param>
    /// <param name="RegexString"></param>
    /// <returns></returns>
    public static MatchCollection RegexMatchAll(this string String, string RegexString)
    {
        var Regex = new Regex(RegexString, RegexOptions.None);
        return Regex.Matches(String);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="String"></param>
    /// <param name="RegexString"></param>
    /// <returns></returns>
    public static GroupCollection RegexMatch(this string String, string RegexString)
    {
        var Regex = new Regex(RegexString, RegexOptions.None);
        var Match = Regex.Match(String);
        if (Match == Match.Empty) return null;
        return Match.Groups;
    }

    public static string Substr(this string String, int StartIndex)
    {
        if (StartIndex < 0) StartIndex = String.Length + StartIndex;
        StartIndex = MathUtils.Clamp(StartIndex, 0, String.Length);
        return String.Substring(StartIndex);
    }

    public static string Substr(this string String, int StartIndex, int Length)
    {
        if (StartIndex < 0) StartIndex = String.Length + StartIndex;
        StartIndex = MathUtils.Clamp(StartIndex, 0, String.Length);
        var End = StartIndex + Length;
        if (Length < 0) Length = String.Length + Length - StartIndex;
        Length = MathUtils.Clamp(Length, 0, String.Length - StartIndex);
        return String.Substring(StartIndex, Length);
    }

    public static byte[] GetStringzBytes(this string String, Encoding Encoding)
    {
        return String.GetBytes(Encoding).Concat(new byte[] {0}).ToArray();
    }

    public static byte[] GetBytes(this string This)
    {
        return This.GetBytes(Encoding.UTF8);
    }

    public static byte[] GetBytes(this string This, Encoding Encoding)
    {
        return Encoding.GetBytes(This);
    }

    public static string EscapeString(this string This)
    {
        var That = "";
        foreach (var C in This)
        {
            switch (C)
            {
                case '\"':
                    That += @"\""";
                    break;
                case '\'':
                    That += @"\'";
                    break;
                case '\n':
                    That += @"\n";
                    break;
                case '\r':
                    That += @"\r";
                    break;
                case '\t':
                    That += @"\t";
                    break;
                default:
                    That += C;
                    break;
            }
        }
        return That;
    }

    public static long GetLong(object Value)
    {
        return Convert.ToInt64(Value);
    }

    public static ulong GetUnsignedLong(object Value)
    {
        return Convert.ToUInt64(Value);
    }

    public static long ConvertToLong(object Value)
    {
        if (Value is int) return (uint) Convert.ToInt32(Value);
        if (Value is uint) return Convert.ToUInt32(Value);
        try
        {
            return Convert.ToInt64(Value);
        }
        catch
        {
            try
            {
                return (long) Convert.ToUInt64(Value);
            }
            catch
            {
                return -1;
            }
        }
    }

    public static string Sprintf(this string This, params object[] _Params)
    {
        string Ret = "";
        var Params = new Queue<object>(_Params);
        for (int n = 0; n < This.Length; n++)
        {
            char C = This[n];
            if (C == '%')
            {
                C = This[++n];
                if (C == '%')
                {
                    Ret += "%";
                }
                else
                {
                    int PadDir = +1;
                    char PadChar = ' ';
                    int Pad = 0;
                    var Param = Params.Dequeue();
                    var Result = "";
                    //long IntegerParam = ConvertToLong(Param);
                    //if (Pad <= 8) IntegerParam = (long)(uint)IntegerParam;
                    //if (Pad <= 4) IntegerParam = (long)(ushort)IntegerParam;
                    for (; n < This.Length; n++)
                    {
                        C = This[n];
                        switch (C)
                        {
                            case '-':
                                PadDir = -1;
                                break;
                            /*
                            case 'c': arg = String.fromCharCode(arg); goto EndParamLabel;
                            case 'd': arg = parseInt(arg, 10); goto EndParamLabel;
                            case 'e': arg = match[7] ? arg.toExponential(match[7]) : arg.toExponential(); goto EndParamLabel;
                            case 'f': arg = match[7] ? parseFloat(arg).toFixed(match[7]) : parseFloat(arg); goto EndParamLabel;
                            case 'o': arg = arg.toString(8); goto EndParamLabel;
                            case 's': arg = ((arg = String(arg)) && match[7] ? arg.substring(0, match[7]) : arg); goto EndParamLabel;
                            case 'u': arg = Math.abs(arg); goto EndParamLabel;
                            */
                            case 's':
                                Result = Convert.ToString(Param);
                                break;
                            case 'b':
                                Result = Convert.ToString(ConvertToLong(Param), 2);
                                break;
                            case 'd':
                                Result = Convert.ToString(ConvertToLong(Param), 10);
                                break;
                            case 'X':
                            case 'x':
                                Result = Convert.ToString(ConvertToLong(Param), 16);
                                if (C == 'X')
                                {
                                    Result = Result.ToUpper();
                                }
                                break;
                            default:
                                if (C >= '0' && C <= '9')
                                {
                                    if (C == '0' && Pad == 0)
                                    {
                                        PadChar = '0';
                                    }
                                    else
                                    {
                                        Pad *= 10;
                                        Pad += C - '0';
                                    }
                                }
                                break;
                        }
                        if (Result.Length > 0) break;
                    }

                    if (PadDir > 0)
                    {
                        Result = Result.PadLeft(Pad, PadChar);
                    }
                    else
                    {
                        Result = Result.PadRight(Pad, PadChar);
                    }
                    //Console.WriteLine(Pad);
                    //Console.WriteLine(PadChar);
                    //Console.WriteLine("{[0}']", Result);
                    Ret += Result;
                }
            }
            else
            {
                Ret += C;
            }
        }
        return Ret;
    }
}