using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.World.Particles;

namespace Tankontroller.World.Bullets
{
    public abstract class Bullet
    {
        public float Radius { get; protected set; }
        public Vector2 Position { get; protected set; }
        public Vector2 Velocity { get; protected set; }
        public Color Colour { get; private set; }
        public float LifeTime { get; protected set; }

        public Bullet(Vector2 pPosition, Vector2 pVelocity, Color pColour, float lifeTime)
        {
            Position = pPosition;
            Velocity = pVelocity;
            Colour = pColour;
            Radius = 5.0f;
            LifeTime = lifeTime;
        }

        public virtual void Update(float pSeconds)
        {
            Position = Position + Velocity * pSeconds;
        }

        public virtual bool CollideWithPlayArea(Rectangle pRectangle)
        {
            if (!pRectangle.Contains(Position))
            {
                return true;
            }
            return false;
        }

        public virtual bool Collide(RectWall pWall)
        {
            Rectangle rectangle = pWall.Rectangle;
            if (rectangle.Contains(Position))
            {
                return true;
            }
            return false;
        }

        public virtual bool Collide(Tank pTank)
        {
            if (pTank.PointIsInTank(Position))
            {
                return true;
            }
            return false;
        }

        public virtual bool Collide(Bullet pBullet) // This is unused but I'm keeping it for potential implementation
        {
            if (Vector2.Distance(Position, pBullet.Position) < 2 * Radius)
            {
                return true;
            }
            return false;
        }

        protected Vector2 GetCollisionNormal(Rectangle pRect)
        {
            float difference = Math.Abs(Position.X - pRect.Left);
            Vector2 collisionNormal = new Vector2(-1, 0);
            if (difference > Math.Abs(Position.X - pRect.Right))
            {
                difference = Math.Abs(Position.X - pRect.Right);
                collisionNormal = new Vector2(1, 0);
            }
            if (difference > Math.Abs(Position.Y - pRect.Top))
            {
                difference = Math.Abs(Position.Y - pRect.Top);
                collisionNormal = new Vector2(0, -1);
            }
            if (difference > Math.Abs(Position.Y - pRect.Bottom))
            {
                collisionNormal = new Vector2(0, 1);
            }
            return collisionNormal;
        }

        public abstract bool DoCollision(Tank pTank);
        public abstract bool DoCollision(Rectangle pRectangle);
        public abstract bool DoCollision(RectWall pWall);
        public abstract bool DoCollision(Bullet pBullet);

        public abstract bool LifeTimeExpired();

        public virtual void Draw(SpriteBatch pBatch, Texture2D pTexture)
        {
            Particle.DrawCircle(pBatch, pTexture, (int)Radius + 2 * Particle.EDGE_THICKNESS, Position, Color.Black);
            Particle.DrawCircle(pBatch, pTexture, (int)Radius, Position, Colour);
        }
    }
}
