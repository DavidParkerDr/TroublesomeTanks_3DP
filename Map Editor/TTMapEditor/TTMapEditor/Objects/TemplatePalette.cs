using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TTMapEditor.Managers;
using TTMapEditor.Maps;

namespace TTMapEditor.Objects
{
    /// <summary>
    /// Palette area that exposes draggable templates for creating walls, tanks and pickups.
    /// Handles mouse interaction for dragging these templates onto the map preview and
    /// instantiating new map objects when a drag ends inside the valid play area.
    /// </summary>
    internal class TemplatePalette
    {
        private readonly SpriteFont mTitleFont;
        private readonly Texture2D mPixelTexture;
        private readonly Texture2D mCircleTexture;

        private readonly MapPreview mMapPreview;
        private readonly MapBoundaryValidator mMapBoundaryValidator;

        private readonly RectWall mWallTemplateVisual;
        private readonly Tank mTankTemplateVisual;
        private readonly Pickup mPickupTemplateVisual;

        private readonly DraggableTemplate<RectWall> mWallTemplate;
        private readonly DraggableTemplate<Tank> mTankTemplate;
        private readonly DraggableTemplate<Pickup> mPickupTemplate;

        private readonly int mMaxTanks;

        /// <summary>
        /// True if any of the palette templates is currently being dragged.
        /// </summary>
        public bool IsDraggingAny
        {
            get
            {
                return mWallTemplate.mIsDragging || mTankTemplate.mIsDragging || mPickupTemplate.mIsDragging;
            }
        }

        /// <summary>
        /// Creates a new template palette positioned relative to the viewport width.
        /// Sets up visual rectangles for wall, tank and pickup templates and
        /// configures draggable wrappers around each template visual.
        /// </summary>
        /// <param name="pTitleFont">Font used for template labels.</param>
        /// <param name="pPixelTexture">Texture used for wall and tank rectangles.</param>
        /// <param name="pCircleTexture">Texture used for pickup circles.</param>
        /// <param name="pMapPreview">Map preview that receives instantiated objects on drop.</param>
        /// <param name="pBoundaryValidatorm">Validator used to ensure drops stay within the play area.</param>
        /// <param name="pViewPortWidth">Current viewport width used to position palette items.</param>
        /// <param name="pMaxTanks">Maximum number of tanks allowed in the map.</param>
        public TemplatePalette(SpriteFont pTitleFont, Texture2D pPixelTexture, Texture2D pCircleTexture, MapPreview pMapPreview, MapBoundaryValidator pBoundaryValidatorm, int pViewPortWidth, int pMaxTanks)
        {
            mTitleFont = pTitleFont;
            mPixelTexture = pPixelTexture;
            mCircleTexture = pCircleTexture;
            mMapPreview = pMapPreview;
            mMapBoundaryValidator = pBoundaryValidatorm;
            mMaxTanks = pMaxTanks;

            // Palette layout: position templates horizontally near the right side of the viewport.
            mWallTemplateVisual = new RectWall(mPixelTexture, new Rectangle(pViewPortWidth - 5 * pViewPortWidth / 8, 200, 200, 50));
            mTankTemplateVisual = new Tank(mPixelTexture, new Rectangle(pViewPortWidth - pViewPortWidth / 8, 200, 14, 14));
            mPickupTemplateVisual = new Pickup(mCircleTexture, new Rectangle(pViewPortWidth - pViewPortWidth / 3, 200, 14, 14));

            // Draggable wrappers operate on the same rectangles as the visuals.
            mWallTemplate = new DraggableTemplate<RectWall>(mWallTemplateVisual);
            mTankTemplate = new DraggableTemplate<Tank>(mTankTemplateVisual);
            mPickupTemplate = new DraggableTemplate<Pickup>(mPickupTemplateVisual);
        }

        /// <summary>
        /// Updates all draggable templates based on the current mouse position and input state.
        /// Starts, continues, or ends drags and creates new map objects on successful drop.
        /// </summary>
        /// <param name="pMousePosition">Current mouse position in screen coordinates.</param>
        public void Update(Vector2 pMousePosition)
        {
            HandleWallTemplateDragging(pMousePosition);
            HandleTankTemplateDragging(pMousePosition);
            HandlePickupTemplateDragging(pMousePosition);
        }

        /// <summary>
        /// Draws all template visuals and their labels in the palette.
        /// </summary>
        /// <param name="pSpriteBatch">Sprite batch used for rendering.</param>
        public void Draw(SpriteBatch pSpriteBatch)
        {
            DrawWallTemplate(pSpriteBatch);
            DrawTankTemplate(pSpriteBatch);
            DrawPickupTemplate(pSpriteBatch);
        }

        /// <summary>
        /// Handles drag lifecycle for the wall template:
        /// - Starts drag when clicking on the wall visual.
        /// - Updates position while mouse is held.
        /// - On release, instantiates a new wall if dropped inside the play area.
        /// </summary>
        private void HandleWallTemplateDragging(Vector2 pMousePosition)
        {
            // Start drag if mouse click begins on the wall template.
            if (!mWallTemplate.mIsDragging && mWallTemplate.mTemplate.IsPointWithin(pMousePosition) && InputManager.isLeftMouseClicked())
            {
                mWallTemplate.BeginDrag(pMousePosition);
            }

            // While dragging and button is held, update template position.
            if (mWallTemplate.mIsDragging && !InputManager.isLeftMouseReleased())
            {
                mWallTemplate.Update(pMousePosition);
            }

            // On mouse release: stop dragging, spawn wall if within bounds, then reset visual.
            if (mWallTemplate.mIsDragging && InputManager.isLeftMouseReleased())
            {
                mWallTemplate.EndDrag(pResetToOriginal: false);

                if (mMapBoundaryValidator.IsRectWithinPlayArea(mWallTemplate.mTemplate.mRectangle))
                {
                    RectWall newWall = new RectWall(mPixelTexture, mWallTemplate.mTemplate.mRectangle);
                    mMapPreview.AddObject(newWall);
                }

                mWallTemplate.Reset();
            }
        }

