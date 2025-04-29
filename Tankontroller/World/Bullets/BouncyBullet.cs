using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Tankontroller.World.Particles;

namespace Tankontroller.World.Bullets
{
    public class BouncyBullet : Bullet
    {
        private static readonly Texture2D m_BouncyBulletTopTexture = Tankontroller.Instance().CM().Load<Texture2D>("BouncyBulletTop");
        private static readonly Texture2D m_BouncyBulletBackTexture = Tankontroller.Instance().CM().Load<Texture2D>("BouncyBulletBack");
        float numOfBounces;
        public BouncyBullet(Vector2 pPosition, Vector2 pVelocity, Color pColour, float pNumOfBounces) : base(pPosition, pVelocity, pColour, pNumOfBounces) {
            numOfBounces = pNumOfBounces;
        }
        public override void Update(float pSeconds)
        {
            base.Update(pSeconds);
        }

        public override bool DoCollision(Rectangle pRectangle)
        {
            Vector2 collisonNormal = GetCollisionNormal(pRectangle);
            if (numOfBounces <= 0)
            {
                CreateExplosion(-collisonNormal);
                return true;
            }
            Velocity = Vector2.Reflect(Velocity, collisonNormal);
            numOfBounces--;
            return false;
        }

        public override bool DoCollision(RectWall pWall)
        {
            Vector2 collisonNormal = GetCollisionNormal(pWall.Rectangle);
            if (numOfBounces <= 0)
            {
                CreateExplosion(collisonNormal);
                return true;
            }
            Velocity = Vector2.Reflect(Velocity, collisonNormal);
            numOfBounces--;
            return false;
        }

        public override bool DoCollision(Tank pTank)
        {
            CreateExplosion(Vector2.Normalize(Position - pTank.GetWorldPosition()));
            return true;
        }

        public override bool DoCollision(Bullet pBullet)
        {
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
            Particle.DrawCircle(pBatch, m_BouncyBulletBackTexture, (int)Radius * 3, Position, Colour);
            Particle.DrawCircle(pBatch, m_BouncyBulletTopTexture, (int)Radius * 3, Position, Color.White);
        }
    }
}
