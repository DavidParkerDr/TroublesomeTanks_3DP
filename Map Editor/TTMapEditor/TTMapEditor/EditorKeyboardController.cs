using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TTMapEditor.Managers;
using TTMapEditor.Maps;
using TTMapEditor.Objects;

namespace TTMapEditor
{
    /// <summary>
    /// Handles keyboard input for the map editor, allowing manipulation of the
    /// currently selected object (tanks, walls, pickups) in the preview map.
    /// </summary>
    /// <remarks>
    /// This controller is responsible for:
    /// <list type="bullet">
    /// <item>
    /// <description>Deleting the selected object.</description>
    /// </item>
    /// <item>
    /// <description>Rotating selected tanks with the arrow keys.</description>
    /// </item>
    /// <item>
    /// <description>
    /// Toggling pickup types on selected pickups via number keys (1–4).
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Rotating and scaling rectangular walls while enforcing map boundaries.
    /// </description>
    /// </item>
    /// </list>
    /// It uses <see cref="SelectionManager"/> to know which object is active,
    /// <see cref="MapPreview"/> to apply destructive operations (e.g. delete),
    /// and <see cref="MapBoundaryValidator"/> to ensure wall edits remain inside
    /// the valid play area.
    /// </remarks>
    internal class EditorKeyboardController
    {
        /// <summary>
        /// Reference to the map preview currently being edited.
        /// </summary>
        private readonly MapPreview mMapPreview;

        /// <summary>
        /// Manages which scene object is currently selected in the editor.
        /// </summary>
        private readonly SelectionManager mSelectionManager;

        /// <summary>
        /// Validates that walls remain within the playable map area when
        /// rotated or scaled.
        /// </summary>
        private readonly MapBoundaryValidator mMapBoundaryValidator;

        /// <summary>
        /// Stores the last valid rectangle for the currently edited wall,
        /// used to revert changes if a transform moves it out of bounds.
        /// </summary>
        private Rectangle mLastValidRect;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorKeyboardController"/> class.
        /// </summary>
        /// <param name="pMapPreview">Target <see cref="MapPreview"/> being edited.</param>
        /// <param name="pSelectionManager">
        /// The <see cref="SelectionManager"/> providing the current selection.
        /// </param>
        /// <param name="pMapBoundaryValidator">
        /// Validator used to keep wall edits within the play area bounds.
        /// </param>
        public EditorKeyboardController(MapPreview pMapPreview, SelectionManager pSelectionManager, MapBoundaryValidator pMapBoundaryValidator)
        {
            mMapPreview = pMapPreview;
            mSelectionManager = pSelectionManager;
            mMapBoundaryValidator = pMapBoundaryValidator;
        }

        /// <summary>
        /// Processes keyboard input and applies it to the currently selected object,
        /// if any is selected.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The method performs context-sensitive actions:
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// <c>Delete</c> removes the selected object from the map.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// For <see cref="Pickup"/> objects, number keys 1–4 toggle the pickup type.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// For <see cref="Tank"/> objects, left/right arrows rotate the tank.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// For <see cref="RectWall"/> objects, <c>Ctrl</c> toggles between rotate
        /// and scale mode; arrow keys then rotate or resize, with boundary checks.
        /// </description>
        /// </item>
        /// </list>
        /// If there is no selected object, or the object reports itself as not selected,
        /// the method returns without performing any action.
        /// </remarks>
        public void Update()
        {
            SceneObject selectedObject = mSelectionManager.GetSelectedObject();
            if (selectedObject == null || !selectedObject.GetIsSelected())
            {
                return;
            }

            // Delete
            if (InputManager.isKeyPressed(Keys.Delete))
            {
                mMapPreview.RemoveObject(selectedObject);
                mSelectionManager.SetSelectedObject(null);
                return;
            }

            // Cache last valid rect for walls
            if (selectedObject is RectWall)
            {
                mLastValidRect = selectedObject.mRectangle;
            }

            if (selectedObject is Pickup selectedPickup)
            {
                if (InputManager.isKeyPressed(Keys.D1))
                {
                    selectedPickup.TogglePickupType(PickupType.HEALTH);
                }
                if (InputManager.isKeyPressed(Keys.D2))
                {
                    selectedPickup.TogglePickupType(PickupType.EMP);
                }
                if (InputManager.isKeyPressed(Keys.D3))
                {
                    selectedPickup.TogglePickupType(PickupType.MINE);
                }
                if (InputManager.isKeyPressed(Keys.D4))
                {
                    selectedPickup.TogglePickupType(PickupType.BOUNCY_BULLET);
                }
            }

            HandleTankRotation(selectedObject as Tank);
            HandleWallTransform(selectedObject as RectWall);
        }

