using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.World;
using Tankontroller.World.Bullets;
using Tankontroller.World.Particles;

namespace Tankontroller.Managers
{
    /// <summary>
    /// Singleton reponsible for collision detection logic and some collision responses
    /// </summary>
    internal class CollisionManager
    {
        public static readonly bool DRAW_COLLISION_SHAPES = DGS.Instance.GetBool("DRAW_COLLISION_SHAPES");

        private static CollisionManager mInstance = new();

        static CollisionManager() { }
        private CollisionManager() { }

        public static CollisionManager Instance
        {
            get { return mInstance; }
        }

        static public bool Collide(Tank pTank, Tank pTank_2) // Tank on Tank Collision
        {
            Vector2[] Tank1Corners = new Vector2[4];
            Vector2[] Tank2Corners = new Vector2[4];
            pTank.GetCorners(Tank1Corners);
            pTank_2.GetCorners(Tank2Corners);
            for (int i = 0; i < 4; i++)
            {
                if(pTank.PointIsInTank(Tank2Corners[i]) || pTank_2.PointIsInTank(Tank1Corners[i]))
                {
                    return true;
                }
            }
            return false;
        }

        static public bool Collide(Tank pTank, Rectangle pRectangle, bool inverse) //Tank and Rectangle Collision
        {
            //account for the inverse case
            Vector2[] tankCorners = new Vector2[4];
            pTank.GetCorners(tankCorners);

            // if the inverse case is true, then we want to check if the tank is outside the rectangle
            if (inverse) {
                foreach (Vector2 corner in tankCorners)
                    if (!pRectangle.Contains(corner))
                        return true;
            }
            else {
                foreach (Vector2 corner in tankCorners)
                {
                    if (pRectangle.Contains(corner))
                        return true;
                }
                if (pTank.PointIsInTank(new Vector2(pRectangle.Left, pRectangle.Top)) ||
                   pTank.PointIsInTank(new Vector2(pRectangle.Right, pRectangle.Top)) ||
                   pTank.PointIsInTank(new Vector2(pRectangle.Left, pRectangle.Bottom)) ||
                   pTank.PointIsInTank(new Vector2(pRectangle.Right, pRectangle.Bottom)))
                    return true;
            }
            return false;
        }

        static public bool Collide(Bullet pBullet, Tank pTank) //Bullet and Tank Collision
        {
            if (pTank.TankInRadius(pBullet.Radius, pBullet.Position))
            {
                return true;
            }
            return false;
        }
        
