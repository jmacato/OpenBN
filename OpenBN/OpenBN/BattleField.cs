using System;
using System.Threading;
using System.Net.Sockets;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using OpenBN.ScriptedSprites;

namespace OpenBN
{
    public class BattleField : Microsoft.Xna.Framework.Game
    {

        public string BGCode = "BG/SS";
        public Size screenres = new Size(240, 160);
        public Vector2 screenresvect = new Vector2(240, 160);
        public int screenresscalar = 2;

        //Utility Vectors
        public Vector2 cancelX = new Vector2(0, 1);
        public Vector2 cancelY = new Vector2(1, 0);

        Rectangle Viewbox = new Rectangle(0, 0, 240, 160);
        Rectangle defaultrect = new Rectangle(0, 0, 240, 160);

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Vector2 bgpos = new Vector2(0, 0);

        //bgwrkr for bg scroll
        BackgroundWorker bgUpdater = new BackgroundWorker();
        BackgroundWorker flash = new BackgroundWorker();
        BackgroundWorker UserNavBgWrk = new BackgroundWorker();
        BackgroundWorker SixtyHzBgWrkr = new BackgroundWorker();

        //List of sfx's & bgm's
        Dictionary<int, string> sfxdict = new Dictionary<int, string>();
        Dictionary<int, string> bgmdict = new Dictionary<int, string>();

        Dictionary<Keys, bool> KeyLatch = new Dictionary<Keys, bool>();
        Dictionary<int, Texture2D> BGDict = new Dictionary<int, Texture2D>();

        //For bgm looping
        SoundEffectInstance bgminst;

        List<IBattleEntity> RenderQueue = new List<IBattleEntity>();
        List<string> EnemyNames = new List<string>(3);


        Inputs Input;
        Stage Stage;
        TiledBackground myBackground;
        UserNavi MegamanEXE;
        CustomWindow CustWindow;

        Texture2D flsh;

        Keys[] MonitoredKeys = new Keys[] { Keys.A, Keys.S, Keys.X, Keys.Z,
                                            Keys.Up, Keys.Down, Keys.Left, Keys.Right,
                                            Keys.Q, Keys.W, Keys.R, Keys.M};
        Keys[] ArrowKeys = new Keys[] { Keys.Up, Keys.Down, Keys.Left, Keys.Right };

        RenderTarget2D EnemyNameCache;
        FontHelper Fonts;

        float flash_opacity = 1;
        int updateBGScroll = 0;
        int BGFrame = 1;
        bool terminateGame = false;
        bool bgReady = false;
        bool mute = false;
        public bool DisplayEnemyNames = true;
        string debugTXT = "";

        protected override void Initialize()
        {
            base.Initialize();
            //Assign bgwrkrs
            bgUpdater.DoWork += BgUpdater_DoWork;
            UserNavBgWrk.DoWork += UserNavBgWrk_DoWork;
            flash.DoWork += Flash_DoWork;
            SixtyHzBgWrkr.DoWork += SixtyHzBgWrkr_DoWork;
            foreach (Keys x in MonitoredKeys)
            {
                KeyLatch.Add(x, false);
            }
        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            flsh = RectangleFill(new Rectangle(0, 0, screenres.W, screenres.H), ColorHelper.FromHex(0xF8F8F8), false);

            Stage = new Stage(Content);
            Stage.SB = spriteBatch;
            Input = new Inputs(MonitoredKeys);
            Fonts = new FontHelper(Content);
            CustWindow = new CustomWindow(Content, Fonts);
            CustWindow.SB = spriteBatch;

            RenderQueue.Add(Stage);

            MegamanEXE = new UserNavi("MM", Content, spriteBatch);

            MegamanEXE.btlcol = 2;
            MegamanEXE.btlrow = 2;
            MegamanEXE.enableRender = false;
            RenderQueue.Add(MegamanEXE);

            LoadSfx();
            LoadBgm();

            EnemyNames.Add("Mettaur");
            EnemyNames.Add("Mettaur");


            flash.RunWorkerAsync();

        }

