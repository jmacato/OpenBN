using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenBN.Interfaces;
using OpenBN.Sprites;

namespace OpenBN.BattleElements
{

    /// <summary>
    /// Base class for In-battle Navis
    /// </summary>
    public class Navi : BattleModule
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
        private const int WAITCOUNT = 4;

        private bool _Freezed;
        public bool Freezed
        {
            get
            {
                if (Parent != null)
                {
                    Freezed = ((Battle)Parent).FreezeObjects;

                }
                else
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
        /// Initializes a new instance of the <see cref="Navi"/> class.
        /// </summary>
        /// <param name="game">Game.</param>
        /// <param name="stage">Stage.</param>
        public Navi(Game game, Stage stage) : base(game)
        {
            CurrentAnimation = "MM";
            Row = 1;
            Column = 1;
            NaviSprite = new Sprite("Navi/MM/MM.sasl", "Navi/MM/MM", Graphics, Content);
            this.stage = stage;
            FirstFrameRect = NaviSprite.AnimationGroup.Values.First().Frames.Values.First();

            waitframe_l = WAITCOUNT - 1;
            waitframe_r = WAITCOUNT - 1;
            waitframe_u = WAITCOUNT - 1;
            waitframe_d = WAITCOUNT - 1;
            waitframe_a = WAITCOUNT - 1;
            waitframe_b = WAITCOUNT - 1;
        }

        /// <summary>
        /// Cycles through the whole navi animation list.
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
        /// Update components
        /// </summary>
        /// <param name="gameTime">Game time.</param>
        public override void Update(GameTime gameTime)
        {
            if (Freezed) { base.Update(gameTime); return; }
            UpdateTeleport();
            NaviSprite.AnimationGroup[CurrentAnimation].Next();
            this.AnimationFinished = !NaviSprite.AnimationGroup[CurrentAnimation].Active;
            HandleInputs();

            Battle.PublicDebug = CurrentAnimation;

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

            if (KEY_LEFT.KeyState == KeyState.Down && KEY_RIGHT.KeyState == KeyState.Down) return;
            if (KEY_UP.KeyState == KeyState.Down && KEY_DOWN.KeyState == KeyState.Down) return;

            switch (KEY_LEFT.KeyState)
            {
                case KeyState.Down:
                    waitframe_l++;
                    {
                        if (waitframe_l % WAITCOUNT == 0)
                        {
                            NavigateStage(0, -1);
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
                        NavigateStage(0,1);
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
                        NavigateStage(-1,0);
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
                        NavigateStage(1,0);
                    }
                    break;
                case KeyState.Up:
                    waitframe_d = waitframe_d % OVERFLOW_MODULO;
                    break;
            }
        }



        private void NavigateStage(int NextRow, int NextColumn)
        {
            if(CurrentTeleportState == TeleportState.NONE)
            {
                var r = Row + NextRow;
                var c = Column + NextColumn;

                if (r < 0 | r > stage.PnlRowPnt.Count -1 )
                    return;
                if (c < 0 | c > stage.PnlColPnt.Count -1 )
                   return;

                NewRow = r;
                NewCol = c;
                CurrentTeleportState = TeleportState.STATE_1;
            }
        }

        int NewRow = 0, NewCol = 0;
        TeleportState CurrentTeleportState;

        enum TeleportState
        {
            NONE,
            STATE_1,
            STATE_2,
            STATE_3
        }

        private void UpdateTeleport()
        {
            var x = NaviSprite.AnimationGroup[CurrentAnimation];

            switch (CurrentTeleportState)
            {
                case TeleportState.STATE_1:
                    CurrentAnimation = "MM_TELEPORT1";
                    CurrentTeleportState = TeleportState.STATE_2;
                    break;
                case TeleportState.STATE_2:
                    if (!x.Active)
                    {
                        CurrentAnimation = "MM_TELEPORT2";
                        Row = NewRow;
                        Column = NewCol;
                        CurrentTeleportState = TeleportState.STATE_3;
                    }
                    break;
                case TeleportState.STATE_3:
                    if (!x.Active)
                    {
                        CurrentTeleportState = TeleportState.NONE;
                        NaviSprite.ResetAllGroups();
                        CurrentAnimation = "MM";
                    }
                    break;
            }


        }

        /// <summary>
        /// Draw this instance.
        /// </summary>
        public override void Draw()
        {
            base.Draw();
            var up = NaviSprite.AnimationGroup[CurrentAnimation].CurrentFrame;

            var t = stage.StgPos.Y + stage.PnlRowPnt[Row] + Stage.StagePanelHeight - Stage.StageFloorPadding;
            var o = 20 - FirstFrameRect.Width / 2;
            var x = stage.StgPos.X + stage.PnlColPnt[Column] + o;
            var destrect = new Rectangle(x, t, up.Width, up.Height);

            spriteBatch.Draw(NaviSprite.texture, destrect, up, Color.White, 0,
                new Vector2(0, up.Height),
                SpriteEffects.None, 0);
        }

    }


}
