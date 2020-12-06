using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenBN.Helpers;
using OpenBN.Interfaces;
using OpenBN.Sprites;
using static OpenBN.Helpers.Misc;
using static OpenBN.Helpers.MyMath;

namespace OpenBN.BattleElements
{
    public class CustomWindow : BattleModule
    {
        #region Declares
        public static float[] HeightScaleSpline = { 0.2857f, 0.3571f, 0.5f, 0.6428f, 1f, 1.15f, 1f };
        public int BtlMsgWaitFrames = 60;
        public int BtlMsgCounter = 0;
        public bool BtlMsgDrawFlag = false;
        public string BattleMessageText;
        public BtlMsgStatus BattleMsgStatus;
        public Vector2 BtlMsgAnchor = new(Battle.Instance.screenRes.W / 2, 72);
        public Vector2 BtlMsgOrigin = new(0, 0);
        public Vector2 BtlMsgBounds = new(0, 0);
        public Vector2 BtlMsgScale = new(1, 1);
        public Keys[] HandledKeys;
        public List<Keys> LatchOnlyKeys;
        public int[] waitcounter;

        private const int OVERFLOW_MODULO = 1024;
        private const int WAITCOUNT = 5;
        public int HPState, CurrentHP, LastHP, MaxHP;

        public int SlotColumn, SlotRow;
        public int waitframe_l,waitframe_r,waitframe_u,waitframe_d,waitframe_a,waitframe_b,selection_status;

        public string ChipCodeStr { get; set; }
        public IBattleChip SelectedChip;
        public Stack<IBattleChip> SelectedStack;
        public IBattleChip[,] Slots;
        public CustomBarState CBState;
        public CustomBarModifiers CBModifier;

        /// <summary>
        ///     Determine the type of focusable object in the Custom Window
        ///     0 - Nothing
        ///     1 - Chip Slot
        ///     2 - OK Button
        /// </summary>
        public readonly int[,] SlotType;

        /// <summary>
        ///     Stores the Current Status of each slots
        ///     0 - Nothing
        ///     1 - Contains BC
        ///     2 - BC's Selected
        /// </summary>
        public readonly int[,] SlotStatus;
        public TimeSpan CBTime;
        public Dictionary<string, Rectangle> CustTextures;
        public Sprite CustomWin, CustomBar;
        public SpriteFont hpfnt, HPFontNorm, HPFontCrit, HPFontRecv, ChipCodesA, ChipCodesB, ChipDmg, BtlMsgFont;
        public readonly FontHelper Fonts;
        public bool showCust, IsEmblemRotating;
        public double EmblemRot, EmblemScalar, CustBarProgress;
        public Vector2 custPos, EmblemOrigin, EmblemPos;
        public Rectangle CWSrcRect, HPBarSrcRect, CurrentFocusRect;
        public readonly Rectangle CustBarRect;
        public Texture2D Emblem;
        public TimeSpan OldCustBarTimeSpan;
        public TimeSpan CustBarTimeSpan;

        public enum BtlMsgStatus { NONE, GROW, STAY, SHRINK }

        #endregion


        public CustomWindow(Game parent) : base(parent)
        {
            Fonts = ((Battle)parent).Fonts;
            MaxHP = 9999;
            CurrentHP = MaxHP;
            LastHP = CurrentHP;
            showCust = false;
            EmblemRot = MathHelper.TwoPi;
            EmblemScalar = 1;
            CustBarProgress = 0.0f;
            SlotColumn = 1;
            SlotRow = 1;
            waitframe_l = WAITCOUNT - 1;
            waitframe_r = WAITCOUNT - 1;
            waitframe_u = WAITCOUNT - 1;
            waitframe_d = WAITCOUNT - 1;
            waitframe_a = WAITCOUNT - 1;
            waitframe_b = WAITCOUNT - 1;
            selection_status = 0;
            showCust = false;
            IsEmblemRotating = false;
            CustBarRect = new Rectangle(48, 0, 144, 16);
            custPos = new Vector2(-120, 0);
            CustTextures = new Dictionary<string, Rectangle>();
            CustBarTimeSpan = TimeSpan.Zero;
            CBState = CustomBarState.Empty;
            CBModifier = CustomBarModifiers.Normal;
            CBTime = new TimeSpan(0);
            SlotType = new[,] { { 1, 1, 1, 1, 1, 2 }, { 1, 1, 1, 0, 0, 0 } };
            SlotStatus = new int[2, 6];
            Slots = new IBattleChip[2, 6];
            Initialize();
        }

