using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenBN.BattleElements
{
    public interface IBackground
    {
        void Update(Rectangle screenRectangle);
        void Draw(SpriteBatch spriteBatch);
    }
}