//using System;
//using System.Threading;
//using System.Linq;
//using System.IO;
//using System.Diagnostics;
//using System.ComponentModel;
//using System.Collections.Generic;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Audio;
//using OpenBN.ScriptedSprites;
//using System.Reflection;
//using Microsoft.Xna.Framework.Content;

//namespace OpenBN.Battle_Elements
//{
//    class SoundHelper
//    {
//        ContentManager Content;


//        //List of sfx's & bgm's
//        Dictionary<int, string> sfxdict = new Dictionary<int, string>();
//        Dictionary<int, string> bgmdict = new Dictionary<int, string>();

//        public SoundHelper()
//        {

//        }
//        /// <summary>
//        /// Load references for the Sound Effects files
//        /// </summary>
//        private void LoadSfx()
//        {
//            DirectoryInfo dir = new DirectoryInfo(Content.RootDirectory + "/SFX");
//            FileInfo[] files = dir.GetFiles("*.*");
//            foreach (FileInfo file in files)
//            {
//                int key = Convert.ToInt16(file.Name.Split('-')[1].Split('.')[0]);
//                sfxdict[key] = ("SFX/SFX-" + key.ToString().PadLeft(2, '0'));
//            }
//        }

//        /// <summary>
//        /// Load references for the BG music files
//        /// </summary>
//        private void LoadBgm()
//        {
//            DirectoryInfo dir = new DirectoryInfo(Content.RootDirectory + "/BGM");
//            FileInfo[] files = dir.GetFiles("*.*");
//            foreach (FileInfo file in files)
//            {
//                int key = Convert.ToInt16(file.Name.Split('-')[1].Split('.')[0]);
//                bgmdict[key] = ("BGM/BGM-" + key.ToString().PadLeft(2, '0'));

//            }
//        }

//        /// <summary>
//        /// Play BG music on loop
//        /// </summary>
//        /// <param name="key">BGM ID</param>
//        private void PlayBgm(int key)
//        {

//            if (key == 0 && bgminst.State == SoundState.Playing && !mute)
//            {
//                bgminst.Stop();
//            }

//            var x = Content.Load<SoundEffect>(bgmdict[key]);
//            bgminst = x.CreateInstance();
//            bgminst.IsLooped = true;
//            bgminst.Play();
//            bgminst.Volume = 0.80f;
//            //   bgminst.Pitch = 0.1f;
//        }

//        /// <summary>
//        /// Play SFX 
//        /// </summary>
//        /// <param name="key">SFX ID</param>
//        private void PlaySfx(int key)
//        {
//            if (mute) return;
//            if (key == 0 && bgminst.State == SoundState.Playing && key > sfxdict.Count() && !mute)
//            {
//                return;
//            }
//            var x = Content.Load<SoundEffect>(sfxdict[key]);
//            x.Play();
//        }
//    }
//}
