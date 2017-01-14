using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OpenBN
{

    /// <summary>
    /// Base class for In-battle Navis
    /// </summary>
    public class Navi : BattleComponent
    {
        int Row, Column;
        private Sprite NaviSprite;
        private string CurrentAnimation;
        public bool AnimationFinished;
        private Stage stage;
        private Rectangle FirstFrameRect;
        public int waitframe_l,
                waitframe_r,
                waitframe_u,
                waitframe_d,
                waitframe_a,
                waitframe_b;

        private const int OVERFLOW_MODULO = 1024;
        private const int WAITCOUNT = 7;

        private bool _Freezed;

        public bool Freezed
        {
            get
            {
                if (Parent != null)
                {
                    Freezed = (Parent.FreezeObjects);

                } else
                {
                    Freezed = false;
                }
                return _Freezed;
            }
            set
            {
                _Freezed = value;
            }
        }
   
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBN.Navi"/> class.
        /// </summary>
        /// <param name="game">Game.</param>
        /// <param name="stage">Stage.</param>
        public Navi(Game game, Stage stage) : base(game)
        {
            CurrentAnimation = "MM";
            Row = 2;
            Column = 1;
            NaviSprite = new Sprite("Navi/MM/MM.sasl", "Navi/MM/MM", Graphics, Content);
            this.stage = stage;
            FirstFrameRect = NaviSprite.AnimationGroup.Values.First().Frames.Values.First();

            waitframe_l = 6;
            waitframe_r = 6;
            waitframe_u = 6;
            waitframe_d = 6;
            waitframe_a = 6;
            waitframe_b = 6;
        }

        /// <summary>
        /// Changes the animation.
        /// </summary>
        public void ChangeAnimation()
        {
            var rand = new Random();
            int r;
            string t;
            r = rand.Next(0, NaviSprite.AnimationGroup.Keys.Count - 1);
            t = NaviSprite.AnimationGroup.Keys.ToArray()[r];
            CurrentAnimation = t; // "MM_FIRE_RECOIL";
            NaviSprite.ResetAllGroups();
        }

        /// <summary>
        /// Update the specified gameTime.
        /// </summary>
        /// <param name="gameTime">Game time.</param>
        public override void Update(GameTime gameTime)
        {
            if (Freezed) { base.Update(gameTime); return; }

            NaviSprite.AdvanceAllGroups();
            this.AnimationFinished = !NaviSprite.AnimationGroup[CurrentAnimation].Active;
            HandleInputs();
            base.Update(gameTime);
        }



        public void HandleInputs()
        {

            var KEY_LEFT = Input.KbStream[Keys.Left];
            var KEY_RIGHT = Input.KbStream[Keys.Right];
            var KEY_UP = Input.KbStream[Keys.Up];
            var KEY_DOWN = Input.KbStream[Keys.Down];
            var KEY_A = Input.KbStream[Keys.A];
            var KEY_B = Input.KbStream[Keys.S];

            switch (KEY_LEFT.KeyState)
            {
                case KeyState.Down:
                    waitframe_l++;
                    {
                        if (waitframe_l % WAITCOUNT == 0)
                        {
                            Debug.Print("KEY_LEFT");
                        }
                    }
                    break;
                case KeyState.Up:
                    waitframe_l = waitframe_l % OVERFLOW_MODULO;
                    break;
            }

            switch (KEY_RIGHT.KeyState)
            {
                case KeyState.Down:
                    waitframe_r++;
                    if (waitframe_r % WAITCOUNT == 0)
                    {
                        Debug.Print("KEY_RIGHT");
                    }
                    break;
                case KeyState.Up:
                    waitframe_r = waitframe_r % OVERFLOW_MODULO;
                    break;
            }

            switch (KEY_UP.KeyState)
            {
                case KeyState.Down:
                    waitframe_u++;
                    if (waitframe_u % WAITCOUNT == 0)
                    {
                        Debug.Print("KEY_UP");
                    }
                    break;
                case KeyState.Up:
                    waitframe_u = waitframe_u % OVERFLOW_MODULO;
                    break;
            }

            switch (KEY_DOWN.KeyState)
            {
                case KeyState.Down:
                    waitframe_d++;
                    if (waitframe_d % WAITCOUNT == 0)
                    {
                        Debug.Print("KEY_DOWN");
                    }
                    break;
                case KeyState.Up:
                    waitframe_d = waitframe_d % OVERFLOW_MODULO;
                    break;
            }



            //			switch (KEY_DOWN.KeyState)
            //			{
            //			case KeyState.Down:
            //				waitframe_d++;
            //				if (waitframe_d%WAITCOUNT == 0)
            //				{
            //					TraverseChipSlots(SlotRow + 1, SlotColumn);
            //				}
            //				break;
            //			case KeyState.Up:
            //				waitframe_d = waitframe_d%OVERFLOW_MODULO;
            //				break;
            //			}



        }

        /// <summary>
        /// Draw this instance.
        /// </summary>
        public override void Draw()
        {
            base.Draw();
            Rectangle up = NaviSprite.AnimationGroup[CurrentAnimation].CurrentFrame;

            var t = stage.StgPos.Y + stage.PnlRowPnt[Row] - Stage.StageFloorPadding;
            var o = 20 - (FirstFrameRect.Width / 2);
            var x = stage.StgPos.X + stage.PnlColPnt[Column] + o;
            Rectangle destrect = new Rectangle(x, t, up.Width, up.Height);

            spriteBatch.Draw(NaviSprite.texture, destrect, up, Color.White, 0,
                new Vector2(0, up.Height),
                SpriteEffects.None, 0);
        }

    }

 
}