        public void Initialize()
        {
            CustomWin = new Sprite("Misc/Custwindow-SS.sasl", "Misc/Custwindow", Graphics, Content);
            CustomBar = new Sprite("Misc/CustBar.sasl", "Misc/CustBar", Graphics, Content);
            CWSrcRect = CustomWin.AnimationGroup["CUST"].Frames["CUSTWIN"];
            HPBarSrcRect = CustomWin.AnimationGroup["CUST"].Frames["HPBAR"];
            SetFocus("CHIPSLOT_1_1");
            HPFontNorm = Fonts.List["HPFont"];
            HPFontCrit = Fonts.List["HPFontMinus"];
            HPFontRecv = Fonts.List["HPFontPlus"];
            ChipCodesA = Fonts.List["ChipCodesA"];
            ChipCodesB = Fonts.List["ChipCodesB"];
            ChipDmg = Fonts.List["ChipDmg"];
            BtlMsgFont = Fonts.List["BattleMessage"];
            OKButtonHandler = new ButtonDelegate(OKButtonPress);
            HPFontNorm.Spacing = 1;
            HPFontCrit.Spacing = 1;
            HPFontRecv.Spacing = 1;
            ChipCodesA.Spacing = 0;
            ChipCodesB.Spacing = 0;
            BtlMsgFont.Spacing = 0;
            hpfnt = HPFontNorm;
            Emblem = Content.Load<Texture2D>("Navi/MM/Emblem");
            EmblemOrigin = new Vector2((float)Math.Ceiling((float)Emblem.Width), (float)Math.Ceiling((float)Emblem.Height)) / 2;
            EmblemPos = new Vector2(104, 12);
            EmblemRot = 0;
            var y = new ChipIconProvider(Content, Graphics);
            /*  For testing only */
            Slots[0, 0] = new TestBattleChip(54, y, Content, "Static", 20, ChipElements.WIND, '@');
            Slots[0, 1] = new TestBattleChip(69, y, Content, "PoisSeed", -2, ChipElements.NULL, '@');
            Slots[0, 2] = new TestBattleChip(70, y, Content, "Sword", 80, ChipElements.SWORD, 'A');
            Slots[0, 3] = new TestBattleChip(71, y, Content, "WideSwrd", 80, ChipElements.SWORD, 'B');
            Slots[0, 4] = new TestBattleChip(72, y, Content, "LongSwrd", 100, ChipElements.SWORD, 'C');
            Slots[1, 0] = new TestBattleChip(57, y, Content, "MachGun3", 70, ChipElements.TARGET, 'A');
            Slots[1, 1] = new TestBattleChip(61, y, Content, "MegEnBom", 80, ChipElements.NULL, 'B');
            Slots[1, 2] = new TestBattleChip(66, y, Content, "BugBomb", 100, ChipElements.NULL, 'C');
            Slots[0, 5] = new CustomWindowButtons(selection_status, Content, OKButtonHandler);
            /*  For testing only */
            for (var si = 0; si < Slots.GetLength(0); si++)
                for (var sj = 0; sj < Slots.GetLength(1); sj++)
                {
                    var chkslot = Slots[si, sj];
                    if (chkslot != null)
                    {
                        if (chkslot.GetType().Name != "CustomWindowButtons")
                            SlotStatus[si, sj] = 1;
                    }
                }
            HandledKeys = new Keys[] { Keys.Left, Keys.Right, Keys.Up, Keys.Down, Keys.Z, Keys.X };
            LatchOnlyKeys = new List<Keys>() { Keys.X, Keys.Z };
            waitcounter = new int[HandledKeys.Length];
            DisplayBattleChip(Slots[0, 0]);
            SelectedStack = new Stack<IBattleChip>();
            Initialized = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            //Custom Window transition update logic
            UpdateTransition();
            //HP Bar update logic
            UpdateHPBar();
            UpdateCustBar();
            UpdateBtlMsg();
            if (showCust)
            {
                //Emblem Rotation update logic
                UpdateEmblem();
                UpdateSelectedStack();
                //Keyboard Handling logic
                HandleInputs();
            }
            //Advance frames
            CustomWin.AdvanceAllGroups();
        }

