using System;
using Microsoft.Xna.Framework;

namespace OpenBN
{
    public static class ColorHelper
    {
        public static Color FromHex(UInt32 color)
        {
            int r = (int)(color & 0xFF0000) >> 16;
            int g = (int)(color & 0xFF00) >> 8;
            int b = (int)(color & 0xFF);
            return Color.FromNonPremultiplied(r, g, b, 0xFF);
        }

        public static Color FromHex(UInt32 color, float alpha)
        {
            int r = (int)(color & 0xFF0000) >> 16;
            int g = (int)(color & 0x00FF00) >> 8;
            int b = (int)(color & 0x0000FF);
            int a = (int)(alpha * 0xFF);
            return Color.FromNonPremultiplied(r, g, b, a);
        }
    }
}
