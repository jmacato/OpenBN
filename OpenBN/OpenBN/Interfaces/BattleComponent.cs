using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace OpenBN
{
    public class BattleComponent
    {
        internal SpriteBatch spriteBatch { get; private set; }
        internal ContentManager Content { get; private set; }
        internal GraphicsDevice Graphics { get; private set; }
        internal Inputs Input { get; private set; }

        internal bool Initialized { get; set; }
        public GameTime gameTime { get; private set; }

        public BattleComponent(Game parent)
        {
            this.spriteBatch = ((Battle)parent).spriteBatch;
            this.Content = ((Battle)parent).Content;
            this.Graphics = ((Battle)parent).GraphicsDevice;
            this.Input = ((Battle)parent).Input;

            if (((Battle)parent).Components == null) ((Battle)parent).Components = new List<BattleComponent>();
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