        /// <summary>
        /// Applies discrete left/right rotation to the specified tank using the
        /// arrow keys, if a tank is provided.
        /// </summary>
        /// <param name="pTank">The selected <see cref="Tank"/>, or <c>null</c>.</param>
        private static void HandleTankRotation(Tank pTank)
        {
            if (pTank == null)
            {
                return;
            }

            float rotationStep = MathHelper.ToRadians(15.0f);

            if (InputManager.isKeyPressed(Keys.Left))
            {
                pTank.Rotate(-rotationStep);
            }

            if (InputManager.isKeyPressed(Keys.Right))
            {
                pTank.Rotate(rotationStep);
            }
        }

        /// <summary>
        /// Handles transformation of a rectangular wall, delegating to rotation
        /// or scaling logic depending on its current mode.
        /// </summary>
        /// <param name="pWall">The selected <see cref="RectWall"/>, or <c>null</c>.</param>
        private void HandleWallTransform(RectWall pWall)
        {
            if (pWall == null)
            {
                return;
            }

            if (InputManager.isKeyPressed(Keys.LeftControl) || InputManager.isKeyPressed(Keys.RightControl))
            {
                pWall.SwitchRotationScaling();
            }

            if (pWall.GetIsRotating())
            {
                HandleWallRotation(pWall);
            }
            else
            {
                HandleWallScaling(pWall);
            }
        }

        /// <summary>
        /// Rotates a wall left or right in fixed angle steps, reverting the change
        /// if the new orientation moves it outside the playable area.
        /// </summary>
        /// <param name="pWall">The wall to rotate.</param>
        private void HandleWallRotation(RectWall pWall)
        {
            float rotationStep = MathHelper.ToRadians(15.0f);
            float previousRotation = pWall.mRotation;

            if (InputManager.isKeyPressed(Keys.Left))
            {
                pWall.Rotate(rotationStep);
                if (!mMapBoundaryValidator.IsWallWithinPlayArea(pWall))
                {
                    pWall.mRotation = previousRotation;
                }
                else
                {
                    mLastValidRect = pWall.mRectangle;
                }
            }

            if (InputManager.isKeyPressed(Keys.Right))
            {
                pWall.Rotate(-rotationStep);
                if (!mMapBoundaryValidator.IsWallWithinPlayArea(pWall))
                {
                    pWall.mRotation = previousRotation;
                }
                else
                {
                    mLastValidRect = pWall.mRectangle;
                }
            }
        }

        /// <summary>
        /// Scales a wall’s width and height using the arrow keys, restoring the
        /// previous rectangle if the new size extends beyond the play area.
        /// </summary>
        /// <param name="pWall">The wall to scale.</param>
        private void HandleWallScaling(RectWall pWall)
        {
            if (InputManager.isKeyPressed(Keys.Left))
            {
                pWall.ScaleWidth(0.75f);
            }
            if (InputManager.isKeyPressed(Keys.Right))
            {
                pWall.ScaleWidth(1.25f);
            }
            if (InputManager.isKeyPressed(Keys.Up))
            {
                pWall.ScaleHeight(1.25f);
            }
            if (InputManager.isKeyPressed(Keys.Down))
            {
                pWall.ScaleHeight(0.75f);
            }

            if (!mMapBoundaryValidator.IsWallWithinPlayArea(pWall))
            {
                pWall.SetWallRectangle(mLastValidRect);
            }
            else
            {
                mLastValidRect = pWall.mRectangle;
            }
        }
    }
}