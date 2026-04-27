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
    public class BouncyBullet : Bullet
    {
        private static readonly Texture2D m_BouncyBulletTopTexture = Tankontroller.Instance().CM().Load<Texture2D>("BouncyBulletTop");
        private static readonly Texture2D m_BouncyBulletBackTexture = Tankontroller.Instance().CM().Load<Texture2D>("BouncyBulletBack");
        float numOfBounces;
        public BouncyBullet(Vector2 pPosition, Vector2 pVelocity, Color pColour, float pNumOfBounces) : base(pPosition, pVelocity, pColour, pNumOfBounces) {
            numOfBounces = pNumOfBounces;
            CircleShape.Radius *= 3.0f;
        }
        public override void Update(float pSeconds)
        {
            base.Update(pSeconds);
        }

        public override bool TankCollisionResponse(Tank pTank)
        {
            pTank.TakeDamage();
            CreateExplosion(Vector2.Normalize(Position - pTank.Transform.Position));
            return true;
        }

        public override bool BulletCollisionResponse(Bullet pBullet) => false;

        public override bool WallCollisionResponse(CollisionEvent collisionEvent)
        {
            Vector2 collisionNormal = collisionEvent.CollisionNormal ?? Vector2.One;
            if (numOfBounces <= 0)
            {
                CreateExplosion(collisionNormal);
                return true;
            }
            // Only reflect if the bullet is moving towards the wall
            if (Vector2.Dot(Velocity, collisionNormal) < 0)
            {
                Velocity = Vector2.Reflect(Velocity, collisionNormal);
                numOfBounces--;
            }
            return false;
        }

        private void CreateExplosion(Vector2 pCollisionNormal)
        {
            ExplosionInitialisationPolicy explosion = new ExplosionInitialisationPolicy(Position, pCollisionNormal, Colour);
            ParticleManager.Instance().InitialiseParticles(explosion, 100);
        }

        public override bool LifeTimeExpired()
        {
            return (LifeTime <= 0);
        }

        public override void Draw(SpriteBatch pBatch, Texture2D pTexture)
        {
            Particle.DrawCircle(pBatch, m_BouncyBulletBackTexture, (int)Radius, Position, Colour);
            Particle.DrawCircle(pBatch, m_BouncyBulletTopTexture, (int)Radius, Position, Color.White);

            // Draw collision shape if enabled in DGS
            if (CollisionManager.DRAW_COLLISION_SHAPES)
            {
                DrawUtilities.DrawCircle(pBatch, Position, Radius, Color.DodgerBlue);
            }
        }
    }
}
