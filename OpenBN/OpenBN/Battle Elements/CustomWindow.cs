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
        public int CurrentHP { get; set; }
        public int MaxHP { get; set; }

        Texture2D customtextures;
        Dictionary<string, Rectangle> CustSrcRects;
        Dictionary<string, Rectangle> CustTextures = new Dictionary<string, Rectangle>();
        SpriteFont HPFontNorm, HPFontCrit;

        public bool showCust { get; private set; }
        Vector2 custPos = new Vector2(-120,0);

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
            HPFontNorm = CM.Load<SpriteFont>("Misc/exe-hp-font-norm");
            HPFontCrit = CM.Load<SpriteFont>("Misc/exe-hp-font-crit");

            HPFontNorm.Spacing = 1;
            HPFontCrit.Spacing = 1;


            CurrentHP = 9999;
            MaxHP = 9999;

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

                SpriteFont hpfnt;

                if (CurrentHP >= MaxHP * 0.20)
                {
                    hpfnt = HPFontNorm;
                } else
                {
                    hpfnt = HPFontCrit;
                }
                int hptextX = (int)hpfnt.MeasureString(CurrentHP.ToString()).X;
                Vector2 hptxtrct = new Vector2(hprct.X + (hprct.Width - hptextX) - 5 ,hprct.Y);
                SB.DrawString(hpfnt, CurrentHP.ToString(), hptxtrct, Color.White);


            }
        }
    }
}
