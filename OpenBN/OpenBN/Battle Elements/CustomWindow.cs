﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;



using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Input;

using OpenBN.ScriptedSprites;
using static OpenBN.Helpers.Misc;
using static OpenBN.MyMath;
using System.Diagnostics;

namespace OpenBN
{


    public class CustomWindow : IBattleEntity
    {
        public string ID { get; set; }
        public SpriteBatch SB { get; set; }
        public ContentManager Content { get; set; }
        public GraphicsDevice Graphics { get; set; }
        public Inputs Input { get; set; }


        public int CurrentHP { get; private set; }
        public int LastHP { get; private set; }
        public int MaxHP { get; set; }

        public string ChipCodeStr = "@ABCD";

        public int HPState;

        Dictionary<string, Rectangle> CustTextures = new Dictionary<string, Rectangle>();
        SpriteFont HPFontNorm, HPFontCrit, HPFontRecv, hpfnt, ChipCodesA, ChipCodesB, ChipDmg;

        FontHelper Fonts;
        public bool showCust { get; private set; }
        public bool Initialized { get; set; }

        Vector2 custPos = new Vector2(-120, 0);
        Sprite CWSS;

        Texture2D Emblem;
        double EmblemRot, EmblemScalar = 1;
        Vector2 EmblemOrigin, EmblemPos;
        bool IsEmblemRotating = false;

        Texture2D CustomWindowTexture, HPBarTexture;

        bool DrawEnabled = false;
        Random Rnd = new Random();

        int slotindex = 1;
        int varks_l = 0, varks_r = 0;

        int[][] ChipSlotTypes =
        {
            new int[] {1,1,1,1,1,2},
            new int[] {1,1,1,0,0,2}
        };

