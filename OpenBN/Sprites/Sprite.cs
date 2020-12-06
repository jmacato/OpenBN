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
        private readonly Dictionary<string, Rectangle> _tempFrames = new();
        private readonly Dictionary<int, AnimationCommand> _tempCmd = new();
        public Texture2D Texture { get; }
        
        public Sprite(string scriptdir, string texturedir, GraphicsDevice graphics, ContentManager cm)
        {
            var script = File.ReadAllText(cm.RootDirectory + "/" + scriptdir.Trim('/').Trim('\\'));
            int colSize = 0, rowSize = 0;
            var t = script.Replace('\r',char.MinValue).Split("\n".ToCharArray());
            var i = 0; var curanimkey = "";                  

            Texture = cm.Load<Texture2D>(texturedir);
            
            foreach (var y in t)
            {
                var x = y.Trim().Trim('\t').Split(' ');
                switch (x[0])
                {
                    case "DEF":

                        var ptr = x[1];
                        var rectparams = x[2].Split(',');

                        int rX, rY, rW, rH;

                        if (rectparams.Length == 2)
                        {
                            var row = colSize;
                            var col = rowSize;

                            row = Convert.ToInt32(rectparams[0]);
                            col = Convert.ToInt32(rectparams[1]);

                            rX = colSize * (col-1);
                            rY = rowSize * (row-1);
                            rW = colSize;
                            rH = rowSize;
                        } else
                        {
                            rX = Convert.ToInt16(rectparams[0]);
                            rY = Convert.ToInt16(rectparams[1]);
                            rW = Convert.ToInt16(rectparams[2]);
                            rH = Convert.ToInt16(rectparams[3]);
                        }
                         
                        var srcrect = new Rectangle(rX,rY,rW,rH);
                        _tempFrames.Add(ptr, srcrect);
                        break;

                    case "BEGIN":
                        i=0;
                        var aa = new Animation();
                        curanimkey = x[1];
                        AnimationGroup.Add(curanimkey, aa);
                        break;

                    case "END":
                        i=0;
                        AnimationGroup[curanimkey].Frames = _tempFrames;
                        AnimationGroup[curanimkey].Commands = _tempCmd;
                        AnimationGroup[curanimkey].FirstFrame = AnimationGroup[curanimkey].Frames.Keys.First();
                        AnimationGroup[curanimkey].Next();
                        _tempFrames = new Dictionary<string, Rectangle>();
                        _tempCmd = new Dictionary<int, AnimationCommand>();                        
                        curanimkey = "";
                        break;

                    case "META":
                        Metadata.Add(x[1], x[2]);
                        break;

                    case "SET_COL":
                        colSize = Convert.ToInt32(x[1]);
                        break;

                    case "SET_ROW":
                        rowSize = Convert.ToInt32(x[1]);
                        break;
                    
                    case "SHOW":
                        if (AnimationGroup.Count == 0) break;
                        i++;
                        var sh = new AnimationCommand();
                        sh.Cmd = AnimationCommands.Show;
                        sh.Args = x[1];
                        _tempCmd.Add(i, sh);
                        break;
                    
                    case "WAIT":
                        if (AnimationGroup.Count == 0) break;
                        i++;
                        var wt = new AnimationCommand();
                        wt.Cmd = AnimationCommands.Wait;
                        wt.Args = x[1];
                        _tempCmd.Add(i, wt);
                        break;

                    case "STOP":
                        if (AnimationGroup.Count == 0) break;
                        i++;
                        var sp = new AnimationCommand();
                        sp.Cmd = AnimationCommands.Stop;
                        sp.Args = "0";
                        _tempCmd.Add(i, sp);
                        break;

                    case "LOOP":
                        if (AnimationGroup.Count == 0) break;
                        i++;
                        var lp = new AnimationCommand();
                        lp.Cmd = AnimationCommands.Loop;
                        lp.Args = _tempFrames.Keys.First();
                        _tempCmd.Add(i, lp);
                        break;
                }
            }
        }

        public void AdvanceAllGroups()
        {
            foreach(var anim in AnimationGroup.Keys)
            {
                AnimationGroup[anim].Next();
            }
        }

        public void ResetAllGroups()
        {
            foreach (var anim in AnimationGroup.Values)
            {
                anim.Reset();
            }
        }

        internal void Dispose()
        {
            _tempCmd.Clear();
            _tempFrames.Clear();
        }
    }
}
