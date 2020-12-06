using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace OpenBN.Interfaces
{
    interface IParentComponent
    {
        List<BattleModule> Components { get; set; }
        void UpdateComponents(GameTime gameTime);

        void DrawComponents();

    }
}
