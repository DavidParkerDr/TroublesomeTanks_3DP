using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Tankontroller.World.Shapes
{
    /// <summary>
    /// Represents an axis-aligned rectangle collision shape. <br></br>
    /// NOTE: The reason the MonoGame Rectangle struct is not used directly is because it uses integer values, which can lead to precision issues in collision detection.
    /// </summary>
    public class RectangleAxisAlignedShape : CollisionShape
    {
        public Vector2 Size { get; set; } = Vector2.One;
        public Vector2 HalfExtents => Size * 0.5f;
        public Vector2 Min => WorldPosition - HalfExtents;
        public Vector2 Max => WorldPosition + HalfExtents;

        public RectangleAxisAlignedShape(Transform pOwner, Vector2 pSize, bool pEnabled = true) : base(pOwner, pEnabled)
        {
            Size = pSize;
        }

        public RectangleAxisAlignedShape(Transform pOwner, Vector2 pSize, Vector2 pLocalOffset, bool pEnabled = true) : base(pOwner, pLocalOffset, pEnabled)
        {
            Size = pSize;
        }

        /// <summary>
        /// Converts the oriented rectangle to a MonoGame rectangle that fully contains it. Useful for drawing.
        /// NOTE: Could be optimized by caching the rectangle and only recalculating when position, size, or rotation changes.
        /// </summary>
        public Rectangle ToRectangle() =>
            new((int)MathF.Round(WorldPosition.X - HalfExtents.X), (int)MathF.Round(WorldPosition.Y - HalfExtents.Y), (int)MathF.Round(Size.X), (int)MathF.Round(Size.Y));

        /// <summary>
        /// Draws the oriented rectangle using a texture.
        /// The texture will be scaled to fit the rectangle's size.
        /// </summary>
        public void Draw(SpriteBatch pSpriteBatch, Texture2D pTexture, Color color)
        {
            pSpriteBatch.Draw(pTexture, ToRectangle(), color);
        }

        public override CollisionEvent Intersects(CollisionShape pOther)
        {
            return pOther switch
            {
                PointShape point => IntersectsPoint(point),
                CircleShape circle => IntersectsCircle(circle),
                RectangleAxisAlignedShape rectangleAligned => IntersectsAlignedRectangle(rectangleAligned),
                RectangleOrientedShape rectangleOriented => IntersectsOrientedRectangle(rectangleOriented),
                _ => throw new NotImplementedException($"Intersection with shape {this} and {pOther} is not implemented."),
            };
        }

        public override CollisionEvent Intersects(Vector2 point)
        {
            // Build AABB min/max from rectangle center and half-extents
            Vector2 rectangleMin = Min;
            Vector2 rectangleMax = Max;

            // Check if point is inside AABB and report collision event
            if (point.X >= rectangleMin.X &&
                point.X <= rectangleMax.X &&
                point.Y >= rectangleMin.Y &&
                point.Y <= rectangleMax.Y)
            {
                Vector2 normal = NormalizeZeroSafe(WorldPosition - point);
                return new CollisionEvent(true, point, normal);
            }

            return new CollisionEvent(false);
        }

        /// <summary>
        /// Checks for intersection with a point shape - if the point is inside the rectangle.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (the point itself).
        /// 2. The normal of the collision (pointing into the rectangle center). </returns>
        public CollisionEvent IntersectsPoint(PointShape pPoint)
        {
            return Intersects(pPoint.WorldPosition);
        }

        /// <summary>
        /// Check for intersection with an axis-aligned rectangle shape - if the circle overlaps with the rectangle.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (midpoint of overlap between the circle and rectangle)
        /// 2. The normal of the collision (pointing away from the rectangle) </returns>
        public CollisionEvent IntersectsCircle(CircleShape pCircle)
        {
            CollisionEvent collisionEvent = pCircle.IntersectsAlignedRectangle(this);
            if (collisionEvent.CollisionNormal.HasValue)
                collisionEvent.CollisionNormal *= -1;
            return collisionEvent;
        }

        /// <summary>
        /// Check for intersection with another axis-aligned rectangle shape - if the rectangles overlap.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (midpoint of overlap between the rectangles)
        /// 2. The normal of the collision (pointing away from other rectangle) </returns>
        public CollisionEvent IntersectsAlignedRectangle(RectangleAxisAlignedShape pRectangleAligned)
        {
            Vector2 aMin = Min;
            Vector2 aMax = Max;
            Vector2 bMin = pRectangleAligned.Min;
            Vector2 bMax = pRectangleAligned.Max;

            float overlapX = MathF.Min(aMax.X, bMax.X) - MathF.Max(aMin.X, bMin.X);
            float overlapY = MathF.Min(aMax.Y, bMax.Y) - MathF.Max(aMin.Y, bMin.Y);

            // No overlap (use <= 0 to treat touching as non-colliding)
            if (overlapX <= 0f || overlapY <= 0f)
            {
                return new CollisionEvent(false);
            }

            // Intersection rectangle and midpoint
            Vector2 intersectionMin = new(MathF.Max(aMin.X, bMin.X), MathF.Max(aMin.Y, bMin.Y));
            Vector2 intersectionMax = new(MathF.Min(aMax.X, bMax.X), MathF.Min(aMax.Y, bMax.Y));
            Vector2 collisionPosition = (intersectionMin + intersectionMax) * 0.5f;

            // Choose axis of minimum penetration for the normal
            Vector2 direction = WorldPosition - pRectangleAligned.WorldPosition;
            Vector2 collisionNormal;
            if (overlapX < overlapY)
            {
                float sign = MathF.Sign(direction.X);
                collisionNormal = new Vector2(sign, 0f);
            }
            else
            {
                float sign = MathF.Sign(direction.Y);
                collisionNormal = new Vector2(0f, sign);
            }
            collisionNormal = NormalizeZeroSafe(collisionNormal);

            return new CollisionEvent(true, collisionPosition, collisionNormal);
        }

        /// <summary>
        /// Check for intersection with an oriented rectangle shape - if the rectangles overlap.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (midpoint of overlap between the rectangles)
        /// 2. The normal of the collision (pointing away from the oriented rectangle) </returns>
        public CollisionEvent IntersectsOrientedRectangle(RectangleOrientedShape pRectangleOriented)
        {
            // Check for trivial case of rectangles having the same center position
            if (WorldPosition == pRectangleOriented.WorldPosition)
            {
                return new CollisionEvent(true, WorldPosition);
            }

            // Build OBB local axes in world space
            float cos = (float)Math.Cos(pRectangleOriented.WorldRotation);
            float sin = (float)Math.Sin(pRectangleOriented.WorldRotation);
            Vector2 orientedRectangleXAxis = new(cos, sin);
            Vector2 orientedRectangleYAxis = new(-sin, cos);

            // AABB local axes in world space
            Vector2 alignedRectangleXAxis = new(1f, 0f);
            Vector2 alignedRectangleYAxis = new(0f, 1f);

            Vector2 alignedRectangleCenter = WorldPosition;
            Vector2 orientedRectangleCenter = pRectangleOriented.WorldPosition;

            Vector2 alignedRectangleHalfExtents = HalfExtents;
            Vector2 orientedRectangleHalfExtents = pRectangleOriented.HalfExtents;

            // Test axes
            Vector2[] axes = { alignedRectangleXAxis, alignedRectangleYAxis, orientedRectangleXAxis, orientedRectangleYAxis };

            float minOverlap = float.MaxValue;
            Vector2 minAxis = Vector2.Zero;

            foreach (Vector2 axis in axes)
            {
                // Project centers
                float projectedAlignedRectangleCenter = Vector2.Dot(alignedRectangleCenter, axis);
                float projectedOrientedRectangleCenter = Vector2.Dot(orientedRectangleCenter, axis);

                float projectionHalfExtentAlignedRectangle = alignedRectangleHalfExtents.X * MathF.Abs(axis.X) + alignedRectangleHalfExtents.Y * MathF.Abs(axis.Y);
                float projectionHalfExtentOrientedRectangle = orientedRectangleHalfExtents.X * MathF.Abs(Vector2.Dot(orientedRectangleXAxis, axis)) + orientedRectangleHalfExtents.Y * MathF.Abs(Vector2.Dot(orientedRectangleYAxis, axis));

                float distanceBetweenProjectedCenters = MathF.Abs(projectedOrientedRectangleCenter - projectedAlignedRectangleCenter);
                float overlap = projectionHalfExtentAlignedRectangle + projectionHalfExtentOrientedRectangle - distanceBetweenProjectedCenters;

                // No overlap -> separating axis found
                if (overlap <= 0f)
                    return new CollisionEvent(false);

                if (overlap < minOverlap)
                {
                    minOverlap = overlap;
                    minAxis = axis;
                }
            }

            // Determine normal direction
            float sign = MathF.Sign(Vector2.Dot(alignedRectangleCenter - orientedRectangleCenter, minAxis));
            if (sign == 0f) sign = 1f;
            Vector2 collisionNormal = NormalizeZeroSafe(minAxis * sign);

            // Approximate contact point: use support points on each rect in direction of the normal,
            // then take their midpoint as an approximate collision position.
            float alignedRectangleSignX = MathF.Sign(Vector2.Dot(collisionNormal, alignedRectangleXAxis));
            float alignedRectangleSignY = MathF.Sign(Vector2.Dot(collisionNormal, alignedRectangleYAxis));
            Vector2 supportA = alignedRectangleCenter + new Vector2(alignedRectangleSignX * alignedRectangleHalfExtents.X, alignedRectangleSignY * alignedRectangleHalfExtents.Y);

            float orientedRectangleSignX = MathF.Sign(Vector2.Dot(collisionNormal, orientedRectangleXAxis));
            float orientedRectangleSignY = MathF.Sign(Vector2.Dot(collisionNormal, orientedRectangleYAxis));
            Vector2 supportB = orientedRectangleCenter + orientedRectangleXAxis * (orientedRectangleSignX * orientedRectangleHalfExtents.X) + orientedRectangleYAxis * (orientedRectangleSignY * orientedRectangleHalfExtents.Y);

            Vector2 collisionPosition = (supportA + supportB) * 0.5f;

            return new CollisionEvent(true, collisionPosition, collisionNormal);
        }
    }
}
