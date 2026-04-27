using Microsoft.Xna.Framework;
using System;

namespace Tankontroller.World.Shapes
{
    /// <summary>
    /// Represents a circle collision shape defined by a radius and a center position
    /// </summary>
    public class CircleShape : CollisionShape
    {
        public float Radius { get; set; } = 1.0f;

        public CircleShape(Transform pOwner, float pRadius, bool pEnabled = true) : base(pOwner, pEnabled)
        {
            Radius = pRadius;
        }

        public CircleShape(Transform pOwner, float pRadius, Vector2 pLocalOffset, bool pEnabled = true) : base(pOwner, pLocalOffset, pEnabled)
        {
            Radius = pRadius;
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
            Vector2 difference = WorldPosition - point;
            if (difference.LengthSquared() <= Radius * Radius)
            {
                Vector2 normal = NormalizeZeroSafe(difference);
                return new CollisionEvent(true, point, normal);
            }
            return new CollisionEvent(false);
        }

        /// <summary>
        /// Check for intersection with a point shape - if the point is inside the circle.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (the point itself)
        /// 2. The normal of the collision (pointing away from the point, into the circle) </returns>
        public CollisionEvent IntersectsPoint(PointShape pPoint)
        {
            return Intersects(pPoint.WorldPosition);
        }

        /// <summary>
        /// Check for intersection with another circle shape - if the circles overlap.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (midpoint of overlap between the two circles)
        /// 2. The normal of the collision (pointing away from the other circle) </returns>
        public CollisionEvent IntersectsCircle(CircleShape pCircle)
        {
            Vector2 difference = WorldPosition - pCircle.WorldPosition;
            float radiusSum = Radius + pCircle.Radius;
            float distanceSquared = difference.LengthSquared();

            if (distanceSquared <= radiusSum * radiusSum)
            {
                float distance = (float)Math.Sqrt(distanceSquared);

                // Calculate the collision normal, avoiding division by zero
                Vector2 collisionNormal = distance > 0f ? Vector2.Normalize(difference) : new Vector2(1f, 0);

                // Closest point on the other circle's perimeter to this circle
                Vector2 pointOnOther = pCircle.WorldPosition + collisionNormal * pCircle.Radius;

                // Closest point on this circle's perimeter to the other circle
                Vector2 pointOnThis = WorldPosition - collisionNormal * Radius;

                // Collision position is the midpoint between the two closest points (the overlap area center)
                Vector2 collisionPosition = (pointOnOther + pointOnThis) / 2f;

                return new CollisionEvent(true, collisionPosition, collisionNormal);
            }
            return new CollisionEvent(false);
        }

        /// <summary>
        /// Check for intersection with an axis-aligned rectangle shape - if the circle overlaps with the rectangle.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (midpoint of overlap between the circle and rectangle)
        /// 2. The normal of the collision (pointing away from the rectangle) </returns>
        public CollisionEvent IntersectsAlignedRectangle(RectangleAxisAlignedShape pRectangleAligned)
        {
            Vector2 circleCenter = WorldPosition;
            Vector2 min = pRectangleAligned.Min;
            Vector2 max = pRectangleAligned.Max;

            // Clamp circle center to rectangle to find closest point
            float clampedX = Math.Clamp(circleCenter.X, min.X, max.X);
            float clampedY = Math.Clamp(circleCenter.Y, min.Y, max.Y);
            Vector2 closesPointOnRectangle = new(clampedX, clampedY);

            // If the circle center is inside the rectangle
            if (circleCenter.X >= min.X && 
                circleCenter.X <= max.X && 
                circleCenter.Y >= min.Y && 
                circleCenter.Y <= max.Y)
            {
                // Compute minimal penetration axis (choose the face with smallest penetration)
                Vector2 toCenter = circleCenter - pRectangleAligned.WorldPosition;
                Vector2 halfExtents = pRectangleAligned.HalfExtents;

                float penetrationX = halfExtents.X - Math.Abs(toCenter.X);
                float penetrationY = halfExtents.Y - Math.Abs(toCenter.Y);

                float signX = Math.Sign(toCenter.X);
                float signY = Math.Sign(toCenter.Y);

                Vector2 normal = penetrationX < penetrationY
                    ? new Vector2(signX, 0f)
                    : new Vector2(0f, signY);

                normal = NormalizeZeroSafe(normal);
                return new CollisionEvent(true, circleCenter, normal);
            }

            Vector2 difference = circleCenter - closesPointOnRectangle;
            float differenceLengthSquared = difference.LengthSquared();

            if (differenceLengthSquared <= Radius * Radius)
            {
                Vector2 collisionNormal = NormalizeZeroSafe(difference);

                Vector2 closestPointOnCircle = circleCenter - collisionNormal * Radius;

                Vector2 collisionPosition = (closesPointOnRectangle + closestPointOnCircle) / 2f;
                return new CollisionEvent(true, collisionPosition, collisionNormal);
            }

            return new CollisionEvent(false);
        }