        public void Initialize()
        {
            CWSS = new Sprite("Misc/Custwindow-SS.sasl", "Misc/Custwindow", Graphics, Content);

            CustomWindowTexture = CWSS.AnimationGroup["CUST"].Frames["CUSTWIN"];
            HPBarTexture = CWSS.AnimationGroup["CUST"].Frames["HPBAR"];

            SetFocus("CHIPSLOT_1_1");

            HPFontNorm = Fonts.List["HPFont"];
            HPFontCrit = Fonts.List["HPFontMinus"];
            HPFontRecv = Fonts.List["HPFontPlus"];
            ChipCodesA = Fonts.List["ChipCodesA"];
            ChipCodesB = Fonts.List["ChipCodesB"];
            ChipDmg = Fonts.List["ChipDmg"];

            HPFontNorm.Spacing = 1;
            HPFontCrit.Spacing = 1;
            HPFontRecv.Spacing = 1;
            ChipCodesA.Spacing = 0;
            ChipCodesB.Spacing = 0;

            hpfnt = HPFontNorm;

            Emblem = Content.Load<Texture2D>("Navi/MM/Emblem");
            EmblemOrigin = new Vector2((float)Math.Ceiling((float)Emblem.Width),
                                       (float)Math.Ceiling((float)Emblem.Height)) / 2;
            EmblemPos = new Vector2(103, 11);
            EmblemRot = 0;
            Initialized = true;

            Slots[1] = new TestBattleChip(Content, "schip011", "Spreadr3", 90, ChipElements.NULL, 'A');
            Slots[2] = new TestBattleChip(Content, "schip084", "Muramasa", -1, ChipElements.SWORD, '@');
            Slots[3] = new TestBattleChip(Content, "schip062", "FlshBom1", 40, ChipElements.NULL, 'Q');
            Slots[4] = new TestBattleChip(Content, "schip021", "FireBrn3", 150, ChipElements.FIRE, 'C');
            Slots[5] = new TestBattleChip(Content, "schip025", "TrnArrw3", 50, ChipElements.AQUA, 'D');


            DisplayBattleChip(Slots[1]);


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

        public CustomWindow(FontHelper Font, ref Inputs input) : this(Font)
        {
            this.Input = input;
        }

        public void Show()
        {
            showCust = true;
            ChipCodeStr = "";

            //for (int i = 0; i < 5; i++)
            //{
            //    var x = Rnd.Next(64, 90);
            //    x = Rnd.Next(64, 90);
            //    ChipCodeStr += (char)x;
            //}
        }

        public void Hide()
        {
            showCust = false;
        }

        public void Update() //Called only every frame (i guess)
        {
            //Custom Window transition update logic
            UpdateTransition();
            //HP Bar update logic
            UpdateHPBar();

            if (showCust)
            {
                //Emblem Rotation update logic
                UpdateEmblem();
                //Keyboard Handling logic
                HandleInputs();
                //Update the Chip Slot codes
                UpdateChipSlotCodes();
            }

            //Advance frames
            CWSS.AdvanceAllGroups();

        }

        private void UpdateChipSlotCodes()
        {
            ChipCodeStr = "";
            for (int i = 1; i < 6; i++)
            {
                ChipCodeStr += Slots[i].Code;
            }
            DisplayBattleChip(Slots[slotindex]);
        }

        private void HandleInputs()
        {
            if (Input != null)
            {
                var ks_l = Input.KbStream[Keys.Left];
                var ks_r = Input.KbStream[Keys.Right];
                switch (ks_l.KeyState)
                {
                    case KeyState.Down:
                        varks_l++;
                        {
                            if (varks_l % 8 == 0)
                            {
                                slotindex = (slotindex - 1) % 6;
                                slotindex = InverseClamp(slotindex, 1, 5);
                                SetFocus("CHIPSLOT_1_" + slotindex.ToString());

                            }
                        }
                        break;
                    case KeyState.Up:
                        varks_l = varks_l % 128;
                        break;
                }
                switch (ks_r.KeyState)
                {
                    case KeyState.Down:
                        varks_r++;
                        if (varks_r % 8 == 0)
                        {
                            slotindex = (slotindex + 1) % 6;
                            slotindex = Clamp(slotindex, 1, 5);
                            SetFocus("CHIPSLOT_1_" + slotindex.ToString());
                        }
                        break;
                    case KeyState.Up:
                        varks_r = varks_r % 128;
                        break;
                }
            }
        }

        private void UpdateEmblem()
        {
            if (IsEmblemRotating)
            {
                var x = EmblemRot + 0.45;
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

        private void UpdateHPBar()
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
                HPState = 0;
            else
                HPState = 1;

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

        private void UpdateTransition()
        {
            if (showCust)
            {
                if (custPos.X != 0)
                    custPos.X = MyMath.Clamp(custPos.X + 15, -120, 0);
            }
            else
            {
                if (custPos.X != -120)
                    custPos.X = MyMath.Clamp(custPos.X - 15, -120, 0);
            }
        }

        public void Draw()
        {
            if (!Initialized) return;
            SB.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            if (CWSS != null && DrawEnabled)
            {
                if (custPos.X != -120)
                {
                    DrawCustWindow();
                    DrawChipSlotCodes();
                    DrawEmblem();
                    DrawBattleChip();
                    DrawFocusRects();
                }
                DrawHPBar();
            }
            SB.End();
        }

        public void DrawFocusRects()
        {

            var x = custPos.X + CurrentFocusRect.X;
            var y = CurrentFocusRect.Y;
            var w = CurrentFocusRect.Width;
            var h = CurrentFocusRect.Height;

            Texture2D TextTL = CWSS.AnimationGroup["CURSOR0_TL"].CurrentFrame;
            Texture2D TextTR = CWSS.AnimationGroup["CURSOR0_TR"].CurrentFrame;
            Texture2D TextBL = CWSS.AnimationGroup["CURSOR0_BL"].CurrentFrame;
            Texture2D TextBR = CWSS.AnimationGroup["CURSOR0_BR"].CurrentFrame;

            var of = 4;

            Vector2 TL = new Vector2(x, y) - new Vector2(of, of);
            Vector2 TR = new Vector2(x + w - 8, y) - new Vector2(-of, of);
            Vector2 BL = new Vector2((x + w) - 8, y + h - 8) - new Vector2(-of, -of);
            Vector2 BR = new Vector2(x, (y + h) - 8) - new Vector2(of, -of);

            SB.Draw(TextTL, TL, Color.White);
            SB.Draw(TextTR, TR, Color.White);
            SB.Draw(TextBL, BL, Color.White);
            SB.Draw(TextBR, BR, Color.White);

        }

        public void DrawCustWindow()
        {
            var y = new Rectangle((int)custPos.X, 0, CustomWindowTexture.Width, CustomWindowTexture.Height);
            SB.Draw(CustomWindowTexture, y, Color.White);
        }

        public void DrawHPBar()
        {
            var hprct = new Rectangle((int)custPos.X + 122, 0, HPBarTexture.Width, HPBarTexture.Height);
            SB.Draw(HPBarTexture, hprct, Color.White);
            int hptextX = (int)hpfnt.MeasureString(CurrentHP.ToString()).X;
            Vector2 hptxtrct = new Vector2(hprct.X + (hprct.Width - hptextX) - 6, hprct.Y);
            SB.DrawString(hpfnt,
                CurrentHP.ToString(),
                hptxtrct,
                Color.White);

        }

        public void DrawEmblem()
        {

            SB.Draw(Emblem, new Rectangle(
                                (int)(custPos.X + EmblemPos.X), (int)EmblemPos.Y,
                                (int)Math.Ceiling(Emblem.Width * EmblemScalar),
                                (int)Math.Ceiling(Emblem.Height * EmblemScalar)
                                ),
                                null,
                                Color.White,
                                (float)EmblemRot,
                                EmblemOrigin,
                                SpriteEffects.None,
                                0);
        }

        public void DrawChipSlotCodes()
        {
            var startpoint = new Vector2(custPos.X + 8, 119);
            var Measure = ChipCodesB.MeasureString(ChipCodeStr);
            SB.DrawString(ChipCodesB, ChipCodeStr, startpoint, Color.White);
        }

        public void DrawBattleChip()
        {
            if (SelectedChip != null)
            {
                var img_vect = new Vector2((int)custPos.X + 16, 24);
                var name_vect = new Vector2((int)custPos.X + 17, 9);
                var code_vect = new Vector2((int)custPos.X + 16, 72);
                var elem_vect = new Vector2((int)custPos.X + 25, 73);
                var ChipElem = CWSS.AnimationGroup["CUST"].Frames["TYPE_" + SelectedChip.Element.ToString()];
                string Dmg_Disp = "";
                switch (SelectedChip.Damage)
                {
                    case -1:
                        Dmg_Disp = "///";
                        break;
                    case -2:
                        Dmg_Disp = "";
                        break;
                    default:
                        Dmg_Disp = SelectedChip.Damage.ToString();
                        break;
                }

                var Dmg_MS = 71 - ChipDmg.MeasureString(Dmg_Disp).X;
                var dmg_vect = new Vector2((int)custPos.X + Dmg_MS, 75);

                SB.Draw(SelectedChip.Image, img_vect, Color.White);
                SB.Draw(ChipElem, elem_vect, Color.White);

                SB.DrawString(Fonts.List["Normal"], SelectedChip.DisplayName, name_vect, Color.White);
                SB.DrawString(ChipCodesA, SelectedChip.Code.ToString(), code_vect, Color.White);
                SB.DrawString(ChipDmg, Dmg_Disp, dmg_vect, Color.White);
            }
        }

        public void RotateEmblem()
        {
            if (IsEmblemRotating) EmblemRot = 0;
            IsEmblemRotating = true;
        }

        public void DisplayBattleChip(IBattleChip BattleChip)
        {
            SelectedChip = BattleChip;
        }

        public void SetHP(int TargetHP)
        {
            LastHP = (int)MyMath.Clamp(LastHP + TargetHP, 0, MaxHP);
        }

        public void SetFocus(string rectname)
        {
            CurrentFocusRect = RectFromString(CWSS.Metadata[rectname]);
            // CWSS.ResetAllGroups();
        }

        IBattleChip SelectedChip { get; set; }
        Rectangle CurrentFocusRect { get; set; }
        IBattleChip[] Slots = new IBattleChip[6];
    }
}
