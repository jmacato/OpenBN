using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBN
{
    class CustomWindow : IBattleEntity
    {

        public string ID { get; set; }
        public SpriteBatch SB { get; set; }
        public ContentManager CM { get; set; }

        private Texture2D customtextures;
        private Dictionary<string, Rectangle> CustSrcRects;
        private Dictionary<string, Rectangle> CustTextures = new Dictionary<string, Rectangle>();

        public bool showCust { get; private set; }
        private Vector2 custPos = new Vector2(-120,0);

        public CustomWindow(ContentManager x)
        {
            CM = x;
            CustSrcRects = new Dictionary<string, Rectangle>()

                {
                    {"CustWind"     , new Rectangle(0, 0, 120, 160)},
                    {"HPBAR"        , new Rectangle(0, 160, 44, 16)},
                    {"ChipSt1"      , new Rectangle(120, 6, 22, 17)},
                    {"ChipSel1"     , new Rectangle(120, 0, 6, 6)},
                    {"ChipSel2"     , new Rectangle(127, 0, 6, 6)},
                };

            customtextures = CM.Load<Texture2D>("Misc/CustomWindow");
            showCust = false;
        }

        public void Show()
        {
            showCust = true;
        }

        public void Hide()
        {
            showCust = false;
        }

        public void Update()
        {
            if (showCust)
            {
                custPos.X = MathHelper.Clamp(custPos.X+10,-120,0);
            } else
            {
                custPos.X = MathHelper.Clamp(custPos.X-10,-120, 0);
            }
            //  if (xx == -1 | xx == 1) xx = 0;
        }
        public void Draw()
        {
            if (customtextures != null)
            {
                var u = CustSrcRects["CustWind"];
                var y = new Rectangle((int)custPos.X, 0, u.Width, u.Height);
                SB.Draw(customtextures, y, u, Color.White);

                var hp = CustSrcRects["HPBAR"];
                var hprct = new Rectangle((int)custPos.X + 122, 2, hp.Width, hp.Height);
                SB.Draw(customtextures, hprct, hp, Color.White);

            }
        }
    }
}
