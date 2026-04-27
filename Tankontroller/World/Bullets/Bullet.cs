using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.Managers;
using Tankontroller.Utilities;
using Tankontroller.World.Particles;
using Tankontroller.World.Shapes;
using Tankontroller.World.WorldObject;

namespace Tankontroller.World.Bullets
{
    public abstract class Bullet : ICollidable
    {
        public Transform Transform { get; private set; } = new Transform();
        public CollisionShape CollisionShape => CircleShape;
        public CircleShape CircleShape { get; private set; }
        public float Radius => CircleShape.Radius;
        public Vector2 Position => Transform.Position;
        public Vector2 Velocity { get; protected set; }
        public Color Colour { get; private set; }
        public float LifeTime { get; protected set; }

        public Bullet(Vector2 pPosition, Vector2 pVelocity, Color pColour, float lifeTime)
        {
            Transform.Position = pPosition;
            CircleShape = new CircleShape(Transform, 5.0f * Tankontroller.Instance().ScaleFactor());

            Velocity = pVelocity;
            Colour = pColour;
            LifeTime = lifeTime;
        }

        public virtual void Update(float pSeconds)
        {
            // Move at the correct speed according to the frame time and resolution scale factor
            Transform.Position = Position + Velocity * pSeconds * Tankontroller.Instance().ScaleFactor();
        }

        /// <summary>
        /// Handles the response to a collision with a tank.
        /// </summary>
        /// <returns> True if the bullet should be removed, false otherwise.</returns>
        public abstract bool TankCollisionResponse(Tank pTank);
        /// <summary>
        /// Handles the response to a collision with another bullet.
        /// </summary>
        /// <returns> True if the bullet should be removed, false otherwise.</returns>
        public abstract bool BulletCollisionResponse(Bullet pBullet);
        /// <summary>
        /// Handles the response to a collision with a wall.
        /// </summary>
        /// <returns> True if the bullet should be removed, false otherwise.</returns>
        public abstract bool WallCollisionResponse(CollisionEvent collisionEvent);

        public abstract bool LifeTimeExpired();

        public virtual void Draw(SpriteBatch pBatch, Texture2D pTexture)
        {
            Particle.DrawCircle(pBatch, pTexture, (int)Radius + 2 * Particle.EDGE_THICKNESS, Position, Color.Black);
            Particle.DrawCircle(pBatch, pTexture, (int)Radius, Position, Colour);

            // Draw collision shape if enabled in DGS
            if (CollisionManager.DRAW_COLLISION_SHAPES)
            {
                DrawUtilities.DrawCircle(pBatch, Position, Radius, Color.DodgerBlue);
            }
        }
    }
}
