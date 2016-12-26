using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace OpenBN
{
    public interface IBattleEntity
    {
        SpriteBatch SB { get; set; }
        GraphicsDevice Graphics { get; set; }
        ContentManager Content { get; set; }

        bool Initialized { get; set; }

        /// <summary>
        /// 
        /// </summary>
        void Draw();

        /// <summary>
        /// 
        /// </summary>
        void Update();

        /// <summary>
        /// 
        /// </summary>
        void Initialize();
    }

}