        /// <summary>
        /// Check for intersection with an oriented rectangle shape - if the circle overlaps with the rectangle.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (midpoint of overlap between the circle and rectangle)
        /// 2. The normal of the collision (pointing away from the rectangle) </returns>
        public CollisionEvent IntersectsOrientedRectangle(RectangleOrientedShape pRectangleOriented)
        {
            Vector2 circleWorldPosition = WorldPosition;
            Vector2 circleLocalPosition = circleWorldPosition - pRectangleOriented.WorldPosition;

            float minusRotation = -pRectangleOriented.WorldRotation;
            float negativeCos = (float)Math.Cos(minusRotation);
            float negativeSin = (float)Math.Sin(minusRotation);

            Vector2 localSpaceCirclePosition = new(
                circleLocalPosition.X * negativeCos - circleLocalPosition.Y * negativeSin,
                circleLocalPosition.X * negativeSin + circleLocalPosition.Y * negativeCos
            );

            Vector2 halfExtents = pRectangleOriented.HalfExtents;

            // If center inside oriented rectangle -> compute minimal penetration face and build contact accordingly
            if (localSpaceCirclePosition.X >= -halfExtents.X && 
                localSpaceCirclePosition.X <= halfExtents.X && 
                localSpaceCirclePosition.Y >= -halfExtents.Y && 
                localSpaceCirclePosition.Y <= halfExtents.Y)
            {
                // Minimal penetration in rectangle local space
                Vector2 toCenterLocal = localSpaceCirclePosition;
                float penetrationX = halfExtents.X - Math.Abs(toCenterLocal.X);
                float penetrationY = halfExtents.Y - Math.Abs(toCenterLocal.Y);

                float signX = Math.Sign(toCenterLocal.X);
                float signY = Math.Sign(toCenterLocal.Y);

                Vector2 normalLocal = penetrationX < penetrationY
                    ? new Vector2(signX, 0f)
                    : new Vector2(0f, signY);

                // Rotate normalLocal back to world
                float cos = (float)Math.Cos(pRectangleOriented.WorldRotation);
                float sin = (float)Math.Sin(pRectangleOriented.WorldRotation);
                Vector2 normalWorld = new(
                    normalLocal.X * cos - normalLocal.Y * sin,
                    normalLocal.X * sin + normalLocal.Y * cos
                );

                normalWorld = NormalizeZeroSafe(normalWorld);

                return new CollisionEvent(true, circleWorldPosition, normalWorld);
            }

            // Center outside rectangle: clamp in local space to get closest point on rect, then test distance
            float clampedX = Math.Clamp(localSpaceCirclePosition.X, -halfExtents.X, halfExtents.X);
            float clampedY = Math.Clamp(localSpaceCirclePosition.Y, -halfExtents.Y, halfExtents.Y);
            Vector2 closestLocalOutside = new(clampedX, clampedY);

            Vector2 differenceLocal = localSpaceCirclePosition - closestLocalOutside;
            float distanceSquaredLocal = differenceLocal.LengthSquared();

            if (distanceSquaredLocal <= Radius * Radius)
            {
                Vector2 normalLocal = NormalizeZeroSafe(differenceLocal);

                // Rotate normalLocal back to world
                float cos = (float)Math.Cos(pRectangleOriented.WorldRotation);
                float sin = (float)Math.Sin(pRectangleOriented.WorldRotation);
                Vector2 normalWorld = new(
                    normalLocal.X * cos - normalLocal.Y * sin,
                    normalLocal.X * sin + normalLocal.Y * cos
                );

                normalWorld = NormalizeZeroSafe(normalWorld);

                // Compute world-space closest point on rectangle boundary
                Vector2 pointOnRectangleWorld = pRectangleOriented.WorldPosition + new Vector2(
                    closestLocalOutside.X * cos - closestLocalOutside.Y * sin,
                    closestLocalOutside.X * sin + closestLocalOutside.Y * cos
                );

                // Point on circle perimeter along normal
                Vector2 pointOnCircleWorld = circleWorldPosition - normalWorld * Radius;
                Vector2 collisionPosition = (pointOnRectangleWorld + pointOnCircleWorld) / 2f;


                return new CollisionEvent(true, collisionPosition, normalWorld);
            }

            return new CollisionEvent(false);
        }
    }
}
