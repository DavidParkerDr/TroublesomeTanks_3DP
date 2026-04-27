using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.Managers;

namespace Tankontroller.World.Pickups
{
    public class EMPPickup : Pickup
    {
        private static readonly Texture2D mEMPTexture = Tankontroller.Instance().CM().Load<Texture2D>("EMP");

        public EMPPickup(Vector2 pPositon) : base(Tankontroller.Instance().CM().Load<Texture2D>("circle"), new Rectangle(400, 500, 40, 40), new Vector2(0,0))
        {
            m_Position = pPositon;
            m_Pickup_Rect = new Rectangle((int)m_Position.X - (m_Pickup_Rect.Width / 2), (int)m_Position.Y - (m_Pickup_Rect.Height / 2), (int)(4f * mScalerX), (int)(4f * mScalerX));
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            pSpriteBatch.Draw(mEMPTexture, m_Pickup_Rect, Color.White);
        }

        public override bool PickUpCollision(Tank tank)
        {
            if (CollisionManager.Collide(tank, m_Pickup_Rect, false))
            {
                tank.SetBulletType(BulletType.BOUNCY_EMP);
                return true;
            }
            return false;
        }
    }
}
