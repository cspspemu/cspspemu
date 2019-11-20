using System;

namespace cscodec.util
{
    public sealed class HashUtil
    {
        /**
        * An initial value for a <code>hashCode</code>, to which is added contributions
        * from fields. Using a non-zero value decreases collisons of <code>hashCode</code>
        * values.
        */
        public const int SEED = 23;

        /**
        * booleans.
        */
        public static int hash(int aSeed, bool aBoolean)
        {
            return firstTerm(aSeed) + (aBoolean ? 1 : 0);
        }

        /**
        * chars.
        */
        public static int hash(int aSeed, char aChar)
        {
            return firstTerm(aSeed) + (int) aChar;
        }

        /**
        * ints.
        */
        public static int hash(int aSeed, int aInt)
        {
            /*
            * Implementation Note
            * Note that byte and short are handled by this method, through
            * implicit conversion.
            */
            return firstTerm(aSeed) + aInt;
        }

        /**
        * longs.
        */
        public static int hash(int aSeed, long aLong)
        {
            return firstTerm(aSeed) + (int) (aLong ^ (long) ((ulong) aLong >> 32));
        }

        /**
        * floats.
        */
        public static int hash(int aSeed, float aFloat)
        {
            return hash(aSeed, BitConverter.ToInt32(BitConverter.GetBytes(aFloat), 0));
        }

        /**
        * doubles.
        */
        public static int hash(int aSeed, double aDouble)
        {
            return hash(aSeed, BitConverter.DoubleToInt64Bits(aDouble));
        }

        /**
        * <code>aObject</code> is a possibly-null object field, and possibly an array.
        *
        * If <code>aObject</code> is an array, then each element may be a primitive
        * or a possibly-null object.
        */
        public static int hash(int aSeed, object aObject)
        {
            int result = aSeed;
            if (aObject == null)
            {
                result = hash(result, 0);
            }
            else if (!isArray(aObject))
            {
                result = hash(result, aObject.GetHashCode());
            }
            else
            {
                object[] objAr = (object[]) aObject;
                int length = objAr.Length;
                for (int idx = 0; idx < length; ++idx)
                {
                    object item = objAr[idx];
                    //recursive call!
                    result = hash(result, item);
                }
            }
            return result;
        }


        /// PRIVATE ///
        private const int fODD_PRIME_NUMBER = 37;

        private static int firstTerm(int aSeed)
        {
            return fODD_PRIME_NUMBER * aSeed;
        }

        private static bool isArray(object aObject)
        {
            return aObject.GetType().IsArray;
        }
    }
}