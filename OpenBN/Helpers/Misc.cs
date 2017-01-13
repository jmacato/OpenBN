using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBN.Helpers
{
    static class Misc
    {

        public static Rectangle RectFromString(string str)
        {
            var spl = str.Trim().Split(',');
            var x = Convert.ToInt32(spl[0]);
            var y = Convert.ToInt32(spl[1]);
            var w = Convert.ToInt32(spl[2]);
            var h = Convert.ToInt32(spl[3]);
            return new Rectangle(x, y, w, h);
        }

    }
}
