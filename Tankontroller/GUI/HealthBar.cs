using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.World;

namespace Tankontroller.GUI
{
    public class HealthBar
    {
        private static readonly Texture2D mHeartEmpty = Tankontroller.Instance().CM().Load<Texture2D>("healthbars/heart_bw");
        private static readonly Texture2D mHeartFull = Tankontroller.Instance().CM().Load<Texture2D>("healthbars/heart_colour");
        private Rectangle mBoundsRectangle;
        private Rectangle[] mRectangles;
        private int mMaxHealth;

        public HealthBar(int pMaxHealth, Rectangle pRectangle)
        {
            mBoundsRectangle = pRectangle;
            mMaxHealth = pMaxHealth;
            mRectangles = new Rectangle[mMaxHealth];
            PrepareRectangles();
        }

        private void PrepareRectangles()
        {
            int paddingWidth = 5;
            int width = (mBoundsRectangle.Width - paddingWidth * (mMaxHealth + 1)) / mMaxHealth;
            float heightRatio = (float)mHeartEmpty.Height / mHeartEmpty.Width;
            int height = (int)(width * heightRatio);
            if (height > mBoundsRectangle.Height)
            {
                height = mBoundsRectangle.Height;
                width = (int)(height / heightRatio);
            }
            int left = mBoundsRectangle.Left + paddingWidth;
            int top = mBoundsRectangle.Top;
            if (height < mBoundsRectangle.Height)
            {
                top += (mBoundsRectangle.Height - height) / 2;
            }
            for (int i = 0; i < mMaxHealth; i++)
            {
                Rectangle rectangle = new Rectangle(left, top, width, height);
                mRectangles[i] = rectangle;
                left += paddingWidth + width;
            }
        }

        public void Reposition(Rectangle pRectangle)
        {
            mBoundsRectangle = pRectangle;
            PrepareRectangles();
        }

        public void Draw(SpriteBatch pSpriteBatch, int health)
        {
            for (int i = 0; i < mMaxHealth; i++)
            {
                pSpriteBatch.Draw(mHeartEmpty, mRectangles[i], Color.White);
                if (i < health)
                {
                    pSpriteBatch.Draw(mHeartFull, mRectangles[i], Color.Red);
                }
            }
        }
    }
}
