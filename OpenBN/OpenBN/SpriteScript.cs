﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenBN.ScriptedSprites
{
    class SSParser
    {
       public Dictionary<string, Animation> AnimationGroup = new Dictionary<string, Animation>();
        public Dictionary<string, Texture2D> TempFrames = new Dictionary<string, Texture2D>();
       public Dictionary<int, AnimationCommand> TempCmd = new Dictionary<int, AnimationCommand>();

        public SSParser(string scriptdir, string texturedir, GraphicsDevice graphics, ContentManager CM)
        {
            var script = File.ReadAllText(CM.RootDirectory + "/" + scriptdir.Trim('/').Trim('\\'));
            Texture2D texture = CM.Load<Texture2D>(texturedir);

            SpriteBatch SB = new SpriteBatch(graphics);
            var t = script.Split("\r\n".ToCharArray());
            int i = 0; string curanimkey = "";
            foreach (string y in t)
            {
                var x = y.Split(' ');
                switch (x[0])
                {
                    case "DEF":

                        var ptr = x[1];
                        var rectparams = x[2].Split(',');

                        var r_x = Convert.ToInt16(rectparams[0]);
                        var r_y = Convert.ToInt16(rectparams[1]);
                        var r_w = Convert.ToInt16(rectparams[2]);
                        var r_h = Convert.ToInt16(rectparams[3]);

                        var srcrect = new Rectangle(r_x,r_y,r_w,r_h);
                        var dstrect = new Rectangle(0, 0, r_w, r_h);

                        RenderTarget2D frm_hndlr = new RenderTarget2D(graphics, r_w, r_h);
                        graphics.SetRenderTarget(frm_hndlr);
                        graphics.Clear(Color.Transparent);
                        SB.Begin();
                        SB.Draw(texture, dstrect, srcrect, Color.White);
                        SB.End();
                        graphics.SetRenderTarget(null);
                        TempFrames.Add(ptr, frm_hndlr);

                        break;

                    case "BEGIN":
                        i=0;
                        var AA = new Animation();
                        curanimkey = x[1];
                        AnimationGroup.Add(curanimkey, AA);
                        break;

                    case "END":
                        i=0;
                        AnimationGroup[curanimkey].Frames = TempFrames;
                        AnimationGroup[curanimkey].Commands = TempCmd;
                        AnimationGroup[curanimkey].FirstFrame = AnimationGroup[curanimkey].Frames.Keys.First();
                        AnimationGroup[curanimkey].Next();
                        TempFrames = new Dictionary<string, Texture2D>();
                        TempCmd = new Dictionary<int, AnimationCommand>();                        
                        curanimkey = "";
                        break;

                    case "SHOW":
                        if (AnimationGroup.Count == 0) break;
                        i++;
                        var SH = new AnimationCommand();
                        SH.Cmd = AnimationCommands.SHOW;
                        SH.Args = x[1];
                        TempCmd.Add(i, SH);
                        break;
                    case "WAIT":
                        if (AnimationGroup.Count == 0) break;
                        i++;
                        var WT = new AnimationCommand();
                        WT.Cmd = AnimationCommands.WAIT;
                        WT.Args = x[1];
                        TempCmd.Add(i, WT);
                        break;

                    case "LOOP":
                        if (AnimationGroup.Count == 0) break;
                        i++;
                        var LP = new AnimationCommand();
                        LP.Cmd = AnimationCommands.LOOP;
                        LP.Args = TempFrames.Keys.First();
                        TempCmd.Add(i, LP);
                        break;
                }
            }

        }
        
    }

    public class Animation
    {
        public Dictionary<string, Texture2D> Frames { get; set; }
        public Dictionary<int, AnimationCommand> Commands { get; set; }
        public Texture2D CurrentFrame { get; private set; }
        public int frmptr { get; private set; }
        public int PC { get; set; }
        public string FirstFrame;

        public Animation()
        {
            FirstFrame = "";
            PC = 1;
            frmptr = 0;
            Commands = new Dictionary<int, AnimationCommand>();
            Frames = new Dictionary<string, Texture2D>();
        }

        int wait = 0;

        public bool Next()
        {
            if (wait != 0) { wait--; return true; }
            switch (Commands[PC].Cmd)
            {
                case AnimationCommands.SHOW:
                    CurrentFrame = Frames[Commands[PC].Args];
                    break;
                case AnimationCommands.LOOP:
                    PC = Commands.Keys.First();
                    return true;
                case AnimationCommands.STOP:
                    return false;
                case AnimationCommands.WAIT:
                    wait = Convert.ToInt32(Commands[PC].Args);
                    break;
            }

            PC++;
            PC = (int)MathHelper.Clamp(PC,0,Commands.Count);
            return false;
        }
    }


    public class AnimationCommand
    {
        public AnimationCommands Cmd { get; set; }
        public string Args { get; set; }
    }

    public enum AnimationCommands
    {
        SHOW,
        STOP,
        WAIT,
        LOOP
    }

}
