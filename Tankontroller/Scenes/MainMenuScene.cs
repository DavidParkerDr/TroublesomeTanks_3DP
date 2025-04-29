using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Tankontroller.Controller;
using Tankontroller.GUI;

namespace Tankontroller.Scenes
{
    public class MainMenuScene : IScene
    {
        private static readonly bool SHOW_LIST_ON_MAIN_MENU = DGS.Instance.GetBool("SHOW_LIST_ON_MAIN_MENU");
        private static readonly bool ONLY_KEYBOARD_ON_MAIN_MENU = DGS.Instance.GetBool("ONLY_KEYBOARD_ON_MAIN_MENU");
        private static readonly string DEFAULT_MAP_FILE = DGS.Instance.GetString("DEFAULT_MAP_FILE");
        IGame mGameInstance = Tankontroller.Instance();

        ButtonList mButtonList = null;
        private static readonly Texture2D mForgroundTexture = Tankontroller.Instance().CM().Load<Texture2D>("menu_white");
        private static readonly Texture2D mBackgroundTexture = Tankontroller.Instance().CM().Load<Texture2D>("background_01");
        private static readonly Texture2D mTitleTexture = Tankontroller.Instance().CM().Load<Texture2D>("menu_title");
        Rectangle mBackgroundRectangle;
        Rectangle mTitleRectangle;
        Rectangle mControllerInfoRect;
        private float secondsLeft;

        private string defaultMapFile = DEFAULT_MAP_FILE; // Default map file


        public MainMenuScene()
        {
            spriteBatch = new SpriteBatch(mGameInstance.GDM().GraphicsDevice);
            int screenWidth = mGameInstance.GDM().GraphicsDevice.Viewport.Width;
            int screenHeight = mGameInstance.GDM().GraphicsDevice.Viewport.Height;

            mBackgroundRectangle = new Rectangle(0, 0, screenWidth, screenHeight);

            mTitleRectangle = new Rectangle((screenWidth / 2) - (644 / 2), (screenHeight / 2) - (128 / 2), 644, 128);

            mControllerInfoRect = new Rectangle(0, 0, screenWidth / 5, screenHeight);

            mButtonList = new ButtonList();

            Texture2D startGameButtonTexture = mGameInstance.CM().Load<Texture2D>("menu_play_white");
            Texture2D startGameButtonTexturePressed = mGameInstance.CM().Load<Texture2D>("menu_play_dark");
            Texture2D levelSelectionButtonTexture = mGameInstance.CM().Load<Texture2D>("menu_map_white");
            Texture2D levelSelectionButtonTexturePressed = mGameInstance.CM().Load<Texture2D>("menu_map_dark");
            Texture2D pickupinfoButtonTexture = mGameInstance.CM().Load<Texture2D>("menu_pickup_white");
            Texture2D pickupinfoButtonTexturePressed = mGameInstance.CM().Load<Texture2D>("menu_pickup_dark");
            Texture2D exitGameButtonTexture = mGameInstance.CM().Load<Texture2D>("menu_quit_white");
            Texture2D exitGameButtonTexturePressed = mGameInstance.CM().Load<Texture2D>("menu_quit_dark");

            // Generate the button list
            int buttonWidth = startGameButtonTexture.Width;
            int buttonHeight = startGameButtonTexture.Height;
            int buttonY = (screenHeight) / 2 + buttonHeight;
            int buttonX = (screenWidth - buttonWidth) / 2 - (int)(startGameButtonTexture.Width * 1.875f);
            Rectangle buttonRect = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);

            // Start button
            Button startGameButton = new Button(startGameButtonTexture, startGameButtonTexturePressed, buttonRect, Color.Red, StartGame);
            startGameButton.Selected = true;
            mButtonList.Add(startGameButton);

            // Level Selection Button
            buttonRect.X += (int)(startGameButtonTexture.Width * 1.25f);
            Button levelSelectionButton = new Button(levelSelectionButtonTexture, levelSelectionButtonTexturePressed, buttonRect, Color.Red, SelectLevel);
            levelSelectionButton.Selected = false;
            mButtonList.Add(levelSelectionButton);

            // Pickup Scene button
            buttonRect.X += (int)(startGameButtonTexture.Width * 1.25f);
            Button pickupinfoButton = new Button(pickupinfoButtonTexture, pickupinfoButtonTexturePressed, buttonRect, Color.Red, PickupInfo);
            pickupinfoButton.Selected = false;
            mButtonList.Add(pickupinfoButton);

            //Makes the exit game button
            buttonRect.X += (int)(startGameButtonTexture.Width * 1.25f);
            Button exitGameButton = new Button(exitGameButtonTexture, exitGameButtonTexturePressed, buttonRect, Color.Red, ExitGame);
            exitGameButton.Selected = false;
            mButtonList.Add(exitGameButton);

            secondsLeft = 0.1f;
            mGameInstance.GetSoundManager().ReplaceCurrentMusicInstance("Music/Music_start", true);
        }

        public void SetDefaultMapFile(string mapFile)
        {
            defaultMapFile = mapFile;
        }

        private void SelectLevel()
        {
            mGameInstance.SM().Transition(new LevelSelectionScene(this), false);
        }

        private void PickupInfo()
        {
            mGameInstance.SM().Transition(new PickupAndBulletScene(this), false);
        }

        private void ExitGame()
        {
            mGameInstance.SM().Transition(null);
        }

        private void StartGame()
        {
            mGameInstance.SM().Transition(new PlayerSelectionScene(defaultMapFile), false);
        }

        public override void Update(float pSeconds)
        {
            Escape();
            mGameInstance.GetControllerManager().DetectControllers();

            foreach (IController controller in mGameInstance.GetControllerManager().GetControllers())
            {
                controller.UpdateController();

                if (ONLY_KEYBOARD_ON_MAIN_MENU && !(controller is KeyboardController))
                {
                    continue; // If enabled in DSG skip any controller that isn't a keyboard
                }

                if (controller.IsPressed(Control.TURRET_LEFT) && !controller.WasPressed(Control.TURRET_LEFT))
                {
                    mButtonList.SelectPreviousButton();
                }
                if (controller.IsPressed(Control.TURRET_RIGHT) && !controller.WasPressed(Control.TURRET_RIGHT))
                {
                    mButtonList.SelectNextButton();
                }

                if (controller.IsPressed(Control.FIRE) && !controller.WasPressed(Control.FIRE) ||
                    controller.IsPressed(Control.RECHARGE) && !controller.WasPressed(Control.RECHARGE))
                {
                    SoundEffectInstance buttonPress = mGameInstance.GetSoundManager().GetSoundEffectInstance("Sounds/Button_Push");
                    buttonPress.Play();
                    mButtonList.PressSelectedButton();
                }
            }
        }

        public override void Draw(float pSeconds)
        {
            mGameInstance.GDM().GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            Color backColour = Color.White;

            spriteBatch.Draw(mBackgroundTexture, mBackgroundRectangle, backColour);
            spriteBatch.Draw(mForgroundTexture, mBackgroundRectangle, backColour);

            spriteBatch.Draw(mTitleTexture, mTitleRectangle, backColour);

            if (SHOW_LIST_ON_MAIN_MENU) mGameInstance.GetControllerManager().Draw(spriteBatch, mControllerInfoRect);

            mButtonList.Draw(spriteBatch);

            spriteBatch.End();
        }

        public override void Escape()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                ExitGame();
            }
        }
    }
}
