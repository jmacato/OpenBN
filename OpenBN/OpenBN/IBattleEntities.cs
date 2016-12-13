using Microsoft.Xna.Framework.Graphics;

namespace OpenBN
{
    public interface IBattleEntity
    {
        string ID { get; set; }
        SpriteBatch SB { get; set; }
        void Draw();
        void Next();
    }

}
