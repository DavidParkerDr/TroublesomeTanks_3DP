using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Tankontroller.World;
using Tankontroller.World.Bullets;
using Tankontroller.World.Particles;
using Tankontroller.World.Shapes;

namespace Tankontroller.Managers
{
    /// <summary>
    /// Static class that manages logic for all collision detection and response
    /// </summary>
    internal static class CollisionManager
    {
        public static readonly bool DRAW_COLLISION_SHAPES = DGS.Instance.GetBool("DRAW_COLLISION_SHAPES");

        /// <summary>
        /// This is the collision response for moving a tank away from a wall when a tank collides with a wall
        /// </summary>
        /// <returns> If the function fails return false, otherwise returns true</returns>
        static public bool ResolveTankWallCollision(Tank tank, CollisionShape wall)
        {
            CollisionEvent collisionEvent = tank.CollisionShape.Intersects(wall);

            // Nothing to do if there's no collision
            if (!collisionEvent.HasCollided)
                return false;

            // Get the collision normal from the collision event (pointing away from the wall).
            Vector2 normal = collisionEvent.CollisionNormal ?? Vector2.UnitX;

            // Normalize to ensure unit vector (defensive programming, should already be normalized)
            if (normal.LengthSquared() > 0f)
            {
                normal = Vector2.Normalize(normal);
            }

            // Small iterative nudge until the polygon no longer collides.
            const float step = 1.0f; // pixels per iteration (tweak for smoothness/accuracy)
            const int maxSteps = 200; // safety to avoid infinite loop
            for (int i = 0; i < maxSteps; ++i)
            {
                tank.OffsetPosition(normal * step);

                // Re-check collision using the new system
                if (!tank.CollisionShape.Intersects(wall).HasCollided)
                    return true;
            }

            // If resolution failed, revert to previous position (safe fallback)
            tank.PutBack();
            return false;
        }
    }
}
