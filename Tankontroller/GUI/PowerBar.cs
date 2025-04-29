using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tankontroller.GUI
{
    //-------------------------------------------------------------------------------------------------
    // PowerBar
    //
    // This class is used to represent the power bar of a tank. It displays the power bar in the game.
    //-------------------------------------------------------------------------------------------------
    public class PowerBar
    {
        private static Texture2D mBorderTex = Tankontroller.Instance().CM().Load<Texture2D>("powerBar_border");
        private static Texture2D mPowerTex = Tankontroller.Instance().CM().Load<Texture2D>("powerBar_power");
        private Vector2 mPos;
        private int mWidth, mHeight;

        public PowerBar(Vector2 pPos, int pWidth, int pHeight)
        {
            mPos = pPos;
            mWidth = pWidth;
            mHeight = pHeight;
        }

        public void Draw(SpriteBatch pSpriteBatch, float pCharge, bool enoughCharge)
        {
            pSpriteBatch.Draw(mBorderTex, new Rectangle((int)mPos.X, (int)mPos.Y, mWidth, mHeight), null, Color.White, 0f, new Vector2(0, 0), SpriteEffects.None, 0f);

            if (pCharge > 0)
            {
                int height = (int)(mHeight / 100f * (100 / Controller.Controller.MAX_CHARGE * pCharge));
                if (enoughCharge)
                {
                    pSpriteBatch.Draw(mPowerTex, new Rectangle((int)mPos.X, (int)mPos.Y + mHeight - height, mWidth, height), null, Color.LightBlue, 0f, new Vector2(0, 0), SpriteEffects.None, 0f);
                }
                else
                {
                    pSpriteBatch.Draw(mPowerTex, new Rectangle((int)mPos.X, (int)mPos.Y, mWidth, mHeight), null, Color.Red, 0f, new Vector2(0, 0), SpriteEffects.None, 0f);
                    pSpriteBatch.Draw(mPowerTex, new Rectangle((int)mPos.X, (int)mPos.Y + mHeight - height, mWidth, height), null, Color.LightBlue, 0f, new Vector2(0, 0), SpriteEffects.None, 0f);
                }
                //pSpriteBatch.DrawString(gameFont, pCharge, new Vector2(665, 40), Color.Wheat, 0f, new Vector2(0, 0), 0.6f, SpriteEffects.None, 0f);
            }

            else if (pCharge == 0)
            {
                pSpriteBatch.Draw(mPowerTex, new Rectangle((int)mPos.X, (int)mPos.Y, mWidth, mHeight), null, Color.Red, 0f, new Vector2(0, 0), SpriteEffects.None, 0f);
                //pSpriteBatch.DrawString(gameFont, "No Power!", new Vector2(625, 40), Color.Red, 0f, new Vector2(0, 0), 0.6f, SpriteEffects.None, 0f);
            }
        }
    }
}
