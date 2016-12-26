using System;
using System.Threading;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using OpenBN.ScriptedSprites;
using System.Reflection;

namespace OpenBN
{
    public class BattleField : Microsoft.Xna.Framework.Game
    {

        #region Declares

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

        //For bgm looping
        SoundEffectInstance bgminst;

        List<IBattleEntity> RenderQueue = new List<IBattleEntity>();
        List<string> EnemyNames = new List<string>(3);

        Inputs Input;
        Stage Stage;
        TiledBackground myBackground;
        UserNavi UserNavi;
        CustomWindow CustWindow;
        Texture2D flsh;

        SpriteBatch targetBatch;
        RenderTarget2D target;

        RenderTarget2D EnemyNameCache;
        FontHelper Fonts;
        Sprite BG_SS;
        Effect Desaturate;
        System.Windows.Forms.Timer mTimer;

        float flash_opacity = 1;
        bool terminateGame;
        bool mute = false;
        public bool DisplayEnemyNames = true;
        string debugTXT = "";
        bool manualTick = true;
        int manualTickCount = 0;
        float desat = 1;
        public int InactiveWaitMs = 10;
        int scrollcnt = 0;

        Keys[] MonitoredKeys;
        Keys[] ArrowKeys;
        #endregion
        System.Windows.Forms.Form myForm;

        public bool IsGameActive { get; private set; }

