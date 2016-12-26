using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// Sprite Animation Scripting Language Parser
/// (c) Jumar Macato 2016  
/// </summary>
namespace OpenBN.ScriptedSprites
{
    class Sprite
    {
        public Dictionary<string, Animation> AnimationGroup = new Dictionary<string, Animation>();
        public Dictionary<string, string> Metadata = new Dictionary<string, string>();

        private Dictionary<string, Texture2D> TempFrames = new Dictionary<string, Texture2D>();
        private Dictionary<int, AnimationCommand> TempCmd = new Dictionary<int, AnimationCommand>();

        public Sprite(string scriptdir, string texturedir, GraphicsDevice graphics, ContentManager CM)
        {
            var script = File.ReadAllText(CM.RootDirectory + "/" + scriptdir.Trim('/').Trim('\\'));
            Texture2D texture = CM.Load<Texture2D>(texturedir);
            int ColSize = 0, RowSize = 0;
            SpriteBatch SB = new SpriteBatch(graphics);
            var t = script.Split("\r\n".ToCharArray());
            int i = 0; string curanimkey = "";
            foreach (string y in t)
            {

                var x = y.Trim().Trim('\t').Split(' ');
                switch (x[0])
                {
                    case "DEF":

                        var ptr = x[1];
                        var rectparams = x[2].Split(',');

                        int r_x = 0, r_y = 0, r_w = 0, r_h = 0;

                        if ((rectparams.Count() == 2))
                        {
                            var row = ColSize;
                            var col = RowSize;

                            row = Convert.ToInt32(rectparams[0]);
                            col = Convert.ToInt32(rectparams[1]);

                            r_x = (ColSize * (col-1));
                            r_y = (RowSize * (row-1));
                            r_w = ColSize;
                            r_h = RowSize;
                        } else
                        {
                            r_x = Convert.ToInt16(rectparams[0]);
                            r_y = Convert.ToInt16(rectparams[1]);
                            r_w = Convert.ToInt16(rectparams[2]);
                            r_h = Convert.ToInt16(rectparams[3]);
                        }
                         
                        var srcrect = new Rectangle(r_x,r_y,r_w,r_h);
                        var dstrect = new Rectangle(0, 0, r_w, r_h);

                        RenderTarget2D frm_hndlr = new RenderTarget2D(graphics, r_w, r_h);
                        graphics.SetRenderTarget(frm_hndlr);
                        graphics.Clear(Color.Transparent);
                        SB.Begin();
                        SB.Draw(texture, dstrect, srcrect, Color.White);
                        SB.End();
                        graphics.SetRenderTarget(null);

                        Texture2D Trgt = new Texture2D(graphics, frm_hndlr.Width, frm_hndlr.Height);
                        Color[] colors = new Color[frm_hndlr.Width * frm_hndlr.Height];

                        frm_hndlr.GetData<Color>(colors);
                        Trgt.SetData<Color>(colors);
                        frm_hndlr.Dispose();

                        TempFrames.Add(ptr, Trgt);
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

                    case "META":
                        Metadata.Add(x[1], x[2]);
                        break;

                    case "SET_COL":
                        ColSize = Convert.ToInt32(x[1]);
                        break;

                    case "SET_ROW":
                        RowSize = Convert.ToInt32(x[1]);
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
        public void AdvanceAllGroups()
        {
            foreach(string Anim in AnimationGroup.Keys)
            {
                AnimationGroup[Anim].Next();
            }
        }

        //public Animation GetGroup(string AnimationGroupKey)
        //{
        //    AnimationGroup[AnimationGroupKey].Active = true;
        //    Animation y = AnimationGroup[AnimationGroupKey];
        //    return y;
        //}

    }



    public class Animation
    {
        public Dictionary<string, Texture2D> Frames { get; set; }
        public Dictionary<int, AnimationCommand> Commands { get; set; }
        public Texture2D CurrentFrame { get; private set; }
        public int frmptr { get; private set; }
        public int PC { get; set; }
        public string FirstFrame;
        public bool Active { get; private set; }

        public Animation()
        {
            FirstFrame = "";
            PC = 1;
            frmptr = 0;
            Commands = new Dictionary<int, AnimationCommand>();
            Frames = new Dictionary<string, Texture2D>();
            Active = true;
        }

        int wait = 0;

        public bool Next()
        {
            if (!Active) return false;
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
                    Active = false;
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