        private void SixtyHzBgWrkr_DoWork(object sender, DoWorkEventArgs e)
        {
            do
            {
                CustWindow.Update();
                Thread.Sleep(12);
            } while (!terminateGame);
        }

        public BattleField()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth = screenres.W * screenresscalar;
            graphics.PreferredBackBufferHeight = screenres.H * screenresscalar;
            this.Window.Title = "OpenBN";
            Content.RootDirectory = "Content";

            this.Window.AllowUserResizing = false;
            this.Window.ClientSizeChanged += Window_ClientSizeChanged;
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            UpdateViewbox();
            graphics.PreferredBackBufferHeight = Viewbox.Height;
            graphics.PreferredBackBufferWidth = Viewbox.Width;
            graphics.ApplyChanges();
        }

        private void UserNavBgWrk_DoWork(object sender, DoWorkEventArgs e)
        {

            MegamanEXE.btlcol = 1;
            MegamanEXE.btlrow = 1;
            MegamanEXE.enableRender = true;
            MegamanEXE.SetAnimation("DEFAULT");
            MegamanEXE.battlepos = Stage.GetStageCoords(MegamanEXE.btlrow, MegamanEXE.btlcol, MegamanEXE.battleposoffset);

            int BusterState = 0;

            int tmpcol = MegamanEXE.btlcol;
            int tmprow = MegamanEXE.btlrow;

            do
            {

                if (CustWindow.showCust) continue;

                if (Input != null && MegamanEXE.finish)
                {
                    #region Buster & Charge Shot
                    var ks_x = Input.KbStream[Keys.X];
                    switch (ks_x.KeyState)
                    {
                        case KeyState.Down:
                            if (ks_x.DurDelta < 800)
                            {
                                if (BusterState == 1) break;
                                Debug.Print("chrg_Anim_start");
                                PlaySfx(14);
                                BusterState = 1;
                            }
                            else if (ks_x.DurDelta > 1500)
                            {
                                if (BusterState == 2) break;
                                Debug.Print("chrg_Anim_start2");
                                PlaySfx(15);
                                BusterState = 2;
                            }
                            KeyLatch[Keys.X] = true;
                            break;
                        case KeyState.Up:
                            if (KeyLatch[Keys.X] == true)
                            {
                                KeyLatch[Keys.X] = false;
                                if (ks_x.DurDelta < 1500)
                                {
                                    Debug.Print("BstrSht");
                                    MegamanEXE.SetAnimation("BUSTER");
                                    PlaySfx(7);
                                    BusterState = 0;
                                    break;
                                }
                                else if (Input.KbStream[Keys.X].DurDelta > 1500)
                                {
                                    Debug.Print("ChgSht");
                                    MegamanEXE.SetAnimation("BUSTER");
                                    PlaySfx(76);
                                    BusterState = 0;
                                    break;
                                }
                                else
                                {
                                    if (BusterState > 0)
                                    {
                                        BusterState = 0;
                                        KeyLatch[Keys.X] = false;
                                    }
                                }
                            }
                            break;
                    }
                    #endregion
                    #region Stage Movement
                    foreach (Keys ky_ar in ArrowKeys)
                    {
                        // if (!MegamanEXE.finish) break;
                        var arrw_ks = Input.KbStream[ky_ar].KeyState;
                        var arrw_dt = Input.KbStream[ky_ar].DurDelta;
                        int tmp1 = 0;
                        int tmp2 = 0;
                        switch (arrw_ks)
                        {
                            case KeyState.Up:
                                if (KeyLatch[ky_ar] == true && MegamanEXE.finish)
                                {
                                    tmp1 = tmpcol;
                                    tmp2 = tmprow;

                                    switch (ky_ar)
                                    {
                                        case Keys.Left:
                                            tmp1--;
                                            break;
                                        case Keys.Right:
                                            tmp1++;
                                            break;
                                        case Keys.Up:
                                            tmp2--;
                                            break;
                                        case Keys.Down:
                                            tmp2++;
                                            break;
                                    }
                                    if (Stage.IsMoveAllowed(tmp2, tmp1))
                                    {
                                        switch (ky_ar)
                                        {
                                            case Keys.Left:
                                                tmpcol--;
                                                break;
                                            case Keys.Right:
                                                tmpcol++;
                                                break;
                                            case Keys.Up:
                                                tmprow--;
                                                break;
                                            case Keys.Down:
                                                tmprow++;
                                                break;
                                        }
                                        MegamanEXE.SetAnimation("TELEPORT0");
                                        KeyLatch[ky_ar] = false;
                                        break;
                                    }
                                    KeyLatch[ky_ar] = false;
                                }

                                break;
                            case KeyState.Down:
                                if (KeyLatch[ky_ar] == false)
                                {
                                    KeyLatch[ky_ar] = true;
                                }
                                break;
                        }

                    }
                    #endregion
                }

                if (MegamanEXE.CurAnimation == "TELEPORT0" && MegamanEXE.finish)
                {
                    debugTXT = "  c" + tmpcol.ToString() + " r" + tmprow.ToString();
                    MegamanEXE.btlcol = tmpcol;
                    MegamanEXE.btlrow = tmprow;
                    MegamanEXE.battlepos = Stage.GetStageCoords(tmprow, tmpcol, MegamanEXE.battleposoffset);
                    MegamanEXE.SetAnimation("TELEPORT");
                }
                else if (MegamanEXE.CurAnimation != "DEFAULT" && MegamanEXE.finish)
                {
                    MegamanEXE.SetAnimation("DEFAULT");
                }

                Thread.Sleep(10);

                if (MegamanEXE != null) MegamanEXE.Update();
            } while (!terminateGame);
        }

