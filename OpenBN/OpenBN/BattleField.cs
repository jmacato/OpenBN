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
using static OpenBN.MyMath;
using System.Reflection;

namespace OpenBN
{
    public class BattleField : Game, IParentComponent
    {

        #region Declares        
        public Size screenres;
        public Vector2 screenresvect ,cancelX, cancelY, bgpos;
        Rectangle Viewbox;
        Rectangle defaultrect;
        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        BackgroundWorker bgUpdater = new BackgroundWorker();
        BackgroundWorker flash = new BackgroundWorker();
        Dictionary<Keys, bool> KeyLatch;
        List<string> EnemyNames;
        public Inputs Input;
        public FontHelper Fonts;
        Stage Stage;
        TiledBackground myBackground;
        CustomWindow CustWindow;
        Texture2D flsh;
        RenderTarget2D target, EnemyNameCache;
        Sprite BG_SS;
        Effect Desaturate;
        System.Windows.Forms.Timer mTimer;
        float flash_opacity, desat;
        bool terminateGame, DisplayEnemyNames, manualTick;
        int manualTickCount, InactiveWaitMs, scrollcnt, screenresscalar;
        Keys[] MonitoredKeys;
        Keys[] ArrowKeys;
        #endregion

        System.Windows.Forms.Form myForm;

        public bool IsGameActive { get; private set; }
        public bool BGChanged { get; private set; }

        public new List<BattleComponent> Components { get; set; }

