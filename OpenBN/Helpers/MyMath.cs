namespace OpenBN.Helpers
{
    public static class MyMath
    {
        public enum ClampStatus
        {
            None,
            Min,
            Max
        }

        public static T ClampR<T>(T value, T min, T max, ref ClampStatus clampStat) where T : System.IComparable<T>
        {
            var result = value;
            clampStat = ClampStatus.None;
            if (value.CompareTo(max) > 0)
            {
                result = max;
                clampStat = ClampStatus.Max;
            }
            if (value.CompareTo(min) < 0)
            {
                result = min;
                clampStat = ClampStatus.Min;
            }
            return result;
        }

        public static T Clamp<T>(T value, T min, T max) where T : System.IComparable<T>
        {
            var result = value;
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

        public static T InverseClamp<T>(T value, T min, T max) where T : System.IComparable<T>
        {
            var result = value;
            if (value.CompareTo(max) > 0)
            {
                result = min;
            }
            if (value.CompareTo(min) < 0)
            {
                result = max;
            }
            return result;
        }

        public static float Map(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

    }
}
