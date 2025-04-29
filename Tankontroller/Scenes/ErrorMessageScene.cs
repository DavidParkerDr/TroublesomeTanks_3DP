using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tankontroller.Scenes
{
    internal class ErrorMessageScene : IScene
    {
        private static readonly SpriteFont mFont = Tankontroller.Instance().CM().Load<SpriteFont>("handwritingfont");
        private const string mTitle = "an ERROR occurred";
        private Vector2 mTitlePos;
        private Vector2 mMessagePos;
        private SpriteBatch mSpriteBatch;
        private float mSeconds;
        private string mErrorMessage;

        public ErrorMessageScene(Exception pError)
        {
            mSpriteBatch = new SpriteBatch(Tankontroller.Instance().GDM().GraphicsDevice);
            mSeconds = 0.0f;
            mErrorMessage = pError.Message;
            int screenWidth = Tankontroller.Instance().GDM().GraphicsDevice.Viewport.Width;
            int screenHeight = Tankontroller.Instance().GDM().GraphicsDevice.Viewport.Height;
            mMessagePos = new Vector2(screenWidth / 2, screenHeight / 2);
            mMessagePos -= mFont.MeasureString(mErrorMessage) / 2; // Center the text
            mTitlePos = new Vector2(screenWidth / 2, screenHeight / 4);
            mTitlePos -= mFont.MeasureString(mTitle) / 2; // Center the text
            // Write the error message to a file
            System.IO.File.WriteAllText("ErrorLog" + DateTime.Now.ToFileTimeUtc() + ".txt", pError.ToString());
        }

        public override void Update(float pSeconds)
        {
            mSeconds += pSeconds;
            if (mSeconds > 5.0f)
            {
                Tankontroller.Instance().SM().Transition(null);
            }
        }

        public override void Draw(float pSeconds)
        {
            Tankontroller.Instance().GDM().GraphicsDevice.Clear(Color.Black);
            mSpriteBatch.Begin();
            mSpriteBatch.DrawString(mFont, mTitle, mTitlePos, Microsoft.Xna.Framework.Color.Red);
            mSpriteBatch.DrawString(mFont, mErrorMessage, mMessagePos, Microsoft.Xna.Framework.Color.White);
            mSpriteBatch.End();
        }
    }
}
