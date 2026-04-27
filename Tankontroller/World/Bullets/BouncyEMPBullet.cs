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
    class BouncyEMPBullet : Bullet
    {
        private static readonly List<Texture2D> EMPTextures = new List<Texture2D>();
        private static readonly Texture2D EMPTexture1 = Tankontroller.Instance().CM().Load<Texture2D>("ShockWave1");
        private static readonly Texture2D EMPTexture2 = Tankontroller.Instance().CM().Load<Texture2D>("ShockWave2");
        private static readonly Texture2D EMPTexture3 = Tankontroller.Instance().CM().Load<Texture2D>("ShockWave3");
        private static readonly Texture2D EMPTexture4 = Tankontroller.Instance().CM().Load<Texture2D>("ShockWave4");
        private int index = 0;
        public BouncyEMPBullet(Vector2 pPosition, Vector2 pVelocity, Color pColour, float pLifeTime) : base(pPosition, pVelocity, pColour, pLifeTime)
        {
            EMPTextures.Add(EMPTexture1);
            EMPTextures.Add(EMPTexture2);
            EMPTextures.Add(EMPTexture3);
            EMPTextures.Add(EMPTexture4);
            CircleShape.Radius *= 3.0f;
        }
        private float Rotation = 0.0f;

        public override void Update(float pSeconds)
        {
            Random rand = new Random();
            EMPBlastInitPolicy explosion = new EMPBlastInitPolicy(Position, 0.5f);
            ParticleManager.Instance().InitialiseParticles(explosion, 1);
            LifeTime -= pSeconds;
            base.Update(pSeconds);
        }

        public override bool TankCollisionResponse(Tank pTank)
        {
            EMPBlastInitPolicy explosion = new EMPBlastInitPolicy(Position, 6.5f);
            ParticleManager.Instance().InitialiseParticles(explosion, 200);
            return true;
        }

        public override bool BulletCollisionResponse(Bullet pBullet)
        {
            Vector2 collisionNormal = Vector2.Normalize(Velocity);
            return false;
        }

        public override bool WallCollisionResponse(CollisionEvent collisionEvent)
        {
            Vector2 collisionNormal = collisionEvent.CollisionNormal ?? Vector2.One;
            // Only reflect if the bullet is moving towards the wall
            if (Vector2.Dot(Velocity, collisionNormal) < 0)
            {
                Velocity = Vector2.Reflect(Velocity, collisionNormal);
            }
            return false;
        }

        public override bool LifeTimeExpired()
        {
            return LifeTime <= 0.0f;
        }

        public override void Draw(SpriteBatch pBatch, Texture2D pTexture)
        {
            Particle.DrawCircle(pBatch, pTexture, (int)Radius + 2 * Particle.EDGE_THICKNESS, Position, Color.Black);
            Particle.DrawCircle(pBatch, pTexture, (int)Radius, Position, Colour);
            int rand = new Random().Next(0, 20);
            if (rand == 0)
            {
               index = new Random().Next(0, 4);
            }
            pBatch.Draw(EMPTextures[index], Position, null, Color.White, Rotation, new Vector2(EMPTextures[index].Width / 2, EMPTextures[index].Height / 2), Radius * 0.02f, SpriteEffects.None, 0.0f);

            // Draw collision shape if enabled in DGS
            if (CollisionManager.DRAW_COLLISION_SHAPES)
            {
                DrawUtilities.DrawCircle(pBatch, Position, Radius, Color.DodgerBlue);
            }
        }
    }
}
