using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Tankontroller.Controller;
using Tankontroller.GUI;
using Tankontroller.Managers;


namespace Tankontroller.Scenes
{
    //-------------------------------------------------------------------------------------------------
    // PlayerSelectionScene
    //
    // This class is used to display the player selection screen.
    // The player selection screen displays the avatars of the players, and allows the players to
    // select their avatars and colours.
    //-------------------------------------------------------------------------------------------------
    public class PlayerSelectionScene : IScene
    {
        private string mapFile;

        Texture2D mBackgroundTexture = null;
        Rectangle mBackgroundRectangle;
        Texture2D[] mCountDownTextures = new Texture2D[6];
        Rectangle[] mCountDownRectangles = new Rectangle[6];

        List<AvatarPicker> mAvatarPickers;
        float mCountdown;
        bool mPlayersWereReady;

        public PlayerSelectionScene(string mapFile)
        {
            this.mapFile = mapFile;

            Tankontroller game = (Tankontroller)Tankontroller.Instance();

            spriteBatch = new SpriteBatch(game.GDM().GraphicsDevice);
            int screenWidth = game.GDM().GraphicsDevice.Viewport.Width;
            int screenHeight = game.GDM().GraphicsDevice.Viewport.Height;

            mBackgroundTexture = game.CM().Load<Texture2D>("background_06");
            mBackgroundRectangle = new Rectangle(0, 0, screenWidth, screenHeight);

            game.GetSoundManager().ReplaceCurrentMusicInstance("Music/Music_start", true);

            AvatarPicker.ResetBlockedIndexList();
            prepareAvatarPickers();
            mPlayersWereReady = false;
            prepareCountDown();
        }

        private void prepareCountDown()
        {
            Tankontroller game = (Tankontroller)Tankontroller.Instance();
            int screenWidth = game.GDM().GraphicsDevice.Viewport.Width;
            int screenHeight = game.GDM().GraphicsDevice.Viewport.Height;
            mCountDownTextures[0] = game.CM().Load<Texture2D>("countdown/five");
            mCountDownTextures[1] = game.CM().Load<Texture2D>("countdown/four");
            mCountDownTextures[2] = game.CM().Load<Texture2D>("countdown/three");
            mCountDownTextures[3] = game.CM().Load<Texture2D>("countdown/two");
            mCountDownTextures[4] = game.CM().Load<Texture2D>("countdown/one");
            mCountDownTextures[5] = game.CM().Load<Texture2D>("countdown/ready");

            for (int i = 0; i < mCountDownTextures.Length; i++)
            {
                Texture2D texture = mCountDownTextures[i];
                float widthRatio = (float)texture.Width / texture.Height;
                int height = 200;
                int width = (int)(height * widthRatio);
                Rectangle rectangle = new Rectangle((screenWidth - width) / 2, (screenHeight - height) / 2, width, height);
                mCountDownRectangles[i] = rectangle;
            }

        }
        private void prepareAvatarPickers() // this is quite slow, I assume because it is loading all the textures;
        {
            Tankontroller game = (Tankontroller)Tankontroller.Instance();

            int screenWidth = game.GDM().GraphicsDevice.Viewport.Width;
            int screenHeight = game.GDM().GraphicsDevice.Viewport.Height;
            int halfWidth = screenWidth / 2;
            int halfHeight = screenHeight / 2;
            mAvatarPickers = new List<AvatarPicker>();
            int left = 0;
            int top = 0;
            Rectangle rectangle = new Rectangle(left, top, halfWidth, halfHeight);
            mAvatarPickers.Add(new AvatarPicker(rectangle));
            left += halfWidth;
            rectangle = new Rectangle(left, top, halfWidth, halfHeight);
            mAvatarPickers.Add(new AvatarPicker(rectangle));
            left -= halfWidth;
            top += halfHeight;
            rectangle = new Rectangle(left, top, halfWidth, halfHeight);
            mAvatarPickers.Add(new AvatarPicker(rectangle));
            left += halfWidth;
            rectangle = new Rectangle(left, top, halfWidth, halfHeight);
            mAvatarPickers.Add(new AvatarPicker(rectangle));
        }

        private List<Player> GetReadyPlayers()
        {
            List<Player> players = new List<Player>();
            foreach (AvatarPicker avatarPicker in mAvatarPickers)
            {
                if (avatarPicker.HasController())
                {
                    players.Add(new Player(avatarPicker.GetController(), avatarPicker.GetAvatar()));
                }
            }
            return players;
        }

        private void StartGame()
        {
            IGame game = Tankontroller.Instance();
            game.SM().Transition(new GameScene(GetReadyPlayers(), mapFile), true);
        }