        //Handles the flashing intro and SFX
        private void Flash_DoWork(object sender, DoWorkEventArgs e)
        {

            flash_opacity = 1;
            PlaySfx(21);
            
            for (int bgi = 1; bgi < 32; bgi++)
            {
                BGDict.Add(bgi - 1, Content.Load<Texture2D>(BGCode + "/bg" + bgi.ToString()));
            }
            bgUpdater.RunWorkerAsync();
            UserNavBgWrk.RunWorkerAsync();
            Thread.Sleep(1000); //-10% Opacity per frame
            do
            {
                flash_opacity -= 0.1f;
                Thread.Sleep(30);

            } while (flash_opacity >= 0);
            PlayBgm(2);
            SixtyHzBgWrkr.RunWorkerAsync();
            CustWindow.Show();
            Stage.showCust = true;
        }

        //Handles the scrolling BG
        private void BgUpdater_DoWork(object sender, DoWorkEventArgs e)
        {
            //do { Thread.Sleep(10); } while (haltingflag);
            //haltingflag = true;
            myBackground = new TiledBackground(BGDict[2], 240, 160);
            myBackground._startCoord = bgpos;
            bool scrolltick = false;
            var scrollcnt2 = 0;
            var scrollcnt = 0;
            int framedur = 3;
            bgReady = true;

            do
            {
                //Freaky stuff
                if (terminateGame) return;
                if (scrollcnt == 4) { scrolltick = true; }
                if (scrolltick)
                {
                    if (scrollcnt2 > 12)
                    {
                        scrollcnt2 = 0;
                        scrollcnt = 0;
                    }
                    else
                    {
                        bgpos.X = (bgpos.X - 1) % 128;

                        if (bgpos.X % 2 != 0)
                        {
                            bgpos.Y = (bgpos.Y - 1) % 64;
                        }
                        scrolltick = false;
                        scrollcnt2++;
                        scrollcnt = 0;
                    }
                }

                if (updateBGScroll == framedur)
                {
                    if (BGFrame + 1 == 31) { BGFrame = 0; }
                    if (BGFrame > 1 && BGFrame < 11) { framedur = 7; } else { framedur = 20; }
                    BGFrame++;
                    if (terminateGame) return;
                    myBackground = new TiledBackground(BGDict[BGFrame], 240, 160);
                    updateBGScroll = 0;
                }
                Thread.Sleep(7);
                updateBGScroll++;
                scrollcnt++;
            } while (!terminateGame | e.Cancel == false);
            bgReady = false;
            return;
        }
        protected override void UnloadContent()
        { terminateGame = true; }

