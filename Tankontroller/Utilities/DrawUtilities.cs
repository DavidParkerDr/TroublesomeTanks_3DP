using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Tankontroller.Utilities
{
    static internal class DrawUtilities
    {
        // Potential TODO: Cache a dictionary of ring textures of different thicknesses

        // Cached objects
        private static Texture2D mCircleTexture = null;
        private static Texture2D mRingTexture = null;
        private static Texture2D mPixelTexture = null;
        private static Texture2D mCircleMaskTexture = null;

        // Default parameters
        private const int DEFAULT_CIRCLE_RADIUS = 256;
        private const int DEFAULT_RING_THICKNESS = 128;
        private const int DEFAULT_MASK_SIZE = 256;

        // -----------------------------------------------------------------------------------------

        #region Circle & Ring

        /// <summary>
        /// Draw a circle at the given position with the given radius and tint using a specified texture.
        /// </summary>
        public static void DrawCircle(SpriteBatch pSpriteBatch, Texture2D pCircleTexture, Vector2 pPosition, float pRadius, Color pTint)
        {
            Vector2 origin = new(pCircleTexture.Width / 2f, pCircleTexture.Height / 2f);
            float scale = (pRadius * 2f) / pCircleTexture.Width;
            pSpriteBatch.Draw(pCircleTexture, pPosition, null, pTint, 0f, origin, scale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draw a circle at the given position with the given radius and tint using a pregenerated circle texture.
        /// </summary>
        public static void DrawCircle(SpriteBatch pSpriteBatch, Vector2 pPosition, float pRadius, Color pTint)
        {
            // Generate the circle texture if it doesn't exist
            mCircleTexture ??= CreateCircleTexture(pSpriteBatch.GraphicsDevice, DEFAULT_CIRCLE_RADIUS);
            // Draw the circle using the pregenerated texture
            DrawCircle(pSpriteBatch, mCircleTexture, pPosition, pRadius, pTint);
        }

        /// <summary>
        /// Draw a ring at the given position with the given radius and tint using a pregenerated ring texture.
        /// </summary>
        public static void DrawRing(SpriteBatch pSpriteBatch, Vector2 pPosition, float pRadius, Color pTint)
        {
            // Generate the ring texture if it doesn't exist
            mRingTexture ??= CreateRingTexture(pSpriteBatch.GraphicsDevice, DEFAULT_CIRCLE_RADIUS, DEFAULT_RING_THICKNESS);
            // Draw the ring using the pregenerated texture
            DrawCircle(pSpriteBatch, mRingTexture, pPosition, pRadius, pTint);
        }

        /// <summary>
        /// Generate a white premultiplied alpha circle texture with the given radius.
        /// (The premultipled alpha allows for smooth antialiased edges).
        /// </summary>
        /// <returns> Generated white cricle texture (generally for caching in DrawUtilities) </returns>
        public static Texture2D CreateCircleTexture(GraphicsDevice pGraphicsDevice, int pRadius)
        {
            // Create a square texture that fits the circle
            int diameter = pRadius * 2;
            Texture2D circleTexture = new(pGraphicsDevice, diameter, diameter);
            Color[] colorData = new Color[diameter * diameter];
            float radius = diameter / 2f;
            Vector2 center = new(radius, radius);

            // Fill in the texture data with a circle
            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    // Calculate distance from center
                    Vector2 point = new(x, y);
                    float distanceFromCenter = Vector2.Distance(point, center);
                    // Simple linear antialias from radius-1..radius
                    float alpha = MathHelper.Clamp(radius - distanceFromCenter + 1f, 0f, 1f);
                    // Premultiplied alpha white circle
                    colorData[y * diameter + x] = new Color(alpha, alpha, alpha, alpha);
                }
            }

            circleTexture.SetData(colorData);
            return circleTexture;
        }

        /// <summary>
        /// Generate a white premultiplied alpha ring texture with the given radius.
        /// (The premultipled alpha allows for smooth antialiased edges).
        /// </summary>
        /// <returns> Generated white cricle texture (generally for caching in DrawUtilities) </returns>
        public static Texture2D CreateRingTexture(GraphicsDevice pGraphicsDevice, int pRadius, int pThickness)
        {
            // Create a square texture that fits the ring
            int diameter = pRadius * 2;
            Texture2D ringTexture = new(pGraphicsDevice, diameter, diameter);
            Color[] colorData = new Color[diameter * diameter];
            float radius = diameter / 2f;
            Vector2 center = new(radius, radius);

            // Define inner and outer edges of the ring
            float outerEdge = radius;
            float innerEdge = radius - pThickness;

            // Fill in the texture data with a ring
            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    Vector2 point = new(x, y);
                    float distanceFromCenter = Vector2.Distance(point, center);

                    // antialiased ring: blend at both inner and outer boundaries
                    float alphaOuter = MathHelper.Clamp(outerEdge - distanceFromCenter + 1f, 0f, 1f);
                    float alphaInner = MathHelper.Clamp(distanceFromCenter - innerEdge + 1f, 0f, 1f);
                    float alpha = alphaOuter * alphaInner; // nonzero only where distanceFromCenter is between inner and outer

                    // keep the same color scheme as CreateCircleTexture (white with alpha)
                    colorData[y * diameter + x] = new Color(alpha, alpha, alpha, alpha);
                }
            }

            ringTexture.SetData(colorData);
            return ringTexture;
        }

        #endregion Circle & Ring

        // -----------------------------------------------------------------------------------------

        #region Rectangle

        /// <summary>
        /// Draw a rectangle at the given position with the given size and tint using a pixel texture.
        /// </summary>
        /// <param name="pRectangle"> Reference rectangle for the size and origin </param>
        public static void DrawRectangle(SpriteBatch pSpriteBatch, Rectangle pRectangle, Color pColor, float pRotationRadians, Vector2 pOrigin, float pScale)
        {
            // Generate the pixel texture if it doesn't exist
            mPixelTexture ??= CreatePixelTexture(pSpriteBatch.GraphicsDevice);
            // Draw the rectangle using the pixel texture scaled to the desired size
            Vector2 origin = new(pRectangle.Width / 2f, pRectangle.Height / 2f);
            pSpriteBatch.Draw(mPixelTexture, pOrigin, pRectangle, pColor, pRotationRadians, origin, pScale, SpriteEffects.None, 0.0f);
        }

        #endregion Rectangle

        /// <summary>
        /// Generate a white 1x1 pixel texture.
        /// </summary>
        /// <returns> Generated 1x1 pixel texture (generally for caching in DrawUtilities) </returns>
        public static Texture2D CreatePixelTexture(GraphicsDevice pGraphicsDevice)
        {
            Texture2D pixelTexture = new(pGraphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });
            return pixelTexture;
        }

        #region Circle Mask (Death Zone)

        /// <summary>
        /// Generate an inverted circle mask texture - opaque outside, transparent inside circle.
        /// Perfect for death zone effects.
        /// </summary>
        /// <returns> Generated circle mask texture with transparent center </returns>
        public static Texture2D CreateInvertedCircleMask(GraphicsDevice pGraphicsDevice, int pSize, float pScreenScale = 1f)
        {
            Texture2D maskTexture = new(pGraphicsDevice, pSize, pSize);
            Color[] colorData = new Color[pSize * pSize];
            float radius = pSize / 2f;
            Vector2 center = new(radius, radius);

            for (int y = 0; y < pSize; y++)
            {
                for (int x = 0; x < pSize; x++)
                {
                    Vector2 point = new(x, y);
                    float distanceFromCenter = Vector2.Distance(point, center);

                    // Inverted: 0 alpha at center, 1 alpha at edges
                    // Add antialiasing near the edge
                    float alpha = MathHelper.Clamp((distanceFromCenter - (radius / pScreenScale) + 2f) / 2f, 0f, 1f);
                    // Alternative sharper edge without antialiasing:
                    alpha = distanceFromCenter > (radius / pScreenScale) ? 1f : 0f;

                    // Premultiplied alpha
                    colorData[y * pSize + x] = new Color(alpha, alpha, alpha, alpha);
                }
            }

            maskTexture.SetData(colorData);
            return maskTexture;
        }

        /// <summary>
        /// Draw a closing iris/death zone effect by drawing an inverted circle mask that scales.
        /// The center is transparent (safe zone) and edges are opaque (death zone).
        /// </summary>
        public static void DrawDeathZone(SpriteBatch pSpriteBatch, Vector2 pSafeZoneCenter, float pSafeZoneRadius, Color pTint, float pScreenScale)
        {
            // Generate the mask texture if it doesn't exist
            mCircleMaskTexture ??= CreateInvertedCircleMask(pSpriteBatch.GraphicsDevice, DEFAULT_MASK_SIZE, pScreenScale);

            // Scale the mask to cover the entire screen with the hole at the safe zone
            DrawCircle(pSpriteBatch, mCircleMaskTexture, pSafeZoneCenter, pSafeZoneRadius * pScreenScale, pTint);
        }

        #endregion Circle Mask (Death Zone)

    }
}
