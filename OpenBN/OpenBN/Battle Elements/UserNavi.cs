using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenBN
{

    class UserNavi : IBattleEntity
    {
        public string ID { get; set; }
        //Position on battlefield
        public int btlrow { get; set; }
        public int btlcol { get; set; }
        public Vector2 battlepos { get; set; }
        //Custom offset vector for adjusting btlfld pos
        public Vector2 battleposoffset { get; set; }
        //Current frame
        public int CurFrame { get; set; }
        //Current animation index
        public int anmindx { get; set; }
        //Total frames per animation index
        public List<int> frmlmt { get; set; }
        Texture2D curframetext;
        public string CurAnimation { get; set; }
        //Spritebatch to write onto
        public Dictionary<string, List<string>> AnimationDict = new Dictionary<string, List<string>>();
        public bool finish { get; set; }
        public bool enableRender { get; set; }

        public SpriteBatch SB { get; set; }
        ContentManager Content;

        public void SetAnimation(string key)
        {
            CurAnimation = key;
            finish = false;
            curframetext = Content.Load<Texture2D>(AnimationDict[CurAnimation][0]);
            this.battleposoffset = new Vector2(-6, -curframetext.Height);

        }

        public void Update()
        {

        }

        public void Next()
        {
            if (finish) return;

            if (AnimationDict[CurAnimation].Count() < CurFrame + 1)
            {
                CurFrame = 0;
                finish = true;
                return;
            }
            CurFrame++;
            curframetext = Content.Load<Texture2D>(AnimationDict[CurAnimation][CurFrame - 1]);
        }

        public UserNavi(string navicode, ContentManager Contentx, SpriteBatch spriteBtch)
        {
            Content = Contentx; //Set content manager
            SB = spriteBtch;
            var dirx = Content.RootDirectory + "/Navi/" + navicode; //Set the working dir to navi code dir
            dirx = dirx.Replace("/", "\\").Replace("/", @"\");
            var x = Directory.GetFiles(dirx, "*", SearchOption.AllDirectories).ToList(); //Enumerate all files under the dir
            foreach (string dir in x)
            {
                var dirarry = dir.Split('\\');
                var key = dirarry[dirarry.Count() - 2];
                var val = dirarry[dirarry.Count() - 1].Split('.')[0];
                var textval = ("Navi\\" + navicode + "\\" + key + "\\" + val);
                if (AnimationDict.ContainsKey(key))
                {
                    //Key already in the list, add them to the temp. list
                    AnimationDict[key].Add(textval);
                }
                else
                {
                    //Add Folder name as key for the animation frames
                    List<string> newtext2d = new List<string>();
                    newtext2d.Add(textval);
                    AnimationDict.Add(key, newtext2d);
                }
            }
            SetAnimation("DEFAULT");
            enableRender = false;
        }

        public void Draw()
        {
            if (enableRender)
            {

                SB.Draw(curframetext, new Rectangle((int)this.battlepos.X, (int)this.battlepos.Y, curframetext.Width, curframetext.Height), Color.White);
            }
        }
    }

}