        public BattleField()
        {

            IsFixedTimeStep = false;

            // Necessary enchantments to ward off the updater and focusing bugs
            // UUU LAA UUU LAA *summons cybeasts instead*
            // PS: If monogame does focusing logic better, i'll definitely switch X|
            {
                mTimer = new System.Windows.Forms.Timer { Interval = (int)(1000 / 60) };
                mTimer.Tick += (s, e) =>
                {
                    if (manualTickCount > 2) { manualTick = true; Tick(); manualTick = false; }
                    manualTickCount++;
                };
                object host = typeof(Game).GetField("host", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
                host.GetType().BaseType.GetField("Suspend", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(host, null);
                host.GetType().BaseType.GetField("Resume", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(host, null);
                myForm = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Window.Handle);
            }


            Initialize();

            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = false;

            //Set real screen resolution
            graphics.PreferredBackBufferWidth = screenres.W * screenresscalar;
            graphics.PreferredBackBufferHeight = screenres.H * screenresscalar;

            Window.Title = "OpenBN";
            Content.RootDirectory = "Content";

            // this.Window.AllowUserResizing = true;
            Window.ClientSizeChanged += Window_ClientSizeChanged;

        }


        new void Initialize()
        {
            bgpos = new Vector2(0, 0);
            bgUpdater = new BackgroundWorker();
            cancelX = new Vector2(0, 1);
            cancelY = new Vector2(1, 0);
            defaultrect = new Rectangle(0, 0, 240, 160);
            desat = 1;
            DisplayEnemyNames = true;
            EnemyNames = new List<string>(3);
            flash = new BackgroundWorker();
            flash_opacity = 1;
            InactiveWaitMs = 10;
            KeyLatch = new Dictionary<Keys, bool>();
            manualTick = true;
            manualTickCount = 0;
            screenres = new Size(240, 160);
            screenresscalar = 2;
            screenresvect = new Vector2(240, 160);
            scrollcnt = 0;
            Viewbox = new Rectangle(0, 0, 240, 160);
            mTimer.Start();
        }

        protected override void LoadContent()
        {
            terminateGame = false;

            spriteBatch = new SpriteBatch(GraphicsDevice);
            target = new RenderTarget2D(GraphicsDevice, screenres.W, screenres.H);

            flsh = RectangleFill(new Rectangle(0, 0, screenres.W, screenres.H), ColorHelper.FromHex(0xF8F8F8), false);

            MonitoredKeys = new Keys[] { Keys.A, Keys.S, Keys.X, Keys.Z,
                Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.Q, Keys.W, Keys.R, Keys.M };
            ArrowKeys = new Keys[] { Keys.Up, Keys.Down, Keys.Left, Keys.Right };

            Fonts = new FontHelper(Content);
            Input = new Inputs(MonitoredKeys);
            Input.Halt = true;

            Stage = new Stage(this);
            CustWindow = new CustomWindow(this);

            Desaturate = Content.Load<Effect>("Shaders/Desaturate");
            Desaturate.Parameters["ColourAmount"].SetValue(1);
            LoadBG();

            //Assign bgwrkrs
            bgUpdater.DoWork += BgUpdater_DoWork;
            flash.DoWork += Flash_DoWork;

            foreach (Keys x in MonitoredKeys)
            {
                KeyLatch.Add(x, false);
            }

            flash.RunWorkerAsync();
            UpdateViewbox();
        }

        protected override void OnDeactivated(object sender, EventArgs e)
        {
            IsGameActive = false;
        }
        protected override void OnActivated(object sender, EventArgs e)
        {
            IsGameActive = true;
        }
        protected override void OnExiting(object sender, EventArgs e)
        {
            terminateGame = true;
            mTimer.Dispose();
            Content.Unload();
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
            string[] bgcodelist = { "AD", "CA", "GA", "SS", "SK", "GA_HP", "GV" };
            Random rnd = new Random();

            var bgcode = bgcodelist[(int)rnd.Next(bgcodelist.Count())];

            if (BG_SS != null) BG_SS.Dispose();

            BG_SS = new Sprite("/BG/" + bgcode + "/BG.sasl", "BG/" + bgcode + "/" + bgcode, GraphicsDevice, Content);
            myBackground = new TiledBackground(BG_SS.AnimationGroup.Values.First().CurrentFrame, 240, 160, BG_SS.texture);
            myBackground.startCoord = bgpos;
            BGChanged = true;
        }

        private void MiscUpdates()
        {
            if (!manualTick) { manualTickCount = 0; }
            if (IsGameActive)
            {
                if (desat < 1)
                {
                    desat += 0.1f;
                    desat = Clamp(desat, 0, 1);
                    Desaturate.Parameters["ColourAmount"].SetValue(desat);
                    SoundEffect.MasterVolume = desat;
                }
            }
            else
            {
                if (desat > 0)
                {
                    desat -= 0.1f;
                    desat = Clamp(desat, 0, 1);
                    Desaturate.Parameters["ColourAmount"].SetValue(desat);
                    SoundEffect.MasterVolume = desat;
                }
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
        }

        /// <summary>
        /// Handles User Navi's controls
        /// </summary>
        //private void UserNavBgWrk_DoWork(object sender, DoWorkEventArgs e)
        //{

        //    UserNavi.btlcol = 1;
        //    UserNavi.btlrow = 1;
        //    UserNavi.enableRender = true;
        //    UserNavi.SetAnimation("DEFAULT");
        //    UserNavi.battlepos = Stage.GetStageCoords(UserNavi.btlrow, UserNavi.btlcol, UserNavi.battleposoffset);

        //    int BusterState = 0;

        //    int tmpcol = UserNavi.btlcol;
        //    int tmprow = UserNavi.btlrow;

        //    do
        //    {
        //        if (IsGameActive && !CustWindow.showCust)
        //        {
        //            if (Input != null && UserNavi.finish)
        //            {
        //                #region Buster & Charge Shot
        //                var ks_x = Input.KbStream[Keys.X];
        //                switch (ks_x.KeyState)
        //                {
        //                    case KeyState.Down:
        //                        if (ks_x.DurDelta < 800)
        //                        {
        //                            if (BusterState == 1) break;
        //                            Debug.Print("chrg_Anim_start");
        //                            //    PlaySfx(14);
        //                            BusterState = 1;
        //                        }
        //                        else if (ks_x.DurDelta > 1500)
        //                        {
        //                            if (BusterState == 2) break;
        //                            Debug.Print("chrg_Anim_start2");
        //                            //    PlaySfx(15);
        //                            BusterState = 2;
        //                        }
        //                        KeyLatch[Keys.X] = true;
        //                        break;
        //                    case KeyState.Up:
        //                        if (KeyLatch[Keys.X] == true)
        //                        {
        //                            KeyLatch[Keys.X] = false;
        //                            if (ks_x.DurDelta < 1500)
        //                            {
        //                                Debug.Print("BstrSht");
        //                                UserNavi.SetAnimation("BUSTER");
        //                                //       PlaySfx(7);
        //                                BusterState = 0;
        //                                break;
        //                            }
        //                            else if (Input.KbStream[Keys.X].DurDelta > 1500)
        //                            {
        //                                Debug.Print("ChgSht");
        //                                UserNavi.SetAnimation("BUSTER");
        //                                //      debugTXT += "\r\n" + Input.KbStream[Keys.X].DurDelta.ToString();
        //                                //      PlaySfx(76);
        //                                BusterState = 0;
        //                                break;
        //                            }
        //                            else
        //                            {
        //                                if (BusterState > 0)
        //                                {
        //                                    BusterState = 0;
        //                                    KeyLatch[Keys.X] = false;
        //                                }
        //                            }
        //                        }
        //                        break;
        //                }
        //                #endregion
        //                #region Stage Movement
        //                foreach (Keys ky_ar in ArrowKeys)
        //                {
        //                    // if (!UserNavi.finish) break;
        //                    var arrw_ks = Input.KbStream[ky_ar].KeyState;
        //                    var arrw_dt = Input.KbStream[ky_ar].DurDelta;
        //                    int tmp1 = 0;
        //                    int tmp2 = 0;
        //                    switch (arrw_ks)
        //                    {
        //                        case KeyState.Up:
        //                            if (KeyLatch[ky_ar] == true && UserNavi.finish)
        //                            {
        //                                tmp1 = tmpcol;
        //                                tmp2 = tmprow;

        //                                switch (ky_ar)
        //                                {
        //                                    case Keys.Left:
        //                                        tmp1--;
        //                                        break;
        //                                    case Keys.Right:
        //                                        tmp1++;
        //                                        break;
        //                                    case Keys.Up:
        //                                        tmp2--;
        //                                        break;
        //                                    case Keys.Down:
        //                                        tmp2++;
        //                                        break;
        //                                }
        //                                if (Stage.IsMoveAllowed(tmp2, tmp1))
        //                                {
        //                                    switch (ky_ar)
        //                                    {
        //                                        case Keys.Left:
        //                                            tmpcol--;
        //                                            break;
        //                                        case Keys.Right:
        //                                            tmpcol++;
        //                                            break;
        //                                        case Keys.Up:
        //                                            tmprow--;
        //                                            break;
        //                                        case Keys.Down:
        //                                            tmprow++;
        //                                            break;
        //                                    }
        //                                    UserNavi.SetAnimation("TELEPORT0");
        //                                    KeyLatch[ky_ar] = false;
        //                                    break;
        //                                }
        //                                KeyLatch[ky_ar] = false;
        //                            }

        //                            break;
        //                        case KeyState.Down:
        //                            if (KeyLatch[ky_ar] == false)
        //                            {
        //                                KeyLatch[ky_ar] = true;
        //                            }
        //                            break;
        //                    }
        //                }
        //                #endregion
        //            }

        //            if (UserNavi.CurAnimation == "TELEPORT0" && UserNavi.finish)
        //            {
        //                UserNavi.btlcol = tmpcol;
        //                UserNavi.btlrow = tmprow;
        //                UserNavi.battlepos = Stage.GetStageCoords(tmprow, tmpcol, UserNavi.battleposoffset);
        //                UserNavi.SetAnimation("TELEPORT");
        //            }
        //            else if (UserNavi.CurAnimation != "DEFAULT" && UserNavi.finish)
        //            {
        //                UserNavi.SetAnimation("DEFAULT");
        //            }

        //            Thread.Sleep(10);

        //            if (UserNavi != null) UserNavi.Next();
        //        }
        //        else
        //        {
        //            Thread.Sleep(InactiveWaitMs);
        //        }
        //    } while (!terminateGame);
        //}


        /// <summary>
        /// Handles the flashing intro and SFX
        /// </summary>
        private void Flash_DoWork(object sender, DoWorkEventArgs e)
        {
            flash_opacity = 1;
            //PlaySfx(21);
            //UserNavBgWrk.RunWorkerAsync();

            bgUpdater.RunWorkerAsync();
            Thread.Sleep(1000); //-10% Opacity per frame
            do
            {
                flash_opacity -= 0.1f;
                Thread.Sleep(30);
            } while (flash_opacity >= 0);

            flsh.Dispose();

            // PlayBgm(1);
            //SixtyHzBgWrkr.RunWorkerAsync();
            CustWindow.Show();
            Stage.showCust = true;
            Input.Halt = false;
            flash = null;
            return;
        }

        /// <summary>
        ///  Handles the diagonally scrolling BG
        /// </summary>

        private void BgUpdater_DoWork(object sender, DoWorkEventArgs e)
        {
            double dX = 1, dY = 1;
            double framedel = 1;

            do
            {
                if (terminateGame) return;
                if (IsGameActive)
                {

                    if (BGChanged)
                    {
                        dX = Convert.ToDouble(BG_SS.Metadata["DX"]);
                        dY = Convert.ToDouble(BG_SS.Metadata["DY"]);
                        if (BG_SS.Metadata.ContainsKey("FRAMEDELAY"))
                        {
                            framedel = Convert.ToInt32(BG_SS.Metadata["FRAMEDELAY"]);
                            framedel = MyMath.Clamp(framedel, 2, 128);
                            // debugTXT = "\r\n FM:" + framedel;
                        }
                        else
                        {
                            framedel = 2;
                        }
                        BGChanged = false;
                    }

                    BG_SS.AnimationGroup.Values.First().Next();
                    myBackground.curtextrect = BG_SS.AnimationGroup.Values.First().CurrentFrame;
                    var bgFrameBounds = BG_SS.AnimationGroup.Values.First().CurrentFrame;

                    if (!(dX == 0 & dY == 0))
                    {
                        if (scrollcnt % framedel == 0)
                        {
                            bgpos.X = (int)(Math.Ceiling(bgpos.X + dX) % bgFrameBounds.Width);
                            if (bgpos.X % 2 != 0)
                                bgpos.Y = (int)(Math.Ceiling(bgpos.Y + dY) % bgFrameBounds.Height);
                            scrollcnt = 0;
                        }
                        scrollcnt++;
                    }

                    Thread.Sleep((int)(16));
                }
                else { Thread.Sleep(InactiveSleepTime); }
            } while (!terminateGame);
        }

        protected override void Update(GameTime gameTime)
        {
            MiscUpdates();
            if (IsGameActive)
            {
                //Send fresh data to input handler
                Input.Update(Keyboard.GetState(), gameTime);
                UpdateComponents(gameTime);
                HandleInputs();
            }
        }

        private void HandleInputs()
        {
            var ks_z = Input.KbStream[Keys.Z];
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


                        CustWindow.RotateEmblem();
                        LoadBG();


                        KeyLatch[Keys.M] = false;
                    }
                    break;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            //  if (!IsGameActive) { return; }
            GraphicsDevice.SetRenderTarget(target);
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            if (BG_SS != null) { myBackground.Update(new Rectangle((int)bgpos.X, (int)bgpos.Y, 0, 0)); myBackground.Draw(spriteBatch); }

            DrawComponents();
            CustWindow.Draw();
            DrawEnemyNames();
            DrawDebugText();

            //Draw the flash
            if (flash_opacity > 0) { spriteBatch.Draw(flsh, defaultrect, Color.FromNonPremultiplied(0xF8, 0xF8, 0xf8, 255) * flash_opacity); }

            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, Desaturate);
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Draw(target, Viewbox, Color.White);
            spriteBatch.End();
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
            Texture2D pixel = new Texture2D(GraphicsDevice, Rect.Width, Rect.Height);
            // Create a 1D array of color data to fill the pixel texture with.  
            Color[] colorData = new Color[Rect.Width * Rect.Height];
            // Set the texture data with our color information.  

            if (Draw)
            {
                //They just wanna draw it
                spriteBatch.Draw(pixel, Rect, Color.White);
            }
            else
            {
                for (int x = 0; x < colorData.Count(); x++)
                {
                    colorData[x] = Colr;
                }
                pixel.SetData<Color>(colorData);
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
            //// return;
            // var Font1 = Fonts.List["BattleMessage"];
            // Font1.Spacing = 0;
            // //Measure text length and store to vector
            // var FontVect = Font1.MeasureString("<BATTLE=START.>");
            // //Calculate vectors
            // var InitTextPos = (screenresvect / 2) - (FontVect / 2) - new Vector2(0,8);
            // var TextPos = InitTextPos;
            // //Draw it
            // spriteBatch.DrawString(Font1, "<BATTLE=START.>",TextPos, Color.White);

            // return;
            var Font1 = Fonts.List["Debug"];
            Font1.Spacing = 0;
            //Measure text length and store to vector
            string DebugText = "";
            DebugText += "BGPRGC{3,4}\r\n";
            DebugText += "BGPOSX{0,4}\r\n";
            DebugText += "BGPOSY{1,4}\r\n";
            DebugText += "EMBROT{2,4}\r\n";
            DebugText += "CUSTOM {4:EN;4;DIS}\r\n";
            DebugText += "CUSTOM {5,5}\r\n";



            DebugText = String.Format(DebugText, bgpos.X, bgpos.Y,
                Math.Round(CustWindow.EmblemRot, 2),
                BG_SS.AnimationGroup.Values.First().PC.ToString().ToUpper()
                , CustWindow.showCust.GetHashCode()
                , Math.Round(CustWindow.CustBarProgress * 100, 2));

            var FontVect = Font1.MeasureString(DebugText);
            //Calculate vectors
            //  var InitTextPos = (screenresvect) * new Vector2(1,1) - (FontVect) ;
            var InitTextPos = new Vector2(screenresvect.X - FontVect.X - 1, (screenresvect.Y / 2) - (FontVect.Y / 2));
            var TextPos = InitTextPos;
            //Draw it
            spriteBatch.DrawString(Font1, DebugText, TextPos, Color.White * 0.5f);




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

        public void UpdateComponents(GameTime gameTime)
        {
            foreach (BattleComponent x in this.Components)
            {
                x.Update(gameTime);
            }
        }

        public void DrawComponents()
        {
            foreach (BattleComponent x in this.Components)
            {
                x.Draw();
            }
        }

        #endregion

    }
}
