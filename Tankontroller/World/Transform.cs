using System;
using Microsoft.Xna.Framework;

namespace Tankontroller.World
{
    /// <summary>
    /// Stores position, rotation and scale of an object in the world
    /// </summary>
    public class Transform
    {
        public Vector2 Position { get; set; }
        /// <summary> In radians </summary>
        public float Rotation { get; set; }
        public Vector2 Scale { get; set; }

        public Transform()
        {
            Position = Vector2.Zero;
            Rotation = 0f;
            Scale = Vector2.One;
        }

        public Transform(Vector2 pPosition)
        {
            Position = pPosition;
            Rotation = 0f;
            Scale = Vector2.One;
        }

        public Transform(Vector2 pPosition, float pRotation)
        {
            Position = pPosition;
            Rotation = pRotation;
            Scale = Vector2.One;
        }

        public Transform(Vector2 pPosition, float pRotation, Vector2 pScale)
        {
            Position = pPosition;
            Rotation = pRotation;
            Scale = pScale;
        }
    }
}
