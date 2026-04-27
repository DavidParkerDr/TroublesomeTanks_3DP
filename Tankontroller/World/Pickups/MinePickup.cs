using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;
using Tankontroller.Managers;

namespace Tankontroller.World.Pickups
{
    public class MinePickup : Pickup
    {
        private static readonly Texture2D mMineTexture = Tankontroller.Instance().CM().Load<Texture2D>("MinePickup");

        public MinePickup(Vector2 pPosition) : base(mMineTexture, new Rectangle(400, 500, 45, 45), pPosition) { }

        public override void TriggerEffect(Tank pTank)
        {
            pTank.SetBulletType(BulletType.MINE);
        }
    }
}
