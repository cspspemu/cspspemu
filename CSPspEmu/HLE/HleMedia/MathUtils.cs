using System.Runtime;

namespace cscodec
{
    public class MathUtils
    {
        
        
        public static int Clamp(int Value, int Min, int Max)
        {
            if (Value < Min) return Min;
            if (Value > Max) return Max;
            return Value;
        }
    }
}