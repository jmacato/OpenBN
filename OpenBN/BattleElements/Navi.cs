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
        int _row, _column;
        private Sprite _naviSprite;
        private string _currentAnimation;
        public bool AnimationFinished;
        private Stage _stage;
        private Rectangle _firstFrameRect;
        public int WaitframeL,
                WaitframeR,
                WaitframeU,
                WaitframeD,
                WaitframeA,
                WaitframeB;

        private const int OverflowModulo = 1024;
        private const int Waitcount = 4;

        private bool _freezed;
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
                return _freezed;
            }
            set
            {
                _freezed = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Navi"/> class.
        /// </summary>
        /// <param name="game">Game.</param>
        /// <param name="stage">Stage.</param>
        public Navi(Game game, Stage stage) : base(game)
        {
            _currentAnimation = "MM";
            _row = 1;
            _column = 1;
            _naviSprite = new Sprite("Navi/MM/MM.sasl", "Navi/MM/MM", Graphics, Content);
            this._stage = stage;
            _firstFrameRect = _naviSprite.AnimationGroup.Values.First().Frames.Values.First();

            WaitframeL = Waitcount - 1;
            WaitframeR = Waitcount - 1;
            WaitframeU = Waitcount - 1;
            WaitframeD = Waitcount - 1;
            WaitframeA = Waitcount - 1;
            WaitframeB = Waitcount - 1;
        }

        /// <summary>
        /// Cycles through the whole navi animation list.
        /// </summary>
        public void ChangeAnimation()
        {
            var rand = new Random();
            int r;
            string t;
            r = rand.Next(0, _naviSprite.AnimationGroup.Keys.Count - 1);
            t = _naviSprite.AnimationGroup.Keys.ToArray()[r];
            _currentAnimation = t; // "MM_FIRE_RECOIL";
            _naviSprite.ResetAllGroups();
        }

        /// <summary>
        /// Update components
        /// </summary>
        /// <param name="gameTime">Game time.</param>
        public override void Update(GameTime gameTime)
        {
            if (Freezed) { base.Update(gameTime); return; }
            UpdateTeleport();
            _naviSprite.AnimationGroup[_currentAnimation].Next();
            this.AnimationFinished = !_naviSprite.AnimationGroup[_currentAnimation].Active;
            HandleInputs();

            Battle.PublicDebug = _currentAnimation;

            base.Update(gameTime);
        }

        public void HandleInputs()
        {

            var keyLeft = Input.KbStream[Keys.Left];
            var keyRight = Input.KbStream[Keys.Right];
            var keyUp = Input.KbStream[Keys.Up];
            var keyDown = Input.KbStream[Keys.Down];
            var keyA = Input.KbStream[Keys.A];
            var keyB = Input.KbStream[Keys.S];

            if (keyLeft.KeyState == KeyState.Down && keyRight.KeyState == KeyState.Down) return;
            if (keyUp.KeyState == KeyState.Down && keyDown.KeyState == KeyState.Down) return;

            switch (keyLeft.KeyState)
            {
                case KeyState.Down:
                    WaitframeL++;
                    {
                        if (WaitframeL % Waitcount == 0)
                        {
                            NavigateStage(0, -1);
                        }
                    }
                    break;
                case KeyState.Up:
                    WaitframeL = WaitframeL % OverflowModulo;
                    break;
            }

            switch (keyRight.KeyState)
            {
                case KeyState.Down:
                    WaitframeR++;
                    if (WaitframeR % Waitcount == 0)
                    {
                        NavigateStage(0,1);
                    }
                    break;
                case KeyState.Up:
                    WaitframeR = WaitframeR % OverflowModulo;
                    break;
            }

            switch (keyUp.KeyState)
            {
                case KeyState.Down:
                    WaitframeU++;
                    if (WaitframeU % Waitcount == 0)
                    {
                        NavigateStage(-1,0);
                    }
                    break;
                case KeyState.Up:
                    WaitframeU = WaitframeU % OverflowModulo;
                    break;
            }

            switch (keyDown.KeyState)
            {
                case KeyState.Down:
                    WaitframeD++;
                    if (WaitframeD % Waitcount == 0)
                    {
                        NavigateStage(1,0);
                    }
                    break;
                case KeyState.Up:
                    WaitframeD = WaitframeD % OverflowModulo;
                    break;
            }
        }



        private void NavigateStage(int nextRow, int nextColumn)
        {
            if(_currentTeleportState == TeleportState.None)
            {
                var r = _row + nextRow;
                var c = _column + nextColumn;

                if (r < 0 | r > _stage.PnlRowPnt.Count -1 )
                    return;
                if (c < 0 | c > _stage.PnlColPnt.Count -1 )
                   return;

                _newRow = r;
                _newCol = c;
                _currentTeleportState = TeleportState.State1;
            }
        }

        int _newRow = 0, _newCol = 0;
        TeleportState _currentTeleportState;

        enum TeleportState
        {
            None,
            State1,
            State2,
            State3
        }

        private void UpdateTeleport()
        {
            var x = _naviSprite.AnimationGroup[_currentAnimation];

            switch (_currentTeleportState)
            {
                case TeleportState.State1:
                    _currentAnimation = "MM_TELEPORT1";
                    _currentTeleportState = TeleportState.State2;
                    break;
                case TeleportState.State2:
                    if (!x.Active)
                    {
                        _currentAnimation = "MM_TELEPORT2";
                        _row = _newRow;
                        _column = _newCol;
                        _currentTeleportState = TeleportState.State3;
                    }
                    break;
                case TeleportState.State3:
                    if (!x.Active)
                    {
                        _currentTeleportState = TeleportState.None;
                        _naviSprite.ResetAllGroups();
                        _currentAnimation = "MM";
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
            var up = _naviSprite.AnimationGroup[_currentAnimation].CurrentFrame;

            var t = _stage.StgPos.Y + _stage.PnlRowPnt[_row] + Stage.StagePanelHeight - Stage.StageFloorPadding;
            var o = 20 - _firstFrameRect.Width / 2;
            var x = _stage.StgPos.X + _stage.PnlColPnt[_column] + o;
            var destrect = new Rectangle(x, t, up.Width, up.Height);

            SpriteBatch.Draw(_naviSprite.Texture, destrect, up, Color.White, 0,
                new Vector2(0, up.Height),
                SpriteEffects.None, 0);
        }

    }


}
