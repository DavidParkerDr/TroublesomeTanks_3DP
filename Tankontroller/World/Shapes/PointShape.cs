using Microsoft.Xna.Framework;
using System;

namespace Tankontroller.World.Shapes
{
    /// <summary>
    /// Represents a point shape for collision detection. <br></br>
    /// NOTE: Currently the PointShape is a little redundant, as it can be represented with a Vector2.
    ///       However, I left the implementation of the class in place for potential future use.
    /// </summary>
    public class PointShape : CollisionShape
    {
        public PointShape(Transform pOwner, bool pEnabled = true) : base(pOwner, pEnabled) { }
        public PointShape(Transform pOwner, Vector2 pLocalOffset, bool pEnabled = true) : base(pOwner, pLocalOffset, pEnabled) { }

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
            if (Vector2.DistanceSquared(WorldPosition, point) <= float.Epsilon)
            {
                return new CollisionEvent(true, WorldPosition);
            }
            return new CollisionEvent(false);
        }

        /// <summary>
        /// Check for intersection with another point shape - if they occupy the same position.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (the point itself). </returns>
        public CollisionEvent IntersectsPoint(PointShape pPoint)
        {
            return Intersects(pPoint.WorldPosition);
        }

        /// <summary>
        /// Check for intersection with a circle shape - if the point is inside the circle.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (the point itself)
        /// 2. The normal of the collision (pointing away from the circle) </returns>
        public CollisionEvent IntersectsCircle(CircleShape pCircle)
        {
            CollisionEvent collisionEvent = pCircle.Intersects(WorldPosition);
            if (collisionEvent.CollisionNormal.HasValue)
                collisionEvent.CollisionNormal *= -1;
            return collisionEvent;
        }

        /// <summary>
        /// Checks for intersection with an axis-aligned rectangle shape - if the point is inside the rectangle.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (the point itself).
        /// 2. The normal of the collision (pointing away from the rectangle center). </returns>
        public CollisionEvent IntersectsAlignedRectangle(RectangleAxisAlignedShape pRectangleAligned)
        {
            CollisionEvent collisionEvent = pRectangleAligned.Intersects(WorldPosition);
            if (collisionEvent.CollisionNormal.HasValue)
                collisionEvent.CollisionNormal *= -1;
            return collisionEvent;
        }

        /// <summary>
        /// Checks for intersection with an oriented rectangle shape - if the point is inside the rectangle.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (the point itself).
        /// 2. The normal of the collision (pointing away from the rectangle center). </returns>
        public CollisionEvent IntersectsOrientedRectangle(RectangleOrientedShape pRectangleOriented)
        {
            CollisionEvent collisionEvent = pRectangleOriented.Intersects(WorldPosition);
            if (collisionEvent.CollisionNormal.HasValue)
                collisionEvent.CollisionNormal *= -1;
            return collisionEvent;
        }
    }
}
