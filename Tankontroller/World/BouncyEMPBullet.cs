﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Tankontroller.World.Particles;

namespace Tankontroller.World
{
    class BouncyEMPBullet : Bullet
    {
        private static readonly List<Texture2D> EMPTextures = new List<Texture2D>();
        private static readonly Texture2D EMPTexture1 = Tankontroller.Instance().CM().Load<Texture2D>("ShockWave1");
        private static readonly Texture2D EMPTexture2 = Tankontroller.Instance().CM().Load<Texture2D>("ShockWave2");
        private static readonly Texture2D EMPTexture3 = Tankontroller.Instance().CM().Load<Texture2D>("ShockWave3");
        private static readonly Texture2D EMPTexture4 = Tankontroller.Instance().CM().Load<Texture2D>("ShockWave4");
        public BouncyEMPBullet(Vector2 pPosition, Vector2 pVelocity, Color pColour, float pLifeTime) : base(pPosition, pVelocity, pColour, pLifeTime) {
            EMPTextures.Add(EMPTexture1);
            EMPTextures.Add(EMPTexture2);
            EMPTextures.Add(EMPTexture3);
            EMPTextures.Add(EMPTexture4);
        }
        private float Rotation = 0.0f;

        public override void Update(float pSeconds)
        {
            Random rand = new Random();
            EMPBlastInitPolicy explosion = new EMPBlastInitPolicy(Position, 0.5f);
            Particles.ParticleManager.Instance().InitialiseParticles(explosion, 1);
            //Rotation += 0.01f;
            LifeTime -= pSeconds;
            base.Update(pSeconds);
        }

        public override bool DoCollision(Rectangle pRectangle)
        {
            Vector2 collisonNormal = GetCollisionNormal(pRectangle);
            Velocity = Vector2.Reflect(Velocity, collisonNormal);
            return false;
        }

        public override bool DoCollision(RectWall pWall)
        {
            Vector2 collisonNormal = GetCollisionNormal(pWall.Rectangle);
            Velocity = Vector2.Reflect(Velocity, collisonNormal);
            return false;
        }

        public override bool DoCollision(Tank pTank)
        {
            CreateBlast();
            return true;
        }

        public override bool DoCollision(Bullet pBullet)
        {
            Vector2 collisionNormal = Vector2.Normalize(Velocity);
            return false;
        }

        private void CreateBlast()
        {
            EMPBlastInitPolicy explosion = new EMPBlastInitPolicy(Position, 6.5f);
            Particles.ParticleManager.Instance().InitialiseParticles(explosion, 200);
        }

        public override bool LifeTimeExpired()
        {
            return (LifeTime <= 0.0f);
        }

        public override void Draw(SpriteBatch pBatch, Texture2D pTexture)
        {
            Particle.DrawCircle(pBatch, pTexture, ((int)BULLET_RADIUS * 3) + 2 * Particle.EDGE_THICKNESS, Position, Color.Black);
            Particle.DrawCircle(pBatch, pTexture, ((int)BULLET_RADIUS * 3), Position, Colour);
            int i = new Random().Next(0, 4);
            pBatch.Draw(EMPTextures[i], Position, null, Color.White, Rotation, new Vector2(EMPTextures[i].Width / 2, EMPTextures[i].Height / 2), BULLET_RADIUS * 0.02f, SpriteEffects.None, 0.0f);
        }
    }
}
