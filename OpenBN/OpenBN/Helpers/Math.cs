using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBN
{
    public static class MyMath
    {
        public enum ClampStatus
        {
            None,
            Min,
            Max
        }

        public static T ClampR<T>(T value, T min, T max, ref ClampStatus ClampStat) where T : System.IComparable<T>
        {
            T result = value;
            ClampStat = ClampStatus.None;
            if (value.CompareTo(max) > 0)
            {
                result = max;
                ClampStat = ClampStatus.Max;
            }
            if (value.CompareTo(min) < 0)
            {
                result = min;
                ClampStat = ClampStatus.Min;
            }
            return result;
        }

        public static T Clamp<T>(T value, T min, T max) where T : System.IComparable<T>
        {
            T result = value;
            if (value.CompareTo(max) > 0)
            {
                result = max;
            }
            if (value.CompareTo(min) < 0)
            {
                result = min;
            }
            return result;
        }

    }
}
