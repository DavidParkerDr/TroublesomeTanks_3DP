using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Tankontroller.Controller;
using Tankontroller.GUI;
using Tankontroller.World;

namespace Tankontroller.Scenes
{
    //-------------------------------------------------------------------------------------------------
    // This class is used to display the game over screen after the gameScene ends. It repositions the
    // GUIs of each player. The winner in the centre and the losers below them.
    //-------------------------------------------------------------------------------------------------
    public class GameOverScene : IScene
    {
        private static readonly float DISPLAY_TIME = DGS.Instance.GetFloat("SECONDS_TO_DISPLAY_GAMEOVER_SCREEN");
        IGame mGameInstance = Tankontroller.Instance();
        private List<Player> mPlayers;
        Texture2D mBackgroundTexture = null;
        Rectangle mRectangle;
        int mWinner;
        private float secondsLeft;
        private float secondsTillButtonAppears = 8.0f;
        private Texture2D mContinueButtonTexture;
        private Rectangle mContinueButtonRectangle;
        private Texture2D mContinueTextTexture;
        private Rectangle mContinueTextRectangle;
        public GameOverScene(Texture2D pBackgroundTexture, List<Player> pPlayers, int pWinner)
        {
            mBackgroundTexture = pBackgroundTexture;
            spriteBatch = new SpriteBatch(mGameInstance.GDM().GraphicsDevice);
            int screenWidth = mGameInstance.GDM().GraphicsDevice.Viewport.Width;
            int screenHeight = mGameInstance.GDM().GraphicsDevice.Viewport.Height;
            int height = screenHeight / 2;
            int width = (int)(mBackgroundTexture.Width * (float)height / mBackgroundTexture.Height);
            int x = (screenWidth - width) / 2;
            int y = (screenHeight - height) / 2;
            mRectangle = new Rectangle(0, 0, screenWidth, screenHeight);
            Tankontroller game = (Tankontroller)Tankontroller.Instance();
            mContinueButtonTexture = game.CM().Load<Texture2D>("fire");
            mContinueButtonRectangle = new Rectangle(10, screenHeight / 2, mContinueButtonTexture.Width / 2, mContinueButtonTexture.Height / 2);
            mContinueTextTexture = game.CM().Load<Texture2D>("back");
            mContinueTextRectangle = new Rectangle(20 + mContinueButtonTexture.Width / 2, screenHeight / 2 + mContinueButtonTexture.Height / 4, mContinueTextTexture.Width, mContinueTextTexture.Height);
            secondsLeft = DISPLAY_TIME;
            mGameInstance.GetSoundManager().ReplaceCurrentMusicInstance("Music/Music_start", true);
            mPlayers = pPlayers;
            mWinner = pWinner;
            RepositionGUIs();
        }
        //Repositions the GUIs of each player
        public void RepositionGUIs()
        {
            int loserCount = mPlayers.Count;
            if (mWinner != -1)
            {
                loserCount -= 1;
            }
            int textureWidth = mGameInstance.GDM().GraphicsDevice.Viewport.Width / 2;
            int textureHeight = (int)(textureWidth * ((float)254 / (float)540));
            int loserTextureWidth = textureWidth / 2;
            int centreXOffset = loserTextureWidth / 2;
            for (int i = 0; i < mPlayers.Count; i++)
            {
                if (i != mWinner) //Repositon the losers
                {
                    int centreY = mGameInstance.GDM().GraphicsDevice.Viewport.Height / 2 + textureHeight / 2;
                    int loserTextureHeight = (int)(loserTextureWidth * ((float)254 / (float)540));
                    Rectangle newRectangle = new Rectangle(centreXOffset, centreY, loserTextureWidth, loserTextureHeight);
                    mPlayers[i].GUI.RepositionForGameOver(newRectangle);
                    centreXOffset += loserTextureWidth;
                }
                else //Reposition the winner
                {
                    int centreX = mGameInstance.GDM().GraphicsDevice.Viewport.Width / 2 - textureWidth / 2;
                    int centreY = textureHeight / 2;
                    Rectangle newRectangle = new Rectangle(centreX, centreY, textureWidth, textureHeight);
                    mPlayers[mWinner].GUI.RepositionForGameOver(newRectangle);
                }
            }
        }
        public override void Update(float pSeconds)
        {
            secondsLeft -= pSeconds;
            secondsTillButtonAppears -= pSeconds;

            mGameInstance.GetControllerManager().DetectControllers();

            if (secondsTillButtonAppears <= 0.0f)
            {
                foreach (IController controller in mGameInstance.GetControllerManager().GetControllers())
                {
                    controller.UpdateController();
                    if (controller.IsPressed(Control.FIRE))
                    {
                        IGame game = Tankontroller.Instance();
                        game.GetControllerManager().SetAllTheLEDsWhite();
                        game.SM().Transition(null);
                    }
                }
            }

            if (secondsLeft <= 0.0f)
            {
                IGame game = Tankontroller.Instance();
                game.GetControllerManager().SetAllTheLEDsWhite();
                game.SM().Transition(null);
            }
        }
        public override void Draw(float pSeconds)
        {
            Tankontroller.Instance().GDM().GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            spriteBatch.Draw(mBackgroundTexture, mRectangle, Color.White);
            for (int i = 0; i < mPlayers.Count; i++)
            {
                if (i == mWinner)
                {
                    mPlayers[i].GUI.DrawAvatar(spriteBatch, Tank.MAX_HEALTH);
                    mPlayers[i].GUI.DrawHealthBar(spriteBatch, mPlayers[i].Tank.Health());
                }
                else { 
                mPlayers[i].GUI.DrawAvatar(spriteBatch, 1);
                mPlayers[i].GUI.DrawHealthBar(spriteBatch, mPlayers[i].Tank.Health());
                }
            }
            if (secondsTillButtonAppears <= 0.0f)
            {
                spriteBatch.Draw(mContinueButtonTexture, mContinueButtonRectangle, Color.White);
                spriteBatch.Draw(mContinueTextTexture, mContinueTextRectangle, Color.White);
            }

            spriteBatch.End();
        }
        public override void Escape()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Tankontroller.Instance().SM().Transition(null);
            }
        }
    }
}
