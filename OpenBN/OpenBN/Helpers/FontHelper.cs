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
    public class FontHelper
    {
       public Dictionary<string, SpriteFont> List { get; set; }

        public FontHelper(ContentManager Content)
        {
            List = new Dictionary<string, SpriteFont>();
            DirectoryInfo dir = new DirectoryInfo(Content.RootDirectory + "/Fonts");
            FileInfo[] files = dir.GetFiles("*.*");
            foreach (FileInfo file in files)
            {
                var key = file.Name.Split('.')[0];
                var val = Content.Load<SpriteFont>("Fonts/" + key);
                val.Spacing = 1;
                List.Add(key, val);
            }
        }

    }
}