        static public bool Collide(Bullet pBullet, Rectangle pRectangle, bool inverse) //Bullet and Rectangle Collision
        {
            //if the inverse case is true, then we want to check if the bullet is outside the rectangle
            if (inverse) {
                if (!pRectangle.Contains(pBullet.Position))
                    return true;
            }
            else {
                if (pRectangle.Contains(pBullet.Position))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// This is the collision response for moving a tank away from a wall when a tank collides with a wall
        /// </summary>
        /// <returns> If the function fails return false, otherwise returns true</returns>
        static public bool ResolveTankWallCollision(Tank tank, RectWall wall)
        {
            Rectangle r = wall.Rectangle;

            // nothing to do if there's no collision
            if (!Collide(tank, r, false))
                return false;

            // tank center in world coords
            Vector2 center = tank.GetWorldPosition();

            // closest point on the rect to the tank center
            float cx = Math.Clamp(center.X, r.Left, r.Right);
            float cy = Math.Clamp(center.Y, r.Top, r.Bottom);
            Vector2 closest = new Vector2(cx, cy);

            // normal pointing from rect -> tank center (direction to push tank)
            Vector2 normal = center - closest;

            // If center is contained inside the rect, pick the shortest axis to escape along
            if (normal.LengthSquared() < 1e-6f)
            {
                float leftPen = center.X - r.Left;
                float rightPen = r.Right - center.X;
                float topPen = center.Y - r.Top;
                float bottomPen = r.Bottom - center.Y;

                float min = MathF.Min(MathF.Min(leftPen, rightPen), MathF.Min(topPen, bottomPen));
                if (min == leftPen) normal = new Vector2(-1, 0);
                else if (min == rightPen) normal = new Vector2(1, 0);
                else if (min == topPen) normal = new Vector2(0, -1);
                else normal = new Vector2(0, 1);
            }

            normal = Vector2.Normalize(normal);

            // Small iterative nudge until the polygon no longer collides.
            const float step = 1.0f; // pixels per iteration (tweak for smoothness/accuracy)
            const int maxSteps = 200; // safety to avoid infinite loop
            for (int i = 0; i < maxSteps; ++i)
            {
                tank.OffsetPosition(normal * step);
                if (!Collide(tank, r, false))
                    return true;
            }

            // If resolution failed, revert to previous position (safe fallback)
            tank.PutBack();
            return false;
        }

        /// <summary>
        /// This is the collision response for when the tank collides with the edges of the play area, moving it back into a playable area
        /// </summary>
        /// <returns> If the function fails returns false, otherwise returns true</returns>
        static public bool ResolveTankPlayAreaCollision(Tank tank, Rectangle playArea)
        {
            // nothing to do if tank is fully inside
            if (!Collide(tank, playArea, true))
                return false;

            // get tank corners
            Vector2[] corners = new Vector2[4];
            tank.GetCorners(corners);

            float minX = corners[0].X;
            float maxX = corners[0].X;
            float minY = corners[0].Y;
            float maxY = corners[0].Y;
            for (int i = 1; i < 4; ++i)
            {
                minX = MathF.Min(minX, corners[i].X);
                maxX = MathF.Max(maxX, corners[i].X);
                minY = MathF.Min(minY, corners[i].Y);
                maxY = MathF.Max(maxY, corners[i].Y);
            }

            // Use inclusive bounds: right/bottom are treated as (Right - 1) / (Bottom - 1)
            float leftBound = playArea.Left;
            float rightBound = playArea.Right - 1;
            float topBound = playArea.Top;
            float bottomBound = playArea.Bottom - 1;

            float pushX = 0f;
            if (minX < leftBound) pushX = leftBound - minX;         // push right
            else if (maxX > rightBound) pushX = rightBound - maxX; // push left

            float pushY = 0f;
            if (minY < topBound) pushY = topBound - minY;         // push down
            else if (maxY > bottomBound) pushY = bottomBound - maxY; // push up

            const float eps = 1e-6f;

            // Try X-only first
            if (MathF.Abs(pushX) > eps)
            {
                tank.OffsetPosition(new Vector2(pushX, 0f));
                if (!Collide(tank, playArea, true))
                    return true;
                tank.PutBack();
            }

            // Try Y-only
            if (MathF.Abs(pushY) > eps)
            {
                tank.OffsetPosition(new Vector2(0f, pushY));
                if (!Collide(tank, playArea, true))
                    return true;
                tank.PutBack();
            }

            // If both axes need pushing, try the smaller axis first then the other
            if (MathF.Abs(pushX) > eps && MathF.Abs(pushY) > eps)
            {
                Vector2 first = MathF.Abs(pushX) < MathF.Abs(pushY) ? new Vector2(pushX, 0f) : new Vector2(0f, pushY);
                Vector2 second = first.X == 0f ? new Vector2(pushX, 0f) : new Vector2(0f, pushY);

                tank.OffsetPosition(first);
                if (!Collide(tank, playArea, true))
                    return true;

                tank.OffsetPosition(second);
                if (!Collide(tank, playArea, true))
                    return true;

                tank.PutBack();
            }

            // Fallback: nudge tank iteratively toward the nearest valid center inside play area
            Vector2 center = tank.GetWorldPosition();
            Vector2 targetCenter = new Vector2(
                Math.Clamp(center.X, leftBound + 1f, rightBound - 1f),
                Math.Clamp(center.Y, topBound + 1f, bottomBound - 1f)
            );
            Vector2 dir = targetCenter - center;
            if (dir.LengthSquared() < eps)
            {
                tank.PutBack();
                return false;
            }
            dir = Vector2.Normalize(dir);

            const float step = 1.0f;
            const int maxSteps = 300;
            for (int i = 0; i < maxSteps; ++i)
            {
                tank.OffsetPosition(dir * step);
                if (!Collide(tank, playArea, true))
                    return true;
            }

            // failed -> revert
            tank.PutBack();
            return false;
        }
    }
}
