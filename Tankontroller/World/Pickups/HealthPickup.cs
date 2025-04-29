using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.Managers;

namespace Tankontroller.World.Pickups
{
    public class HealthPickup : Pickup
    {
        private static readonly Texture2D mHeartBack = Tankontroller.Instance().CM().Load<Texture2D>("healthpickup");

        public HealthPickup(Vector2 pPosition) : base(Tankontroller.Instance().CM().Load<Texture2D>("circle"), new Rectangle(400, 500, 40, 40), new Vector2(0,0))
        {
            m_Position = pPosition;
            m_Pickup_Rect = new Rectangle((int)m_Position.X - (m_Pickup_Rect.Width / 2), (int)m_Position.Y - (m_Pickup_Rect.Height / 2), (int)(3f * mScalerX), (int)(4.5f * mScalerX));
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
                pSpriteBatch.Draw(mHeartBack, m_Pickup_Rect, Color.White);
        }

        public override bool PickUpCollision(Tank tank)
        {
            if (CollisionManager.Collide(tank, m_Pickup_Rect, false))
            {
                tank.Heal();
                return true;
            }
            return false;
        }
    }
}
