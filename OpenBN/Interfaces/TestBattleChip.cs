using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OpenBN.BattleElements;

namespace OpenBN.Interfaces
{
    public class TestBattleChip : IBattleChip
    {
        public Texture2D Image { get; set; }
        public Texture2D Icon { get; set; }
        public string DisplayName { get; set; }
        public int Damage { get; set; }
        public ChipElements Element { get; set; }
        public char Code { get; set; }
        public SpriteBatch SB { get; set; }
        public GraphicsDevice Graphics { get; set; }
        public ContentManager Content { get; set; }
        public bool IsSelected { get; set; }

        public Point SlotRowCol { get; set; }

        public void Execute(CustomWindow CW, Battle BT, Stage ST)
        {

        }

        public TestBattleChip(int id, ChipIconProvider iconprov, ContentManager Content, string dspn, int dmg, ChipElements elem, char code)
        {

            this.Content = Content;
            Image = Content.Load<Texture2D>("BC/schip" + id.ToString().PadLeft(3, '0'));
            DisplayName = dspn;
            Damage = dmg;
            Element = elem;
            Code = code;
            Icon = iconprov.Icons[id];
            IsSelected = false;
        }
    }
}