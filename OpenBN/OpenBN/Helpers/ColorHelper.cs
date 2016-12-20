using System;
using System.Threading;
using System.Net.Sockets;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;


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
