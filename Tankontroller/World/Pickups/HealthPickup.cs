using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.Managers;

namespace Tankontroller.World.Pickups
{
    public class HealthPickup : Pickup
    {
        private static readonly Texture2D mHeartTexture = Tankontroller.Instance().CM().Load<Texture2D>("healthpickup");

        public HealthPickup(Vector2 pPosition) : base(mHeartTexture, new Rectangle(400, 500, 35, 43), pPosition) { }

        public override void TriggerEffect(Tank pTank)
        {
            pTank.Heal();
        }
    }
}
