using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tankontroller.World.Particles;
using Tankontroller.World.Shapes;

namespace Tankontroller.World.Bullets
{
    class DefaultBullet : Bullet
    {
        public DefaultBullet(Vector2 pPosition, Vector2 pVelocity, Color pColour, float pLifeTime) : base(pPosition, pVelocity, pColour, pLifeTime) { }

        public override void Update(float pSeconds)
        {
            BulletInitialisationPolicy bulletParticles = new BulletInitialisationPolicy(Position, Colour);
            ParticleManager.Instance().InitialiseParticles(bulletParticles, 2);
            base.Update(pSeconds);
        }

        public override bool TankCollisionResponse(Tank pTank)
        {
            pTank.TakeDamage();
            CreateExplosion(Vector2.Normalize(Position - pTank.Transform.Position));
            return true;
        }

        public override bool BulletCollisionResponse(Bullet pBullet)
        {
            Vector2 collisionNormal = Vector2.Normalize(Velocity);
            CreateExplosion(collisionNormal);
            CreateExplosion(-collisionNormal);
            return true;
        }

        public override bool WallCollisionResponse(CollisionEvent collisionEvent)
        {
            CreateExplosion(collisionEvent.CollisionNormal ?? Vector2.One);
            return true;
        }

        private void CreateExplosion(Vector2 pCollisionNormal)
        {
            ExplosionInitialisationPolicy explosion = new ExplosionInitialisationPolicy(Position, pCollisionNormal, Colour);
            ParticleManager.Instance().InitialiseParticles(explosion, 100);
        }

        public override bool LifeTimeExpired()
        {
            return false;
        }
    }

}