        /// <summary>
        /// Handles drag lifecycle for the tank template.
        /// Additionally enforces the maximum number of tanks allowed in the map
        /// before instantiating a new tank on drop.
        /// </summary>
        private void HandleTankTemplateDragging(Vector2 pMousePosition)
        {
            // Start drag if mouse click begins on the tank template.
            if (!mTankTemplate.mIsDragging && mTankTemplate.mTemplate.IsPointWithin(pMousePosition) && InputManager.isLeftMouseClicked())
            {
                mTankTemplate.BeginDrag(pMousePosition);
            }

            // While dragging and button is held, update template position.
            if (mTankTemplate.mIsDragging && !InputManager.isLeftMouseReleased())
            {
                mTankTemplate.Update(pMousePosition);
            }

            // On mouse release: stop dragging, spawn tank if within bounds and under limit, then reset visual.
            if (mTankTemplate.mIsDragging && InputManager.isLeftMouseReleased())
            {
                mTankTemplate.EndDrag(pResetToOriginal: false);

                if (mMapBoundaryValidator.IsRectWithinPlayArea(mTankTemplate.mTemplate.mRectangle) && mMapPreview.GetTanks().Count < mMaxTanks)
                {
                    Tank newTank = new Tank(mPixelTexture, mTankTemplate.mTemplate.mRectangle);
                    mMapPreview.AddObject(newTank);
                }

                mTankTemplate.Reset();
            }
        }

        /// <summary>
        /// Handles drag lifecycle for the pickup template.
        /// Instantiates a new pickup if the drop location is inside the play area.
        /// </summary>
        private void HandlePickupTemplateDragging(Vector2 pMousePosition)
        {
            // Start drag if mouse click begins on the pickup template.
            if (!mPickupTemplate.mIsDragging && mPickupTemplate.mTemplate.IsPointWithin(pMousePosition) && InputManager.isLeftMouseClicked())
            {
                mPickupTemplate.BeginDrag(pMousePosition);
            }

            // While dragging and button is held, update template position.
            if (mPickupTemplate.mIsDragging && !InputManager.isLeftMouseReleased())
            {
                mPickupTemplate.Update(pMousePosition);
            }

            // On mouse release: stop dragging, spawn pickup if within bounds, then reset visual.
            if (mPickupTemplate.mIsDragging && InputManager.isLeftMouseReleased())
            {
                mPickupTemplate.EndDrag(pResetToOriginal: false);

                if (mMapBoundaryValidator.IsRectWithinPlayArea(mPickupTemplate.mTemplate.mRectangle))
                {
                    Pickup newPickup = new Pickup(mCircleTexture, mPickupTemplate.mTemplate.mRectangle);
                    mMapPreview.AddObject(newPickup);
                }

                mPickupTemplate.Reset();
            }
        }

        /// <summary>
        /// Draws the wall template visual and its label in the palette.
        /// </summary>
        private void DrawWallTemplate(SpriteBatch pSpriteBatch)
        {
            mWallTemplateVisual.DrawOutline(pSpriteBatch);
            mWallTemplateVisual.Draw(pSpriteBatch);

            float labelWidth = mTitleFont.MeasureString("Wall").X;
            float labelHeight = mTitleFont.MeasureString("Wall").Y;

            Vector2 labelPosition = new Vector2(
                mWallTemplateVisual.mRectangle.X + labelWidth / 4f,
                mWallTemplateVisual.mRectangle.Y - labelHeight);

            pSpriteBatch.DrawString(mTitleFont, "Wall", labelPosition, Color.Black);
        }

        /// <summary>
        /// Draws the tank template visual and its label in the palette.
        /// </summary>
        private void DrawTankTemplate(SpriteBatch pSpriteBatch)
        {
            mTankTemplateVisual.DrawOutline(pSpriteBatch);
            mTankTemplateVisual.Draw(pSpriteBatch);

            float labelWidth = mTitleFont.MeasureString("Tank").X;
            float labelHeight = mTitleFont.MeasureString("Tank").Y;

            Vector2 labelPosition = new Vector2(
                mTankTemplateVisual.mRectangle.X - labelWidth / 2f,
                mTankTemplateVisual.mRectangle.Y - labelHeight);

            pSpriteBatch.DrawString(mTitleFont, "Tank", labelPosition, Color.Black);
        }

        /// <summary>
        /// Draws the pickup template visual and its label in the palette.
        /// </summary>
        private void DrawPickupTemplate(SpriteBatch pSpriteBatch)
        {
            mPickupTemplateVisual.DrawOutline(pSpriteBatch);
            mPickupTemplateVisual.Draw(pSpriteBatch);

            float labelWidth = mTitleFont.MeasureString("Pickup").X;
            float labelHeight = mTitleFont.MeasureString("Pickup").Y;

            Vector2 labelPosition = new Vector2(
                mPickupTemplateVisual.mRectangle.X - labelWidth / 2f,
                mPickupTemplateVisual.mRectangle.Y - labelHeight);

            pSpriteBatch.DrawString(mTitleFont, "Pickup", labelPosition, Color.Black);
        }
    }
}
