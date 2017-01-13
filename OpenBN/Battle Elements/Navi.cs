using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
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
            NaviSprite.AdvanceAllGroups();
            this.AnimationFinished = !NaviSprite.AnimationGroup[CurrentAnimation].Active;
            HandleInputs();
            base.Update(gameTime);
        }

        public void HandleInputs()
        {
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
