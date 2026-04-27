using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.Managers;
using Tankontroller.Utilities;
using Tankontroller.World.Particles;
using Tankontroller.World.Shapes;

namespace Tankontroller.World.Bullets
{
    class Shockwave : Bullet
    {
        private static readonly Texture2D ShockWaveTexture = Tankontroller.Instance().CM().Load<Texture2D>("Shockwave");
        public Shockwave(Vector2 pPosition, Vector2 pVelocity, Color pColour, float pLifeTime) : base(pPosition, pVelocity, pColour, pLifeTime) { }
        public override void Update(float pSeconds)
        {
            CircleShape.Radius += 0.5f;
            LifeTime -= pSeconds;
            base.Update(pSeconds);
        }

        public override bool TankCollisionResponse(Tank pTank) => false;
        public override bool BulletCollisionResponse(Bullet pBullet) => false;
        public override bool WallCollisionResponse(CollisionEvent collisionEvent) => false;

        public override bool LifeTimeExpired()
        {
            return (LifeTime <= 0);
        }
        public override void Draw(SpriteBatch pBatch, Texture2D pTexture)
        {
            Color ShockWaveColour = Color.Yellow;
            ShockWaveColour.A = (byte)(0.0f);
            Particle.DrawCircle(pBatch, ShockWaveTexture, (int)Radius, Position, ShockWaveColour);

            // Draw collision shape if enabled in DGS
            if (CollisionManager.DRAW_COLLISION_SHAPES)
            {
                DrawUtilities.DrawRing(pBatch, Position, Radius, Color.DodgerBlue);
            }
        }
    }
}
