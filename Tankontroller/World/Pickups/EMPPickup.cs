using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;
using Tankontroller.Managers;

namespace Tankontroller.World.Pickups
{
    public class EMPPickup : Pickup
    {
        private static readonly Texture2D mEMPTexture = Tankontroller.Instance().CM().Load<Texture2D>("EMP");

        public EMPPickup(Vector2 pPositon) : base(mEMPTexture, new Rectangle(400, 500, 45, 45), pPositon) { }

        public override void TriggerEffect(Tank pTank)
        {
            pTank.SetBulletType(BulletType.BOUNCY_EMP);
        }
    }
}
