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
        public int LastHP { get; set; }
        public int MaxHP { get; set; }

        int HPState = 0;

        Texture2D customtextures;
        Dictionary<string, Rectangle> CustSrcRects;
        Dictionary<string, Rectangle> CustTextures = new Dictionary<string, Rectangle>();
        SpriteFont HPFontNorm, HPFontCrit, HPFontRecv;

        public bool showCust { get; private set; }

        Vector2 custPos = new Vector2(-120,0);

        public CustomWindow(ContentManager x, FontHelper Fonts)
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
            HPFontNorm = Fonts.List["HPFont"];
            HPFontCrit = Fonts.List["HPFontMinus"];
            HPFontRecv = Fonts.List["HPFontPlus"];

            HPFontNorm.Spacing = 1;
            HPFontCrit.Spacing = 1;
            HPFontRecv.Spacing = 1;

            MaxHP = 9999;
            CurrentHP = 0;
            LastHP = CurrentHP;

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
                if (custPos.X != 0)
                    custPos.X = MathHelper.Clamp(custPos.X+10,-120,0);
            } else
            {
                if (custPos.X != -120)
                    custPos.X = MathHelper.Clamp(custPos.X-10,-120, 0);
            }

            HPState = 0;

            if (CurrentHP >= MaxHP * 0.20)
            { HPState = 0; }
            else { HPState = 1; }

            if (CurrentHP != LastHP)
            {
                if (CurrentHP < LastHP)
                {
                    CurrentHP = (int)MathHelper.Clamp(CurrentHP + 15, CurrentHP, LastHP);
                    HPState = 2;
                }
                else if (CurrentHP > LastHP)
                {
                    CurrentHP = (int)MathHelper.Clamp(CurrentHP - 15, LastHP, CurrentHP);
                    HPState = 1;
                }
            }
        }

        public void SetHP(int TargetHP)
        {
            LastHP = (int)MathHelper.Clamp(TargetHP, 0, MaxHP);
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

                switch (HPState)
                {
                    case 1:
                        hpfnt = HPFontCrit;
                        break;
                    case 2:
                        hpfnt = HPFontRecv;
                        break;
                    default:
                        hpfnt = HPFontNorm;
                        break;
                }

                int hptextX = (int)hpfnt.MeasureString(CurrentHP.ToString()).X;
                Vector2 hptxtrct = new Vector2(hprct.X + (hprct.Width - hptextX) - 5 ,hprct.Y);
                SB.DrawString(hpfnt, CurrentHP.ToString(), hptxtrct, Color.White);


            }
        }
    }
}
