using Microsoft.Xna.Framework;

namespace TTMapEditor.Objects
{
    /// <summary>
    /// Encapsulates drag behavior for a scene-object template.
    /// Tracks original bounds, drag offset, and updates the template position
    /// while the user drags with the mouse.
    /// </summary>
    /// <typeparam name="T">Concrete <see cref="SceneObject"/> type being dragged.</typeparam>
    internal class DraggableTemplate<T> where T : SceneObject
    {

        public T mTemplate { get; }

        public Rectangle mOriginalRect { get; private set; }

        public bool mIsDragging { get; private set; }

        public Vector2 mDragOffset { get; private set; }

        /// <summary>
        /// Initializes a new draggable wrapper around the provided template object.
        /// </summary>
        /// <param name="pTemplate">The template instance that will be dragged.</param>
        public DraggableTemplate(T pTemplate)
        {
            mTemplate = pTemplate;
            mOriginalRect = pTemplate.mRectangle;
            mIsDragging = false;
            mDragOffset = Vector2.Zero;
        }

        /// <summary>
        /// Begins a drag operation.
        /// Captures the template's current rectangle and the offset from the mouse
        /// position to the template's top-left corner.
        /// Caller is responsible for hit testing and deciding when to start dragging.
        /// </summary>
        /// <param name="pMousePosition">Mouse position in screen coordinates when the drag starts.</param>
        public void BeginDrag(Vector2 pMousePosition)
        {
            mOriginalRect = mTemplate.mRectangle;
            mDragOffset = new Vector2(pMousePosition.X - mOriginalRect.X, pMousePosition.Y - mOriginalRect.Y);
            mIsDragging = true;
        }

        /// <summary>
        /// Updates the template position while a drag is in progress.
        /// Does nothing if <see cref="mIsDragging"/> is <c>false</c>.
        /// </summary>
        /// <param name="pMousePosiition">Current mouse position in screen coordinates.</param>
        public void Update(Vector2 pMousePosiition)
        {
            if (!mIsDragging) return;
            int newX = (int)(pMousePosiition.X - mDragOffset.X);
            int newY = (int)(pMousePosiition.Y - mDragOffset.Y);
            mTemplate.UpdatePosition(newX, newY);
        }

        /// <summary>
        /// Ends the drag operation and returns the final template rectangle.
        /// Optionally resets the template to the original rectangle captured at
        /// <see cref="BeginDrag(Microsoft.Xna.Framework.Vector2)"/>.
        /// </summary>
        /// <param name="pResetToOriginal">
        /// If <c>true</c>, the template rectangle is restored to <see cref="mOriginalRect"/>.
        /// </param>
        /// <returns>The final rectangle of the template at drag end.</returns>
        public Rectangle EndDrag(bool pResetToOriginal = true)
        {
            if (!mIsDragging) return mTemplate.mRectangle;
            Rectangle final = mTemplate.mRectangle;
            if (pResetToOriginal)
            {
                mTemplate.SetRectangle(mOriginalRect);
            }
            mIsDragging = false;
            return final;
        }

        /// <summary>
        /// Restores the template to the original rectangle and clears all drag state.
        /// </summary>
        public void Reset()
        {
            mTemplate.SetRectangle(mOriginalRect);
            mIsDragging = false;
            mDragOffset = Vector2.Zero;
        }
    }
}
