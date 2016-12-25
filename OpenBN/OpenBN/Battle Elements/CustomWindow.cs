using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using OpenBN.ScriptedSprites;
using System.Collections.Generic;
using System.IO;

namespace OpenBN
{
    class CustomWindow : IBattleEntity
    {

        public string ID { get; set; }
        public SpriteBatch SB { get; set; }
        public ContentManager CM { get; set; }
        public int CurrentHP { get; private set; }
        public int LastHP { get; private set; }
        public int MaxHP { get; set; }
        public string ChipCodeStr = "@ABCD";

        public int HPState;

        Texture2D customtextures;
        Dictionary<string, Rectangle> CustTextures = new Dictionary<string, Rectangle>();
        SpriteFont HPFontNorm, HPFontCrit, HPFontRecv;

        FontHelper Fonts;

        public bool showCust { get; private set; }

        Vector2 custPos = new Vector2(-120, 0);
        Sprite CWSS;
        public CustomWindow(ContentManager x, FontHelper Font, GraphicsDevice graphics)
        {
            Fonts = Font;
            CM = x;

            CWSS = new Sprite("Misc/Custwindow-SS.sasl", "Misc/Custwindow", graphics, CM);

            CustomWindowTexture = CWSS.AnimationGroup["CUST"].Frames["0"];
            HPBarTexture = CWSS.AnimationGroup["CUST"].Frames["1"];

            HPFontNorm = Fonts.List["HPFont"];
            HPFontCrit = Fonts.List["HPFontMinus"];
            HPFontRecv = Fonts.List["HPFontPlus"];
            ChipCodes = Fonts.List["ChipCodesB"];

            HPFontNorm.Spacing = 1;
            HPFontCrit.Spacing = 1;
            HPFontRecv.Spacing = 1;
            ChipCodes.Spacing = 0;

            MaxHP = 9999;
            CurrentHP = MaxHP;
            LastHP = CurrentHP;

            showCust = false;
            DrawEnabled = true;
            hpfnt = HPFontNorm;

        }
        bool DrawEnabled = false;
        Random Rnd = new Random();
        public void Show()
        {
            showCust = true;
            ChipCodeStr = "";
            for (int i = 0; i < 5; i++)
            {
                var x = Rnd.Next(64, 90);
                x = Rnd.Next(64, 90);
                ChipCodeStr += (char)x;
            }

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
                    custPos.X = MyMath.Clamp(custPos.X + 10, -120, 0);
            }
            else
            {
                if (custPos.X != -120)
                    custPos.X = MyMath.Clamp(custPos.X - 10, -120, 0);
            }

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

            HPState = 0;

            if (CurrentHP >= MaxHP * 0.20)
            { HPState = 0; }
            else { HPState = 1; }

            if (CurrentHP != LastHP)
            {
                if (CurrentHP < LastHP)
                {
                    CurrentHP = MyMath.Clamp(CurrentHP + 9, CurrentHP, LastHP);
                    HPState = 2;
                }
                else if (CurrentHP > LastHP)
                {
                    CurrentHP = MyMath.Clamp(CurrentHP - 9, LastHP, CurrentHP);
                    HPState = 1;
                }
            }
        }

        public void SetHP(int TargetHP)
        {
            LastHP = (int)MyMath.Clamp(LastHP + TargetHP, 0, MaxHP);
        }

        SpriteFont hpfnt;
        Texture2D CustomWindowTexture, HPBarTexture;


        public void Draw()
        {
            SB.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

            if (CWSS != null && DrawEnabled)
            {
                if (custPos.X != -120)
                {
                    var y = new Rectangle((int)custPos.X, 0, CustomWindowTexture.Width, CustomWindowTexture.Height);
                    SB.Draw(CustomWindowTexture, y, Color.White);
                    DrawMiniChipCodes(custPos.X);
                }
                var hprct = new Rectangle((int)custPos.X + 122, 1, HPBarTexture.Width, HPBarTexture.Height);
                SB.Draw(HPBarTexture, hprct, Color.White);
                
                int hptextX = (int)hpfnt.MeasureString(CurrentHP.ToString()).X;
                Vector2 hptxtrct = new Vector2(hprct.X + (hprct.Width - hptextX) - 6, hprct.Y);
                SB.DrawString(hpfnt, CurrentHP.ToString(), hptxtrct, Color.White);
            }
            SB.End();
        }

        SpriteFont ChipCodes;

        public void DrawMiniChipCodes(float x)
        {
            var startpoint = new Vector2(x + 8, 119);
            var Measure = ChipCodes.MeasureString(ChipCodeStr);
            SB.DrawString(ChipCodes, ChipCodeStr, startpoint, Color.White);
        }
    }
}
