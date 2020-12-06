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
        public int PC { get; set; }
        public string FirstFrame;
        public bool Active { get; private set; }

        public Animation()
        {
            FirstFrame = "";
            PC = 1;
            Commands = new Dictionary<int, AnimationCommand>();
            Frames = new Dictionary<string, Rectangle>();
            Active = true;
        }

        int wait = 0;
        public bool Next()
        {
	
            if (!Active) return false;
            if (wait != 0) { wait--; return true; }
            try{
                switch (Commands[PC].Cmd)
                {
                    case AnimationCommands.SHOW:
                        CurrentFrame = Frames[Commands[PC].Args];
                        break;
                    case AnimationCommands.LOOP:
                        PC = Commands.Keys.First();
                        return true;
                    case AnimationCommands.STOP:
                        Active = false;
                        return false;
                    case AnimationCommands.WAIT:
                        wait = Convert.ToInt32(Commands[PC].Args.Trim());
                        break;
                }

            } catch {
            }
            PC++;
            PC = (int)MathHelper.Clamp(PC,0,Commands.Count);
            return false;
        }

        internal void Reset()
        {
            PC = 1;
            Active = true;
        }
    }
}