        public override void Draw()
        {
            base.Draw();
            if (custPos.X != -120)
            {
                DrawCustWindow();
                DrawChipSlots();
                DrawSelectedChipSlot();
                DrawEmblem();
                DrawBattleChip();
                DrawFocusRects();
            }
            DrawHPBar();
            if (custPos.X == -120)
            {
                DrawBtlMsg();
                DrawCustBar();
            }
        }



        public delegate void ButtonDelegate();
        public ButtonDelegate OKButtonHandler;
        public void OKButtonPress()
        {
            FinishCustomizing();
        }


        private void ShowBattleMessage(string message)
        {
            var msg = message.ToUpper();
            msg = msg.Replace(" ", "=");
            msg = msg.Replace("!", ".");
            BattleMessageText = String.Format("<={0}=>", msg);
            BtlMsgBounds = BtlMsgFont.MeasureString(BattleMessageText);
            BtlMsgOrigin = BtlMsgBounds / 2;
            BattleMsgStatus = BtlMsgStatus.GROW;
        }
        private void UpdateSelectedStack()
        {
            //A bit hacky but its enough
            selection_status = Clamp(SelectedStack.Count, 0, 1);
            for (var si = 0; si < Slots.GetLength(0); si++)
                for (var sj = 0; sj < Slots.GetLength(1); sj++)
                {
                    var chkslot = Slots[si, sj];
                    if (chkslot != null)
                    {
                        if (chkslot.GetType().Name == "CustomWindowButtons")
                            ((CustomWindowButtons)Slots[si, sj]).SetStatus = selection_status;
                    }
                }
        }
        private void UpdateBtlMsg()
        {
            switch (BattleMsgStatus)
            {
                case BtlMsgStatus.GROW:
                    if (custPos.X != -120) break;
                    if (BtlMsgCounter < HeightScaleSpline.Length)
                    {
                        BtlMsgScale.Y = HeightScaleSpline[BtlMsgCounter];
                        BtlMsgCounter++;
                        BtlMsgDrawFlag = true;
                    }
                    else if (BtlMsgCounter >= HeightScaleSpline.Length)
                    {
                        BtlMsgCounter = 0;
                        BattleMsgStatus = BtlMsgStatus.STAY;
                    }
                    break;
                case BtlMsgStatus.STAY:
                    BtlMsgCounter++;
                    if (BtlMsgCounter > BtlMsgWaitFrames)
                    {
                        BattleMsgStatus = BtlMsgStatus.SHRINK;
                        BtlMsgCounter = HeightScaleSpline.Length - 1;
                    }
                    break;
                case BtlMsgStatus.SHRINK:
                    if (BtlMsgCounter >= 0)
                    {
                        BtlMsgScale.Y = HeightScaleSpline[BtlMsgCounter];
                        BtlMsgCounter--;
                    }
                    else if (BtlMsgCounter < 0)
                    {
                        BtlMsgCounter = 0;
                        BattleMsgStatus = BtlMsgStatus.NONE;
                        BtlMsgDrawFlag = false;

                        UnfreezeObjects();
                    }
                    break;
            }
        }

        void UnfreezeObjects()
        {
            OldCustBarTimeSpan = DateTime.UtcNow.TimeOfDay;
            CBState = CustomBarState.Loading;
            ((Battle)Parent).UnfreezeObjects();
        }

        private void DrawBtlMsg()
        {
            if (BattleMsgStatus != BtlMsgStatus.NONE && BtlMsgDrawFlag)
            {
                spriteBatch.DrawString(
                    BtlMsgFont,
                    BattleMessageText,
                    BtlMsgAnchor,
                    Color.White,
                    0f,
                    BtlMsgOrigin,
                    BtlMsgScale,
                    SpriteEffects.None,
                    0f);
            }
        }