        protected override void Update(GameTime gameTime)
        {
            //Send fresh data to input handler
            Input.Update(Keyboard.GetState(), gameTime);

            var ks_z = Input.KbStream[Keys.Z];
            var ks_q = Input.KbStream[Keys.Q];
            var ks_r = Input.KbStream[Keys.R];
            var ks_m = Input.KbStream[Keys.M];

            switch (ks_z.KeyState)
            {
                case KeyState.Down:
                    if (KeyLatch[Keys.Z] == false)
                    {
                        KeyLatch[Keys.Z] = true;
                    }
                    break;
                case KeyState.Up:
                    if (KeyLatch[Keys.Z] == true)
                    {
                        KeyLatch[Keys.Z] = false;
                        if (CustWindow.showCust)
                        {
                            CustWindow.Hide();
                            Stage.showCust = false;
                        }
                        else
                        {
                            CustWindow.Show();
                            Stage.showCust = true;
                        }
                    }
                    break;
            }

            switch (ks_q.KeyState)
            {
                case KeyState.Down:
                    if (KeyLatch[Keys.Q] == false)
                    {
                        KeyLatch[Keys.Q] = true;
                    }
                    break;
                case KeyState.Up:
                    if (KeyLatch[Keys.Q] == true)
                    {
                        CustWindow.SetHP(-500);
                        KeyLatch[Keys.Q] = false;
                    }
                    break;
            }

            switch (ks_r.KeyState)
            {
                case KeyState.Down:
                    if (KeyLatch[Keys.R] == false)
                    {
                        KeyLatch[Keys.R] = true;
                    }
                    break;
                case KeyState.Up:
                    if (KeyLatch[Keys.R] == true)
                    {
                        CustWindow.SetHP(500);
                        KeyLatch[Keys.R] = false;
                    }
                    break;
            }

            switch (ks_m.KeyState)
            {
                case KeyState.Down:
                    if (KeyLatch[Keys.M] == false)
                    {
                        KeyLatch[Keys.M] = true;
                    }
                    break;
                case KeyState.Up:
                    if (KeyLatch[Keys.M] == true)
                    {
                        mute = !mute;

                        if (mute)
                        {
                            SoundEffect.MasterVolume = 0f;

                        } else
                        {
                            SoundEffect.MasterVolume = 1f;
                        }

                        KeyLatch[Keys.M] = false;
                    }
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (terminateGame) return;
            MegamanEXE.battlepos = Stage.GetStageCoords(MegamanEXE.btlrow, MegamanEXE.btlcol, MegamanEXE.battleposoffset);

            SpriteBatch targetBatch = new SpriteBatch(GraphicsDevice);
            RenderTarget2D target = new RenderTarget2D(GraphicsDevice, screenres.W, screenres.H);
            GraphicsDevice.SetRenderTarget(target);
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            //Render Objects, Back to front layer
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

            //Draw the background
            if (bgUpdater.IsBusy && bgReady)
            {
                myBackground.Update(new Rectangle((int)bgpos.X, (int)bgpos.Y, 0, 0));
                myBackground.Draw(spriteBatch);
            }

            if (RenderQueue.Count > 0) { foreach (IBattleEntity s in RenderQueue) { s.Draw(); } }

            DrawEnemyNames();
            DrawDebugText();
            CustWindow.Draw();

            //Draw the flash
            if (flash.IsBusy) { spriteBatch.Draw(flsh, defaultrect, Color.FromNonPremultiplied(0xF8, 0xF8, 0xf8, 255) * flash_opacity); }
            spriteBatch.End();

            //Set rendering back to the back buffer
            GraphicsDevice.SetRenderTarget(null);

            //Update screen metrics
            UpdateViewbox();

            //Render target to back buffer
            targetBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            GraphicsDevice.Clear(Color.Black);
            targetBatch.Draw(target, Viewbox, Color.White);
            targetBatch.End();

            //Loop again
            base.Draw(gameTime);
        }

        /// <summary>
        /// Creates a rect with color fill, with option to create another texture.
        /// </summary>
        /// <param name="Rect">Parameters of the target rect</param>
        /// <param name="Colr">The color to fill</param>
        /// <returns></returns>
        private Texture2D RectangleFill(Rectangle Rect, Color Colr, bool Draw = true)
        {

            // Make a 1x1 texture named pixel.  
            Texture2D pixel = new Texture2D(GraphicsDevice, 1, 1);
            // Create a 1D array of color data to fill the pixel texture with.  
            Color[] colorData = { Colr };
            // Set the texture data with our color information.  
            pixel.SetData<Color>(colorData);

            if (Draw)
            {
                //They just wanna draw it
                spriteBatch.Draw(pixel, Rect, Color.White);
            }
            else
            {
                var SprtBtch = new SpriteBatch(GraphicsDevice);
                //They want ze copy of it
                RenderTarget2D FilledRect = new RenderTarget2D(GraphicsDevice, Rect.Width, Rect.Height);
                GraphicsDevice.SetRenderTarget(FilledRect);
                GraphicsDevice.Clear(Color.Transparent);
                SprtBtch.Begin();
                SprtBtch.Draw(pixel, Rect, Color.White);
                SprtBtch.End();
                GraphicsDevice.SetRenderTarget(null);
                SprtBtch = null;
                pixel = FilledRect;
            }

            return pixel;
        }


        /// <summary>
        /// Draws the black bg & enemy names, upto 3 names allowed.
        /// With built-in caching to avoid tearing in the BG.
        /// </summary>
        private void DrawEnemyNames()
        {
            if (!DisplayEnemyNames) { return; }
            if (EnemyNameCache == null)
            {
                //Load Font
                var Font2 = Fonts.List["Normal2"];
                // Do this frame-expensive operations once
                EnemyNameCache = new RenderTarget2D(GraphicsDevice, screenres.W, screenres.H);
                GraphicsDevice.SetRenderTarget(EnemyNameCache);
                GraphicsDevice.Clear(Color.Transparent);

                Vector2 TextOffset = new Vector2(-Font2.MeasureString("{").X, 0);

                for (int i = 0; i < EnemyNames.Count; i++)
                {
                    var EnemyName = EnemyNames[i];
                    //Measure text length and store to vector
                    var FontVect = Font2.MeasureString(EnemyName);

                    //Calculate vectors
                    var InitTextPos = (screenresvect - FontVect) * cancelY - new Vector2(2, -2);
                    var TextPos = TextOffset + InitTextPos;
                    var RectFill = 
                        new Rectangle(
                        (int)(TextPos.X - TextOffset.X),
                        (int)TextPos.Y + 2, (int)(FontVect.X) + 2,
                        (int)FontVect.Y - 4);

                    //Fill background
                    RectangleFill(RectFill, ColorHelper.FromHex(0x282828));

                    // { character is the chevron chr. in the Normal2 font.
                    EnemyName = "{" + EnemyName;

                    //Draw it
                    spriteBatch.DrawString(Font2, EnemyName, TextPos, Color.White);
                    TextOffset += (FontVect * cancelX) + new Vector2(0, 1);
                    Debug.Print("Drawn!");
                }
                GraphicsDevice.SetRenderTarget(null);
                spriteBatch.Draw(EnemyNameCache, new Rectangle(0, 0, EnemyNameCache.Width, EnemyNameCache.Height), Color.White);
            }
            else
            {
                //Draw ze cache
                spriteBatch.Draw(EnemyNameCache, new Rectangle(0, 0, EnemyNameCache.Width, EnemyNameCache.Height), Color.White);
            }
        }

        /// <summary>
        /// Draw some text on right center side
        /// for debugging
        /// </summary>
        private void DrawDebugText()
        {
            var Font1 = Fonts.List["Normal"];
            //Measure text length and store to vector
            var FontVect = Font1.MeasureString(debugTXT);
            //Calculate vectors
            var InitTextPos = (screenresvect / 2) - FontVect + ((screenresvect / 2) * cancelY);
            var TextPos = InitTextPos;
            //Draw it
            spriteBatch.DrawString(Font1, debugTXT, TextPos, Color.White);
        }
        
        /// <summary>
        /// Load references for the Sound Effects files
        /// </summary>
        private void LoadSfx()
        {
            DirectoryInfo dir = new DirectoryInfo(Content.RootDirectory + "/SFX");
            FileInfo[] files = dir.GetFiles("*.*");
            foreach (FileInfo file in files)
            {
                int key = Convert.ToInt16(file.Name.Split('-')[1].Split('.')[0]);
                sfxdict[key] = ("SFX/SFX-" + key.ToString().PadLeft(2, '0'));
            }
        }

        /// <summary>
        /// Load references for the BG music files
        /// </summary>
        private void LoadBgm()
        {
            DirectoryInfo dir = new DirectoryInfo(Content.RootDirectory + "/BGM");
            FileInfo[] files = dir.GetFiles("*.*");
            foreach (FileInfo file in files)
            {
                int key = Convert.ToInt16(file.Name.Split('-')[1].Split('.')[0]);
                bgmdict[key] = ("BGM/BGM-" + key.ToString().PadLeft(2, '0'));

            }
        }

        /// <summary>
        /// Play BG music on loop
        /// </summary>
        /// <param name="key">BGM ID</param>
        private void PlayBgm(int key)
        {
            if (mute) return;
            if (key == 0 && bgminst.State == SoundState.Playing && !mute)
            {
                bgminst.Stop();
            }
            var x = Content.Load<SoundEffect>(bgmdict[key]);
            bgminst = x.CreateInstance();
            bgminst.IsLooped = true;
            bgminst.Play();
            bgminst.Volume = 0.80f;
            //   bgminst.Pitch = 0.1f;
        }

        /// <summary>
        /// Play SFX 
        /// </summary>
        /// <param name="key">SFX ID</param>
        private void PlaySfx(int key)
        {
            if (mute) return;
            if (key == 0 && bgminst.State == SoundState.Playing && key > sfxdict.Count() && !mute)
            {
                return;
            }
            var x = Content.Load<SoundEffect>(sfxdict[key]);
            x.Play();
        }

        /// <summary>
        /// Resizes the viewbox with aspect ratio 
        /// </summary>
        private void UpdateViewbox()
        {
            // 240/160 | 3:2 aspect ratio
            double origratio = 1.5;
            double viewportratio = (double)GraphicsDevice.Viewport.Width / (double)GraphicsDevice.Viewport.Height;

            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;

            if (origratio > viewportratio)
            {
                viewportHeight = Convert.ToInt16(viewportWidth / origratio);
            }
            else
            {
                viewportWidth = Convert.ToInt16(viewportHeight * origratio);
            }

            int viewportX = (GraphicsDevice.Viewport.Width / 2) - (viewportWidth / 2);
            int viewportY = (GraphicsDevice.Viewport.Height / 2) - (viewportHeight / 2);

            Viewbox = new Rectangle(viewportX, viewportY, viewportWidth, viewportHeight);
        }

    }
}
