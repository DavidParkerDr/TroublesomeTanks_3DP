using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Tankontroller.World.Particles;

namespace Tankontroller.World.Bullets
{
    class MineBullet : Bullet
    {
        private readonly Texture2D MineTexture = Tankontroller.Instance().CM().Load<Texture2D>("Mine");
        public MineBullet(Vector2 pPosition, Vector2 pVelocity, Color pColour, float pLifeTime) : base(pPosition, pVelocity, pColour, pLifeTime) { }
        public override void Update(float pSeconds)
        {
            LifeTime -= pSeconds;
            base.Update(pSeconds);
        }

        public override bool DoCollision(Rectangle pRectangle)
        {
            return false;
        }

        public override bool DoCollision(RectWall pWall)
        {
            return false;
        }

        public override bool DoCollision(Tank pTank)
        {
            MineBlastInitPolicy explosion = new MineBlastInitPolicy(Position, 1.0f);
            ParticleManager.Instance().InitialiseParticles(explosion, 200);
            return true;
        }

        public override bool DoCollision(Bullet pBullet)
        {
            return false;
        }

        public override bool LifeTimeExpired()
        {
            return (LifeTime <= 0);
        }

        public override void Draw(SpriteBatch pBatch, Texture2D pTexture)
        {
            Particle.DrawCircle(pBatch, pTexture, (int)(Radius * 3.5f), Position, Colour);
            Particle.DrawCircle(pBatch, MineTexture, (int)(Radius * 4), Position, Color.White);
        }
    }
}
