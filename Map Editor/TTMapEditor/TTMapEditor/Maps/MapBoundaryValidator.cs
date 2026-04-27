using Microsoft.Xna.Framework;
using System;
using TTMapEditor.Objects;

namespace TTMapEditor.Maps
{
    /// <summary>
    /// Provides helper methods to validate whether map elements
    /// lie completely within the configured playable area.
    /// </summary>
    internal class MapBoundaryValidator
    {

        private readonly Rectangle mPlayArea;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapBoundaryValidator"/> class.
        /// </summary>
        /// <param name="pPlayArea">
        /// The axis-aligned rectangle describing the bounds of the playable area.
        /// All validations are performed against this rectangle.
        /// </param>
        public MapBoundaryValidator(Rectangle pPlayArea)
        {
            mPlayArea = pPlayArea;
        }

        /// <summary>
        /// Determines whether the specified axis-aligned rectangle lies completely
        /// within the playable area.
        /// </summary>
        /// <param name="pRect">The rectangle to validate.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="pRect"/> is fully inside <see cref="mPlayArea"/>;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool IsRectWithinPlayArea(Rectangle pRect)
        {
            // Check each side of the rectangle against the play area's bounds.
            return pRect.Left >= mPlayArea.Left
                && pRect.Top >= mPlayArea.Top
                && pRect.Right <= mPlayArea.Right
                && pRect.Bottom <= mPlayArea.Bottom;
        }

        /// <summary>
        /// Determines whether the specified wall (which may be rotated) lies completely
        /// within the playable area.
        /// </summary>
        /// <param name="pWall">The rectangular wall to validate.</param>
        /// <returns>
        /// <c>true</c> if all four rotated corners of the wall are inside
        /// <see cref="mPlayArea"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool IsWallWithinPlayArea(RectWall pWall)
        {
            // World-space center of the wall's rectangle.
            Vector2 center = new Vector2(pWall.mRectangle.Center.X, pWall.mRectangle.Center.Y);

            // Local-space (unrotated) corner positions relative to the center.
            Vector2[] corners =
            {
                new Vector2(-pWall.mRectangle.Width / 2f, -pWall.mRectangle.Height / 2f),
                new Vector2(pWall.mRectangle.Width / 2f, -pWall.mRectangle.Height / 2f),
                new Vector2(pWall.mRectangle.Width / 2f, pWall.mRectangle.Height / 2f),
                new Vector2(-pWall.mRectangle.Width / 2f, pWall.mRectangle.Height / 2f)
            };

            // Precompute rotation matrix components using the wall's rotation angle (radians).
            float cos = MathF.Cos(pWall.mRotation);
            float sin = MathF.Sin(pWall.mRotation);

            // Rotate each corner around the center and test against the play area.
            for (int i = 0; i < corners.Length; i++)
            {
                // Apply 2D rotation to the corner in local space.
                float rotatedX = corners[i].X * cos - corners[i].Y * sin;
                float rotatedY = corners[i].X * sin + corners[i].Y * cos;

                // Translate rotated corner into world space.
                Vector2 worldPos = new Vector2(center.X + rotatedX, center.Y + rotatedY);

                // If any corner lies outside the play area's bounds, the wall is invalid.
                if (worldPos.X < mPlayArea.Left
                    || worldPos.X > mPlayArea.Right
                    || worldPos.Y < mPlayArea.Top
                    || worldPos.Y > mPlayArea.Bottom)
                {
                    return false;
                }
            }

            // All corners were inside the play area.
            return true;
        }
    }
}