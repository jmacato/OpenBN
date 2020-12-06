using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenBN.BattleElements
{
    public class TiledBackground : IBackground
    {
        public Texture2D Texture;
        readonly int _horizontalTileCount;
        readonly int _verticalTileCount;
        public Vector2 StartCoord;
        public Rectangle Curtextrect;
        public TiledBackground(Rectangle curtextrect, int environmentWidth, int environmentHeight, Texture2D maintext)
        {
            Texture = maintext;
            this.Curtextrect = curtextrect;
            _horizontalTileCount = (int)(System.Math.Round((double)environmentWidth / curtextrect.Width) + 1);
            _verticalTileCount = (int)(System.Math.Round((double)environmentHeight / curtextrect.Height) + 1);

            StartCoord = new Vector2(0, 0);
        }

        public void Update(Rectangle cameraRectangle)
        {
            StartCoord.X = cameraRectangle.X / Curtextrect.Width * Curtextrect.Width - cameraRectangle.X;
            StartCoord.Y = cameraRectangle.Y / Curtextrect.Height * Curtextrect.Height - cameraRectangle.Y;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (var i = -1; i < _horizontalTileCount; i++)
            {
                for (var j = -1; j < _verticalTileCount; j++)
                {
                    spriteBatch.Draw(Texture,
                    new Rectangle(
                    (int)StartCoord.X + i * Curtextrect.Width,
                    (int)StartCoord.Y + j * Curtextrect.Height,
                    Curtextrect.Width, Curtextrect.Height), Curtextrect,
                    Color.White);
                }
            }
        }
    }
}
