using Microsoft.Xna.Framework;

namespace Tankontroller.World.Shapes
{
    /// <summary>
    /// Represents the result of a collision detection operation.
    /// Stores useful collision information. <br></br>
    /// TODO: Add penetration depth to the collision event. This would make Tank/Wall collision resolution more robust.
    /// </summary>
    public class CollisionEvent
    {
        /// <summary>
        /// Whether a collision has occurred
        /// </summary>
        public bool HasCollided { get; set; } = false;

        /// <summary>
        /// Represents the position where the collision occurred (the contact point).
        /// If the shapes are overlapping, this represents the central point of the overlapping area.
        /// Null if no collision has occurred or if not applicable.
        /// </summary>
        public Vector2? CollisionPosition { get; set; } = null;

        /// <summary>
        /// Represents the normal vector of a collision.
        /// Null if no collision has occurred or if not applicable.
        /// </summary>
        /// <remarks>The collision normal typically indicates the direction perpendicular to the surface
        /// at the point of contact. It is commonly used in physics calculations to determine reflection, response
        /// forces, or to resolve overlaps.</remarks>
        public Vector2? CollisionNormal { get; set; } = null;

        public CollisionEvent(bool pHasCollided)
        {
            HasCollided = pHasCollided;
        }

        public CollisionEvent(bool pHasCollided, Vector2 pCollisionPosition)
        {
            HasCollided = pHasCollided;
            CollisionPosition = pCollisionPosition;
        }

        public CollisionEvent(bool pHasCollided, Vector2 pCollisionPosition, Vector2 pCollisionNormal)
        {
            HasCollided = pHasCollided;
            CollisionPosition = pCollisionPosition;
            CollisionNormal = pCollisionNormal;
        }
    }
}
