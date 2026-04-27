using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace TTMapEditor.Objects
{
    /// <summary>
    /// Base type for all drawable objects in the map editor scene.
    /// Encapsulates a texture, a main rectangle, an outline rectangle and simple selection state.
    /// </summary>
    public abstract class SceneObject
    {
        /// <summary>
        /// Texture used when drawing this object and its outline.
        /// </summary>
        protected Texture2D mTexture;

        /// <summary>
        /// Main on-screen bounds of the object in world/editor coordinates.
        /// Used for drawing and hit-testing.
        /// </summary>
        public Rectangle mRectangle { get; protected set; }

        /// <summary>
        /// Rectangle used when drawing an outline around the main bounds.
        /// Its size is derived from <see cref="mRectangle"/> plus <see cref="OutlinePad"/>.
        /// </summary>
        protected Rectangle mOutlineRectangle;

        /// <summary>
        /// Number of pixels to expand the outline rectangle beyond the main bounds.
        /// </summary>
        private const int OutlinePad = 2;

        /// <summary>
        /// Tracks whether the object is currently selected in the editor.
        /// </summary>
        private bool mIsSelected = false;

        /// <summary>
        /// Initializes a new instance of <see cref="SceneObject"/> with a texture and initial bounds.
        /// </summary>
        /// <param name="texture">Texture to render for this scene object.</param>
        /// <param name="rectangle">Initial on-screen bounds.</param>
        protected SceneObject(Texture2D texture, Rectangle rectangle)
        {
            mTexture = texture;
            SetRectangle(rectangle);
        }

        /// <summary>
        /// Default tint color used when drawing this object.
        /// Derived classes can override to customize appearance.
        /// </summary>
        protected virtual Color Colour => Color.White;

        /// <summary>
        /// Draws the object using its texture and main rectangle.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch used for rendering.</param>
        public virtual void Draw(SpriteBatch spriteBatch) =>
            spriteBatch.Draw(mTexture, mRectangle, Colour);

        /// <summary>
        /// Draws an outline for the object using the outline rectangle.
        /// Typically used for selection or hover visualization.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch used for rendering.</param>
        public virtual void DrawOutline(SpriteBatch spriteBatch) =>
            spriteBatch.Draw(mTexture, mOutlineRectangle, Color.Black);

        /// <summary>
        /// Checks whether a point lies within the main bounds of this object.
        /// </summary>
        /// <param name="point">Point to test, in the same coordinate space as <see cref="mRectangle"/>.</param>
        /// <returns><c>true</c> if the point is inside <see cref="mRectangle"/>; otherwise <c>false</c>.</returns>
        public virtual bool IsPointWithin(Vector2 point) =>
            mRectangle.Contains(point);

        /// <summary>
        /// Updates the position of the object while preserving its current size.
        /// </summary>
        /// <param name="x">New X coordinate.</param>
        /// <param name="y">New Y coordinate.</param>
        public void UpdatePosition(int x, int y) =>
            SetRectangle(new Rectangle(x, y, mRectangle.Width, mRectangle.Height));

        /// <summary>
        /// Sets the main rectangle and recomputes the outline rectangle to match it.
        /// </summary>
        /// <param name="rectangle">New main bounds for the object.</param>
        public void SetRectangle(Rectangle rectangle)
        {
            mRectangle = rectangle;
            mOutlineRectangle = new Rectangle(
                rectangle.X - OutlinePad,
                rectangle.Y - OutlinePad,
                rectangle.Width + OutlinePad * 2,
                rectangle.Height + OutlinePad * 2);
        }

        /// <summary>
        /// Toggles the selection state of this object.
        /// </summary>
        public void ToggleSelected() => mIsSelected = !mIsSelected;

        /// <summary>
        /// Sets the selection state of this object explicitly.
        /// </summary>
        /// <param name="selected"><c>true</c> to mark as selected; <c>false</c> to clear selection.</param>
        public void SetSelected(bool selected) => mIsSelected = selected;

        /// <summary>
        /// Gets the current selection state of this object.
        /// </summary>
        /// <returns><c>true</c> if selected; otherwise <c>false</c>.</returns>
        public bool GetIsSelected() => mIsSelected;
    }
}
