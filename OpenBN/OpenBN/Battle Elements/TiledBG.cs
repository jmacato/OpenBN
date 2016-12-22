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
        public Texture2D _texture;
        readonly int _horizontalTileCount;
        readonly int _verticalTileCount;
        public Vector2 _startCoord;

        public TiledBackground(Texture2D texture, int environmentWidth, int environmentHeight)
        {
            _texture = texture;
            _horizontalTileCount = (int)(Math.Round((double)environmentWidth / _texture.Width) + 1);
            _verticalTileCount = (int)(Math.Round((double)environmentHeight / _texture.Height) + 1);

            _startCoord = new Vector2(0, 0);
        }

        public void Update(Rectangle _cameraRectangle)
        {
            _startCoord.X = ((_cameraRectangle.X / _texture.Width) * _texture.Width) - _cameraRectangle.X;
            _startCoord.Y = ((_cameraRectangle.Y / _texture.Height) * _texture.Height) - _cameraRectangle.Y;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = -1; i < _horizontalTileCount; i++)
            {
                for (int j = -2; j < _verticalTileCount; j++)
                {
                    spriteBatch.Draw(_texture,
                    new Rectangle(
                    (int)_startCoord.X + (i * _texture.Width),
                    (int)_startCoord.Y + (j * _texture.Height),
                    _texture.Width, _texture.Height),
                    Color.White);
                }
            }
        }
    }

    public class AnimatedSprite
    {
        /// <summary>
        /// Set Animation Frame Buffer
        /// </summary>
        public List<Texture2D> TextureList { get; }

        /// <summary>
        /// Commands list:
        /// Fxx (hex 0-FF) - Current Frame, if no succeding D command, default to single frame
        /// Dxx (hex 2-FF) - How many frames the current target frame should be displayed.
        /// LPP            - Loop animation
        /// RXX            - Loop animation 0xXX times
        /// </summary>
        public List<string> Commands { get; }

        public bool AnimationFinished { get; private set;}

        public bool IsLooping { get; set; }

        public void StopLoop()
        {

        }

        public void Update()
        {

        }

        public void Render()
        {

        }

    }

}
