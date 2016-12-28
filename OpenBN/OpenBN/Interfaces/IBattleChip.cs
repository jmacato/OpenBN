using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBN
{
    public interface IBattleChip
    {
        Texture2D Image { get; set; }
        string DisplayName { get; set; }
        string Description { get; set; }
        ChipElements Element { get; set; }
        char Code { get; set; }

        void Execute(CustomWindow CW, BattleField BT, Stage ST);

        SpriteBatch SB { get; set; }
        GraphicsDevice Graphics { get; set; }
        ContentManager Content { get; set; }
    }

    public enum ChipElements
    {
        FIRE, AQUA, THUNDER, LEAF, SWORD, WIND,
        TARGET, BLOCK, ENHANCE, STONE, NULL
    }

    public class TestBattleChip : IBattleChip
    {
        public Texture2D Image { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public ChipElements Element { get; set; }
        public char Code { get; set; }
        public SpriteBatch SB { get; set; }
        public GraphicsDevice Graphics { get; set; }
        public ContentManager Content { get; set; }

        public void Execute(CustomWindow CW, BattleField BT, Stage ST)
        {

        }

        public TestBattleChip()
        {
            Image = Content.Load<Texture2D>("BC/schip178");
            DisplayName = "BugFix";
            Description = "Fix all nav cust bugs.";
            Element = ChipElements.NULL;
            Code = '@';
        }
    }

}
