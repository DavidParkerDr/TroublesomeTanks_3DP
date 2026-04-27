using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Tankontroller.Managers;
using Tankontroller.Utilities;
using Tankontroller.World.Particles;
using Tankontroller.World.Shapes;

namespace Tankontroller.World.Bullets
{
    class MineBullet : Bullet
    {
        private readonly Texture2D MineTexture = Tankontroller.Instance().CM().Load<Texture2D>("Mine");
        public MineBullet(Vector2 pPosition, Vector2 pVelocity, Color pColour, float pLifeTime) : base(pPosition, pVelocity, pColour, pLifeTime)
        {
            CircleShape.Radius *= 3.5f;
        }
        public override void Update(float pSeconds)
        {
            LifeTime -= pSeconds;
            base.Update(pSeconds);
        }

        public override bool TankCollisionResponse(Tank pTank)
        {
            pTank.TakeDamage();
            MineBlastInitPolicy explosion = new MineBlastInitPolicy(Position, 1.0f);
            ParticleManager.Instance().InitialiseParticles(explosion, 200);
            return true;
        }

        public override bool BulletCollisionResponse(Bullet pBullet) => false;

        public override bool WallCollisionResponse(CollisionEvent collisionEvent) => false;

        public override bool LifeTimeExpired()
        {
            return (LifeTime <= 0);
        }

        public override void Draw(SpriteBatch pBatch, Texture2D pTexture)
        {
            Particle.DrawCircle(pBatch, pTexture, (int)Radius, Position, Colour);
            Particle.DrawCircle(pBatch, MineTexture, (int)(Radius * 1.25f), Position, Color.White);

            // Draw collision shape if enabled
            if (CollisionManager.DRAW_COLLISION_SHAPES)
            {
                DrawUtilities.DrawCircle(pBatch, Position, Radius, Color.DodgerBlue);
            }
        }
    }
}
