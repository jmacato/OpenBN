using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenBN
{
    public interface IBackground
    {
        void Update(Rectangle screenRectangle);
        void Draw(SpriteBatch spriteBatch);
    }

    public class TiledBackground : IBackground
    {
        public Texture2D texture;
        readonly int horizontalTileCount;
        readonly int verticalTileCount;
        public Vector2 startCoord;
        public Rectangle curtextrect;
        public TiledBackground(Rectangle curtextrect, int environmentWidth, int environmentHeight, Texture2D maintext)
        {
            texture = maintext;
            this.curtextrect = curtextrect;
            horizontalTileCount = (int)(System.Math.Round((double)environmentWidth / curtextrect.Width) + 1);
            verticalTileCount = (int)(System.Math.Round((double)environmentHeight / curtextrect.Height) + 1);

            startCoord = new Vector2(0, 0);
        }

        public void Update(Rectangle cameraRectangle)
        {
            startCoord.X = ((cameraRectangle.X / curtextrect.Width) * curtextrect.Width) - cameraRectangle.X;
            startCoord.Y = ((cameraRectangle.Y / curtextrect.Height) * curtextrect.Height) - cameraRectangle.Y;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = -1; i < horizontalTileCount; i++)
            {
                for (int j = -1; j < verticalTileCount; j++)
                {
                    spriteBatch.Draw(texture,
                    new Rectangle(
                    (int)startCoord.X + (i * curtextrect.Width),
                    (int)startCoord.Y + (j * curtextrect.Height),
                    curtextrect.Width, curtextrect.Height), curtextrect,
                    Color.White);
                }
            }
        }
    }
}
