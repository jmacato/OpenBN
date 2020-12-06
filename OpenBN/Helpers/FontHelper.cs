using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace OpenBN.Helpers
{
    public class FontHelper
    {
       public Dictionary<string, SpriteFont> List { get; private set; }

        public FontHelper(ContentManager Content)
        {
            List = new Dictionary<string, SpriteFont>();
            var dir = new DirectoryInfo(Content.RootDirectory + "/Fonts");
            var files = dir.GetFiles("*.*");
            foreach (var file in files)
            {
                var key = file.Name.Split('.')[0];
                var val = Content.Load<SpriteFont>("Fonts/" + key);
                val.Spacing = 1;
                List.Add(key, val);
            }
        }
    }
}
