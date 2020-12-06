using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OpenBN.BattleElements;

namespace OpenBN.Interfaces
{
    public interface IBattleChip
    {
        Texture2D Image { get; set; }
        string DisplayName { get; set; }
        int Damage { get; set; }
        ChipElements Element { get; set; }
        char Code { get; set; }
        Texture2D Icon { get; set; }
        bool IsSelected { get; set; }
        Point SlotRowCol { get; set; }

        void Execute(CustomWindow cw, Battle bt, Stage st);

        SpriteBatch Sb { get; set; }
        GraphicsDevice Graphics { get; set; }
        ContentManager Content { get; set; }
    }
}
