﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBN.ScriptedSprites
{
    class SSParser
    {
        public SSParser(string script, Texture2D texture, GraphicsDevice graphics)
        {
            Animation Animation = new Animation();

            SpriteBatch SB = new SpriteBatch(graphics);

            var t = script.Split("\r\n".ToCharArray());
            int i = 0;
            foreach (string y in t)
            {
                var x = y.Split(' ');
                switch (x[0])
                {
                    case "DEF":

                        var ptr = Convert.ToInt16(x[1]);
                        var rectparams = x[2].Split(',');

                        var r_x = Convert.ToInt16(rectparams[0]);
                        var r_y = Convert.ToInt16(rectparams[1]);
                        var r_w = Convert.ToInt16(rectparams[2]);
                        var r_h = Convert.ToInt16(rectparams[3]);

                        var srcrect = new Rectangle(r_x,r_y,r_w,r_h);
                        var dstrect = new Rectangle(0, 0, r_w, r_h);

                        RenderTarget2D frm_hndlr = new RenderTarget2D(graphics, r_w, r_h);

                        SB.Begin();
                        SB.Draw(texture, dstrect, srcrect, Color.White);
                        SB.End();

                        Animation.Frames.Add(ptr, frm_hndlr);
                        break;

                    case "SHOW":
                        i++;
                        var SH = new AnimationCommand();
                        SH.Cmd = AnimationCommands.SHOW;
                        SH.Args = Convert.ToInt16(x[1]);
                        Animation.Commands.Add(i, SH);
                        break;

                    case "WAIT":
                        var WT = new AnimationCommand();
                        WT.Cmd = AnimationCommands.SHOW;
                        WT.Args = Convert.ToInt16(x[1]);
                        Animation.Commands.Add(i, WT);
                        break;

                    case "LOOP":
                        var LP = new AnimationCommand();
                        LP.Cmd = AnimationCommands.SHOW;
                        LP.Args = 0;
                        Animation.Commands.Add(i, LP);
                        break;
                }
            }
            SB.Dispose();
        }

    }

    public class Animation
    {
        public Dictionary<int, Texture2D> Frames { get; set; }
        public Dictionary<int, AnimationCommand> Commands { get; set; }
        public Texture2D CurrentFrame { get; private set; }
        public int frmptr { get; private set; }
        public int PC { get; private set; }

        public Animation()
        {
            PC = 0;
            frmptr = 0;
        }

        int wait = 0;

        public bool Next()
        {
            if (wait == 0) { PC++; } else { wait--; return true; }
            switch (Commands[PC].Cmd)
            {
                case AnimationCommands.SHOW:
                    CurrentFrame = Frames[Commands[PC].Args];
                    break;
                case AnimationCommands.LOOP:
                    PC = 0;
                    return true;
                case AnimationCommands.STOP:
                    return false;
                case AnimationCommands.WAIT:
                    wait = Commands[PC].Args;
                    return true;
            }
            return false;
        }



    }


    public class AnimationCommand
    {
        public AnimationCommands Cmd { get; set; }
        public int Args { get; set; }
    }

    public enum AnimationCommands
    {
        SHOW,
        STOP,
        WAIT,
        LOOP
    }

}