using System;
using Microsoft.Xna.Framework;

namespace OpenBN.Helpers
{
    public static class ColorHelper
    {
        public static Color FromHex(UInt32 color)
        {
            var r = (int)(color & 0xFF0000) >> 16;
            var g = (int)(color & 0xFF00) >> 8;
            var b = (int)(color & 0xFF);
            return Color.FromNonPremultiplied(r, g, b, 0xFF);
        }

        public static Color FromHex(UInt32 color, float alpha)
        {
            var r = (int)(color & 0xFF0000) >> 16;
            var g = (int)(color & 0x00FF00) >> 8;
            var b = (int)(color & 0x0000FF);
            var a = (int)(alpha * 0xFF);
            return Color.FromNonPremultiplied(r, g, b, a);
        }
    }
}
