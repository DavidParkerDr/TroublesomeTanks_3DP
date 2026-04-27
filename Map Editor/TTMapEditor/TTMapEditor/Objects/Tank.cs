using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TTMapEditor.Objects
{
    /// <summary>
    /// Represents a tank object in the map editor.
    /// Inherits common texture/rectangle behavior from <see cref="SceneObject"/>.
    /// Responsible for rendering a rotatable tank sprite with a visual
    /// front/heading indicator to show its facing direction.
    /// </summary>
    internal class Tank : SceneObject
    {
        /// <summary>
        /// Default tint color used when the tank is not selected.
        /// </summary>
        private static readonly Color COLOUR = Color.Blue;

        /// <summary>
        /// Small 1x1 white pixel texture used to draw simple
        /// primitives (front indicator and tip) via scaling.
        /// </summary>
        private static readonly Texture2D sPixel =
            TTMapEditor.Instance().GetContentManager().Load<Texture2D>("white_pixel");

        /// <summary>
        /// Current rotation of the tank in radians.
        /// Rotation is applied around the center of the tank sprite.
        /// </summary>
        public float mRotation { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tank"/> class.
        /// </summary>
        /// <param name="pTexture">Texture representing the tank sprite.</param>
        /// <param name="pRectangle">
        /// Destination rectangle defining the tank's position and size in world space.
        /// </param>
        public Tank(Texture2D pTexture, Rectangle pRectangle)
            : base(pTexture, pRectangle)
        {

        }

        /// <summary>
        /// Adjusts the tank rotation by the given delta (in radians).
        /// Positive values rotate counter-clockwise in screen space.
        /// </summary>
        /// <param name="delta">Rotation increment in radians.</param>
        public void Rotate(float delta) => mRotation += delta;

        /// <summary>
        /// Draws the tank sprite and its front-facing indicator.
        /// The tank is rendered centered within its rectangle, with scaling
        /// to fit and rotation applied. When selected, it is tinted yellow.
        /// A red/orange indicator line and tip show the current facing direction.
        /// </summary>
        /// <param name="pSpriteBatch">Sprite batch used for rendering.</param>
        public override void Draw(SpriteBatch pSpriteBatch)
        {
            // tint when selected
            Color tint = GetIsSelected() ? Color.Yellow : COLOUR;

            // draw centered at rectangle center with rotation and scale to rectangle size
            Vector2 position = new Vector2(mRectangle.Center.X, mRectangle.Center.Y);
            Vector2 origin = new Vector2(mTexture.Width / 2f, mTexture.Height / 2f);
            Vector2 scale = new Vector2(
                mRectangle.Width / (float)mTexture.Width,
                mRectangle.Height / (float)mTexture.Height);

            pSpriteBatch.Draw(
                mTexture,
                position,
                null,
                tint,
                mRotation,
                origin,
                scale,
                SpriteEffects.None,
                0f);

            // Draw a front indicator: a short line in front of the tank that rotates with it.

            // Length of the indicator (in world units), based on the tank size.
            float length = Math.Max(mRectangle.Width, mRectangle.Height) * 0.6f;

            // Thickness of the indicator; clamped to a sensible minimum.
            float thickness = Math.Max(
                2f,
                Math.Min(mRectangle.Width, mRectangle.Height) * 0.12f);

            // Direction vector derived from current rotation (unit-length).
            Vector2 dir = new Vector2(
                (float)Math.Cos(mRotation),
                (float)Math.Sin(mRotation));

            // Center of the indicator line, slightly offset in front of the tank.
            Vector2 indicatorCenter =
                position + dir * (length / 2f + Math.Min(mRectangle.Width, mRectangle.Height) * 0.1f);

            // Origin at pixel center (1x1 pixel), scale to the desired line length/thickness.
            Vector2 pixelOrigin = new Vector2(0.5f, 0.5f);
            Vector2 pixelScale = new Vector2(length, thickness);

            // Draw the front line to show heading.
            pSpriteBatch.Draw(
                sPixel,
                indicatorCenter,
                null,
                Color.Red,
                mRotation,
                pixelOrigin,
                pixelScale,
                SpriteEffects.None,
                0f);

            // Draw a small tip (a short square) at the very front-most point.
            Vector2 tipPos =
                position + dir * (length + Math.Min(mRectangle.Width, mRectangle.Height) * 0.1f + thickness / 2f);
            Vector2 tipScale = new Vector2(thickness * 1.2f, thickness * 1.2f);

            pSpriteBatch.Draw(
                sPixel,
                tipPos,
                null,
                Color.OrangeRed,
                mRotation,
                pixelOrigin,
                tipScale,
                SpriteEffects.None,
                0f);
        }

        /// <summary>
        /// Draws a simple black outline of the tank using its outline rectangle.
        /// Note that this outline is not rotated and stays axis-aligned.
        /// </summary>
        /// <param name="pSpriteBatch">Sprite batch used for rendering.</param>
        public override void DrawOutline(SpriteBatch pSpriteBatch) =>
            pSpriteBatch.Draw(mTexture, mOutlineRectangle, Color.Black);
    }
}
