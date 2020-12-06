using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OpenBN.Helpers;

namespace OpenBN.Interfaces
{
    public class BattleModule
    {
        internal SpriteBatch SpriteBatch { get; set; }
        internal ContentManager Content { get; set; }
        internal GraphicsDevice Graphics { get; set; }
        internal Inputs Input { get; set; }

        internal bool Initialized { get; set; }
        internal IParentComponent Parent { get; set; }

        public GameTime GameTime { get; private set; }

        public BattleModule(Game parent)
        {
            this.Parent = (Battle)parent;
            this.SpriteBatch = ((Battle)parent).SpriteBatch;
            this.Content = ((Battle)parent).Content;
            this.Graphics = ((Battle)parent).GraphicsDevice;
            this.Input = ((Battle)parent).Input;

            if (((Battle)parent).Components == null) ((Battle)parent).Components = new List<BattleModule>();
            ((Battle)parent).Components.Add(this);
        }

        public virtual void Update(GameTime gameTime)
        {
            if (!Initialized) return;
            this.GameTime = gameTime;
        }

        public virtual void Draw()
        {
            if (!Initialized) return;
        }

    }
}
