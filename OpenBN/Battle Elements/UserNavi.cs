using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenBN
{

    public class Navi : BattleComponent
    {
        private Sprite UserNavi;
        private string CurrentAnimation;

        public Navi(Game game) : base (game)
        {
            CurrentAnimation = "MM";
            UserNavi = new Sprite("Navi/MM/MM.sasl", "Navi/MM/MM", Graphics, Content);
        }

        public void ChangeAnimation()
        {
            var rand = new Random();
            int r;
            string t;
                r = rand.Next(0, UserNavi.AnimationGroup.Keys.Count - 1);
                t = UserNavi.AnimationGroup.Keys.ToArray()[r];
            CurrentAnimation = t; // "MM_FIRE_RECOIL";
            UserNavi.ResetAllGroups();
        }

        public override void Update(GameTime gameTime)
        {
            UserNavi.AdvanceAllGroups();
            base.Update(gameTime);
        }

        public override void Draw()
        {
            base.Draw();
            var up = UserNavi.AnimationGroup[CurrentAnimation].CurrentFrame;
            spriteBatch.Draw(UserNavi.texture, new Rectangle(40,120,up.Width,up.Height),up, Color.White,0,new Vector2(0,up.Height),SpriteEffects.None,0f);
            
        }

    }

}
