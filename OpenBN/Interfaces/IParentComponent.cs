using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace OpenBN
{
    interface IParentComponent
    {
        List<BattleModule> Components { get; set; }
        void UpdateComponents(GameTime gameTime);

        void DrawComponents();

    }
}
