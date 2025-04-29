using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tankontroller.Controller;

namespace Tankontroller.Scenes
{
    //This is not created correctly as the scene is just one static image tat cannot be changed
    public class PickupAndBulletScene : IScene
    {
        private static readonly Texture2D mBackgroundTexture = Tankontroller.Instance().CM().Load<Texture2D>("background_01");
        private static readonly Texture2D infoTexture = Tankontroller.Instance().CM().Load<Texture2D>("pickupinfoscreen");
        private Rectangle mBackgroundRectangle;
        private Rectangle mPickupinfoRectangle;
        private Tankontroller mGameInstance;
        private MainMenuScene mStartScene;
        private Texture2D mContinueButtonTexture;
        private Rectangle mContinueButtonRectangle;
        private Texture2D mContinueTextTexture;
        private Rectangle mContinueTextRectangle;

        public PickupAndBulletScene(MainMenuScene startScene)
        {
            mStartScene = startScene;
            mGameInstance = (Tankontroller)Tankontroller.Instance();
            spriteBatch = new SpriteBatch(mGameInstance.GDM().GraphicsDevice);
            int screenWidth = mGameInstance.GDM().GraphicsDevice.Viewport.Width;
            int screenHeight = mGameInstance.GDM().GraphicsDevice.Viewport.Height;
            Tankontroller game = (Tankontroller)Tankontroller.Instance();
            mBackgroundRectangle = new Rectangle(0, 0, screenWidth, screenHeight);
            mContinueButtonTexture = game.CM().Load<Texture2D>("fire");
            mContinueButtonRectangle = new Rectangle(10, screenHeight / 2, mContinueButtonTexture.Width / 2, mContinueButtonTexture.Height / 2);
            mContinueTextTexture = game.CM().Load<Texture2D>("back");
            mContinueTextRectangle = new Rectangle(20 + mContinueButtonTexture.Width / 2, screenHeight / 2 + mContinueButtonTexture.Height / 4, mContinueTextTexture.Width, mContinueTextTexture.Height);
            mPickupinfoRectangle = new Rectangle(0, 0, infoTexture.Width, infoTexture.Height);
        }

        public override void Draw(float pSeconds)
        {
            mGameInstance.GDM().GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            spriteBatch.Draw(mBackgroundTexture, mBackgroundRectangle, Color.White);
            spriteBatch.Draw(infoTexture, mPickupinfoRectangle, Color.White);
            spriteBatch.Draw(mContinueButtonTexture, mContinueButtonRectangle, Color.White);
            spriteBatch.Draw(mContinueTextTexture, mContinueTextRectangle, Color.White);
            spriteBatch.End();
        }

        public override void Update(float pSeconds)
        {
            Escape();
            mGameInstance.GetControllerManager().DetectControllers();

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
        public override void Escape()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                mGameInstance.SM().Transition(mStartScene, true);
            }
        }


    }
}
