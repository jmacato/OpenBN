using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using static OpenBN.Helpers.MyMath;
using System.Reflection;
using System.Runtime.InteropServices;
using OpenBN.BattleElements;
using OpenBN.Helpers;
using OpenBN.Interfaces;
using OpenBN.Sprites;

namespace OpenBN
{
    public class Battle : Game, IParentComponent
    {
        public static string PublicDebug = "";
        #region Declares        
        public Navi UserNavi;
        public Size ScreenRes;
        public Vector2 ScreenResVector, CancelX, CancelY, Bgpos;
        private Rectangle _viewbox;
        private Rectangle _defaultrect;
        public GraphicsDeviceManager Graphics;
        public SpriteBatch SpriteBatch, TargetBatch;
        private BackgroundWorker _bgUpdater, _flash, _mainTimer;
        public static Dictionary<Keys, bool> KeyLatch;
        private List<string> _enemyNames;
        public Inputs Input;
        public FontHelper Fonts;
        public Stage Stage;
        private TiledBackground _myBackground;
        private CustomWindow _custWindow;
        private Texture2D _flsh;
        private RenderTarget2D _target, _enemyNameCache;
        private Sprite _bgSs;
        private Effect _desaturate;
        private float _flashOpacity;
        private bool _terminateGame, _displayEnemyNames;
        private int _scrollcnt, _screenresscalar;
        private Keys[] _monitoredKeys;
        private Keys[] _arrowKeys;
        private KeyboardState _kbstate;
        private Stopwatch _totalGameTime = new();
        private Stopwatch _lastUpdate = new();
        private delegate void RunGameTicks();
        private GameTime _myGameTime;
        private static Battle _instance;
        public static int ConstFramerate = 64;
        public bool FreezeObjects = false;

        #endregion

