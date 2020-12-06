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
        public Vector2 BtlMsgAnchor = new(Battle.Instance.ScreenRes.W / 2, 72);
        public Vector2 BtlMsgOrigin = new(0, 0);
        public Vector2 BtlMsgBounds = new(0, 0);
        public Vector2 BtlMsgScale = new(1, 1);
        public Keys[] HandledKeys;
        public List<Keys> LatchOnlyKeys;
        public int[] Waitcounter;

        private const int OverflowModulo = 1024;
        private const int Waitcount = 5;
        public int HpState, CurrentHp, LastHp, MaxHp;

        public int SlotColumn, SlotRow;
        public int WaitframeL,WaitframeR,WaitframeU,WaitframeD,WaitframeA,WaitframeB,SelectionStatus;

        public string ChipCodeStr { get; set; }
        public IBattleChip SelectedChip;
        public Stack<IBattleChip> SelectedStack;
        public IBattleChip[,] Slots;
        public CustomBarState CbState;
        public CustomBarModifiers CbModifier;

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
        public TimeSpan CbTime;
        public Dictionary<string, Rectangle> CustTextures;
        public Sprite CustomWin, CustomBar;
        public SpriteFont Hpfnt, HpFontNorm, HpFontCrit, HpFontRecv, ChipCodesA, ChipCodesB, ChipDmg, BtlMsgFont;
        public readonly FontHelper Fonts;
        public bool ShowCust, IsEmblemRotating;
        public double EmblemRot, EmblemScalar, CustBarProgress;
        public Vector2 CustPos, EmblemOrigin, EmblemPos;
        public Rectangle CwSrcRect, HpBarSrcRect, CurrentFocusRect;
        public readonly Rectangle CustBarRect;
        public Texture2D Emblem;
        public TimeSpan OldCustBarTimeSpan;
        public TimeSpan CustBarTimeSpan;

        public enum BtlMsgStatus { None, Grow, Stay, Shrink }

        #endregion


        public CustomWindow(Game parent) : base(parent)
        {
            Fonts = ((Battle)parent).Fonts;
            MaxHp = 9999;
            CurrentHp = MaxHp;
            LastHp = CurrentHp;
            ShowCust = false;
            EmblemRot = MathHelper.TwoPi;
            EmblemScalar = 1;
            CustBarProgress = 0.0f;
            SlotColumn = 1;
            SlotRow = 1;
            WaitframeL = Waitcount - 1;
            WaitframeR = Waitcount - 1;
            WaitframeU = Waitcount - 1;
            WaitframeD = Waitcount - 1;
            WaitframeA = Waitcount - 1;
            WaitframeB = Waitcount - 1;
            SelectionStatus = 0;
            ShowCust = false;
            IsEmblemRotating = false;
            CustBarRect = new Rectangle(48, 0, 144, 16);
            CustPos = new Vector2(-120, 0);
            CustTextures = new Dictionary<string, Rectangle>();
            CustBarTimeSpan = TimeSpan.Zero;
            CbState = CustomBarState.Empty;
            CbModifier = CustomBarModifiers.Normal;
            CbTime = new TimeSpan(0);
            SlotType = new[,] { { 1, 1, 1, 1, 1, 2 }, { 1, 1, 1, 0, 0, 0 } };
            SlotStatus = new int[2, 6];
            Slots = new IBattleChip[2, 6];
            Initialize();
        }

        public void Initialize()
        {
            CustomWin = new Sprite("Misc/Custwindow-SS.sasl", "Misc/Custwindow", Graphics, Content);
            CustomBar = new Sprite("Misc/CustBar.sasl", "Misc/CustBar", Graphics, Content);
            CwSrcRect = CustomWin.AnimationGroup["CUST"].Frames["CUSTWIN"];
            HpBarSrcRect = CustomWin.AnimationGroup["CUST"].Frames["HPBAR"];
            SetFocus("CHIPSLOT_1_1");
            HpFontNorm = Fonts.List["HPFont"];
            HpFontCrit = Fonts.List["HPFontMinus"];
            HpFontRecv = Fonts.List["HPFontPlus"];
            ChipCodesA = Fonts.List["ChipCodesA"];
            ChipCodesB = Fonts.List["ChipCodesB"];
            ChipDmg = Fonts.List["ChipDmg"];
            BtlMsgFont = Fonts.List["BattleMessage"];
            OkButtonHandler = new ButtonDelegate(OkButtonPress);
            HpFontNorm.Spacing = 1;
            HpFontCrit.Spacing = 1;
            HpFontRecv.Spacing = 1;
            ChipCodesA.Spacing = 0;
            ChipCodesB.Spacing = 0;
            BtlMsgFont.Spacing = 0;
            Hpfnt = HpFontNorm;
            Emblem = Content.Load<Texture2D>("Navi/MM/Emblem");
            EmblemOrigin = new Vector2((float)Math.Ceiling((float)Emblem.Width), (float)Math.Ceiling((float)Emblem.Height)) / 2;
            EmblemPos = new Vector2(104, 12);
            EmblemRot = 0;
            var y = new ChipIconProvider(Content, Graphics);
            /*  For testing only */
            Slots[0, 0] = new TestBattleChip(54, y, Content, "Static", 20, ChipElements.Wind, '@');
            Slots[0, 1] = new TestBattleChip(69, y, Content, "PoisSeed", -2, ChipElements.Null, '@');
            Slots[0, 2] = new TestBattleChip(70, y, Content, "Sword", 80, ChipElements.Sword, 'A');
            Slots[0, 3] = new TestBattleChip(71, y, Content, "WideSwrd", 80, ChipElements.Sword, 'B');
            Slots[0, 4] = new TestBattleChip(72, y, Content, "LongSwrd", 100, ChipElements.Sword, 'C');
            Slots[1, 0] = new TestBattleChip(57, y, Content, "MachGun3", 70, ChipElements.Target, 'A');
            Slots[1, 1] = new TestBattleChip(61, y, Content, "MegEnBom", 80, ChipElements.Null, 'B');
            Slots[1, 2] = new TestBattleChip(66, y, Content, "BugBomb", 100, ChipElements.Null, 'C');
            Slots[0, 5] = new CustomWindowButtons(SelectionStatus, Content, OkButtonHandler);
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
            Waitcounter = new int[HandledKeys.Length];
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
            UpdateHpBar();
            UpdateCustBar();
            UpdateBtlMsg();
            if (ShowCust)
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
            if (CustPos.X != -120)
            {
                DrawCustWindow();
                DrawChipSlots();
                DrawSelectedChipSlot();
                DrawEmblem();
                DrawBattleChip();
                DrawFocusRects();
            }
            DrawHpBar();
            if (CustPos.X == -120)
            {
                DrawBtlMsg();
                DrawCustBar();
            }
        }



        public delegate void ButtonDelegate();
        public ButtonDelegate OkButtonHandler;
        public void OkButtonPress()
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
            BattleMsgStatus = BtlMsgStatus.Grow;
        }
        private void UpdateSelectedStack()
        {
            //A bit hacky but its enough
            SelectionStatus = Clamp(SelectedStack.Count, 0, 1);
            for (var si = 0; si < Slots.GetLength(0); si++)
                for (var sj = 0; sj < Slots.GetLength(1); sj++)
                {
                    var chkslot = Slots[si, sj];
                    if (chkslot != null)
                    {
                        if (chkslot.GetType().Name == "CustomWindowButtons")
                            ((CustomWindowButtons)Slots[si, sj]).SetStatus = SelectionStatus;
                    }
                }
        }
        private void UpdateBtlMsg()
        {
            switch (BattleMsgStatus)
            {
                case BtlMsgStatus.Grow:
                    if (CustPos.X != -120) break;
                    if (BtlMsgCounter < HeightScaleSpline.Length)
                    {
                        BtlMsgScale.Y = HeightScaleSpline[BtlMsgCounter];
                        BtlMsgCounter++;
                        BtlMsgDrawFlag = true;
                    }
                    else if (BtlMsgCounter >= HeightScaleSpline.Length)
                    {
                        BtlMsgCounter = 0;
                        BattleMsgStatus = BtlMsgStatus.Stay;
                    }
                    break;
                case BtlMsgStatus.Stay:
                    BtlMsgCounter++;
                    if (BtlMsgCounter > BtlMsgWaitFrames)
                    {
                        BattleMsgStatus = BtlMsgStatus.Shrink;
                        BtlMsgCounter = HeightScaleSpline.Length - 1;
                    }
                    break;
                case BtlMsgStatus.Shrink:
                    if (BtlMsgCounter >= 0)
                    {
                        BtlMsgScale.Y = HeightScaleSpline[BtlMsgCounter];
                        BtlMsgCounter--;
                    }
                    else if (BtlMsgCounter < 0)
                    {
                        BtlMsgCounter = 0;
                        BattleMsgStatus = BtlMsgStatus.None;
                        BtlMsgDrawFlag = false;

                        UnfreezeObjects();
                    }
                    break;
            }
        }

        void UnfreezeObjects()
        {
            OldCustBarTimeSpan = DateTime.UtcNow.TimeOfDay;
            CbState = CustomBarState.Loading;
            ((Battle)Parent).UnfreezeObjects();
        }

        private void DrawBtlMsg()
        {
            if (BattleMsgStatus != BtlMsgStatus.None && BtlMsgDrawFlag)
            {
                SpriteBatch.DrawString(
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
                var keyWaitThresholdMs = 80;
                for (var i = 0; i < HandledKeys.Length; i++)
                {
                    var key = HandledKeys[i];
                    var keyStream = Input.KbStream[key];
                    switch (keyStream.KeyState)
                    {
                        case KeyState.Down:
                            if (keyStream.DurDelta > keyWaitThresholdMs)
                            {
                                if (!LatchOnlyKeys.Contains(key))
                                {
                                    Waitcounter[i]++;
                                    Battle.KeyLatch[key] = false;
                                    if (Waitcounter[i] % Waitcount == 0)
                                    {
                                        ExecuteKeyCommand(key);
                                        Waitcounter[i] = 0;
                                    }
                                }
                                {
                                    if (Battle.KeyLatch[key] == false)
                                    {
                                        Battle.KeyLatch[key] = true;
                                    }
                                }
                            }
                            if (Battle.KeyLatch[key] == false)
                            {
                                Battle.KeyLatch[key] = true;
                            }
                            break;
                        case KeyState.Up:
                            if (Battle.KeyLatch[key] == true)
                            {
                                Battle.KeyLatch[key] = false;
                                ExecuteKeyCommand(key);
                                Waitcounter[i] = 0;
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
        private void DeselectChip(int slotRow, int slotColumn)
        {
            if (SelectedStack.Count > 0)
            {
                var t = SelectedStack.Pop();
                t.IsSelected = false;
            }
        }
        private void SelectChip(int slotRow, int slotColumn)
        {
            if (Slots[slotRow, slotColumn].IsSelected) return;
            if (SelectedStack.Count + 1 > 5) return;
            if (Slots[slotRow, slotColumn].GetType().Name == "CustomWindowButtons")
            {
                Slots[slotRow, slotColumn].Execute(this, (Battle)Parent, null);
                return;
            }
            RotateEmblem();
            Slots[slotRow, slotColumn].IsSelected = true;
            SelectedStack.Push(Slots[slotRow, slotColumn]);
        }
        private void TraverseChipSlots(int row, int col)
        {
            var tmpSlotRow = Clamp(row, 1, SlotType.GetLength(0));
            var tmpSlotColumn = InverseClamp(col, 1, SlotType.GetLength(1));
            var curSlotType = SlotType[tmpSlotRow - 1, tmpSlotColumn - 1];
            var curSlotStat = SlotStatus[tmpSlotRow - 1, tmpSlotColumn - 1];
            switch (curSlotType)
            {
                case 1:
                    if (curSlotStat != 0)
                    {
                        SetFocus(String.Format("CHIPSLOT_{0}_{1}", tmpSlotRow, tmpSlotColumn));
                        DisplayBattleChip(Slots[tmpSlotRow - 1, tmpSlotColumn - 1]);
                        SlotRow = tmpSlotRow;
                        SlotColumn = tmpSlotColumn;
                    }
                    break;
                case 2:
                    SetFocus("OKBUTTON");
                    DisplayBattleChip(Slots[0, 5]);
                    SlotRow = tmpSlotRow;
                    SlotColumn = tmpSlotColumn;
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
        private void UpdateHpBar()
        {
            switch (HpState)
            {
                case 1:
                    Hpfnt = HpFontCrit;
                    break;
                case 2:
                    Hpfnt = HpFontRecv;
                    break;
                default:
                    Hpfnt = HpFontNorm;
                    break;
            }
            if (CurrentHp >= MaxHp * 0.20)
                HpState = 0;
            else
                HpState = 1;
            if (CurrentHp != LastHp)
            {
                if (CurrentHp < LastHp)
                {
                    CurrentHp = MyMath.Clamp(CurrentHp + 9, CurrentHp, LastHp);
                    HpState = 2;
                }
                else if (CurrentHp > LastHp)
                {
                    CurrentHp = MyMath.Clamp(CurrentHp - 9, LastHp, CurrentHp);
                    HpState = 1;
                }
            }
        }
        private void UpdateTransition()
        {
            if (ShowCust)
            {
                if (CustPos.X != 0)
                    CustPos.X = MyMath.Clamp(CustPos.X + 15, -120, 0);
            }
            else
            {
                if (CustPos.X != -120)
                    CustPos.X = MyMath.Clamp(CustPos.X - 15, -120, 0);
            }
        }
        #endregion
        #region Draw Subroutines
        private void DrawFocusRects()
        {
            var x = CustPos.X + CurrentFocusRect.X;
            var y = CurrentFocusRect.Y;
            var w = CurrentFocusRect.Width;
            var h = CurrentFocusRect.Height;
            var textTl = CustomWin.AnimationGroup["CURSOR0_TL"].CurrentFrame;
            var textTr = CustomWin.AnimationGroup["CURSOR0_TR"].CurrentFrame;
            var textBl = CustomWin.AnimationGroup["CURSOR0_BL"].CurrentFrame;
            var textBr = CustomWin.AnimationGroup["CURSOR0_BR"].CurrentFrame;
            var of = 4;
            var tl = new Vector2(x, y) - new Vector2(of, of);
            var tr = new Vector2(x + w - 8, y) - new Vector2(-of, of);
            var bl = new Vector2(x + w - 8, y + h - 8) - new Vector2(-of, -of);
            var br = new Vector2(x, y + h - 8) - new Vector2(of, -of);
            SpriteBatch.Draw(CustomWin.Texture, tl, textTl, Color.White);
            SpriteBatch.Draw(CustomWin.Texture, tr, textTr, Color.White);
            SpriteBatch.Draw(CustomWin.Texture, bl, textBl, Color.White);
            SpriteBatch.Draw(CustomWin.Texture, br, textBr, Color.White);
        }
        private void DrawCustWindow()
        {
            var y = new Rectangle((int)CustPos.X, 0, CwSrcRect.Width, CwSrcRect.Height);
            SpriteBatch.Draw(CustomWin.Texture, y, CwSrcRect, Color.White);
        }
        private void DrawHpBar()
        {
            var hprct = new Rectangle((int)CustPos.X + 122, 0, HpBarSrcRect.Width, HpBarSrcRect.Height);
            SpriteBatch.Draw(CustomWin.Texture, hprct, HpBarSrcRect, Color.White);
            var hptextX = (int)Hpfnt.MeasureString(CurrentHp.ToString()).X;
            var hptxtrct = new Vector2(hprct.X + (hprct.Width - hptextX) - 6, hprct.Y);
            SpriteBatch.DrawString(Hpfnt,
                CurrentHp.ToString(),
                hptxtrct,
                Color.White);
        }
        private void DrawEmblem()
        {
            SpriteBatch.Draw(Emblem, new Rectangle(
                (int)(CustPos.X + EmblemPos.X), (int)EmblemPos.Y,
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
                        if (Slots[si, sj].Element != ChipElements.None)
                        {
                            var rect = String.Format("CHIPSLOT_{0}_{1}", si + 1, sj + 1);
                            var destrect = RectFromString(CustomWin.Metadata[rect]);
                            if (!Slots[si, sj].IsSelected)
                            {
                                destrect = new Rectangle((int)CustPos.X + destrect.X, destrect.Y, 14, 14);
                                SpriteBatch.Draw(Slots[si, sj].Icon, destrect, Color.White);
                            }
                            var code = Slots[si, sj].Code.ToString();
                            var startpoint = new Vector2(CustPos.X + destrect.X - 1, destrect.Y + destrect.Height);
                            SpriteBatch.DrawString(ChipCodesB, code, startpoint, Color.White);
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
                    destrect = new Rectangle((int)CustPos.X + destrect.X, destrect.Y, 14, 14);
                    var crumbsrc = CustomWin.AnimationGroup["CUST"].Frames["SEL_CRUMB"];
                    var crumbdst = new Rectangle(destrect.X - 4, destrect.Y - 2, crumbsrc.Width, crumbsrc.Height);
                    SpriteBatch.Draw(CustomWin.Texture, crumbdst, crumbsrc, Color.White);
                    SpriteBatch.Draw(x.Icon, destrect, Color.White);
                }
        }
        private void DrawBattleChip()
        {
            if (SelectedChip != null)
            {
                var imgVect = new Vector2((int)CustPos.X + 16, 24);
                var nameVect = new Vector2((int)CustPos.X + 17, 9);
                var codeVect = new Vector2((int)CustPos.X + 16, 72);
                var elemVect = new Vector2((int)CustPos.X + 25, 73);
                Rectangle chipElem;
                if (SelectedChip.Element != ChipElements.None)
                {
                    chipElem = CustomWin.AnimationGroup["CUST"].Frames["TYPE_" + SelectedChip.Element];
                }
                else
                {
                    chipElem = CustomWin.AnimationGroup["CUST"].Frames["TYPE_NULL"];
                }
                var dmgDisp = "";
                switch (SelectedChip.Damage)
                {
                    case -1:
                        dmgDisp = "///";
                        break;
                    case -2:
                        dmgDisp = "";
                        break;
                    default:
                        dmgDisp = SelectedChip.Damage.ToString();
                        break;
                }
                var dmgMs = 71 - ChipDmg.MeasureString(dmgDisp).X;
                var dmgVect = new Vector2((int)CustPos.X + dmgMs, 75);
                SpriteBatch.Draw(SelectedChip.Image, imgVect, Color.White);
                if (SelectedChip.Element != ChipElements.None)
                {
                    SpriteBatch.Draw(CustomWin.Texture, elemVect, chipElem, Color.White);
                    SpriteBatch.DrawString(ChipCodesA, SelectedChip.Code.ToString(), codeVect, Color.White);
                    SpriteBatch.DrawString(ChipDmg, dmgDisp, dmgVect, Color.White);
                    SpriteBatch.DrawString(Fonts.List["Normal"], SelectedChip.DisplayName, nameVect, Color.White);
                }
            }
        }
        internal void RotateEmblem()
        {
            if (IsEmblemRotating) EmblemRot = 0;
            IsEmblemRotating = true;
        }
        private void DisplayBattleChip(IBattleChip battleChip)
        {
            SelectedChip = battleChip;
        }
        private void ResetCustBar()
        {
            CbState = CustomBarState.Empty;
            CbTime = TimeSpan.FromMilliseconds(0);
        }
        private void UpdateCustBar()
        {
            if (CbState == CustomBarState.Empty) return;
            var y = DateTime.UtcNow.TimeOfDay - OldCustBarTimeSpan;
            CustomBar.AdvanceAllGroups();
            switch (CbModifier)
            {
                case CustomBarModifiers.Normal:
                    CbTime += y;
                    break;
                case CustomBarModifiers.Slow:
                    CbTime += TimeSpan.FromMilliseconds(y.TotalMilliseconds / 2);
                    break;
                case CustomBarModifiers.Fast:
                    CbTime += TimeSpan.FromMilliseconds(y.TotalMilliseconds * 2);
                    break;
                default:
                    break;
            }
            if (CbState != CustomBarState.Full)
            {
                if (CbTime.TotalSeconds > 10)
                {
                    CbState = CustomBarState.Full;
                    CbTime = TimeSpan.FromMilliseconds(0);
                    CustBarProgress = 1;
                }
                else
                {
                    CustBarProgress = CbTime.TotalMilliseconds / (1000 * 10);
                }
            }
            OldCustBarTimeSpan = DateTime.UtcNow.TimeOfDay;
        }
        private void DrawCustBar()
        {
            //    if (CBState == CustomBarState.Full)
            SpriteBatch.Draw(CustomBar.Texture, CustBarRect, CustomBar.AnimationGroup["CUSTOMFULL"].CurrentFrame, Color.White);
            var prog = (int)Math.Floor(128 * CustBarProgress);
            switch (CbState)
            {
                case CustomBarState.Full:
                    break;
                case CustomBarState.Empty:
                    SpriteBatch.Draw(CustomBar.Texture, new Rectangle(56, 7, 128, 8), CustomBar.AnimationGroup["CUSTOMBAREMPTY"].CurrentFrame, Color.White);
                    break;
                case CustomBarState.Loading:
                    SpriteBatch.Draw(CustomBar.Texture, new Rectangle(56, 7, 128, 8),
            CustomBar.AnimationGroup["CUSTOMBAREMPTY"].CurrentFrame, Color.White);
                    SpriteBatch.Draw(CustomBar.Texture, new Rectangle(56, 7, prog, 8),
            CustomBar.AnimationGroup["CUSTOMBARFILL"].CurrentFrame, Color.White);
                    break;
            }
        }
        #endregion
        #region Setters
        public void Show()
        {
            ((Battle)Parent).FreezeObjects = true;
            ShowCust = true;
            ChipCodeStr = "";
        }
        public void FinishCustomizing()
        {
            ShowCust = false;
            ((Battle)Parent).Stage.ShowCust = false;
            ResetCustBar();
            ShowBattleMessage("Battle Start!");
        }
        public void SetHp(int targetHp)
        {
            LastHp = MyMath.Clamp(LastHp + targetHp, 0, MaxHp);
        }
        public void SetFocus(string rectname)
        {
            CurrentFocusRect = RectFromString(CustomWin.Metadata[rectname]);
        }
        #endregion
    }
}