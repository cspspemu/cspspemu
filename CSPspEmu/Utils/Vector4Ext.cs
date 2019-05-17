using System.Numerics;

namespace CSPspEmu.Utils
{
    public static class Vector4Ext
    {
        public static float Get(this Vector4 that, int index)
        {
            switch (index)
            {
                case 0: return that.X;
                case 1: return that.Y;
                case 2: return that.Z;
                case 3: return that.W;
                default: return 0f;
            }
        }
        
        public static void Set(this Vector4 that, int index, float value)
        {
            switch (index)
            {
                case 0:
                    that.X = value;
                    break;
                case 1:
                    that.Y = value;
                    break;
                case 2:
                    that.Z = value;
                    break;
                case 3:
                    that.W = value;
                    break;
            }
        }
    }
}