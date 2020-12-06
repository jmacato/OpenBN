using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace OpenBN.Sprites
{
    public class Animation
    {
        public Dictionary<string, Rectangle> Frames { get; set; }
        public Dictionary<int, AnimationCommand> Commands { get; set; }
        public Rectangle CurrentFrame { get; private set; }
        public int Pc { get; set; }
        public string FirstFrame;
        public bool Active { get; private set; }

        public Animation()
        {
            FirstFrame = "";
            Pc = 1;
            Commands = new Dictionary<int, AnimationCommand>();
            Frames = new Dictionary<string, Rectangle>();
            Active = true;
        }

        int _wait = 0;
        public bool Next()
        {
	
            if (!Active) return false;
            if (_wait != 0) { _wait--; return true; }
            try{
                switch (Commands[Pc].Cmd)
                {
                    case AnimationCommands.Show:
                        CurrentFrame = Frames[Commands[Pc].Args];
                        break;
                    case AnimationCommands.Loop:
                        Pc = Commands.Keys.First();
                        return true;
                    case AnimationCommands.Stop:
                        Active = false;
                        return false;
                    case AnimationCommands.Wait:
                        _wait = Convert.ToInt32(Commands[Pc].Args.Trim());
                        break;
                }

            } catch {
            }
            Pc++;
            Pc = (int)MathHelper.Clamp(Pc,0,Commands.Count);
            return false;
        }

        internal void Reset()
        {
            Pc = 1;
            Active = true;
        }
    }
}