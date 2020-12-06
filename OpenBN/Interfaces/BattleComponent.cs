﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace OpenBN
{
    public class BattleModule
    {
        internal SpriteBatch spriteBatch { get; set; }
        internal ContentManager Content { get; set; }
        internal GraphicsDevice Graphics { get; set; }
        internal Inputs Input { get; set; }

        internal bool Initialized { get; set; }
        internal IParentComponent Parent { get; set; }

        public GameTime gameTime { get; private set; }

        public BattleModule(Game parent)
        {
            this.Parent = (Battle)parent;
            this.spriteBatch = ((Battle)parent).spriteBatch;
            this.Content = ((Battle)parent).Content;
            this.Graphics = ((Battle)parent).GraphicsDevice;
            this.Input = ((Battle)parent).Input;

            if (((Battle)parent).Components == null) ((Battle)parent).Components = new List<BattleModule>();
            ((Battle)parent).Components.Add(this);
        }

        public virtual void Update(GameTime gameTime)
        {
            if (!Initialized) return;
            this.gameTime = gameTime;
        }

        public virtual void Draw()
        {
            if (!Initialized) return;
        }

    }
}