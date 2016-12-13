using System;

namespace OpenBN
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (BattleField game = new BattleField())
            {
                game.Run();
            }
        }
    }
#endif
}

