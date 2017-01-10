using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using static OpenBN.MyMath;
using System.Reflection;

namespace OpenBN
{
    public class Battle : Game, IParentComponent
    {
        private readonly Form myForm;

        private static Battle instance;


        public static Battle Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Battle();
                }
                return instance;
            }
        }

        public Battle()
        {
            IsFixedTimeStep = false;
            // Necessary enchantments to ward off the updater and focusing bugs
            // UUU LAA UUU LAA *summons cybeasts instead*
            // PS: If monogame does focusing logic better, i'll definitely switch X|
            {
                mTimer = new System.Windows.Forms.Timer { Interval = 1000 / 60 };
                mTimer.Tick += (s, e) =>
                {
                    if (manualTickCount > 2)
                    {
                        manualTick = true;
                        Tick();
                        manualTick = false;
                    }
                    manualTickCount++;
                };
                var host = typeof(Game).GetField("host", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
                host.GetType()
                    .BaseType.GetField("Suspend", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(host, null);
                host.GetType()
                    .BaseType.GetField("Resume", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(host, null);

                myForm = (Form)Control.FromHandle(Window.Handle);
            }

            Initialize();

            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = false;

            //Set real screen resolution
            graphics.PreferredBackBufferWidth = screenRes.W * screenresscalar;
            graphics.PreferredBackBufferHeight = screenRes.H * screenresscalar;

            Window.Title = "OpenBN";
            Content.RootDirectory = "Content";

            // this.Window.AllowUserResizing = true;
            Window.ClientSizeChanged += Window_ClientSizeChanged;
        }

        // public bool IsActive { get; private set; }
        public bool BGChanged { get; private set; }
        public new List<BattleComponent> Components { get; set; }
        public bool Initialized { get; private set; }

        private new void Initialize()
        {
            bgpos = new Vector2(0, 0);
            bgUpdater = new BackgroundWorker();
            cancelX = new Vector2(0, 1);
            cancelY = new Vector2(1, 0);
            defaultrect = new Rectangle(0, 0, 240, 160);
            desat = 1;
            displayEnemyNames = true;
            EnemyNames = new List<string>(3);
            flash_opacity = 1;
            inactiveWaitMs = 10;
            KeyLatch = new Dictionary<Keys, bool>();
            manualTick = true;
            manualTickCount = 0;
            screenRes = new Size(240, 160);
            screenresscalar = 2;
            screenResVector = new Vector2(240, 160);
            scrollcnt = 0;
            Viewbox = new Rectangle(0, 0, 240, 160);
            mTimer.Start();
        }

        protected override void LoadContent()
        {
            if (Initialized) return;
            terminateGame = false;

            spriteBatch = new SpriteBatch(GraphicsDevice);
            target = new RenderTarget2D(GraphicsDevice, screenRes.W, screenRes.H);

            flsh = RectangleFill(new Rectangle(0, 0, screenRes.W, screenRes.H), ColorHelper.FromHex(0xF8F8F8), false);

            MonitoredKeys = new[]
            {
                Keys.A, Keys.S, Keys.X, Keys.Z,
                Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.Q, Keys.W, Keys.R, Keys.M
            };
            ArrowKeys = new[] { Keys.Up, Keys.Down, Keys.Left, Keys.Right };

            Fonts = new FontHelper(Content);
            Input = new Inputs(MonitoredKeys);
            Input.Halt = true;

            Stage = new Stage(this);
            CustWindow = new CustomWindow(this);

            Desaturate = Content.Load<Effect>("Shaders/Desaturate");
            Desaturate.Parameters["ColourAmount"].SetValue(1);
            LoadBG();
            bgUpdater = new BackgroundWorker();
            flash = new BackgroundWorker();
            //Assign bgwrkrs
            bgUpdater.DoWork += BgUpdater_DoWork;
            flash.DoWork += Flash_DoWork;

            foreach (var x in MonitoredKeys)
            {
                KeyLatch.Add(x, false);
            }

            flash.RunWorkerAsync();
            UpdateViewbox();
            Initialized = true;


            base.LoadContent();
        }
        
        private void ResetRenderTarget()
        {
            target = new RenderTarget2D(GraphicsDevice, screenRes.W, screenRes.H);
        }
        

        protected override void OnExiting(object sender, EventArgs e)
        {
            //  terminateGame = true;
            //  mTimer.Dispose();
            //  Content.Unload();
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
            var rnd = new Random();

            var bgcode = bgcodelist[rnd.Next(bgcodelist.Count())];

            if (BG_SS != null) BG_SS.Dispose();

            BG_SS = new Sprite("/BG/" + bgcode + "/BG.sasl", "BG/" + bgcode + "/" + bgcode, GraphicsDevice, Content);
            myBackground = new TiledBackground(BG_SS.AnimationGroup.Values.First().CurrentFrame, 240, 160, BG_SS.texture);
            myBackground.startCoord = bgpos;
            BGChanged = true;
        }

        private void MiscUpdates()
        {

            //if (IsActive)
            //{
            //    if (desat < 1)
            //    {
            //        desat += 0.1f;
            //        desat = Clamp(desat, 0, 1);
            //        Desaturate.Parameters["ColourAmount"].SetValue(desat);
            //        SoundEffect.MasterVolume = desat;
            //    }
            //}
            //else
            //{
            //    if (desat > 0)
            //    {
            //        desat -= 0.1f;
            //        desat = Clamp(desat, 0, 1);
            //        Desaturate.Parameters["ColourAmount"].SetValue(desat);
            //        SoundEffect.MasterVolume = desat;
            //    }
            //}

            // Oh XNA, why thoust focusing logic is broken.
            //switch (myForm.WindowState)
            //{
            //    case FormWindowState.Normal:
            //        IsActive = true;
            //        break;
            //    case FormWindowState.Minimized:
            //        IsActive = false;
            //        break;
            //}
        }

        /// <summary>
        ///     Handles User Navi's controls
        /// </summary>
        /// <summary>
        ///     Handles the flashing intro and SFX
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
        }

        /// <summary>
        ///     Handles the diagonally scrolling BG
        /// </summary>
        private void BgUpdater_DoWork(object sender, DoWorkEventArgs e)
        {
            double dX = 1, dY = 1;
            double framedel = 1;

            do
            {
                // if (terminateGame) return;

                if (!manualTick)
                    manualTickCount = 0;

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
                    //try
                    //{

                    //  if(!curdrawin)  Tick();
                    //} catch (Exception ex)
                    //{
                    //    throw ex;
                    //}
                    Thread.Sleep(16);
                }
            } while (!terminateGame);
        }

        bool curdrawin = false;

        protected override void Update(GameTime gameTime)
        {
            MiscUpdates();
            // if (IsActive)
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
                    if (KeyLatch[Keys.Z])
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
                    if (KeyLatch[Keys.M])
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
            //  if (!IsActive) { return; }
            curdrawin = true;
            ResetRenderTarget();
            GraphicsDevice.SetRenderTarget(target);
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone);
            if (BG_SS != null)
            {
                myBackground.Update(new Rectangle((int)bgpos.X, (int)bgpos.Y, 0, 0));
                myBackground.Draw(spriteBatch);
            }

            DrawComponents();
            CustWindow.Draw();
            //   DrawEnemyNames();
            //  DrawDebugText();

            //Draw the flash
            if (flash_opacity > 0)
            {
                spriteBatch.Draw(flsh, defaultrect, Color.FromNonPremultiplied(0xF8, 0xF8, 0xf8, 255) * flash_opacity);
            }

            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, Desaturate);
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Draw(target, Viewbox, Color.White);
            spriteBatch.End();
            curdrawin = false;
            base.Draw(gameTime);

        }

        #region Declares        

        public Size screenRes;
        public Vector2 screenResVector, cancelX, cancelY, bgpos;
        private Rectangle Viewbox;
        private Rectangle defaultrect;
        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        private BackgroundWorker bgUpdater, flash;
        private Dictionary<Keys, bool> KeyLatch;
        private List<string> EnemyNames;
        public Inputs Input;
        public FontHelper Fonts;
        private Stage Stage;
        private TiledBackground myBackground;
        private CustomWindow CustWindow;
        private Texture2D flsh;
        private RenderTarget2D target, EnemyNameCache;
        private Sprite BG_SS;
        private Effect Desaturate;
        private readonly System.Windows.Forms.Timer mTimer;
        private float flash_opacity, desat;
        private bool terminateGame, displayEnemyNames, manualTick;
        private int manualTickCount, inactiveWaitMs = 10, scrollcnt, screenresscalar;
        private Keys[] MonitoredKeys;
        private Keys[] ArrowKeys;

        #endregion

        #region Helper Functions

        /// <summary>
        ///     Creates a rect with color fill, with option to create another texture.
        /// </summary>
        /// <param name="Rect">Parameters of the target rect</param>
        /// <param name="Colr">The color to fill</param>
        /// <returns></returns>
        private Texture2D RectangleFill(Rectangle Rect, Color Colr, bool Draw = true)
        {
            // Make a 1x1 texture named pixel.  
            var pixel = new Texture2D(GraphicsDevice, Rect.Width, Rect.Height);
            // Create a 1D array of color data to fill the pixel texture with.  
            var colorData = new Color[Rect.Width * Rect.Height];
            // Set the texture data with our color information.  

            if (Draw)
            {
                //They just wanna draw it
                spriteBatch.Draw(pixel, Rect, Color.White);
            }
            else
            {
                for (var x = 0; x < colorData.Count(); x++)
                {
                    colorData[x] = Colr;
                }
                pixel.SetData(colorData);
            }

            return pixel;
        }


        /// <summary>
        ///     Draws the black bg & enemy names.
        ///     With built-in caching to avoid tearing in the BG.
        /// </summary>
        private void DrawEnemyNames()
        {
            if (!displayEnemyNames)
            {
                return;
            }
            if (EnemyNameCache == null)
            {
                //Load Font
                var Font2 = Fonts.List["Normal2"];
                // Do this frame-expensive operations once
                EnemyNameCache = new RenderTarget2D(GraphicsDevice, screenRes.W, screenRes.H);
                GraphicsDevice.SetRenderTarget(EnemyNameCache);
                GraphicsDevice.Clear(Color.Transparent);

                var TextOffset = new Vector2(-Font2.MeasureString("{").X, 0);

                for (var i = 0; i < EnemyNames.Count; i++)
                {
                    var EnemyName = EnemyNames[i];
                    //Measure text length and store to vector
                    var FontVect = Font2.MeasureString(EnemyName);

                    //Calculate vectors
                    var InitTextPos = (screenResVector - FontVect) * cancelY - new Vector2(2, -2);
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
                spriteBatch.Draw(EnemyNameCache, new Rectangle(0, 0, EnemyNameCache.Width, EnemyNameCache.Height),
                    Color.White);
            }
            else
            {
                //Draw ze cache
                spriteBatch.Draw(EnemyNameCache, new Rectangle(0, 0, EnemyNameCache.Width, EnemyNameCache.Height),
                    Color.White);
            }
        }

        /// <summary>
        ///     Draw some text on right center side
        ///     for debugging
        /// </summary>
        private void DrawDebugText()
        {
            var Font1 = Fonts.List["Debug"];
            Font1.Spacing = 0;

            var DebugText = "";
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
            var InitTextPos = new Vector2(screenResVector.X - FontVect.X - 1, (screenResVector.Y / 2) - (FontVect.Y / 2));
            var TextPos = InitTextPos;
            //Draw it
            spriteBatch.DrawString(Font1, DebugText, TextPos, Color.White * 0.5f);
        }

        /// <summary>
        ///     Resizes the viewbox with aspect ratio
        /// </summary>
        private void UpdateViewbox()
        {
            // 240/160 | 3:2 aspect ratio
            var origratio = 1.5;
            var viewportratio = GraphicsDevice.Viewport.Width / (double)GraphicsDevice.Viewport.Height;

            var viewportWidth = GraphicsDevice.Viewport.Width;
            var viewportHeight = GraphicsDevice.Viewport.Height;

            if (origratio > viewportratio)
            {
                viewportHeight = Convert.ToInt16(viewportWidth / origratio);
            }
            else
            {
                viewportWidth = Convert.ToInt16(viewportHeight * origratio);
            }

            var viewportX = (GraphicsDevice.Viewport.Width / 2) - (viewportWidth / 2);
            var viewportY = (GraphicsDevice.Viewport.Height / 2) - (viewportHeight / 2);

            Viewbox = new Rectangle(viewportX, viewportY, viewportWidth, viewportHeight);
        }

        public void UpdateComponents(GameTime gameTime)
        {
            foreach (var x in Components)
            {
                x.Update(gameTime);
            }
        }

        public void DrawComponents()
        {
            foreach (var x in Components)
            {
                x.Draw();
            }
        }

        #endregion
    }
}