        private AvatarPicker getEmptyAvatarPicker()
        {
            foreach (AvatarPicker avatarPicker in mAvatarPickers)
            {
                if (!avatarPicker.HasController())
                {
                    return avatarPicker;
                }
            }
            return null;
        }
        private AvatarPicker getAvatarPickerFromController(IController pController)
        {
            foreach (AvatarPicker avatarPicker in mAvatarPickers)
            {
                if (avatarPicker.HasController())
                {
                    if (avatarPicker.GetController() == pController)
                    {
                        return avatarPicker;
                    }
                }
            }
            return null;
        }

        private bool PlayersReady()
        {
            int countAvatarPickerWithPlayers = 0;
            int countReadyPlayers = 0;
            foreach (AvatarPicker avatarPicker in mAvatarPickers)
            {
                if (avatarPicker.HasController())
                {
                    countAvatarPickerWithPlayers++;
                    if (avatarPicker.Ready())
                    {
                        countReadyPlayers++;
                    }
                }
            }
            return (countReadyPlayers > 1 && countReadyPlayers == countAvatarPickerWithPlayers);
        }

        private void UpdateAvatarPickers(float pSeconds)
        {
            foreach (AvatarPicker avatarPicker in mAvatarPickers)
            {
                avatarPicker.Update(pSeconds);
            }
        }

        public override void Update(float pSeconds)
        {
            IGame game = Tankontroller.Instance();
            game.GetControllerManager().DetectControllers();

            UpdateAvatarPickers(pSeconds);

            foreach (IController controller in game.GetControllerManager().GetControllers())
            {
                controller.UpdateController();
                AvatarPicker avatarPicker = getAvatarPickerFromController(controller);

                if (avatarPicker != null)
                {
                    if (!controller.IsConnected())
                    {
                        avatarPicker.Reset();
                        continue;
                    }
                    if (controller.IsPressed(Control.FIRE) && !controller.WasPressed(Control.FIRE))
                    {
                        avatarPicker.MakeSelection();
                    }
                    if (controller.IsPressed(Control.RECHARGE) && !controller.WasPressed(Control.RECHARGE))
                    {
                        avatarPicker.UndoSelection();
                    }
                    if (controller.IsPressed(Control.TURRET_RIGHT))
                    {
                        avatarPicker.ChangeSelection(1);
                    }
                    else if (controller.IsPressed(Control.TURRET_LEFT))
                    {
                        avatarPicker.ChangeSelection(-1);
                    }
                }
                else
                {
                    if (controller.IsPressed(Control.FIRE) && !controller.WasPressed(Control.FIRE))
                    {
                        avatarPicker = getEmptyAvatarPicker();
                        if (avatarPicker != null)
                            avatarPicker.SetController(controller);
                    }
                }
            }

            if (PlayersReady())
            {
                if (!mPlayersWereReady)
                {
                    mCountdown = 3f;
                    mPlayersWereReady = true;
                }
                // players ready do the countdown
                mCountdown -= pSeconds;
                if (mCountdown <= 0)
                {
                    StartGame();
                }
            }
            else
            {
                mPlayersWereReady = false;
            }

            Escape(); // This check should be at the end to avoid overwriting the controller colour
        }
        public override void Escape()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                ControllerManager.Instance.SetAllTheLEDsWhite();
                Tankontroller.Instance().SM().Transition(null);
            }
        }

        private void DrawAvatarPickers(SpriteBatch pSpriteBatch)
        {
            foreach (AvatarPicker avatarPicker in mAvatarPickers)
            {
                avatarPicker.Draw(pSpriteBatch);
            }
        }
        private void DrawCountDown(SpriteBatch pSpriteBatch)
        {
            if (mPlayersWereReady)
            {
                int index = 5;
                if (mCountdown > 4)
                {
                    index = 0;
                }
                else if (mCountdown > 3)
                {
                    index = 1;
                }
                else if (mCountdown > 2)
                {
                    index = 2;
                }
                else if (mCountdown > 1)
                {
                    index = 3;
                }
                else if (mCountdown > 0)
                {
                    index = 4;
                }
                else
                {
                    index = 5;
                }
                Texture2D texture = mCountDownTextures[index];
                Rectangle rectangle = mCountDownRectangles[index];
                spriteBatch.Draw(texture, rectangle, Color.White);
            }
        }
        public override void Draw(float pSeconds)
        {
            Tankontroller.Instance().GDM().GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            Color backColour = Color.White;

            spriteBatch.Draw(mBackgroundTexture, mBackgroundRectangle, backColour);

            DrawAvatarPickers(spriteBatch);
            DrawCountDown(spriteBatch);
            spriteBatch.End();
        }
    }
}