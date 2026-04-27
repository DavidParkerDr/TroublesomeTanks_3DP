using Microsoft.Xna.Framework;
using System.Collections.Generic;
using TTMapEditor.Maps;
using TTMapEditor.Objects;

namespace TTMapEditor.Managers
{
    /// <summary>
    /// Manages selection and dragging of <see cref="SceneObject"/> instances
    /// within a <see cref="MapPreview"/>. Ensures only one object is selected
    /// at a time and that objects cannot be moved outside the valid play area.
    /// </summary>
    internal class SelectionManager
    {
        SceneObject mSelectedObject;

        Rectangle mSelectedObjectPreviousRect;

        Vector2 mSelectedDragOffset;

        MapPreview mMapPreview;

        MapBoundaryValidator mMapBoundaryValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectionManager"/> class.
        /// </summary>
        /// <param name="pPreview">Map preview that owns the selectable objects.</param>
        /// <param name="pValidator">
        /// Boundary validator used to check whether dragged objects remain within the play area.
        /// </param>
        public SelectionManager(MapPreview pPreview, MapBoundaryValidator pValidator)
        {
            mMapPreview = pPreview;
            mMapBoundaryValidator = pValidator;
        }

        /// <summary>
        /// Clears the current selection on all selectable objects and resets
        /// the internally tracked selected object.
        /// </summary>
        public void DeselectAll()
        {
            foreach (var w in mMapPreview.GetWalls()) w.SetSelected(false);
            foreach (var t in mMapPreview.GetTanks()) t.SetSelected(false);
            foreach (var p in mMapPreview.GetPickups()) p.SetSelected(false);
            mSelectedObject = null;
        }

        /// <summary>
        /// Handles selection and drag interactions for the current frame.
        /// This:
        /// <list type="number">
        /// <item><description>Resolves which object is clicked (pickups &gt; tanks &gt; walls).</description></item>
        /// <item><description>Toggles selection or starts dragging when clicked.</description></item>
        /// <item><description>On mouse release, finalizes the move and reverts if outside play area.</description></item>
        /// </list>
        /// </summary>
        /// <param name="pMousePosition">Current mouse position in world/map coordinates.</param>
        public void HandleInteraction(Vector2 pMousePosition)
        {
            bool handledClick = false;

            // Pickups (top-most priority for selection)
            HandleSelectionFor(mMapPreview.GetPickups(), ref handledClick, pMousePosition);

            // Tanks
            if (!handledClick)
            {
                HandleSelectionFor(mMapPreview.GetTanks(), ref handledClick, pMousePosition);
            }

            // Walls (lowest priority)
            if (!handledClick)
            {
                HandleSelectionFor(mMapPreview.GetWalls(), ref handledClick, pMousePosition);
            }

            // On mouse release finalize move: if object outside play area, revert
            if (mSelectedObject != null && mSelectedObject.GetIsSelected() && InputManager.isLeftMouseReleased())
            {
                // Validate according to specific object type
                if (mSelectedObject is RectWall rw && !mMapBoundaryValidator.IsWallWithinPlayArea(rw))
                {
                    // Revert wall to last valid rectangle
                    mSelectedObject.SetRectangle(mSelectedObjectPreviousRect);
                }
                else if (mSelectedObject is Tank || mSelectedObject is Pickup)
                {
                    Rectangle r = mSelectedObject.mRectangle;
                    if (!mMapBoundaryValidator.IsRectWithinPlayArea(r))
                    {
                        // Revert tank/pickup to last valid rectangle
                        mSelectedObject.SetRectangle(mSelectedObjectPreviousRect);
                    }
                }

                // End selection after mouse release
                mSelectedObject.SetSelected(false);
                mSelectedObject = null;
            }
        }

        /// <summary>
        /// Gets the currently selected scene object, if any.
        /// </summary>
        /// <returns>The selected <see cref="SceneObject"/> or <c>null</c>.</returns>
        public SceneObject GetSelectedObject()
        {
            return mSelectedObject;
        }

        /// <summary>
        /// Sets the currently selected scene object without changing any
        /// selection flags on other objects.
        /// </summary>
        /// <param name="pSceneObject">Object to track as selected.</param>
        public void SetSelectedObject(SceneObject pSceneObject)
        {
            mSelectedObject = pSceneObject;
        }

        /// <summary>
        /// Generic selection and dragging logic for lists of <see cref="SceneObject"/>-derived types.
        /// This method:
        /// <list type="bullet">
        /// <item><description>Detects click hits and toggles selection for objects in the given list.</description></item>
        /// <item><description>Starts dragging on click when an object becomes selected.</description></item>
        /// <item><description>Updates object position while the mouse is held down.</description></item>
        /// <item><description>Validates intermediate drag positions and reverts invalid moves immediately.</description></item>
        /// </list>
        /// </summary>
        /// <typeparam name="T">A type derived from <see cref="SceneObject"/>.</typeparam>
        /// <param name="list">Collection of objects to test for selection/dragging.</param>
        /// <param name="handledClick">
        /// Reference flag that indicates a click has already been handled by a higher-priority list.
        /// </param>
        /// <param name="mousePos">Current mouse position in world/map coordinates.</param>
        public void HandleSelectionFor<T>(List<T> list, ref bool handledClick, Vector2 mousePos) where T : SceneObject
        {
            foreach (T obj in list)
            {
                if (handledClick)
                {
                    break;
                }

                // Handle initial click/selection toggle
                if (obj.IsPointWithin(mousePos) && InputManager.isLeftMouseClicked())
                {
                    if (!obj.GetIsSelected())
                    {
                        // Ensure single selection by clearing all other selections
                        DeselectAll();

                        obj.SetSelected(true);
                        mSelectedObject = obj;

                        // Store rectangle so we can revert later if needed
                        mSelectedObjectPreviousRect = obj.mRectangle;

                        // Record where in the object the mouse grabbed it
                        mSelectedDragOffset = new Vector2(
                            mousePos.X - obj.mRectangle.X,
                            mousePos.Y - obj.mRectangle.Y);
                    }
                    else
                    {
                        // Clicking an already-selected object deselects it
                        obj.SetSelected(false);
                        mSelectedObject = null;
                    }

                    handledClick = true;
                }

                // Dragging logic while left mouse is held down
                if (obj.GetIsSelected() && !InputManager.isLeftMouseReleased())
                {
                    int newX = (int)(mousePos.X - mSelectedDragOffset.X);
                    int newY = (int)(mousePos.Y - mSelectedDragOffset.Y);

                    // Store previous position before updating so we can revert if invalid
                    Rectangle previousRect = obj.mRectangle;

                    obj.UpdatePosition(newX, newY);

                    // Check if the new position is valid within the play area
                    bool isValid;
                    if (obj is RectWall wall)
                    {
                        isValid = mMapBoundaryValidator.IsWallWithinPlayArea(wall);
                    }
                    else
                    {
                        isValid = mMapBoundaryValidator.IsRectWithinPlayArea(obj.mRectangle);
                    }

                    // If invalid, immediately revert to previous position
                    if (!isValid)
                    {
                        obj.SetRectangle(previousRect);
                    }
                    else
                    {
                        // On success, update the last known good rectangle
                        mSelectedObjectPreviousRect = obj.mRectangle;
                    }
                }
            }
        }
    }
}