        public static Battle Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Battle();
                }
                return _instance;
            }
        }

        public Battle()
        {

            Graphics = new GraphicsDeviceManager(this);
            Graphics.IsFullScreen = false;
            ScreenRes = new Size(240, 160);
            _screenresscalar = 2;

            Window.Title = "OpenBN";
            Content.RootDirectory = "Content";

            // this.Window.AllowUserResizing = true;
            Window.ClientSizeChanged += Window_ClientSizeChanged;
            Window.AllowUserResizing = true;
            
            
            // Mosaicing is done as follows ->
            // 15 frames duration, 0->14 pixels X/Y for objects
            // 0->
        }


        public bool BgChanged { get; private set; }
        public new List<BattleModule> Components { get; set; }
        public bool Initialized { get; private set; }

        private void InitializeFields()
        {

            Bgpos = new Vector2(0, 0);
            _bgUpdater = new BackgroundWorker();
            CancelX = new Vector2(0, 1);
            CancelY = new Vector2(1, 0);
            _defaultrect = new Rectangle(0, 0, 240, 160);
            _displayEnemyNames = true;
            _enemyNames = new List<string>(3);
            _flashOpacity = 1;
            KeyLatch = new Dictionary<Keys, bool>();
            ScreenResVector = new Vector2(240, 160);
            _scrollcnt = 0;
            _viewbox = new Rectangle(0, 0, 240, 160);
            _terminateGame = false;
            _totalGameTime = new Stopwatch();
            _lastUpdate = new Stopwatch();
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            TargetBatch = new SpriteBatch(GraphicsDevice);
            _target = new RenderTarget2D(GraphicsDevice, ScreenRes.W, ScreenRes.H);

            // Desaturate = Content.Load<Effect>("Effects/Mosaicing");
            // Desaturate.CurrentTechnique = Desaturate.Techniques["Mosaic"];
            //
            // Desaturate.Parameters["TextureW"].SetValue(240);
            // Desaturate.Parameters["TextureH"].SetValue(160);
            // Desaturate.Parameters["CellSizeW"].SetValue(1);
            // Desaturate.Parameters["CellSizeH"].SetValue(1);
            
            // Desaturate = Content.Load<Effect>("Effects/Mosaicing");
            // Desaturate.CurrentTechnique = Desaturate.Techniques["Mosaic"];
            //
            // Desaturate.Parameters["TextureW"].SetValue(240);
            // Desaturate.Parameters["TextureH"].SetValue(160);
            // Desaturate.Parameters["CellSizeW"].SetValue(1);
            // Desaturate.Parameters["CellSizeH"].SetValue(1);

               
            _flsh = RectangleFill(new Rectangle(0, 0, ScreenRes.W, ScreenRes.H), ColorHelper.FromHex(0xF8F8F8), false);

            _monitoredKeys = new[]
            {
                Keys.A, Keys.S, Keys.X, Keys.Z,
                Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.Q, Keys.W, Keys.R, Keys.M
            };

            _arrowKeys = new[] { Keys.Up, Keys.Down, Keys.Left, Keys.Right };

            Fonts = new FontHelper(Content);
            Input = new Inputs(_monitoredKeys);
            Input.Halt = true;

            Stage = new Stage(this);
            _custWindow = new CustomWindow(this);
            LoadBg();
            _bgUpdater = new BackgroundWorker();
            _flash = new BackgroundWorker();
            _mainTimer = new BackgroundWorker();
            UserNavi = new Navi(this, Stage);

            //Assign bgwrkrs
            _bgUpdater.DoWork += BgUpdater_DoWork;
            _flash.DoWork += Flash_DoWork;
            _mainTimer.DoWork += MainTimer_DoWork;
            GraphicsDevice.DeviceLost += GraphicsDevice_DeviceLost;
            GraphicsDevice.DeviceReset += GraphicsDevice_DeviceReset;

            foreach (var x in _monitoredKeys)
            {
                KeyLatch.Add(x, false);
            }

            _flash.RunWorkerAsync();
            UpdateViewbox();


            _mainTimer.RunWorkerAsync();
            Graphics.SynchronizeWithVerticalRetrace = false;
            Initialized = true;
            _totalGameTime.Start();
            _lastUpdate.Start();
        }

        protected override void Initialize()
        {
            
            //Set real screen resolution
            Graphics.PreferredBackBufferWidth = ScreenRes.W * _screenresscalar;
            Graphics.PreferredBackBufferHeight = ScreenRes.H * _screenresscalar;
            Graphics.ApplyChanges();
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            if (!Initialized) InitializeFields();
            base.LoadContent();
        }

        private void GraphicsDevice_DeviceLost(object sender, EventArgs e)
        {
            Debug.Print("GraphicsDevice_DeviceLost");
        }

        private void GraphicsDevice_DeviceReset(object sender, EventArgs e)
        {
            Debug.Print("GraphicsDevice_DeviceReset");
            foreach (var xx in Components)
            {
                xx.Graphics = GraphicsDevice;
            }
        }

        private void UpdaterCallback(object state)
        {
            var handler = new RunGameTicks(RunTick);
            handler = RunTick;
            handler();
        }

        private void MainTimer_DoWork(object sender, DoWorkEventArgs e)
        {
            var handler = new RunGameTicks(RunTick);
            handler = RunTick;

            var mt = new Stopwatch();

            do
            {
                mt.Start();
                handler();
                WaitFrame(mt);
                mt.Reset();
            } while (!_terminateGame);
        }


        private void WaitFrame(Stopwatch mt)
        {
            do
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(4.1666666667));
            } while (mt.Elapsed <= TimeSpan.FromMilliseconds(1000 / ConstFramerate));
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            UpdateViewbox();
            Graphics.PreferredBackBufferHeight = _viewbox.Height;
            Graphics.PreferredBackBufferWidth = _viewbox.Width;
            Graphics.ApplyChanges();
        }

        private void LoadBg()
        {
            string[] bgcodelist = { "AD", "CA", "GA", "SS", "SK", "GA_HP", "GV" };
            var rnd = new Random();

            var bgcode = bgcodelist[rnd.Next(bgcodelist.Length)];

            if (_bgSs != null) _bgSs.Dispose();

            _bgSs = new Sprite("/BG/" + bgcode + "/BG.sasl", "BG/" + bgcode + "/" + bgcode, GraphicsDevice, Content);
            _myBackground = new TiledBackground(_bgSs.AnimationGroup.Values.First().CurrentFrame, 240, 160, _bgSs.Texture);
            _myBackground.StartCoord = Bgpos;
            BgChanged = true;
        }

        private void Flash_DoWork(object sender, DoWorkEventArgs e)
        {
            _flashOpacity = 1;
            _bgUpdater.RunWorkerAsync();
            Thread.Sleep(1000);
            do
            {
                _flashOpacity -= 0.1f;
                Thread.Sleep(30);
            } while (_flashOpacity >= 0);

            _flsh.Dispose();
            _custWindow.Show();
            Stage.ShowCust = true;
            Input.Halt = false;
            _flash = null;


        }

        private void BgUpdater_DoWork(object sender, DoWorkEventArgs e)
        {
            double dX = 1, dY = 1;
            double framedel = 1;

            do
            {
                {
                    if (BgChanged)
                    {
                        dX = Convert.ToDouble(_bgSs.Metadata["DX"]);
                        dY = Convert.ToDouble(_bgSs.Metadata["DY"]);
                        if (_bgSs.Metadata.ContainsKey("FRAMEDELAY"))
                        {
                            framedel = Convert.ToInt32(_bgSs.Metadata["FRAMEDELAY"]);
                            framedel = Clamp(framedel, 2, 128);
                        }
                        else
                        {
                            framedel = 2;
                        }
                        BgChanged = false;
                    }

                    _bgSs.AnimationGroup.Values.First().Next();
                    _myBackground.Curtextrect = _bgSs.AnimationGroup.Values.First().CurrentFrame;
                    var bgFrameBounds = _bgSs.AnimationGroup.Values.First().CurrentFrame;

                    if (!(dX == 0 & dY == 0))
                    {
                        if (_scrollcnt % framedel == 0)
                        {
                            Bgpos.X = (int)(Math.Ceiling(Bgpos.X + dX) % bgFrameBounds.Width);
                            if (Bgpos.X % 2 != 0)
                                Bgpos.Y = (int)(Math.Ceiling(Bgpos.Y + dY) % bgFrameBounds.Height);
                            _scrollcnt = 0;
                        }
                        _scrollcnt++;
                    }
                    Thread.Sleep(16);
                }
            } while (!_terminateGame);
        }

        private void RunTick()
        {
            Update2();
        }

        private void Update2()
        {
            _myGameTime = new GameTime(_totalGameTime.Elapsed, _lastUpdate.Elapsed);
            Input.Update(_kbstate, _myGameTime);
            UpdateComponents(_myGameTime);
            HandleInputs();
        }

        private void HandleInputs()
        {
            var ksZ = Input.KbStream[Keys.A];
            //var ks_m = Input.KbStream[Keys.M];

            switch (ksZ.KeyState)
            {
                case KeyState.Down:
                    if (KeyLatch[Keys.A] == false)
                    {
                        KeyLatch[Keys.A] = true;
                    }
                    break;
                case KeyState.Up:
                    if (KeyLatch[Keys.A])
                    {
                        KeyLatch[Keys.A] = false;
                        //if (CustWindow.showCust)
                        //{
                        //    CustWindow.Hide();
                        //    Stage.showCust = false;
                        //}
                        //else
                        {
                            _custWindow.Show();
                            Stage.ShowCust = true;
                        }
                    }
                    break;
            }

            //switch (ks_m.KeyState)
            //{
            //    case KeyState.Down:
            //        if (KeyLatch[Keys.M] == false)
            //        {
            //            KeyLatch[Keys.M] = true;
            //        }
            //        break;
            //    case KeyState.Up:
            //        if (KeyLatch[Keys.M])
            //        {
            //            UserNavi.ChangeAnimation();
            //            KeyLatch[Keys.M] = false;
            //        }
            //        break;
            //}

        }

        internal void UnfreezeObjects()
        {
            FreezeObjects = false;
        }

        protected override void Update(GameTime gameTime)
        {
            Graphics.ApplyChanges();
            _kbstate = Keyboard.GetState();
            _lastUpdate.Restart();
            CheckIfActive();
            base.Update(gameTime);
        }

        private void CheckIfActive()
        {
            if (IsActive)
                {
                    if (_desat < 1)
                    {
                        _desat += 0.1f;
                        _desat = MathHelper.Clamp(_desat, 0, 1);
                        // Desaturate.Parameters["percent"].SetValue(desat);
                        // SoundEffect.MasterVolume = desat;
                    }
                    //
                    // if (!mute && bgminst != null)
                    //     if (bgminst.State == SoundState.Paused) bgminst.Resume();
                    // Thread.Sleep(16);
                }
                else
                {
                    if (_desat > 0)
                    {
                        _desat -= 0.1f;
                        _desat = MathHelper.Clamp(_desat, 0, 1);
                        // Desaturate.Parameters["percent"].SetValue(desat);
                        // SoundEffect.MasterVolume = desat;
                    }
                    //
                    // if (!mute && bgminst != null)
                    //     if (bgminst.State == SoundState.Playing) bgminst.Pause();
                    //
                    // Thread.Sleep(16);
                }
        }

        bool _passonce = false;
        private float _desat;

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_target);
            GraphicsDevice.Clear(Color.FromNonPremultiplied(0xF8, 0xF8, 0xf8, 255));
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

            if (_bgSs != null)
            {
                _myBackground.Update(new Rectangle((int)Bgpos.X, (int)Bgpos.Y, 0, 0));
                _myBackground.Draw(SpriteBatch);
            }

            if (_passonce)
            {
                DrawComponents();
                _custWindow.Draw();
                DrawEnemyNames();
                DrawDebugText();
            }

            // Draw the flash
            if (_flashOpacity > Math.Round(0f, 2))
            {
                SpriteBatch.Draw(_flsh, _defaultrect, Color.FromNonPremultiplied(0xF8, 0xF8, 0xf8, 255) * _flashOpacity);
                _passonce = true;
            }



            SpriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);

            TargetBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            GraphicsDevice.Clear(Color.Black);
            TargetBatch.Draw(_target, _viewbox, Color.White);
            TargetBatch.End();
            base.Draw(gameTime);


        }


        #region Helper Functions

        /// <summary>
        ///     Creates a rect with color fill, with option to create another texture.
        /// </summary>
        /// <param name="rect">Parameters of the target rect</param>
        /// <param name="colr">The color to fill</param>
        /// <returns></returns>
        private Texture2D RectangleFill(Rectangle rect, Color colr, bool draw = true)
        {
            // Make a 1x1 texture named pixel.  
            var pixel = new Texture2D(GraphicsDevice, rect.Width, rect.Height);
            // Create a 1D array of color data to fill the pixel texture with.  
            var colorData = new Color[rect.Width * rect.Height];
            // Set the texture data with our color information.  

            if (draw)
            {
                SpriteBatch.Draw(pixel, rect, Color.White);
            }
            else
            {
                for (var x = 0; x < colorData.Length; x++)
                {
                    colorData[x] = colr;
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
            if (!_displayEnemyNames)
            {
                return;
            }
            if (_enemyNameCache == null)
            {
                //Load Font
                var font2 = Fonts.List["Normal2"];
                // Do this frame-expensive operations once
                _enemyNameCache = new RenderTarget2D(GraphicsDevice, ScreenRes.W, ScreenRes.H);
                GraphicsDevice.SetRenderTarget(_enemyNameCache);
                GraphicsDevice.Clear(Color.Transparent);

                var textOffset = new Vector2(-font2.MeasureString("{").X, 0);

                for (var i = 0; i < _enemyNames.Count; i++)
                {
                    var enemyName = _enemyNames[i];
                    //Measure text length and store to vector
                    var fontVect = font2.MeasureString(enemyName);

                    //Calculate vectors
                    var initTextPos = (ScreenResVector - fontVect) * CancelY - new Vector2(2, -2);
                    var textPos = textOffset + initTextPos;
                    var rectFill =
                        new Rectangle(
                            (int)(textPos.X - textOffset.X),
                            (int)textPos.Y + 2, (int)fontVect.X + 2,
                            (int)fontVect.Y - 4);

                    //Fill background
                    RectangleFill(rectFill, ColorHelper.FromHex(0x282828));

                    // { character is the chevron chr. in the Normal2 font.
                    enemyName = "{" + enemyName;

                    //Draw it
                    SpriteBatch.DrawString(font2, enemyName, textPos, Color.White);
                    textOffset += fontVect * CancelX + new Vector2(0, 1);
                    Debug.Print("Drawn!");
                }
                GraphicsDevice.SetRenderTarget(null);
                SpriteBatch.Draw(_enemyNameCache, new Rectangle(0, 0, _enemyNameCache.Width, _enemyNameCache.Height),
                    Color.White);
            }
            else
            {
                //Draw ze cache
                SpriteBatch.Draw(_enemyNameCache, new Rectangle(0, 0, _enemyNameCache.Width, _enemyNameCache.Height),
                    Color.White);
            }
        }

        /// <summary>
        ///     Draw some text on right center side
        ///     for debugging
        /// </summary>
        private void DrawDebugText()
        {
            var font1 = Fonts.List["Debug"];
            font1.Spacing = 0;

            var debugText = "";
            debugText += "BGPRGC{3,4}\r\n";
            debugText += "BGPOSX{0,4}\r\n";
            debugText += "BGPOSY{1,4}\r\n";
            debugText += "EMBROT{2,4}\r\n";
            debugText += "CUSTOM {4:EN;4;DIS}\r\n";


            debugText = String.Format(debugText, Bgpos.X, Bgpos.Y,
                Math.Round(_custWindow.EmblemRot, 2),
                _bgSs.AnimationGroup.Values.First().Pc.ToString().ToUpper()
                , Math.Round(_custWindow.CustBarProgress * 100, 2));

            debugText = PublicDebug.ToUpper().Replace("_", "");

            var fontVect = font1.MeasureString(debugText);
            //Calculate vectors
            var initTextPos = new Vector2(ScreenResVector.X - fontVect.X - 1, ScreenResVector.Y / 2 - fontVect.Y / 2);
            var textPos = initTextPos;
            //Draw it
            SpriteBatch.DrawString(font1, debugText, textPos, Color.White * 0.5f);
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

            var viewportX = GraphicsDevice.Viewport.Width / 2 - viewportWidth / 2;
            var viewportY = GraphicsDevice.Viewport.Height / 2 - viewportHeight / 2;

            _viewbox = new Rectangle(viewportX, viewportY, viewportWidth, viewportHeight);
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

        protected override void OnExiting(object sender, EventArgs args)
        {
            Environment.Exit(0);
            base.OnExiting(sender, args);
        }

        #endregion

    }

}