        private void HandleInputs()
        {
            if (Input != null)
            {
                //All time units are in frames.
                //Approx. 1 frameunit - 1/60th of a second
                //Unless explicitly stated tho.
                var KEY_WAIT_THRESHOLD_MS = 80;
                for (var i = 0; i < HandledKeys.Length; i++)
                {
                    var Key = HandledKeys[i];
                    var KEY_STREAM = Input.KbStream[Key];
                    switch (KEY_STREAM.KeyState)
                    {
                        case KeyState.Down:
                            if (KEY_STREAM.DurDelta > KEY_WAIT_THRESHOLD_MS)
                            {
                                if (!LatchOnlyKeys.Contains(Key))
                                {
                                    waitcounter[i]++;
                                    Battle.KeyLatch[Key] = false;
                                    if (waitcounter[i] % WAITCOUNT == 0)
                                    {
                                        ExecuteKeyCommand(Key);
                                        waitcounter[i] = 0;
                                    }
                                }
                                {
                                    if (Battle.KeyLatch[Key] == false)
                                    {
                                        Battle.KeyLatch[Key] = true;
                                    }
                                }
                            }
                            if (Battle.KeyLatch[Key] == false)
                            {
                                Battle.KeyLatch[Key] = true;
                            }
                            break;
                        case KeyState.Up:
                            if (Battle.KeyLatch[Key] == true)
                            {
                                Battle.KeyLatch[Key] = false;
                                ExecuteKeyCommand(Key);
                                waitcounter[i] = 0;
                            }
                            break;
                    }
                }
            }
        }
        private void ExecuteKeyCommand(Keys key)
        {
            switch (key)
            {
                case Keys.Left:
                    TraverseChipSlots(SlotRow, SlotColumn - 1);
                    break;
                case Keys.Right:
                    TraverseChipSlots(SlotRow, SlotColumn + 1);
                    break;
                case Keys.Up:
                    TraverseChipSlots(SlotRow - 1, SlotColumn);
                    break;
                case Keys.Down:
                    TraverseChipSlots(SlotRow + 1, SlotColumn);
                    break;
                case Keys.X:
                    SelectChip(SlotRow - 1, SlotColumn - 1);
                    break;
                case Keys.Z:
                    DeselectChip(SlotRow - 1, SlotColumn - 1);
                    break;
            }
        }

