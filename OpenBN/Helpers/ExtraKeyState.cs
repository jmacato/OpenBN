using System;
using Microsoft.Xna.Framework.Input;

namespace OpenBN.Helpers
{
    public class ExtraKeyState
    {
        public KeyState KeyState { get; set; }
        public TimeSpan RegisterDuration { get; set; }
        public double DurDelta { get; set; }
        public double OldDelta { get; set; }

        public ExtraKeyState(KeyState k)
        {
            KeyState = k;
        }

    }
}