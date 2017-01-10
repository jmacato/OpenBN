using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenBN
{

    class Navi 
    {
        public GraphicsDevice Graphics { get; set; }
        public ContentManager Content { get; set; }

        //Position on battlefield
        public int btlrow { get; set; }
        public int btlcol { get; set; }
        
        public string navicode { get; set; }

        public SpriteBatch SB { get; set; }
        public bool Initialized { get; set; }

        public void Update()
        {

        }

        public void Initialize()
        {

        }

        public void Next()
        {

        }


        public Navi(string nv)
        {

        }

        public void Draw()
        {

        }
    }

}
