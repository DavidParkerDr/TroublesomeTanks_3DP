using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Tankontroller.World.Shapes
{
    /// <summary>
    /// Represents an oriented rectangle shape for collision detection (rectangle that can be rotated).
    /// </summary>
    public class RectangleOrientedShape : CollisionShape
    {
        // Full size in local space (width, height)
        public Vector2 Size { get; set; } = Vector2.One;

        // Half extents in local space (width/2, height/2)
        public Vector2 HalfExtents => Size * 0.5f;

        // Local rotation relative to Owner.Rotation (radians)
        public float LocalRotation { get; set; } = 0f;

        // World rotation (radians)
        public float WorldRotation => Owner.Rotation + LocalRotation;

        public RectangleOrientedShape(Transform pOwner, Vector2 pSize, bool pEnabled = true) : base(pOwner, pEnabled)
        {
            Size = pSize;
        }

        public RectangleOrientedShape(Transform pOwner, Vector2 pSize, float pLocalRotation, Vector2 pLocalOffset, bool pEnabled = true) : base(pOwner, pLocalOffset, pEnabled)
        {
            Size = pSize;
            LocalRotation = pLocalRotation;
        }

        /// <summary>
        /// Converts the oriented rectangle to a MonoGame rectangle that fully contains it. Useful for drawing.
        /// NOTE: Could be optimized by caching the rectangle and only recalculating when position, size, or rotation changes.
        /// </summary>
        public Rectangle ToRectangle() => 
            new((int)MathF.Round(WorldPosition.X - HalfExtents.X), (int)MathF.Round(WorldPosition.Y - HalfExtents.Y), (int)MathF.Round(Size.X), (int)MathF.Round(Size.Y));

        /// <summary>
        /// Draws the oriented rectangle using a texture. 
        /// The texture will be scaled to fit the rectangle's size, and rotated to match the rectangle's rotation.
        /// </summary>
        public void Draw(SpriteBatch pSpriteBatch, Texture2D pTexture, Color pColor)
        {
            Rectangle referenceRectangle = ToRectangle();
            Vector2 position = new Vector2(referenceRectangle.Center.X, referenceRectangle.Center.Y);
            Vector2 origin = new Vector2(pTexture.Width / 2f, pTexture.Height / 2f);
            Vector2 scale = new Vector2(referenceRectangle.Width / (float)pTexture.Width, referenceRectangle.Height / (float)pTexture.Height);

            pSpriteBatch.Draw(pTexture, WorldPosition, null, pColor, WorldRotation, origin, scale, SpriteEffects.None, 0f);
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
            // Make point local space relative to rectangle center
            Vector2 pointLocal = point - WorldPosition;

            // Rotate by negative world rotation to align rectangle axes with world axes
            float minusRotation = -WorldRotation;
            float cos = (float)Math.Cos(minusRotation);
            float sin = (float)Math.Sin(minusRotation);
            Vector2 localSpacePoint = new(
                pointLocal.X * cos - pointLocal.Y * sin,
                pointLocal.X * sin + pointLocal.Y * cos
            );

            // Check if local point is within the rectangle's half-extents
            if (localSpacePoint.X >= -HalfExtents.X &&
                localSpacePoint.X <= HalfExtents.X &&
                localSpacePoint.Y >= -HalfExtents.Y &&
                localSpacePoint.Y <= HalfExtents.Y)
            {
                // Use vector from rectangle center to point (in world space), normalized.
                Vector2 normal = NormalizeZeroSafe(WorldPosition - point);
                return new CollisionEvent(true, point, normal);
            }

            return new CollisionEvent(false);
        }

        /// <summary>
        /// Checks for intersection with a point - if the point is inside the rectangle.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (the point itself).
        /// 2. The normal of the collision (pointing into the rectangle). </returns>
        public CollisionEvent IntersectsPoint(PointShape pPoint)
        {
            return Intersects(pPoint.WorldPosition);
        }

        /// <summary>
        /// Check for intersection with circle - if the circle overlaps with the rectangle.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (midpoint of overlap between the circle and rectangle)
        /// 2. The normal of the collision (pointing into the rectangle) </returns>
        public CollisionEvent IntersectsCircle(CircleShape pCircle)
        {
            CollisionEvent collisionEvent = pCircle.IntersectsOrientedRectangle(this);
            if (collisionEvent.CollisionNormal.HasValue)
                collisionEvent.CollisionNormal *= -1;
            return collisionEvent;
        }

        /// <summary>
        /// Check for intersection with an axis aligned rectangle shape - if the rectangles overlap.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (midpoint of overlap between the rectangles)
        /// 2. The normal of the collision (pointing away from the axis aligned rectangle) </returns>
        public CollisionEvent IntersectsAlignedRectangle(RectangleAxisAlignedShape pRectangleAligned)
        {
            CollisionEvent collisionEvent = pRectangleAligned.IntersectsOrientedRectangle(this);
            if (collisionEvent.CollisionNormal.HasValue)
                collisionEvent.CollisionNormal *= -1;
            return collisionEvent;
        }

        /// <summary>
        /// Check for intersection with another oriented rectangle shape - if the rectangles overlap.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (midpoint of overlap between the rectangles) TODO: this isn't actually the midpoint, needs fixing
        /// 2. The normal of the collision (pointing away from the other rectangle) </returns>
        public CollisionEvent IntersectsOrientedRectangle(RectangleOrientedShape pRectangleOriented)
        {
            // Check for trivial case of rectangles having the same center position
            if (WorldPosition == pRectangleOriented.WorldPosition)
            {
                return new CollisionEvent(true, WorldPosition);
            }

            // Build local axes for both rectangles in world space
            float cosA = (float)Math.Cos(WorldRotation);
            float sinA = (float)Math.Sin(WorldRotation);
            Vector2 thisAxisX = new(cosA, sinA);
            Vector2 thisAxisY = new(-sinA, cosA);

            float cosB = (float)Math.Cos(pRectangleOriented.WorldRotation);
            float sinB = (float)Math.Sin(pRectangleOriented.WorldRotation);
            Vector2 otherAxisX = new(cosB, sinB);
            Vector2 otherAxisY = new(-sinB, cosB);

            Vector2 centerA = WorldPosition;
            Vector2 centerB = pRectangleOriented.WorldPosition;

            Vector2 halfA = HalfExtents;
            Vector2 halfB = pRectangleOriented.HalfExtents;

            // Candidate axes: face normals of both rectangles
            Vector2[] axes = { thisAxisX, thisAxisY, otherAxisX, otherAxisY };

            float minOverlap = float.MaxValue;
            Vector2 minAxis = Vector2.Zero;

            foreach (Vector2 axis in axes)
            {
                // Project centers onto axis
                float projCenterA = Vector2.Dot(centerA, axis);
                float projCenterB = Vector2.Dot(centerB, axis);

                // Projected half extents for each rectangle onto axis
                float projHalfA = halfA.X * MathF.Abs(Vector2.Dot(thisAxisX, axis)) + halfA.Y * MathF.Abs(Vector2.Dot(thisAxisY, axis));
                float projHalfB = halfB.X * MathF.Abs(Vector2.Dot(otherAxisX, axis)) + halfB.Y * MathF.Abs(Vector2.Dot(otherAxisY, axis));

                float distance = MathF.Abs(projCenterA - projCenterB);
                float overlap = projHalfA + projHalfB - distance;

                // Separating axis found -> no collision
                if (overlap <= 0f)
                    return new CollisionEvent(false);

                if (overlap < minOverlap)
                {
                    minOverlap = overlap;
                    minAxis = axis;
                }
            }

            // Determine normal direction so it consistently points from the other rectangle to this rectangle
            float sign = MathF.Sign(Vector2.Dot(centerA - centerB, minAxis));
            if (sign == 0f) sign = 1f;
            Vector2 collisionNormal = NormalizeZeroSafe(minAxis * sign);

            // Support points: pick rectangle-support points in the direction of the collision normal, then midpoint
            float signAX = MathF.Sign(Vector2.Dot(collisionNormal, thisAxisX));
            float signAY = MathF.Sign(Vector2.Dot(collisionNormal, thisAxisY));
            Vector2 supportA = centerA + thisAxisX * (signAX * halfA.X) + thisAxisY * (signAY * halfA.Y);

            float signBX = MathF.Sign(Vector2.Dot(collisionNormal, otherAxisX));
            float signBY = MathF.Sign(Vector2.Dot(collisionNormal, otherAxisY));
            Vector2 supportB = centerB + otherAxisX * (signBX * halfB.X) + otherAxisY * (signBY * halfB.Y);

            Vector2 collisionPosition = (supportA + supportB) * 0.5f;

            return new CollisionEvent(true, collisionPosition, collisionNormal);
        }
    }
}
