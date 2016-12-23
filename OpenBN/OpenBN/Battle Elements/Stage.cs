using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace OpenBN
{

    public class Stage : IBattleEntity
    {
        public string ID { get; set; }
        public Point StgPos { get; set; }
        public SpriteBatch SB { get; set; }
        public ContentManager CM { get; set; }

        //List of Top-Left Corners of the panels
        public List<int> PnlRowPnt = new List<int> { 0, 24, 48 };
        public List<int> PnlColPnt = new List<int> { 0, 40, 80, 120, 160, 200 };
        public Point BottomLeftPnt = new Point(1, 1);
        public List<Panel> PanelArray = new List<Panel>();

        List<StagePnlColor> DefaultPnlColr = new List<StagePnlColor>
            {
                StagePnlColor.Red,StagePnlColor.Red,StagePnlColor.Red ,StagePnlColor.Blue,StagePnlColor.Blue,StagePnlColor.Blue,
                StagePnlColor.Red,StagePnlColor.Red,StagePnlColor.Red ,StagePnlColor.Red,StagePnlColor.Blue,StagePnlColor.Blue,
                StagePnlColor.Red,StagePnlColor.Red,StagePnlColor.Red ,StagePnlColor.Blue,StagePnlColor.Blue,StagePnlColor.Blue,
            };

        List<StagePnlType> DefaultPnlType = new List<StagePnlType>
            {
                StagePnlType.NORMAL,StagePnlType.NORMAL,StagePnlType.NORMAL,StagePnlType.NORMAL,StagePnlType.NORMAL,StagePnlType.NORMAL,
                StagePnlType.NORMAL,StagePnlType.NORMAL,StagePnlType.NORMAL,StagePnlType.NORMAL,StagePnlType.NORMAL,StagePnlType.NORMAL,
                StagePnlType.NORMAL,StagePnlType.NORMAL,StagePnlType.NORMAL,StagePnlType.NORMAL,StagePnlType.NORMAL,StagePnlType.NORMAL
            };

        public bool showCust { get; set; }
        public int lolxy = 71;

        public void Draw()
        {
            if (showCust)
            {
                lolxy = (int)MathHelper.Clamp(lolxy + 2, 71, 87);
                StgPos = new Point(0,lolxy);
            }
            else
            {
                lolxy = (int)MathHelper.Clamp(lolxy - 2, 71, 87);
                StgPos = new Point(0, lolxy);
            }

            for (int i = 0; i < 3; i++) // For each row
            {
                for (int j = 0; j < 6; j++) // For each col
                {
                    //Get the linear index of the col/row pair
                    var u = GetIndex(i, j);
                    //Designate colors accrd. to DefaultPnlColr
                    var x = DefaultPnlColr[u];
                    //Designate specific pos with offset of the StgPos
                    var y = new Point(PnlColPnt[j] + StgPos.X, PnlRowPnt[i] + StgPos.Y);
                    PanelArray[u].StgPnlPos = y;
                }
            }



            //Draw the red squares first coz blue panels takes the higher z-order
            //on the game
            foreach (Panel Pnl in PanelArray)
            {
                if (Pnl.StgPnlClr == StagePnlColor.Red)
                {
                    var rect = new Rectangle(Pnl.StgPnlPos.X, Pnl.StgPnlPos.Y, Pnl.PnlTexture.Width, Pnl.PnlTexture.Height);
                    SB.Draw(Pnl.PnlTexture, rect, null, Color.White);
                }
            }
            foreach (Panel Pnl in PanelArray)
            {
                if (Pnl.StgPnlClr == StagePnlColor.Blue)
                {
                    var rect = new Rectangle(Pnl.StgPnlPos.X, Pnl.StgPnlPos.Y, Pnl.PnlTexture.Width, Pnl.PnlTexture.Height);
                    SB.Draw(Pnl.PnlTexture, rect, null, Color.White);
                }
            }
        }

        public void Update()
        {
            //Animate the panels if necessary
        }

        public Vector2 GetStageCoords(int row, int col, Vector2 offset)
        {
            var y = ((row + 1) * 24) - 5;
            var i = new Vector2(PnlColPnt[col], y);
            var u = offset;
            return i + u + new Vector2(StgPos.X, StgPos.Y);
        }



        public Stage(ContentManager CMx)
        {
            CM = CMx;

            StgPos = new Point(0, 71);

            for (int i = 0; i < 3; i++) // For each row
            {
                for (int j = 0; j < 6; j++) // For each col
                {
                    //Get the linear index of the col/row pair
                    var u = GetIndex(i, j);
                    //Designate colors accrd. to DefaultPnlColr
                    var x = DefaultPnlColr[u];
                    //Designate specific pos with offset of the StgPos
                    var y = new Point(PnlColPnt[j] + StgPos.X, PnlRowPnt[i] + StgPos.Y);
                    //Set panel type, could be modified on code
                    var z = DefaultPnlType[u];
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

            //Assign panel textures
            foreach (Panel Pnl in PanelArray)
            {
                if (Pnl.StgPnlClr == StagePnlColor.Red)
                {
                    Pnl.PnlTexture = CM.Load<Texture2D>("BattleObj/STGR" + Pnl.StgRowCol.X);
                }

                if (Pnl.StgPnlClr == StagePnlColor.Blue)
                {
                    Pnl.PnlTexture = CM.Load<Texture2D>("BattleObj/STGB" + Pnl.StgRowCol.X);
                }
            }
        }


        public int GetIndex(int i, int j)
        {
            //  Stage are indexed like this
            //  0 | 1 | 2 | 3 | 4 | 5
            //  6 | 7 | 8 | 9 | 10| 11
            // 12 | 13| 14| 15| 16| 17
            // Hence the need of this formula :v
            return (((i + 1) * 6) - 5) + (j - 1);
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


    public class Panel
    {
        public StagePnlColor StgPnlClr { get; set; }
        public StagePnlType StgPnlTyp { get; set; }
        public Point StgPnlPos { get; set; }
        public Point StgRowCol { get; set; }
        public Texture2D PnlTexture { get; set; }
    }

    public enum StagePnlColor
    {
        Red, Blue, None
    }

    public enum StagePnlType
    {
        NORMAL,
        CRACKED,
        CRACKED_HOLE,
        POISON,
        AQUA,
        GRASS,
        SANCTUARY,
        PERM_HOLE,
        ELEV_L, ELEV_R, ELEV_U, ELEV_D,
    }
}
