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
namespace OpenBN
{
    public class Sprite
    {
        public Dictionary<string, Animation> AnimationGroup = new Dictionary<string, Animation>();
        public Dictionary<string, string> Metadata = new Dictionary<string, string>();

        private Dictionary<string, Rectangle> TempFrames = new Dictionary<string, Rectangle>();
        private Dictionary<int, AnimationCommand> TempCmd = new Dictionary<int, AnimationCommand>();

        public Texture2D texture { get; private set; }
        public Sprite(string scriptdir, string texturedir, GraphicsDevice graphics, ContentManager CM)
        {
            var script = File.ReadAllText(CM.RootDirectory + "/" + scriptdir.Trim('/').Trim('\\'));
            int ColSize = 0, RowSize = 0;
            var t = script.Split("\r\n".ToCharArray());
            int i = 0; string curanimkey = "";                  

            texture = CM.Load<Texture2D>(texturedir);
            
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
                        TempFrames.Add(ptr, srcrect);
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
                        TempFrames = new Dictionary<string, Rectangle>();
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

                    case "STOP":
                        if (AnimationGroup.Count == 0) break;
                        i++;
                        var SP = new AnimationCommand();
                        SP.Cmd = AnimationCommands.STOP;
                        SP.Args = "0";
                        TempCmd.Add(i, SP);
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

        public void ResetAllGroups()
        {
            foreach (Animation Anim in AnimationGroup.Values)
            {
                Anim.Reset();
            }
        }

        internal void Dispose()
        {
            TempCmd.Clear();
            TempFrames.Clear();
        }
    }



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