        public BattleField()
        {

            IsFixedTimeStep = false;

            // Necessary enchantments to ward off the updater and focusing bugs
            // UUU LAA UUU LAA *summons cybeasts instead*
            // PS: If monogame does focusing logic better, i'll definitely switch X|
            {
                mTimer = new System.Windows.Forms.Timer { Interval = (int)(TargetElapsedTime.TotalMilliseconds) };
                mTimer.Tick += (s, e) => {
                    if (IsGameActive)
                    {
                        if (manualTickCount > 2) { manualTick = true; Tick(); manualTick = false; }
                        manualTickCount++;
                    }
                };
                object host = typeof(Game).GetField("host", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
                host.GetType().BaseType.GetField("Suspend", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(host, null);
                host.GetType().BaseType.GetField("Resume", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(host, null);
                myForm = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(this.Window.Handle);
            }
    
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = false;
            
            //Set real screen resolution
            graphics.PreferredBackBufferWidth = screenres.W * screenresscalar;
            graphics.PreferredBackBufferHeight = screenres.H * screenresscalar;

            this.Window.Title = "OpenBN";
            Content.RootDirectory = "Content";

            this.Window.AllowUserResizing = false;
            this.Window.ClientSizeChanged += Window_ClientSizeChanged;

        }

     
        protected override void Initialize()
        {
            mTimer.Start();
            base.Initialize();
            terminateGame = false;

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
          //  SoundEffect.MasterVolume = 0f;
            spriteBatch = new SpriteBatch(GraphicsDevice);
            targetBatch = new SpriteBatch(GraphicsDevice);

            target = new RenderTarget2D(GraphicsDevice, screenres.W, screenres.H);

            flsh = RectangleFill(new Rectangle(0, 0, screenres.W, screenres.H), ColorHelper.FromHex(0xF8F8F8), false);

            MonitoredKeys = new Keys[] { Keys.A, Keys.S, Keys.X, Keys.Z, Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.Q, Keys.W, Keys.R, Keys.M };
            ArrowKeys = new Keys[] { Keys.Up, Keys.Down, Keys.Left, Keys.Right };
            
            Fonts = new FontHelper(Content);

            Stage = new Stage();
            CustWindow = new CustomWindow(Fonts);
            UserNavi = new UserNavi("MM");
            
            Input = new Inputs(MonitoredKeys);
            Input.Halt = true;

            RenderQueue.Add(Stage);
            RenderQueue.Add(UserNavi);
            RenderQueue.Add(CustWindow);

            for (int t = 0; t < RenderQueue.Count(); t++)
            {
                RenderQueue[t].Content = Content;
                RenderQueue[t].Graphics = GraphicsDevice;
                RenderQueue[t].SB = spriteBatch;
                RenderQueue[t].Initialize();
            }

            Desaturate = Content.Load<Effect>("Shaders/Desaturate");
            Desaturate.Parameters["ColourAmount"].SetValue(1);
            LoadSfx();
            LoadBgm();
              
            /*
            EnemyNames.Add("Mettaur");
            EnemyNames.Add("Mettaur");
            EnemyNames.Add("Mettaur");
            */

            LoadBG();
            flash.RunWorkerAsync();
        }

        protected override void OnDeactivated(object sender, EventArgs e)
        {
            IsGameActive = false;
            // if (ContentLoaded) PlaySfx(60);
        }
        protected override void OnActivated(object sender, EventArgs e)
        {
           IsGameActive = true;
            //  if (ContentLoaded) PlaySfx(60);
        }

        protected override void OnExiting(object sender, EventArgs e)
        {            
            terminateGame = true;
            mTimer.Dispose();
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            UpdateViewbox();
            graphics.PreferredBackBufferHeight = Viewbox.Height;
            graphics.PreferredBackBufferWidth = Viewbox.Width;
            graphics.ApplyChanges();
        }


        private void LoadBG()
        {
            string[] bgcodelist = { "SS", "SK", "AD", "CA", "GH" };
            Random rnd = new Random();
            var bgcode = bgcodelist[(int)rnd.Next(bgcodelist.Count())];

            BG_SS = new Sprite("/BG/" + bgcode + "/BG.sasl", "BG/" + bgcode + "/" + bgcode, GraphicsDevice, Content);

            myBackground = new TiledBackground(BG_SS.AnimationGroup.Values.First().CurrentFrame, 240, 160);
            myBackground._startCoord = bgpos;
        }
        /// <summary>
        /// 
        /// </summary>
        private void SixtyHzBgWrkr_DoWork(object sender, DoWorkEventArgs e)
        {
            do
            {
                if (IsGameActive)
                {
                    CustWindow.Update();
                    if (desat < 1)
                    {
                        desat += 0.1f;
                        desat = MathHelper.Clamp(desat, 0, 1);
                        Desaturate.Parameters["ColourAmount"].SetValue(desat);
                        SoundEffect.MasterVolume = desat;
                    }

                    if (!mute && bgminst != null)
                        if (bgminst.State == SoundState.Paused && desat > 0) bgminst.Resume();
                    Thread.Sleep(16);
                }
                else
                {
                    if (desat > 0)
                    {
                        desat -= 0.1f;
                        desat = MathHelper.Clamp(desat, 0, 1);
                        Desaturate.Parameters["ColourAmount"].SetValue(desat);
                        SoundEffect.MasterVolume = desat;
                    }

                    if (!mute && bgminst != null)
                    if (bgminst.State == SoundState.Playing && desat < 0.1) bgminst.Pause();

                    Thread.Sleep(16);
                }

                // Oh XNA, why thoust focusing logic is broken.
                switch (myForm.WindowState)
                {
                    case System.Windows.Forms.FormWindowState.Normal:
                        if (this.IsActive) IsGameActive = true;
                        break;
                    case System.Windows.Forms.FormWindowState.Minimized:
                        IsGameActive = false;
                        break;
                }

            } while (!terminateGame);
        }

        /// <summary>
        /// Handles User Navi's controls
        /// </summary>
        private void UserNavBgWrk_DoWork(object sender, DoWorkEventArgs e)
        {

            UserNavi.btlcol = 1;
            UserNavi.btlrow = 1;
            UserNavi.enableRender = true;
            UserNavi.SetAnimation("DEFAULT");
            UserNavi.battlepos = Stage.GetStageCoords(UserNavi.btlrow, UserNavi.btlcol, UserNavi.battleposoffset);

            int BusterState = 0;

            int tmpcol = UserNavi.btlcol;
            int tmprow = UserNavi.btlrow;

            do
            {
                if (IsGameActive && !CustWindow.showCust)
                {
                    if (Input != null && UserNavi.finish)
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
                                        UserNavi.SetAnimation("BUSTER");
                                        PlaySfx(7);
                                        BusterState = 0;
                                        break;
                                    }
                                    else if (Input.KbStream[Keys.X].DurDelta > 1500)
                                    {
                                        Debug.Print("ChgSht");
                                        UserNavi.SetAnimation("BUSTER");
                                        debugTXT += "\r\n" + Input.KbStream[Keys.X].DurDelta.ToString();
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
                            // if (!UserNavi.finish) break;
                            var arrw_ks = Input.KbStream[ky_ar].KeyState;
                            var arrw_dt = Input.KbStream[ky_ar].DurDelta;
                            int tmp1 = 0;
                            int tmp2 = 0;
                            switch (arrw_ks)
                            {
                                case KeyState.Up:
                                    if (KeyLatch[ky_ar] == true && UserNavi.finish)
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
                                            UserNavi.SetAnimation("TELEPORT0");
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

                    if (UserNavi.CurAnimation == "TELEPORT0" && UserNavi.finish)
                    {
                        debugTXT = "  c" + tmpcol.ToString() + " r" + tmprow.ToString();
                        UserNavi.btlcol = tmpcol;
                        UserNavi.btlrow = tmprow;
                        UserNavi.battlepos = Stage.GetStageCoords(tmprow, tmpcol, UserNavi.battleposoffset);
                        UserNavi.SetAnimation("TELEPORT");
                    }
                    else if (UserNavi.CurAnimation != "DEFAULT" && UserNavi.finish)
                    {
                        UserNavi.SetAnimation("DEFAULT");
                    }

                    Thread.Sleep(10);

                    if (UserNavi != null) UserNavi.Next();
                }
                else
                {
                    Thread.Sleep(InactiveWaitMs);
                }
            } while (!terminateGame);
        }
        /// <summary>
        /// Handles the flashing intro and SFX
        /// </summary>
        private void Flash_DoWork(object sender, DoWorkEventArgs e)
        {
            flash_opacity = 1;
            PlaySfx(21);
            UserNavBgWrk.RunWorkerAsync();

            bgUpdater.RunWorkerAsync();
            Thread.Sleep(1000); //-10% Opacity per frame
            do
            {
                flash_opacity -= 0.1f;
                Thread.Sleep(30);
            } while (flash_opacity >= 0);


            PlayBgm(1);
            SixtyHzBgWrkr.RunWorkerAsync();
            CustWindow.Show();
            Stage.showCust = true;
            Input.Halt = false;

            return;
        }
        /// <summary>
        ///  Handles the diagonally scrolling BG
        /// </summary>
        private void BgUpdater_DoWork(object sender, DoWorkEventArgs e)
        {
            do
            {
                if (terminateGame) return;
                if (IsGameActive)
                {
                    if (scrollcnt % 2 == 0)
                    {
                        bgpos.X = (bgpos.X - 1) % 128;
                        if (bgpos.X % 2 != 0)
                        { bgpos.Y = (bgpos.Y - 1) % 64; }
                        scrollcnt = 0;
                    }
                    BG_SS.AnimationGroup.Values.First().Next();
                    myBackground._texture = BG_SS.AnimationGroup.Values.First().CurrentFrame;
                    scrollcnt++;
                    Thread.Sleep(16);
                }
                else { Thread.Sleep(InactiveSleepTime); }
            } while (!terminateGame);
        }

        protected override void Update(GameTime gameTime)
        {
            if (IsGameActive)
            {
                if (!manualTick)
                {
                    manualTickCount = 0;
                }
                //Send fresh data to input handler
                Input.Update(Keyboard.GetState(), gameTime);
                UserNavi.battlepos = Stage.GetStageCoords(UserNavi.btlrow, UserNavi.btlcol, UserNavi.battleposoffset);
                foreach (IBattleEntity Renderable in RenderQueue)
                {
                    Renderable.Update();
                }
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
                            KeyLatch[Keys.M] = false;
                        }
                        break;
                }
            }
            else { Thread.Sleep(InactiveWaitMs); }
        }
        protected override void Draw(GameTime gameTime)
        {
           // if (!IsGameActive) { base.Draw(gameTime); return; }

            GraphicsDevice.SetRenderTarget(target);
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            //Render Objects, Back to front layer
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

            if(BG_SS != null)
            {
                myBackground.Update(new Rectangle((int)bgpos.X, (int)bgpos.Y, 0, 0));
                myBackground.Draw(spriteBatch);
            }

            spriteBatch.End();

            if (RenderQueue.Count > 0) { foreach (IBattleEntity s in RenderQueue) { s.Draw(); } }


            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

            DrawEnemyNames();
            DrawDebugText();

            //Draw the flash
            if (flash.IsBusy) { spriteBatch.Draw(flsh, defaultrect, Color.FromNonPremultiplied(0xF8, 0xF8, 0xf8, 255) * flash_opacity); }
            spriteBatch.End();

            //Set rendering back to the back buffer
            GraphicsDevice.SetRenderTarget(null);

            //Update screen metrics
            UpdateViewbox();

            targetBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, Desaturate);

            GraphicsDevice.Clear(Color.Black);
            targetBatch.Draw(target, Viewbox, Color.White);
            targetBatch.End();
        }



        #region Helper Functions

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
        /// Draws the black bg & enemy names.
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

        #endregion

    }
}
