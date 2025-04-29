using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Tankontroller.Scenes
{
    //-------------------------------------------------------------------------------------------------
    // This is the opening scene that is displayed. It has the Spooky Elephant logo. Renders a texture
    // with a time limit.
    //-------------------------------------------------------------------------------------------------
    public class FlashScreenScene : IScene
    {
        private static readonly float DISPLAY_TIME = DGS.Instance.GetFloat("SECONDS_TO_DISPLAY_FLASH_SCREEN");
        private readonly Texture2D mLogoTexture = null;
        private readonly Rectangle mRectangle;
        private float secondsLeft;

        public FlashScreenScene()
        {
            IGame game = Tankontroller.Instance();
            mLogoTexture = game.CM().Load<Texture2D>("selogo");
            spriteBatch = new SpriteBatch(game.GDM().GraphicsDevice);

            // Creates a rectangle the size of the logo
            int screenWidth = game.GDM().GraphicsDevice.Viewport.Width;
            int screenHeight = game.GDM().GraphicsDevice.Viewport.Height;
            int height = screenHeight / 2;
            int width = (int)(mLogoTexture.Width * (float)height / mLogoTexture.Height);
            int x = (screenWidth - width) / 2;
            int y = (screenHeight - height) / 2;

            mRectangle = new Rectangle(x, y, width, height);
            secondsLeft = DISPLAY_TIME;
        }

        public override void Update(float pSeconds)
        {
            secondsLeft -= pSeconds;
            if (secondsLeft <= 0.0f)
            {
                IGame game = Tankontroller.Instance();
                game.SM().Transition(new MainMenuScene());
            }
        }

        public override void Draw(float pSeconds)
        {
            Tankontroller.Instance().GDM().GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            spriteBatch.Draw(mLogoTexture, mRectangle, Color.White);

            spriteBatch.End();
        }


    }
}