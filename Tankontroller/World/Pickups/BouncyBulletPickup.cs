using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;
using Tankontroller.Managers;

namespace Tankontroller.World.Pickups
{
    public class BouncyBulletPickup : Pickup
    {
        private static readonly Texture2D mBouncyBulletTexture = Tankontroller.Instance().CM().Load<Texture2D>("BouncyBulletPickup");

        public BouncyBulletPickup(Vector2 pPosition) : base(mBouncyBulletTexture, new Rectangle(400, 500, 45, 45), pPosition) { }

        public override void TriggerEffect(Tank pTank)
        {
            pTank.SetBulletType(BulletType.BOUNCY_BULLET);
        }
    }
}
