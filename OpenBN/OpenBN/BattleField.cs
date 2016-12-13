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
        Vector2 stagePos = new Vector2(0, 16);
        Vector2 stageBase = new Vector2(95, 1);
        Vector2 stageVect = new Vector2(40, 25);
        

        //bgwrkr for bg scroll
        BackgroundWorker bgUpdater = new BackgroundWorker();
        BackgroundWorker flash = new BackgroundWorker();
        BackgroundWorker UserNavBgWrk = new BackgroundWorker();
        BackgroundWorker SixtyHzBgWrkr = new BackgroundWorker();

        //List of sfx's & bgm's
        Dictionary<int, string> sfxdict = new Dictionary<int, string>();
        Dictionary<int, string> bgmdict = new Dictionary<int, string>();

        //For bgm looping
        SoundEffectInstance bgminst;

        List<IBattleEntity> RenderQueue = new List<IBattleEntity>();
        List<string> EnemyNames = new List<string>(3);

        Stage Stage;
        private Texture2D bg1;
        private Texture2D flsh;
        private Texture2D enamehdr;
        private TiledBackground myBackground;

        public SpriteFont Font1, Font2;

        private UserNavi MegamanEXE;
        private Keys[] MonitoredList = new Keys[] { Keys.A, Keys.S, Keys.X, Keys.Z, Keys.Up, Keys.Down, Keys.Left, Keys.Right };
        private Inputs Input;

        float flash_opacity = 1;
        int updateBGScroll = 0;
        int BGFrame = 1;
        bool terminateGame = false;
        bool bgReady = false;
        bool mute = false;
        bool latchArrowKeys = false;
        public bool DisplayEnemyNames = true;

        string debugTXT = "";
        
        protected override void Initialize()
        {
            base.Initialize();
            //Assign bgwrkrs
            bgUpdater.DoWork += BgUpdater_DoWork;
            UserNavBgWrk.DoWork += UserNavBgWrk_DoWork;
            flash.DoWork += Flash_DoWork;

            Input = new Inputs(MonitoredList);

            //Default font with gray shadow
            Font1 = Content.Load<SpriteFont>("Misc/exefont");
            Font1.Spacing = 1;

            //Default font for enemy names
            Font2 = Content.Load<SpriteFont>("Misc/exefont2");
            Font2.Spacing = 1;
        }


        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Stage = new Stage(Content, spriteBatch);
            RenderQueue.Add(Stage);

            MegamanEXE = new UserNavi("MM", Content, spriteBatch, stageVect);
            MegamanEXE.battleposoffset = new Vector2(-6, 59);
            MegamanEXE.btlcol = 2;
            MegamanEXE.btlrow = 2;
            MegamanEXE.enableRender = false;
            RenderQueue.Add(MegamanEXE);

            LoadSfx();
            LoadBgm();

            if (!bgUpdater.IsBusy) bgUpdater.RunWorkerAsync();
            if (!flash.IsBusy) flash.RunWorkerAsync();
            SixtyHzBgWrkr.DoWork += SixtyHzBgWrkr_DoWork;

            EnemyNames.Add("Piranha");
            EnemyNames.Add("Swordy");
        }
        
        private void SixtyHzBgWrkr_DoWork(object sender, DoWorkEventArgs e)
        {
            do
            {
                if(MegamanEXE != null) MegamanEXE.Next();
                Thread.Sleep(16);
            } while (!terminateGame);
        }

        public BattleField()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth = screenres.W * screenresscalar;
            graphics.PreferredBackBufferHeight = screenres.H * screenresscalar;
            this.Window.Title = "OpenBN Alpha";
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
            MegamanEXE.SetAnimation("TELEPORT");
            MegamanEXE.battlepos = Stage.GetStageCoords(MegamanEXE.btlrow, MegamanEXE.btlcol, MegamanEXE.battleposoffset);

            do
            {
                if (Input != null)
                {
                    /* Sorry for this crusty code */

                    #region Buster & Charge Shot

                    var BusterShot = Input.KeyboardStream
                    .Where(i =>
                    i.Key == Keys.X &&
                    i.Value.KeyState == KeyState.Up &&
                    i.Value.DurDelta < 700 &&
                    i.Value.DurDelta > 50).Count();

                    var BusterShot2 = Input.KeyboardStream
                    .Where(i =>
                    i.Key == Keys.X &&
                    i.Value.KeyState == KeyState.Down &&
                    i.Value.DurDelta < 700 &&
                    i.Value.DurDelta > 50).Count();

                    var BusterShot3 = Input.KeyboardStream
                    .Where(i =>
                    i.Key == Keys.X &&
                    i.Value.KeyState == KeyState.Down &&
                    i.Value.DurDelta > 700).Count();

                    if (BusterShot > 0 && MegamanEXE.finish)
                    {
                        MegamanEXE.SetAnimation("BUSTER");
                        Debug.Print("BstrShot");
                        Input.InputHandled(new Keys[] { Keys.X });
                    }

                    #endregion

                    #region Stage Movement

                    int tmpcol = MegamanEXE.btlcol;
                    int tmprow = MegamanEXE.btlrow;

                    var movement = Input.KeyboardStream.Select(i => i.Key)
                                  .Where(i => Input.KeyboardStream[i].KeyState == KeyState.Down &&
                                  Input.KeyboardStream[i].DurDelta < 1000 &&
                                  Input.KeyboardStream[i].DurDelta > 50
                                  ).Where(i => i == Keys.Left | i == Keys.Right | i == Keys.Up | i == Keys.Down);

                    var movement2 = Input.KeyboardStream.Select(i => i.Key)
                                  .Where(i => Input.KeyboardStream[i].KeyState == KeyState.Up &&
                                  Input.KeyboardStream[i].DurDelta > 50
                                  ).Where(i => i == Keys.Left | i == Keys.Right | i == Keys.Up | i == Keys.Down);

                    if (movement.Count() > 0 && MegamanEXE.finish){ latchArrowKeys = true;}

                    if (movement2.Count() > 0 && latchArrowKeys) {

                        latchArrowKeys = false;

                        switch (movement2.ToArray()[0])
                        {
                            case Keys.Up:
                                tmpcol--;
                                break;
                            case Keys.Down:
                                tmpcol++;
                                break;
                            case Keys.Left:
                                tmprow--;
                                break;
                            case Keys.Right:
                                tmprow++;
                                break;
                        }
                        Input.InputHandled(new Keys[] { Keys.Left, Keys.Right, Keys.Up, Keys.Down });

                        if (Stage.IsMoveAllowed(tmpcol, tmprow))
                        {
                            MegamanEXE.btlcol = tmpcol;
                            MegamanEXE.btlrow = tmprow;
                            MegamanEXE.SetAnimation("TELEPORT0");
                        }

                    }
                    #endregion

                }

                if (MegamanEXE.CurAnimation == "TELEPORT0" && MegamanEXE.finish)
                {
                    MegamanEXE.SetAnimation("TELEPORT");
                    MegamanEXE.battlepos = Stage.GetStageCoords(MegamanEXE.btlrow, MegamanEXE.btlcol, MegamanEXE.battleposoffset);
                }
                else if (MegamanEXE.CurAnimation != "DEFAULT" && MegamanEXE.finish)
                {
                    MegamanEXE.SetAnimation("DEFAULT");
                }
                Thread.Sleep(20);
            } while (!terminateGame);
        }

        //Handles the flashing intro and SFX
        private void Flash_DoWork(object sender, DoWorkEventArgs e)
        {

            flsh = Content.Load<Texture2D>("Misc/flash");
            enamehdr = Content.Load<Texture2D>("Misc/ENmeHdr");
            flash_opacity = 1;
            PlaySfx(21);
            Thread.Sleep(1000); //-10% Opacity per frame
            do
            {
                flash_opacity -= 0.1f;
                Thread.Sleep(30);

            } while (flash_opacity >= 0);
            PlayBgm(2);
            if (!UserNavBgWrk.IsBusy) UserNavBgWrk.RunWorkerAsync();

            SixtyHzBgWrkr.RunWorkerAsync();
        }

        //Handles the scrolling BG
        private void BgUpdater_DoWork(object sender, DoWorkEventArgs e)
        {
            //do { Thread.Sleep(10); } while (haltingflag);
            //haltingflag = true;
            bg1 = Content.Load<Texture2D>(BGCode + "/bg1");
            myBackground = new TiledBackground(bg1, 240, 160);
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
                        bgpos.Y = (bgpos.Y - 1) % 64;
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
                    bg1 = Content.Load<Texture2D>(BGCode + "/bg" + BGFrame.ToString());
                    myBackground = new TiledBackground(bg1, 240, 160);
                    updateBGScroll = 0;
                }
                Thread.Sleep(8);
                updateBGScroll++;
                scrollcnt++;
            } while (!terminateGame | e.Cancel == false);
            bgReady = false;
            return;
        }


        protected override void UnloadContent()
        {terminateGame = true;}

        protected override void Update(GameTime gameTime)
        {
            //Send fresh data to input handler
            Input.Update(Keyboard.GetState(),gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (terminateGame) return;

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

            if (RenderQueue.Count > 0){foreach (IBattleEntity s in RenderQueue){s.Draw();}}

            DrawEnemyNames();
            DrawDebugText();

            //Draw the flash
            if (flash.IsBusy){spriteBatch.Draw(flsh, defaultrect, Color.FromNonPremultiplied(0xF8, 0xF8, 0xf8, 255) * flash_opacity);}
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
        /// Creates a rect with color fill
        /// </summary>
        /// <param name="Rect">Parameters of the target rect</param>
        /// <param name="Colr">The color to fill</param>
        /// <returns></returns>
        private Texture2D RectangleFill(Rectangle Rect, Color Colr)
        {
            // Make a 1x1 texture named pixel.  
            Texture2D pixel = new Texture2D(GraphicsDevice, 1, 1);
            // Create a 1D array of color data to fill the pixel texture with.  
            Color[] colorData = { Colr };
            // Set the texture data with our color information.  
            pixel.SetData<Color>(colorData);
            spriteBatch.Draw(pixel, Rect, Color.White);
            return pixel;
        }

        /// <summary>
        /// Draws the black bg & enemy names, upto 3 names allowed.
        /// </summary>
        private void DrawEnemyNames()
        {
            if(!DisplayEnemyNames) { return; }
            Vector2 TextOffset = Vector2.Zero;
            for (int i = 0; i < EnemyNames.Count; i++)
            {
                var EnemyName = EnemyNames[i];
                //Measure text length and store to vector
                var FontVect = Font2.MeasureString(EnemyName);
                //Calculate vectors
                var InitTextPos = (screenresvect - FontVect) * cancelY - new Vector2(0, -2);
                var TextPos = TextOffset + InitTextPos;
                var RectFill = new Rectangle((int)TextPos.X, (int)TextPos.Y + 2, (int)FontVect.X, (int)FontVect.Y - 4);
                var HeaderTextPos = TextPos - new Vector2(enamehdr.Width, -2);
                //Draw that diagonal thingy before text
                spriteBatch.Draw(enamehdr, HeaderTextPos, Color.White);
                //Fill background
                RectangleFill(RectFill, Color.FromNonPremultiplied(40, 40, 40, 255));
                //Draw it
                spriteBatch.DrawString(Font2, EnemyName, TextPos, Color.White);
                TextOffset += (FontVect * cancelX) + new Vector2(0,1);
            }
        }
       
        /// <summary>
        /// Draw some text on right center side
        /// for debugging
        /// </summary>
        private void DrawDebugText()
        {
             
                //Measure text length and store to vector
                var FontVect = Font2.MeasureString(debugTXT);
                //Calculate vectors
                var InitTextPos = (screenresvect/2) - FontVect + ((screenresvect/2)*cancelY);
                var TextPos = InitTextPos;
                //Draw it
                spriteBatch.DrawString(Font2, debugTXT, TextPos, Color.Black * 0.9f);
            
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