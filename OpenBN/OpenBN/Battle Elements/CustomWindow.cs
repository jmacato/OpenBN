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
        public ContentManager Content { get; set; }
        public GraphicsDevice Graphics { get; set; }

        public int CurrentHP { get; private set; }
        public int LastHP { get; private set; }
        public int MaxHP { get; set; }
        public string ChipCodeStr = "@ABCD";

        public int HPState;

        Dictionary<string, Rectangle> CustTextures = new Dictionary<string, Rectangle>();
        SpriteFont HPFontNorm, HPFontCrit, HPFontRecv;

        FontHelper Fonts;

        public bool showCust { get; private set; }

        public bool Initialized { get; set; }

        Vector2 custPos = new Vector2(-120, 0);
        Sprite CWSS;

        Texture2D Emblem;
        double EmblemRot, EmblemScalar = 1;
        Vector2 EmblemOrigin, EmblemPos;
        bool IsEmblemRotating = false;

        SpriteFont hpfnt;
        Texture2D CustomWindowTexture, HPBarTexture;

        SpriteFont ChipCodes;

        bool DrawEnabled = false;
        Random Rnd = new Random();

        public void Initialize()
        {
            CWSS = new Sprite("Misc/Custwindow-SS.sasl", "Misc/Custwindow", Graphics, Content);

            CustomWindowTexture = CWSS.AnimationGroup["CUST"].Frames["CUSTWIN"];
            HPBarTexture = CWSS.AnimationGroup["CUST"].Frames["HPBAR"];

            HPFontNorm = Fonts.List["HPFont"];
            HPFontCrit = Fonts.List["HPFontMinus"];
            HPFontRecv = Fonts.List["HPFontPlus"];
            ChipCodes = Fonts.List["ChipCodesB"];

            HPFontNorm.Spacing = 1;
            HPFontCrit.Spacing = 1;
            HPFontRecv.Spacing = 1;
            ChipCodes.Spacing = 0;
            hpfnt = HPFontNorm;

            Emblem = Content.Load<Texture2D>("Navi/MM/Emblem");
            EmblemOrigin = new Vector2((float)Math.Ceiling((float)Emblem.Width),
                                       (float)Math.Ceiling((float)Emblem.Height))/2;
            EmblemPos = new Vector2(103, 11);

            EmblemRot = 0;
            Initialized = true;

        }

        public CustomWindow(FontHelper Font)
        {
            Fonts = Font;
            MaxHP = 9999;
            CurrentHP = MaxHP;
            LastHP = CurrentHP;
            showCust = false;
            DrawEnabled = true;
        }

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
            //Custom Window transition update logic
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
            }
            //HP Bar update logic
            {
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
                if (CurrentHP >= MaxHP * 0.20)
                {
                    HPState = 0;
                }
                else
                {
                    HPState = 1;
                }
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
            //Emblem Rotation update logic
            {
                if (IsEmblemRotating)
                {
                    var x = EmblemRot + 0.4;
                    EmblemRot = MyMath.Clamp(x, 0, MathHelper.TwoPi);
                    if (EmblemRot > MathHelper.Pi)
                    {
                        EmblemScalar = MyMath.Map((float)x, MathHelper.TwoPi, 0, 1, 1.5f);
                        if (EmblemScalar == MathHelper.TwoPi) IsEmblemRotating = false;
                    }
                    else
                    {
                        EmblemScalar = MyMath.Map((float)x, 0, MathHelper.TwoPi, 1.1f, 1.5f);
                    }
                }

            }
        }

        public void SetHP(int TargetHP)
        {
            LastHP = (int)MyMath.Clamp(LastHP + TargetHP, 0, MaxHP);
        }

        public void Draw()
        {
            if (!Initialized) return;

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

                SB.DrawString(hpfnt,
                    CurrentHP.ToString(),
                    hptxtrct,
                    Color.White);

                SB.Draw(Emblem, new Rectangle(
                    (int)(custPos.X + EmblemPos.X), (int)EmblemPos.Y,
                    (int)Math.Ceiling(Emblem.Width*EmblemScalar) ,
                    (int)Math.Ceiling(Emblem.Height*EmblemScalar)
                    ),
                    null,
                    Color.White,
                    (float)EmblemRot,
                    EmblemOrigin,
                    SpriteEffects.None,
                    0 );

            }
            SB.End();
        }

    
        public void DrawMiniChipCodes(float x)
        {
            var startpoint = new Vector2(x + 8, 119);
            var Measure = ChipCodes.MeasureString(ChipCodeStr);
            SB.DrawString(ChipCodes, ChipCodeStr, startpoint, Color.White);
        }
       
        public void RotateEmblem()
        {
            if (IsEmblemRotating) EmblemRot = 0;
            IsEmblemRotating = true;
        }
    }

    public class ChipSlot
    {
        public enum Type
        {
            Null,
            Slot,
            OKButton,
        }
    }

}
