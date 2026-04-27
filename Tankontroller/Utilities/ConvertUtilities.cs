using Microsoft.Xna.Framework;
using System;

namespace Tankontroller.Utilities
{
    internal static class ConvertUtilities
    {
        /// <summary>
        /// Returns an axis-aligned Rectangle that fully contains the four supplied corners.
        /// This is an AABB — it will enclose rotated points but will not preserve their rotation.
        /// </summary>
        public static Rectangle ToRectangle(Vector2 pTopLeft, Vector2 pTopRight, Vector2 pBottomLeft, Vector2 pBottomRight)
        {
            // Find the min and max X and Y values among the four points
            float minX = MathF.Min(MathF.Min(pTopLeft.X, pTopRight.X), MathF.Min(pBottomLeft.X, pBottomRight.X));
            float minY = MathF.Min(MathF.Min(pTopLeft.Y, pTopRight.Y), MathF.Min(pBottomLeft.Y, pBottomRight.Y));
            float maxX = MathF.Max(MathF.Max(pTopLeft.X, pTopRight.X), MathF.Max(pBottomLeft.X, pBottomRight.X));
            float maxY = MathF.Max(MathF.Max(pTopLeft.Y, pTopRight.Y), MathF.Max(pBottomLeft.Y, pBottomRight.Y));

            // Convert to integer rectangle
            int left = (int)MathF.Floor(minX);
            int top = (int)MathF.Floor(minY);
            int right = (int)MathF.Ceiling(maxX);
            int bottom = (int)MathF.Ceiling(maxY);

            int width = Math.Max(0, right - left);
            int height = Math.Max(0, bottom - top);

            // Create and return the Rectangle
            return new Rectangle(left, top, width, height);
        }

        /// <summary>
        /// Returns an axis-aligned Rectangle that fully contains the four supplied corners.
        /// This is an AABB — it will enclose rotated points but will not preserve their rotation.
        /// </summary>
        /// <exception cref="ArgumentException"> When the number of corners is not 4 </exception>
        public static Rectangle ToRectangle(Vector2[] pCorners)
        {
            if (pCorners == null || pCorners.Length != 4)
            {
                throw new ArgumentException("pCorners must be an array of exactly four Vector2 elements.");
            }
            return ToRectangle(pCorners[0], pCorners[1], pCorners[2], pCorners[3]);
        }
    }
}