        #region Update Subroutines
        private void DeselectChip(int SlotRow, int SlotColumn)
        {
            if (SelectedStack.Count > 0)
            {
                var t = SelectedStack.Pop();
                t.IsSelected = false;
            }
        }
        private void SelectChip(int SlotRow, int SlotColumn)
        {
            if (Slots[SlotRow, SlotColumn].IsSelected) return;
            if (SelectedStack.Count + 1 > 5) return;
            if (Slots[SlotRow, SlotColumn].GetType().Name == "CustomWindowButtons")
            {
                Slots[SlotRow, SlotColumn].Execute(this, (Battle)Parent, null);
                return;
            }
            RotateEmblem();
            Slots[SlotRow, SlotColumn].IsSelected = true;
            SelectedStack.Push(Slots[SlotRow, SlotColumn]);
        }
        private void TraverseChipSlots(int row, int col)
        {
            var TmpSlotRow = Clamp(row, 1, SlotType.GetLength(0));
            var TmpSlotColumn = InverseClamp(col, 1, SlotType.GetLength(1));
            var CurSlotType = SlotType[TmpSlotRow - 1, TmpSlotColumn - 1];
            var CurSlotStat = SlotStatus[TmpSlotRow - 1, TmpSlotColumn - 1];
            switch (CurSlotType)
            {
                case 1:
                    if (CurSlotStat != 0)
                    {
                        SetFocus(String.Format("CHIPSLOT_{0}_{1}", TmpSlotRow, TmpSlotColumn));
                        DisplayBattleChip(Slots[TmpSlotRow - 1, TmpSlotColumn - 1]);
                        SlotRow = TmpSlotRow;
                        SlotColumn = TmpSlotColumn;
                    }
                    break;
                case 2:
                    SetFocus("OKBUTTON");
                    DisplayBattleChip(Slots[0, 5]);
                    SlotRow = TmpSlotRow;
                    SlotColumn = TmpSlotColumn;
                    break;
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
        #endregion
        #region Draw Subroutines
        private void DrawFocusRects()
        {
            var x = custPos.X + CurrentFocusRect.X;
            var y = CurrentFocusRect.Y;
            var w = CurrentFocusRect.Width;
            var h = CurrentFocusRect.Height;
            var TextTL = CustomWin.AnimationGroup["CURSOR0_TL"].CurrentFrame;
            var TextTR = CustomWin.AnimationGroup["CURSOR0_TR"].CurrentFrame;
            var TextBL = CustomWin.AnimationGroup["CURSOR0_BL"].CurrentFrame;
            var TextBR = CustomWin.AnimationGroup["CURSOR0_BR"].CurrentFrame;
            var of = 4;
            var TL = new Vector2(x, y) - new Vector2(of, of);
            var TR = new Vector2(x + w - 8, y) - new Vector2(-of, of);
            var BL = new Vector2(x + w - 8, y + h - 8) - new Vector2(-of, -of);
            var BR = new Vector2(x, y + h - 8) - new Vector2(of, -of);
            spriteBatch.Draw(CustomWin.texture, TL, TextTL, Color.White);
            spriteBatch.Draw(CustomWin.texture, TR, TextTR, Color.White);
            spriteBatch.Draw(CustomWin.texture, BL, TextBL, Color.White);
            spriteBatch.Draw(CustomWin.texture, BR, TextBR, Color.White);
        }
        private void DrawCustWindow()
        {
            var y = new Rectangle((int)custPos.X, 0, CWSrcRect.Width, CWSrcRect.Height);
            spriteBatch.Draw(CustomWin.texture, y, CWSrcRect, Color.White);
        }
        private void DrawHPBar()
        {
            var hprct = new Rectangle((int)custPos.X + 122, 0, HPBarSrcRect.Width, HPBarSrcRect.Height);
            spriteBatch.Draw(CustomWin.texture, hprct, HPBarSrcRect, Color.White);
            var hptextX = (int)hpfnt.MeasureString(CurrentHP.ToString()).X;
            var hptxtrct = new Vector2(hprct.X + (hprct.Width - hptextX) - 6, hprct.Y);
            spriteBatch.DrawString(hpfnt,
                CurrentHP.ToString(),
                hptxtrct,
                Color.White);
        }
        private void DrawEmblem()
        {
            spriteBatch.Draw(Emblem, new Rectangle(
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
        private void DrawChipSlots()
        {
            for (var si = 0; si < Slots.GetLength(0); si++)
                for (var sj = 0; sj < Slots.GetLength(1); sj++)
                {
                    if (Slots[si, sj] != null)
                    {
                        if (Slots[si, sj].Element != ChipElements.NONE)
                        {
                            var rect = String.Format("CHIPSLOT_{0}_{1}", si + 1, sj + 1);
                            var destrect = RectFromString(CustomWin.Metadata[rect]);
                            if (!Slots[si, sj].IsSelected)
                            {
                                destrect = new Rectangle((int)custPos.X + destrect.X, destrect.Y, 14, 14);
                                spriteBatch.Draw(Slots[si, sj].Icon, destrect, Color.White);
                            }
                            var code = Slots[si, sj].Code.ToString();
                            var startpoint = new Vector2(custPos.X + destrect.X - 1, destrect.Y + destrect.Height);
                            spriteBatch.DrawString(ChipCodesB, code, startpoint, Color.White);
                        }
                    }
                }
        }
        private void DrawSelectedChipSlot()
        {
            var stack = SelectedStack.ToArray();
            Array.Reverse(stack);
            if (stack.Length != 0)
                for (var i = 0; i < stack.Length; i++)
                {
                    var x = stack[i];
                    var rect = String.Format("SELECTSLOT{0}", i + 1);
                    var destrect = RectFromString(CustomWin.Metadata[rect]);
                    destrect = new Rectangle((int)custPos.X + destrect.X, destrect.Y, 14, 14);
                    var crumbsrc = CustomWin.AnimationGroup["CUST"].Frames["SEL_CRUMB"];
                    var crumbdst = new Rectangle(destrect.X - 4, destrect.Y - 2, crumbsrc.Width, crumbsrc.Height);
                    spriteBatch.Draw(CustomWin.texture, crumbdst, crumbsrc, Color.White);
                    spriteBatch.Draw(x.Icon, destrect, Color.White);
                }
        }
        private void DrawBattleChip()
        {
            if (SelectedChip != null)
            {
                var img_vect = new Vector2((int)custPos.X + 16, 24);
                var name_vect = new Vector2((int)custPos.X + 17, 9);
                var code_vect = new Vector2((int)custPos.X + 16, 72);
                var elem_vect = new Vector2((int)custPos.X + 25, 73);
                Rectangle ChipElem;
                if (SelectedChip.Element != ChipElements.NONE)
                {
                    ChipElem = CustomWin.AnimationGroup["CUST"].Frames["TYPE_" + SelectedChip.Element];
                }
                else
                {
                    ChipElem = CustomWin.AnimationGroup["CUST"].Frames["TYPE_NULL"];
                }
                var Dmg_Disp = "";
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
                spriteBatch.Draw(SelectedChip.Image, img_vect, Color.White);
                if (SelectedChip.Element != ChipElements.NONE)
                {
                    spriteBatch.Draw(CustomWin.texture, elem_vect, ChipElem, Color.White);
                    spriteBatch.DrawString(ChipCodesA, SelectedChip.Code.ToString(), code_vect, Color.White);
                    spriteBatch.DrawString(ChipDmg, Dmg_Disp, dmg_vect, Color.White);
                    spriteBatch.DrawString(Fonts.List["Normal"], SelectedChip.DisplayName, name_vect, Color.White);
                }
            }
        }
        internal void RotateEmblem()
        {
            if (IsEmblemRotating) EmblemRot = 0;
            IsEmblemRotating = true;
        }
        private void DisplayBattleChip(IBattleChip BattleChip)
        {
            SelectedChip = BattleChip;
        }
        private void ResetCustBar()
        {
            CBState = CustomBarState.Empty;
            CBTime = TimeSpan.FromMilliseconds(0);
        }
        private void UpdateCustBar()
        {
            if (CBState == CustomBarState.Empty) return;
            var y = DateTime.UtcNow.TimeOfDay - OldCustBarTimeSpan;
            CustomBar.AdvanceAllGroups();
            switch (CBModifier)
            {
                case CustomBarModifiers.Normal:
                    CBTime += y;
                    break;
                case CustomBarModifiers.Slow:
                    CBTime += TimeSpan.FromMilliseconds(y.TotalMilliseconds / 2);
                    break;
                case CustomBarModifiers.Fast:
                    CBTime += TimeSpan.FromMilliseconds(y.TotalMilliseconds * 2);
                    break;
                default:
                    break;
            }
            if (CBState != CustomBarState.Full)
            {
                if (CBTime.TotalSeconds > 10)
                {
                    CBState = CustomBarState.Full;
                    CBTime = TimeSpan.FromMilliseconds(0);
                    CustBarProgress = 1;
                }
                else
                {
                    CustBarProgress = CBTime.TotalMilliseconds / (1000 * 10);
                }
            }
            OldCustBarTimeSpan = DateTime.UtcNow.TimeOfDay;
        }
        private void DrawCustBar()
        {
            //    if (CBState == CustomBarState.Full)
            spriteBatch.Draw(CustomBar.texture, CustBarRect, CustomBar.AnimationGroup["CUSTOMFULL"].CurrentFrame, Color.White);
            var Prog = (int)Math.Floor(128 * CustBarProgress);
            switch (CBState)
            {
                case CustomBarState.Full:
                    break;
                case CustomBarState.Empty:
                    spriteBatch.Draw(CustomBar.texture, new Rectangle(56, 7, 128, 8), CustomBar.AnimationGroup["CUSTOMBAREMPTY"].CurrentFrame, Color.White);
                    break;
                case CustomBarState.Loading:
                    spriteBatch.Draw(CustomBar.texture, new Rectangle(56, 7, 128, 8),
            CustomBar.AnimationGroup["CUSTOMBAREMPTY"].CurrentFrame, Color.White);
                    spriteBatch.Draw(CustomBar.texture, new Rectangle(56, 7, Prog, 8),
            CustomBar.AnimationGroup["CUSTOMBARFILL"].CurrentFrame, Color.White);
                    break;
            }
        }
        #endregion
        #region Setters
        public void Show()
        {
            ((Battle)Parent).FreezeObjects = true;
            showCust = true;
            ChipCodeStr = "";
        }
        public void FinishCustomizing()
        {
            showCust = false;
            ((Battle)Parent).Stage.showCust = false;
            ResetCustBar();
            ShowBattleMessage("Battle Start!");
        }
        public void SetHP(int TargetHP)
        {
            LastHP = MyMath.Clamp(LastHP + TargetHP, 0, MaxHP);
        }
        public void SetFocus(string rectname)
        {
            CurrentFocusRect = RectFromString(CustomWin.Metadata[rectname]);
        }
        #endregion
    }
}