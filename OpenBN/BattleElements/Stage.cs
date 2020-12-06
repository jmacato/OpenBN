using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OpenBN.Interfaces;
using OpenBN.Sprites;
using Point = OpenBN.Interfaces.Point;

namespace OpenBN.BattleElements
{

    public class Stage : BattleModule
    {
        public Point StgPos { get; set; }

        //List of Top-Left Corners of the panels
        public List<int> PnlRowPnt, PnlColPnt;
        public Point BottomLeftPnt;
        public List<Panel> PanelArray;
        List<StagePnlColor> _defaultPnlColr;
        List<StagePnlType> _defaultPnlType;

        public bool ShowCust { get; set; }
        public int Lolxy = 71;

        public override void Draw()
        {
            base.Draw();
            foreach (var pnl in PanelArray)
            {
                var animationGroupKey = "";

                var curAg = _stageRed;

                switch (pnl.StgPnlClr)
                {
                    case StagePnlColor.Blue:
                        curAg = _stageBlue;
                        break;
                    case StagePnlColor.Red:
                        curAg = _stageRed;
                        break;
                }

                animationGroupKey = pnl.StgPnlTyp.ToString() + pnl.StgRowCol.X;

                Rectangle text, bottomtext;
                Rectangle rect, bottomrect;
                text = curAg.AnimationGroup[animationGroupKey].CurrentFrame;
                rect = new Rectangle(pnl.StgPnlPos.X, pnl.StgPnlPos.Y, text.Width, text.Height);
                if (pnl.StgRowCol.X == 2 & pnl.StgPnlTyp != StagePnlType.None)
                {
                    bottomtext = curAg.AnimationGroup["BOTTOM"].CurrentFrame;
                    bottomrect = new Rectangle(pnl.StgPnlPos.X, pnl.StgPnlPos.Y + text.Height, bottomtext.Width, bottomtext.Height);
                    SpriteBatch.Draw(curAg.Texture, bottomrect, bottomtext, Color.White);
                }
                SpriteBatch.Draw(curAg.Texture, rect, text, Color.White);
            }
        }

        Sprite _stageRed, _stageBlue;
        internal static int StageFloorPadding = 5;
        internal static int StagePanelHeight = 25;
        internal static int StagePanelWidth = 40;

        public override void Update(GameTime gameTime)
        {
            //Animate the panels if necessary
            base.Update(gameTime);

            if (ShowCust)
            {
                Lolxy = (int)MathHelper.Clamp(Lolxy + 2, 71, 87);
                StgPos = new Point(0, Lolxy);
            }
            else
            {
                Lolxy = (int)MathHelper.Clamp(Lolxy - 2, 71, 87);
                StgPos = new Point(0, Lolxy);
            }


            for (var i = 0; i < 3; i++) // For each row
            {
                for (var j = 0; j < 6; j++) // For each col
                {
                    //Get the linear index of the col/row pair
                    var u = GetIndex(i, j);
                    //Designate colors accrd. to DefaultPnlColr
                    var x = _defaultPnlColr[u];
                    //Designate specific pos with offset of the StgPos
                    var y = new Point(PnlColPnt[j] + StgPos.X, PnlRowPnt[i] + StgPos.Y);
                    PanelArray[u].StgPnlPos = y;
                }
            }

            _stageRed.AdvanceAllGroups();
            _stageBlue.AdvanceAllGroups();

        }

        public Vector2 GetStageCoords(int row, int col, Vector2 offset)
        {
            var y = (row + 1) * 24 - 5;
            var i = new Vector2(PnlColPnt[col], y);
            var u = offset;
            return i + u + new Vector2(StgPos.X, StgPos.Y);
        }

        public void Initialize()
        {
            StgPos = new Point(0, 71);
            _stageRed = new Sprite("BattleObj/Stages/Stage.sasl", "BattleObj/Stages/Red", Graphics, Content);
            _stageBlue = new Sprite("BattleObj/Stages/Stage.sasl", "BattleObj/Stages/Blue", Graphics, Content);

            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 6; j++)
                {
                    //Get the linear index of the col/row pair
                    var u = GetIndex(i, j);
                    //Designate colors accrd. to DefaultPnlColr
                    var x = _defaultPnlColr[u];
                    //Designate specific pos with offset of the StgPos
                    var y = new Point(PnlColPnt[j] + StgPos.X, PnlRowPnt[i] + StgPos.Y);
                    //Set panel type, could be modified on code
                    var z = _defaultPnlType[u];

                    var e = new Point(i, j);
                    var q = new Panel()
                    {
                        StgPnlClr = x,
                        StgPnlPos = y,
                        StgPnlTyp = z,
                        StgRowCol = e
                    };
                    PanelArray.Add(q);
                }
            }

            Initialized = true;
        }

        public Stage(Game parent) : base(parent)
        {

            _defaultPnlType = new List<StagePnlType>
            {
                StagePnlType.Normal,StagePnlType.Ice,StagePnlType.Grass,StagePnlType.Poison,StagePnlType.Holy,StagePnlType.Hole,
                StagePnlType.Normal,StagePnlType.Ice,StagePnlType.Grass,StagePnlType.Poison,StagePnlType.Holy,StagePnlType.Hole,
                StagePnlType.Normal,StagePnlType.Ice,StagePnlType.Grass,StagePnlType.Poison,StagePnlType.Holy,StagePnlType.Hole,
            };

            _defaultPnlColr = new List<StagePnlColor>
            {
                StagePnlColor.Red,StagePnlColor.Red,StagePnlColor.Red ,StagePnlColor.Blue,StagePnlColor.Blue,StagePnlColor.Blue,
                StagePnlColor.Red,StagePnlColor.Red,StagePnlColor.Red ,StagePnlColor.Blue,StagePnlColor.Blue,StagePnlColor.Blue,
                StagePnlColor.Red,StagePnlColor.Red,StagePnlColor.Red ,StagePnlColor.Blue,StagePnlColor.Blue,StagePnlColor.Blue,
            };

            PnlRowPnt = new List<int> { 0, 24, 48 };
            PnlColPnt = new List<int> { 0, 40, 80, 120, 160, 200 };
            BottomLeftPnt = new Point(1, 1);
            PanelArray = new List<Panel>();

            Initialize();
        }

        public int GetIndex(int i, int j)
        {
            //  Stage are indexed like this
            //  0 | 1 | 2 | 3 | 4 | 5
            //  6 | 7 | 8 | 9 | 10| 11
            // 12 | 13| 14| 15| 16| 17
            // Hence the need of this formula :v
            return (i + 1) * 6 - 5 + (j - 1);
        }

        public bool IsMoveAllowed(int i, int j)
        {
            var t = GetIndex(i, j);

            if (t >= 0 && t <= 17)
            {
                if (PanelArray[t].StgPnlClr == StagePnlColor.Red)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
