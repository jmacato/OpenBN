using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Sprite Animation Scripting Language Parser
/// (c) Jumar Macato 2016  
/// </summary>
namespace OpenBN.Sprites
{
    public class Sprite
    {
        public readonly Dictionary<string, Animation> AnimationGroup = new();
        public readonly Dictionary<string, string> Metadata = new();
        private readonly Dictionary<string, Rectangle> TempFrames = new();
        private readonly Dictionary<int, AnimationCommand> TempCmd = new();
        public Texture2D texture { get; }
        
        public Sprite(string scriptdir, string texturedir, GraphicsDevice graphics, ContentManager CM)
        {
            var script = File.ReadAllText(CM.RootDirectory + "/" + scriptdir.Trim('/').Trim('\\'));
            int ColSize = 0, RowSize = 0;
            var t = script.Replace('\r',char.MinValue).Split("\n".ToCharArray());
            var i = 0; var curanimkey = "";                  

            texture = CM.Load<Texture2D>(texturedir);
            
            foreach (var y in t)
            {
                var x = y.Trim().Trim('\t').Split(' ');
                switch (x[0])
                {
                    case "DEF":

                        var ptr = x[1];
                        var rectparams = x[2].Split(',');

                        int r_x, r_y, r_w, r_h;

                        if (rectparams.Length == 2)
                        {
                            var row = ColSize;
                            var col = RowSize;

                            row = Convert.ToInt32(rectparams[0]);
                            col = Convert.ToInt32(rectparams[1]);

                            r_x = ColSize * (col-1);
                            r_y = RowSize * (row-1);
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
            foreach(var Anim in AnimationGroup.Keys)
            {
                AnimationGroup[Anim].Next();
            }
        }

        public void ResetAllGroups()
        {
            foreach (var Anim in AnimationGroup.Values)
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
}
