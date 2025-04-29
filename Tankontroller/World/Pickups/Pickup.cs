using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tankontroller.World.Pickups
{
    public abstract class Pickup
    {
        public Rectangle m_Pickup_Rect { get; protected set; }
        public Texture2D m_Texture { get; protected set; }
        public Vector2 m_Position { get; protected set; }

        public int screenWidth = Tankontroller.Instance().GDM().GraphicsDevice.Viewport.Width;
        public int screenHeight = Tankontroller.Instance().GDM().GraphicsDevice.Viewport.Height;
        public float mScalerX;
        public float mScalerY;

        protected Pickup(Texture2D pTexture, Rectangle pRectangle, Vector2 pPosition) {
            mScalerX = ((float)screenWidth / 200f);
            mScalerY = ((float)screenHeight / 200f);
            pRectangle = new Rectangle((int)((pRectangle.X / 10) * mScalerX), (int)((pRectangle.Y / 10) * mScalerY), (int)(pRectangle.Width/10  * mScalerX), (int)(pRectangle.Height/ 10 * mScalerY));
            m_Pickup_Rect = pRectangle;
            m_Texture = pTexture;
            m_Position = pPosition;
        }

        public virtual void Draw(SpriteBatch pSpriteBatch)
        {
            pSpriteBatch.Draw(m_Texture, m_Pickup_Rect, Color.DeepPink);
        }

        public virtual bool PickUpCollision(Tank pTank) { return false; }

        public bool Collide(Tank pTank)
        {
            Vector2[] tankCorners = new Vector2[4];
            pTank.GetCorners(tankCorners);

            foreach (Vector2 corner in tankCorners)
            {
                if (m_Pickup_Rect.Contains(corner))
                {
                    return true;
                }
            }
            if (pTank.PointIsInTank(new Vector2(m_Pickup_Rect.Left, m_Pickup_Rect.Top)) ||
            pTank.PointIsInTank(new Vector2(m_Pickup_Rect.Right, m_Pickup_Rect.Top)) ||
            pTank.PointIsInTank(new Vector2(m_Pickup_Rect.Left, m_Pickup_Rect.Bottom)) ||
               pTank.PointIsInTank(new Vector2(m_Pickup_Rect.Right, m_Pickup_Rect.Bottom)))
            {
                return true;
            }
            return false;
        }
    }
}
