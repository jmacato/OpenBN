﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using OpenBN.ScriptedSprites;
using System;

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
                StagePnlType.NORMAL,StagePnlType.NORMAL,StagePnlType.NORMAL,StagePnlType.POISON,StagePnlType.POISON,StagePnlType.POISON,
                StagePnlType.NORMAL,StagePnlType.NORMAL,StagePnlType.NORMAL,StagePnlType.POISON,StagePnlType.POISON,StagePnlType.POISON,
                StagePnlType.NORMAL,StagePnlType.NORMAL,StagePnlType.NORMAL,StagePnlType.POISON,StagePnlType.POISON,StagePnlType.POISON
            };

        public bool showCust { get; set; }
        public int lolxy = 71;

        public void Draw()
        {

            SB.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

            foreach (Panel Pnl in PanelArray)
            {
                string AnimationGroupKey = "";

                Sprite CurAG = StageRed;
                int layer = 0;

                switch (Pnl.StgPnlClr)
                {
                    case StagePnlColor.Blue:
                        CurAG = StageBlue;
                        break;
                    case StagePnlColor.Red:
                        CurAG = StageRed;
                        layer = 1;

                        break;
                }

                switch (Pnl.StgPnlTyp)
                {
                    case StagePnlType.NORMAL:
                        AnimationGroupKey = "NORM" + Pnl.StgRowCol.X;
                        break;
                    case StagePnlType.HOLE:
                        AnimationGroupKey = "HOLE" + Pnl.StgRowCol.X;
                        break;
                    case StagePnlType.BROKEN:
                        AnimationGroupKey = "BROK" + Pnl.StgRowCol.X;
                        break;
                    case StagePnlType.CRACKED:
                        AnimationGroupKey = "CRAK" + Pnl.StgRowCol.X;
                        break;
                    case StagePnlType.ICE:
                        AnimationGroupKey = "ICED" + Pnl.StgRowCol.X;
                        break;
                    case StagePnlType.GRASS:
                        AnimationGroupKey = "GRAS" + Pnl.StgRowCol.X;
                        break;
                    case StagePnlType.POISON:
                        AnimationGroupKey = "POIS" + Pnl.StgRowCol.X;
                        break;
                    case StagePnlType.NONE:
                        AnimationGroupKey = "NONE";
                        break;
                }

                Texture2D text, bottomtext;
                Rectangle rect, bottomrect;

                text = CurAG.AnimationGroup[AnimationGroupKey].CurrentFrame;
                rect = new Rectangle(Pnl.StgPnlPos.X, Pnl.StgPnlPos.Y, text.Width, text.Height);


                if (Pnl.StgRowCol.X == 2 & Pnl.StgPnlTyp != StagePnlType.NONE)
                {
                    bottomtext = CurAG.AnimationGroup["BOTTOM"].CurrentFrame;
                    bottomrect = new Rectangle(Pnl.StgPnlPos.X, Pnl.StgPnlPos.Y + text.Height, bottomtext.Width, bottomtext.Height);
                    SB.Draw(bottomtext, bottomrect, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                }

                SB.Draw(text, rect, null, Color.White,0, Vector2.Zero, SpriteEffects.None, layer);
            }
            SB.End();
        }

        Sprite StageRed, StageBlue;

        public void Update()
        {
            //Animate the panels if necessary

            if (showCust)
            {
                lolxy = (int)MathHelper.Clamp(lolxy + 2, 71, 87);
                StgPos = new Point(0, lolxy);
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

            if(StageRed != null & StageBlue != null)
            {
                StageRed.AdvanceAllGroups();
                StageBlue.AdvanceAllGroups();
            }

        }

        public Vector2 GetStageCoords(int row, int col, Vector2 offset)
        {
            var y = ((row + 1) * 24) - 5;
            var i = new Vector2(PnlColPnt[col], y);
            var u = offset;
            return i + u + new Vector2(StgPos.X, StgPos.Y);
        }
        
        
        public Stage(ContentManager CMx, GraphicsDevice graphics)
        {
            CM = CMx;

            StgPos = new Point(0, 71);

            StageRed = new Sprite("BattleObj/Stages/Stage.sasl", "BattleObj/Stages/Red", graphics, CM);
            StageBlue = new Sprite("BattleObj/Stages/Stage.sasl", "BattleObj/Stages/Blue", graphics, CM);

           Random xrnd = new Random();

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
                
                    // Test                  
                     StagePnlType xxxx = (StagePnlType)xrnd.Next(6);

                  //  var z = DefaultPnlType[u];
                    var z = xxxx;

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
    }

    public enum StagePnlColor
    {
        Red, Blue, None
    }

    public enum StagePnlType
    {
        NORMAL,
        CRACKED,
        BROKEN,
        POISON,
        ICE,
        GRASS,
        HOLE,
        SANCTUARY,
        ELEV_L, ELEV_R, ELEV_U, ELEV_D,
        NONE
    }
}
