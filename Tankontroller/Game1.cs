using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using Tankontroller.Controller;
using Tankontroller.Managers;
using Tankontroller.Scenes;

namespace Tankontroller
{
    /// <summary>
    /// This interface specifies things that we want to get global access to (probs everything)
    /// </summary>
    public interface IGame
    {
        SoundManager GetSoundManager();
        ControllerManager GetControllerManager();
        SceneManager SM();
        ContentManager CM();
        GraphicsDeviceManager GDM();
        void Exit();
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Tankontroller : Game, IGame
    {
        private SoundManager mSoundManager = SoundManager.Instance;
        private SceneManager mSceneManager = SceneManager.Instance;
        private ControllerManager mControllerManager = ControllerManager.Instance;
        private GraphicsDeviceManager mGraphics;
        private SpriteBatch mBatch;

        private static IGame mGameInterface = null;


        public static IGame Instance()
        {
            if (mGameInterface == null)
            {
                mGameInterface = new Tankontroller();
            }
            return mGameInterface;
        }

        public SoundManager GetSoundManager() { return mSoundManager; }
        public ControllerManager GetControllerManager() { return mControllerManager; }
        public SceneManager SM() { return mSceneManager; }
        public ContentManager CM() { return Content; }
        public GraphicsDeviceManager GDM() { return mGraphics; }
        public Tankontroller()
        {
            mGraphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Set up keyboard controllers for debugging
            if (DGS.Instance.GetBool("ADD_KEYBOARD_CONTROLLER_1"))
            {

                Dictionary<Keys, Control> Player1KeyMap = new Dictionary<Keys, Control>
                {
                    { Keys.Q, Control.LEFT_TRACK_FORWARDS },
                    { Keys.A, Control.LEFT_TRACK_BACKWARDS },
                    { Keys.W, Control.RIGHT_TRACK_FORWARDS },
                    { Keys.S, Control.RIGHT_TRACK_BACKWARDS },
                    { Keys.Z, Control.TURRET_LEFT },
                    { Keys.X, Control.TURRET_RIGHT },
                    { Keys.Space, Control.FIRE },
                    { Keys.C, Control.RECHARGE }
                };

                Dictionary<Keys, int> Player1PortMap = new Dictionary<Keys, int>
                {
                    { Keys.D1, 0 },
                    { Keys.D2, 1 },
                    { Keys.D3, 2 },
                    { Keys.D4, 3 },
                    { Keys.D5, 4 },
                    { Keys.D6, 5 },
                    { Keys.D7, 6 },
                };

                mControllerManager.AddKeyboardController(Player1KeyMap, Player1PortMap);
            }

            if (DGS.Instance.GetBool("ADD_KEYBOARD_CONTROLLER_2"))
            {
                Dictionary<Keys, Control> Player2KeyMap = new Dictionary<Keys, Control>
                {
                    { Keys.Insert, Control.LEFT_TRACK_FORWARDS },
                    { Keys.Delete, Control.LEFT_TRACK_BACKWARDS },
                    { Keys.Home, Control.RIGHT_TRACK_FORWARDS },
                    { Keys.End, Control.RIGHT_TRACK_BACKWARDS },
                    { Keys.PageUp, Control.TURRET_LEFT },
                    { Keys.PageDown, Control.TURRET_RIGHT },
                    { Keys.Enter, Control.FIRE },
                    { Keys.P, Control.RECHARGE }
                };

                Dictionary<Keys, int> Player2PortMap = new Dictionary<Keys, int>
                {
                    { Keys.F1, 0 },
                    { Keys.F2, 1 },
                    { Keys.F3, 2 },
                    { Keys.F4, 3 },
                    { Keys.F5, 4 },
                    { Keys.F6, 5 },
                    { Keys.F7, 6 },
                };

                mControllerManager.AddKeyboardController(Player2KeyMap, Player2PortMap);
            }

            mGraphics.PreferredBackBufferHeight = DGS.Instance.GetInt("SCREENHEIGHT");
            mGraphics.PreferredBackBufferWidth = DGS.Instance.GetInt("SCREENWIDTH");
            mGraphics.IsFullScreen = DGS.Instance.GetBool("IS_FULL_SCREEN");
            Window.Title = "TroubleSome Tanks - CDP Edition!";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            mBatch = new SpriteBatch(GraphicsDevice);
            mSoundManager.Add("Music/Music_start");
            mSoundManager.Add("Music/Music_intro");
            mSoundManager.Add("Music/Music_loopable");
            mSoundManager.Add("Sounds/Button_Push");
            mSoundManager.Add("Sounds/Tank_Gun");
            mSoundManager.Add("Sounds/Tank_Tracks");
            mSoundManager.Add("Sounds/Tank_Clang1");
            mSoundManager.Add("Sounds/Tank_Clang2");
            mSoundManager.Add("Sounds/Tank_Clang3");

            ControllerManager.TextFont = Tankontroller.Instance().CM().Load<SpriteFont>("handwritingfont");
            ControllerManager.CircleTex = Tankontroller.Instance().CM().Load<Texture2D>("circle");
            ControllerManager.PixelTex = new Texture2D(GraphicsDevice, 1, 1);
            ControllerManager.PixelTex.SetData(new Color[] { Color.White });

            mSceneManager.Push(new FlashScreenScene());

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            float seconds = 0.001f * gameTime.ElapsedGameTime.Milliseconds;
            mSceneManager.Update(seconds);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            float seconds = 0.001f * gameTime.ElapsedGameTime.Milliseconds;
            mSceneManager.Draw(seconds);
            base.Draw(gameTime);
        }
    }
}
