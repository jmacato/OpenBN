using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

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

        public TiledBackground(Texture2D texture, int environmentWidth, int environmentHeight)
        {
            this.texture = texture;
            horizontalTileCount = (int)(System.Math.Round((double)environmentWidth / texture.Width) + 1);
            verticalTileCount = (int)(System.Math.Round((double)environmentHeight / texture.Height) + 1);

            startCoord = new Vector2(0, 0);
        }

        public void Update(Rectangle cameraRectangle)
        {
            startCoord.X = ((cameraRectangle.X / texture.Width) * texture.Width) - cameraRectangle.X;
            startCoord.Y = ((cameraRectangle.Y / texture.Height) * texture.Height) - cameraRectangle.Y;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = -1; i < horizontalTileCount; i++)
            {
                for (int j = -1; j < verticalTileCount; j++)
                {
                    spriteBatch.Draw(texture,
                    new Rectangle(
                    (int)startCoord.X + (i * texture.Width),
                    (int)startCoord.Y + (j * texture.Height),
                    texture.Width, texture.Height),
                    Color.White);
                }
            }
        }
    }
}
