using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTMapEditor.GUI;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TTMapEditor.Managers;

namespace TTMapEditor.Scenes
{
    /// <summary>
    /// Main menu scene for the map editor.
    /// 
    /// Displays the title screen, background, and a list of buttons that allow
    /// the user to:
    /// - Load an existing map
    /// - Create a new map
    /// - Exit the editor
    /// 
    /// Handles keyboard navigation for the menu and transitions to the
    /// appropriate scenes based on the selected option.
    /// </summary>
    internal class MainMenuScene : IScene
    {

        IGame mGameInstance = TTMapEditor.Instance();

        ButtonList mButtonList = null;

        private static readonly Texture2D mForegroundTexture = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("menu_white");

        private static readonly Texture2D mBackgroundTexture = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("background_01");

        private static readonly Texture2D mTitleTexture = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("menu_title");

        Rectangle mBackgroundRectangle;

        Rectangle mTitleRectangle;

        Rectangle mControllerInfoRect;

        /// <summary>
        /// Creates the main menu scene, initializes layout rectangles,
        /// loads button textures, and sets up the button list and callbacks.
        /// </summary>
        public MainMenuScene()
        {
            // TODO: Make it scale correctly based on screen size.
            mSpriteBatch = new SpriteBatch(mGameInstance.GetGraphicsDeviceManager().GraphicsDevice);
            int screenWidth = mGameInstance.GetGraphicsDeviceManager().PreferredBackBufferWidth;
            int screenHeight = mGameInstance.GetGraphicsDeviceManager().PreferredBackBufferHeight;

            // Fullscreen background.
            mBackgroundRectangle = new Rectangle(0, 0, screenWidth, screenHeight);

            // Centered title near the top third of the screen.
            mTitleRectangle = new Rectangle((screenWidth / 2) - (644 / 2), (screenHeight / 2) - screenHeight / 3, 644, 128);

            // Reserved side panel for controller/input info (not drawn yet).
            mControllerInfoRect = new Rectangle(0, 0, screenWidth / 5, screenHeight);

            mButtonList = new ButtonList();

            // Load button textures.
            Texture2D startButtonTexture = mGameInstance.GetContentManager().Load<Texture2D>("Load_Map_Button");
            Texture2D exitButtonTexture = mGameInstance.GetContentManager().Load<Texture2D>("Exit_Button");
            Texture2D newMapButtonTexture = mGameInstance.GetContentManager().Load<Texture2D>("New_Map_Button");

            int buttonWidth = startButtonTexture.Width;
            int buttonHeight = startButtonTexture.Height;
            int buttonY = (screenHeight) / 4 + buttonHeight;
            int buttonX = (screenWidth - buttonWidth) / 2;

            // Base rectangle for first button; subsequent buttons are offset from this.
            Rectangle buttonRect = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);

            // "Load Map" button (default selected).
            Button loadMapButton = new Button(startButtonTexture, startButtonTexture, buttonRect, Color.Yellow, LoadMap);
            loadMapButton.mSelected = true;
            mButtonList.AddButton(loadMapButton);

            // "New Map" button.
            buttonRect.Y += (int)(buttonHeight * 1.25f);
            Button newMapButton = new Button(newMapButtonTexture, newMapButtonTexture, buttonRect, Color.Yellow, NewMap);
            newMapButton.mSelected = false;
            mButtonList.AddButton(newMapButton);

            // "Exit" button.
            buttonRect.Y += (int)(buttonHeight * 1.25f);
            Button exitButton = new Button(exitButtonTexture, exitButtonTexture, buttonRect, Color.Yellow, ExitGame);
            exitButton.mSelected = false;
            mButtonList.AddButton(exitButton);
        }

        /// <summary>
        /// Callback for the "Load Map" button.
        /// Transitions to the <see cref="MapSelectionScene"/> to choose an existing map.
        /// </summary>
        public void LoadMap()
        {
            mGameInstance.GetSceneManager().Transition(new MapSelectionScene(this), false);
        }

        /// <summary>
        /// Callback for the "New Map" button.
        /// Creates a new map with a timestamped name and transitions directly
        /// into the map editing scene.
        /// </summary>
        public void NewMap()
        {
            string newMapName = "New Map " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            mGameInstance.GetSceneManager().Transition(new MapEditingScene(this, newMapName, true), false);
        }

        /// <summary>
        /// Renders the main menu, including background, title, and buttons.
        /// </summary>
        /// <param name="pSeconds">Elapsed time in seconds since last draw call (unused).</param>
        public override void Draw(float pSeconds)
        {
            mGameInstance.GetGraphicsDeviceManager().GraphicsDevice.Clear(Color.CornflowerBlue);
            mSpriteBatch.Begin();
            Color backColour = Color.White;

            mSpriteBatch.Draw(mBackgroundTexture, mBackgroundRectangle, backColour);
            mSpriteBatch.Draw(mForegroundTexture, mBackgroundRectangle, backColour);
            mSpriteBatch.Draw(mTitleTexture, mTitleRectangle, backColour);
            mButtonList.Draw(mSpriteBatch);
            mSpriteBatch.End();
        }

        /// <summary>
        /// Updates the main menu logic each frame, including:
        /// - Handling escape to exit
        /// - Navigating the menu via keyboard
        /// - Updating the input manager state
        /// </summary>
        /// <param name="pSeconds">Elapsed time in seconds since last update (unused).</param>
        public override void Update(float pSeconds)
        {
            Escape();
            NavigateMenu();
            InputManager.Update();
        }

        /// <summary>
        /// Exits the game/editor by transitioning to a null scene.
        /// </summary>
        private void ExitGame()
        {
            mGameInstance.GetSceneManager().Transition(null);
        }

        /// <summary>
        /// Handles the Escape key while on the main menu.
        /// Pressing Escape exits the game/editor.
        /// </summary>
        public override void Escape()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                ExitGame();
            }
        }

        /// <summary>
        /// Handles keyboard-based navigation of the menu:
        /// - Down arrow: select next button
        /// - Up arrow: select previous button
        /// - Enter: activate the selected button's action
        /// </summary>
        private void NavigateMenu()
        {
            if (InputManager.isKeyPressed(Keys.Down))
            {
                mButtonList.SelectNextButton();
            }
            else if (InputManager.isKeyPressed(Keys.Up))
            {
                mButtonList.SelectPreviousButton();
            }
            else if (InputManager.isKeyPressed(Keys.Enter))
            {
                mButtonList.PressSelectedButton();
            }
        }
    }
}
