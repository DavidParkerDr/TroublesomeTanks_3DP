using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TTMapEditor.Managers;

namespace TTMapEditor.Scenes
{
    /// <summary>
    /// Scene that allows the user to browse existing map files and select one
    /// to open in the editor. It renders a carousel-style UI that shows the
    /// previous, current and next map thumbnails and a title for the
    /// currently selected map.
    /// </summary>
    internal class MapSelectionScene : IScene
    {
        private readonly IGame mGameInstance;
        private readonly MainMenuScene mStartScene;

        // Static content loaded once for all instances of this scene
        private static readonly Texture2D mBackgroundTexture = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("background_01");
        private static readonly SpriteFont mSpriteFont = TTMapEditor.Instance().GetContentManager().Load<SpriteFont>("TitleFont");
        private static readonly Color BACKGROUND_COLOUR = DGS.Instance.GetColour("COLOUR_BACKGROUND");

        // Layout and state
        private Rectangle mBackgroundRectangle;
        private Vector2 mTitlePosition;
        private List<string> mMapFiles;
        private int mCurrentScrollPosition;

        // Thumbnail textures and layout
        private List<Texture2D> mThumbnailTextures = new List<Texture2D>();
        private Rectangle mCurrentRectangle;
        private Rectangle mPreviousRectangle;
        private Rectangle mNextRectangle;
        private int mThumbnailWidth;
        private int mThumbnailHeight;

        private string mMapDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapSelectionScene"/> class.
        /// Sets up layout based on the current viewport, scans the map directory
        /// for map files, generates thumbnails for each map and configures the
        /// rectangles used to render the thumbnail carousel.
        /// </summary>
        /// <param name="pStartScene">
        /// The main menu scene to return to when the user presses Escape.
        /// </param>
        public MapSelectionScene(MainMenuScene pStartScene)
        {
            mStartScene = pStartScene;
            mGameInstance = (TTMapEditor)TTMapEditor.Instance();
            mSpriteBatch = new SpriteBatch(mGameInstance.GetGraphicsDeviceManager().GraphicsDevice);
            mCurrentScrollPosition = 0;

            mMapDirectory = GetMapDirectory();

            int screenWidth = mGameInstance.GetGraphicsDeviceManager().GraphicsDevice.Viewport.Width;
            int screenHeight = mGameInstance.GetGraphicsDeviceManager().GraphicsDevice.Viewport.Height;

            // Full-screen background and title position
            mBackgroundRectangle = new Rectangle(0, 0, screenWidth, screenHeight);
            mTitlePosition = new Vector2(screenWidth / 2, screenHeight / 5);

            // Thumbnails are sized as a fraction of the screen
            mThumbnailWidth = screenWidth * 96 / 100 / 4;
            mThumbnailHeight = screenHeight * 73 / 100 / 4;

            // Ensure map directory exists
            if (!Directory.Exists(mMapDirectory))
            {
                Directory.CreateDirectory(mMapDirectory);
            }

            // Find all json map files under configured maps directory
            string[] filePaths = Directory.GetFiles(mMapDirectory, "*.json", SearchOption.AllDirectories);

            // Store relative paths (relative to MAP_DIRECTORY) so UI shows short names,
            // but always combine with MAP_DIRECTORY when reading/writing files.
            for (int i = 0; i < filePaths.Length; i++)
            {
                filePaths[i] = Path.GetRelativePath(mMapDirectory, filePaths[i]);
            }
            mMapFiles = new List<string>(filePaths);

            // Generate a thumbnail texture for each map file found.
            foreach (string mapFile in mMapFiles)
            {
                // mapFile is relative path; build full path for file operations
                string fullMapPath = Path.Combine(mMapDirectory, mapFile);

                string thumbnailFileName = Path.GetFileNameWithoutExtension(fullMapPath) + "_thumbnail.png";
                string thumbnailFile = Path.Combine(Path.GetDirectoryName(fullMapPath) ?? mMapDirectory, thumbnailFileName);

                // Always regenerate the thumbnail when the scene is created
                MakeThumbnailTextureFromMapFile(fullMapPath, thumbnailFile);
            }

            // Configure the rectangles for previous, current and next thumbnails
            mPreviousRectangle = new Rectangle(
                (screenWidth / 2) - (mThumbnailWidth / 2) - mThumbnailWidth,
                (screenHeight / 2) - (mThumbnailHeight / 2),
                mThumbnailWidth,
                mThumbnailHeight);

            mCurrentRectangle = new Rectangle(
                (screenWidth / 2) - (mThumbnailWidth),
                (screenHeight / 2) - (mThumbnailHeight),
                mThumbnailWidth * 2,
                mThumbnailHeight * 2);

            mNextRectangle = new Rectangle(
                (screenWidth / 2) - (mThumbnailWidth / 2) + mThumbnailWidth,
                (screenHeight / 2) - (mThumbnailHeight / 2),
                mThumbnailWidth,
                mThumbnailHeight);
        }

        /// <summary>
        /// Transitions to the <see cref="MapEditingScene"/> for the specified map.
        /// The provided map name is a relative path; this method resolves it to
        /// an absolute path in the configured maps directory.
        /// </summary>
        /// <param name="pMapName">Relative path of the selected map file.</param>
        private void SelectMap(string pMapName)
        {
            // pMapName is stored as relative path. Pass an absolute path to the editor.
            string fullMapPath = Path.Combine(mMapDirectory, pMapName);
            mGameInstance.GetSceneManager().Transition(new MapEditingScene(mStartScene, fullMapPath, false), true);
        }

        /// <summary>
        /// Renders the map selection UI, including the background, map name and
        /// the previous/current/next thumbnails. If no maps are available, a
        /// simple "No maps found" message is shown instead.
        /// </summary>
        /// <param name="pSeconds">Elapsed time since last draw (not used).</param>
        public override void Draw(float pSeconds)
        {
            mGameInstance.GetGraphicsDeviceManager().GraphicsDevice.Clear(Color.Black);

            // Display a simple message if there are no maps to choose from
            if (mMapFiles == null || mMapFiles.Count == 0)
            {
                // Safe fallback while map list is not yet populated
                string noMaps = "No maps found";
                mSpriteBatch.Begin();
                Vector2 pos = mTitlePosition - (mSpriteFont.MeasureString(noMaps) / 2);
                mSpriteBatch.DrawString(mSpriteFont, noMaps, pos, Color.White);
                mSpriteBatch.End();
                return;
            }

            mSpriteBatch.Begin();

            // Background
            mSpriteBatch.Draw(mBackgroundTexture, mBackgroundRectangle, Color.White);

            // Map title (file name without extension), centered at title position
            string mapName = mMapFiles[mCurrentScrollPosition].Substring(0, mMapFiles[mCurrentScrollPosition].Length - 5);
            Vector2 titlePos = mTitlePosition - (mSpriteFont.MeasureString(mapName) / 2);
            mSpriteBatch.DrawString(mSpriteFont, mapName, titlePos, Color.White);

            // Calculate the indices of the previous, current, and next thumbnails
            int prevIndex = (mCurrentScrollPosition - 1 + mMapFiles.Count) % mMapFiles.Count;
            int nextIndex = (mCurrentScrollPosition + 1) % mMapFiles.Count;

            // Draw the carousel thumbnails: previous (left), next (right), current (center enlarged)
            mSpriteBatch.Draw(mThumbnailTextures[prevIndex], mPreviousRectangle, Color.White);
            mSpriteBatch.Draw(mThumbnailTextures[nextIndex], mNextRectangle, Color.White);
            mSpriteBatch.Draw(mThumbnailTextures[mCurrentScrollPosition], mCurrentRectangle, Color.White);

            mSpriteBatch.End();
        }

        /// <summary>
        /// Handles input for navigating the map list and selecting a map.
        /// Left/Right arrows move through maps in a circular list; Enter
        /// opens the currently highlighted map; Escape returns to the main menu.
        /// </summary>
        /// <param name="pSeconds">Elapsed time since last update (not used).</param>
        public override void Update(float pSeconds)
        {
            InputManager.Update();
            Escape();

            if (InputManager.isKeyPressed(Keys.Left))
            {
                // Move selection to previous map (wrap at start)
                mCurrentScrollPosition = (mCurrentScrollPosition - 1 + mMapFiles.Count) % mMapFiles.Count;
            }
            if (InputManager.isKeyPressed(Keys.Right))
            {
                // Move selection to next map (wrap at end)
                mCurrentScrollPosition = (mCurrentScrollPosition + 1) % mMapFiles.Count;
            }
            if (InputManager.isKeyPressed(Keys.Enter))
            {
                // Open the selected map in the editor
                SelectMap(mMapFiles[mCurrentScrollPosition]);
            }
        }

        /// <summary>
        /// Builds a thumbnail for a single map by rendering a miniature version
        /// of the map layout (walls, tanks, pickups) to a render target. The
        /// rendered texture is saved as a PNG to <paramref name="thumbnailPath"/>
        /// and also stored in <see cref="mThumbnailTextures"/> for in-scene use.
        /// </summary>
        /// <param name="fullMapPath">Absolute path to the map JSON file.</param>
        /// <param name="thumbnailPath">Destination file path for the PNG thumbnail.</param>
        private void MakeThumbnailTextureFromMapFile(string fullMapPath, string thumbnailPath)
        {
            string mapContent = File.ReadAllText(fullMapPath);
            MapData mapData = JsonSerializer.Deserialize<MapData>(mapContent);

            int thumbnailWidth = mThumbnailWidth * 2;
            int thumbnailHeight = mThumbnailHeight * 2;
            RenderTarget2D renderTarget = new RenderTarget2D(mGameInstance.GetGraphicsDeviceManager().GraphicsDevice, thumbnailWidth, thumbnailHeight);

            // Draw into off-screen render target to build the thumbnail
            mGameInstance.GetGraphicsDeviceManager().GraphicsDevice.SetRenderTarget(renderTarget);
            mGameInstance.GetGraphicsDeviceManager().GraphicsDevice.Clear(Color.Transparent);

            Rectangle playArea = new Rectangle(0, 0, thumbnailWidth, thumbnailHeight);

            // First pass: draw background and outlines for all objects
            mSpriteBatch.Begin();

            Rectangle outlineRect = new Rectangle(0, 0, thumbnailWidth, thumbnailHeight);
            mSpriteBatch.Draw(mGameInstance.GetContentManager().Load<Texture2D>("block"), outlineRect, Color.Black);

            // Todo change to use a colour from the DGS
            Rectangle innerRect = new Rectangle(2, 2, thumbnailWidth - 4, thumbnailHeight - 4);
            mSpriteBatch.Draw(mGameInstance.GetContentManager().Load<Texture2D>("block"), innerRect, BACKGROUND_COLOUR);

            // Draw outlines for walls
            foreach (var wall in mapData.Walls)
            {
                Vector2 pos = new Vector2(float.Parse(wall.Position[0]), float.Parse(wall.Position[1]));
                Vector2 size = new Vector2(float.Parse(wall.Size[0]), float.Parse(wall.Size[1]));
                float rotationDegrees = 0f;
                float.TryParse(wall.Rotation, out rotationDegrees);
                float rotationRadians = rotationDegrees;
                Rectangle wallRect = GetRect(playArea, pos, size);
                DrawOutline(wallRect, pos, size, rotationRadians, wall.Texture);
            }

            // Draw outlines for tanks
            foreach (var tank in mapData.Tanks)
            {
                Rectangle tankRect = new Rectangle(
                    (int)(playArea.X + (playArea.Width * (float.Parse(tank.Position[0]) / 100))),
                    (int)(playArea.Y + (playArea.Height * (float.Parse(tank.Position[1]) / 100))),
                    10, 10
                );
                tankRect.X -= tankRect.Width / 2;
                tankRect.Y -= tankRect.Height / 2;
                DrawOutline(tankRect, "block");
            }

            // Draw outlines for pickups
            foreach (var pickup in mapData.Pickups)
            {
                Rectangle pickupRect = new Rectangle(
                    (int)(playArea.X + (playArea.Width * (float.Parse(pickup.Position[0]) / 100))),
                    (int)(playArea.Y + (playArea.Height * (float.Parse(pickup.Position[1]) / 100))),
                    10, 10
                );
                pickupRect.X -= pickupRect.Width / 2;
                pickupRect.Y -= pickupRect.Height / 2;
                DrawOutline(pickupRect, "circle");
            }

            mSpriteBatch.End();

            // Second pass: draw filled objects on top of the outlines
            mSpriteBatch.Begin();

            // Draw walls
            foreach (var wall in mapData.Walls)
            {
                // Wall position/size in map space (0–100 percent)
                Vector2 posPercent = new Vector2(float.Parse(wall.Position[0]), float.Parse(wall.Position[1]));
                Vector2 sizePercent = new Vector2(float.Parse(wall.Size[0]), float.Parse(wall.Size[1]));
                float rotationDegrees = 0f;
                float.TryParse(wall.Rotation, out rotationDegrees);
                float rotationRadians = rotationDegrees;

                // Rectangle in thumbnail pixel space
                Rectangle wallRect = GetRect(playArea, posPercent, sizePercent);

                // Center of the rect is where we draw the sprite
                Vector2 drawPosition = new Vector2(
                    wallRect.X + wallRect.Width / 2f,
                    wallRect.Y + wallRect.Height / 2f);

                Texture2D wallTexture = mGameInstance.GetContentManager().Load<Texture2D>(wall.Texture);
                Vector2 origin = new Vector2(wallTexture.Width / 2f, wallTexture.Height / 2f);

                // Scale texture to match the rectangle size
                Vector2 scale = new Vector2(
                    wallRect.Width / (float)wallTexture.Width,
                    wallRect.Height / (float)wallTexture.Height);

                // Todo change to use a colour from the DGS
                mSpriteBatch.Draw(
                    wallTexture,
                    drawPosition,
                    null,
                    Color.DarkGray,
                    rotationRadians,
                    origin,
                    scale,
                    SpriteEffects.None,
                    0f);
            }

            // Draw tanks
            foreach (var tank in mapData.Tanks)
            {
                Rectangle tankRect = new Rectangle(
                    (int)(playArea.X + (playArea.Width * (float.Parse(tank.Position[0]) / 100))),
                    (int)(playArea.Y + (playArea.Height * (float.Parse(tank.Position[1]) / 100))),
                    9, 9
                );
                tankRect.X -= tankRect.Width / 2;
                tankRect.Y -= tankRect.Height / 2;
                mSpriteBatch.Draw(mGameInstance.GetContentManager().Load<Texture2D>("block"), tankRect, Color.Blue);
            }

            // Draw pickups
            foreach (var pickup in mapData.Pickups)
            {
                Rectangle pickupRect = new Rectangle(
                    (int)(playArea.X + (playArea.Width * (float.Parse(pickup.Position[0]) / 100))),
                    (int)(playArea.Y + (playArea.Height * (float.Parse(pickup.Position[1]) / 100))),
                    9, 9
                );
                pickupRect.X -= pickupRect.Width / 2;
                pickupRect.Y -= pickupRect.Height / 2;
                mSpriteBatch.Draw(mGameInstance.GetContentManager().Load<Texture2D>("circle"), pickupRect, Color.Red);
            }

            mSpriteBatch.End();

            // Restore default render target (backbuffer)
            mGameInstance.GetGraphicsDeviceManager().GraphicsDevice.SetRenderTarget(null);

            Texture2D thumbnailTexture = renderTarget;

            // Ensure directory exists for thumbnail, then save
            string thumbnailDir = Path.GetDirectoryName(thumbnailPath) ?? mMapDirectory;
            Directory.CreateDirectory(thumbnailDir);
            using (FileStream stream = new FileStream(thumbnailPath, FileMode.Create))
            {
                thumbnailTexture.SaveAsPng(stream, thumbnailWidth, thumbnailHeight);
            }

            // Keep the texture in memory for this scene's carousel rendering
            mThumbnailTextures.Add(thumbnailTexture);
        }

        /// <summary>
        /// Converts percentage-based map coordinates into a pixel-space rectangle
        /// within the supplied play area.
        /// </summary>
        /// <param name="pPlayArea">Destination rectangle representing the thumbnail play area.</param>
        /// <param name="pPos">Object position as a percentage of the map (0–100).</param>
        /// <param name="pSize">Object size as a percentage of the map (0–100).</param>
        /// <returns>Rectangle in pixel coordinates suitable for thumbnail rendering.</returns>
        private Rectangle GetRect(Rectangle pPlayArea, Vector2 pPos, Vector2 pSize)
        {
            return new Rectangle(
                (int)(pPlayArea.X + (pPlayArea.Width * (pPos.X / 100.0))),
                (int)(pPlayArea.Y + (pPlayArea.Height * (pPos.Y / 100.0))),
                (int)(pPlayArea.Width * (pSize.X / 100.0)),
                (int)(pPlayArea.Height * (pSize.Y / 100.0)));
        }

        /// <summary>
        /// Draws a simple rectangular outline around the target rectangle using
        /// the specified texture, expanded slightly by a fixed offset.
        /// </summary>
        /// <param name="pRect">Rectangle to outline.</param>
        /// <param name="pTextureName">Name of the texture asset to draw.</param>
        private void DrawOutline(Rectangle pRect, string pTextureName)
        {
            int offset = 2;
            Texture2D texture = mGameInstance.GetContentManager().Load<Texture2D>(pTextureName);
            mSpriteBatch.Draw(texture, new Rectangle(pRect.X - offset, pRect.Y - offset, pRect.Width + (2 * offset), pRect.Height + (offset * 2)), Color.Black);
        }

        /// <summary>
        /// Draws a scaled and rotated outline for a wall-like object using the
        /// same texture as the object, enlarged slightly to act as a border.
        /// </summary>
        /// <param name="pRectangle">Target rectangle in thumbnail space.</param>
        /// <param name="pPosition">Wall position in map space (percentage, not used directly here).</param>
        /// <param name="pSize">Wall size in map space (percentage, not used directly here).</param>
        /// <param name="pRotation">Rotation of the wall in radians.</param>
        /// <param name="pTextureName">Name of the wall texture asset.</param>
        private void DrawOutline(Rectangle pRectangle, Vector2 pPosition, Vector2 pSize, float pRotation, string pTextureName)
        {
            int offset = 2;
            Texture2D texture = mGameInstance.GetContentManager().Load<Texture2D>(pTextureName);

            // Center of the rectangle in thumbnail/render-target space
            Vector2 center = new Vector2(
                pRectangle.X + pRectangle.Width / 2f,
                pRectangle.Y + pRectangle.Height / 2f);

            // Origin is the texture center
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);

            // Scale so that the sprite covers the rect plus outline offset
            Vector2 scale = new Vector2(
                (pRectangle.Width + offset * 2) / (float)texture.Width,
                (pRectangle.Height + offset * 2) / (float)texture.Height);

            // Todo change to use a colour from the DGS
            mSpriteBatch.Draw(
                texture,
                center,
                null,
                Color.Black,
                pRotation,
                origin,
                scale,
                SpriteEffects.None,
                0f);
        }

        /// <summary>
        /// Handles the Escape key to return from this scene back to the main
        /// menu scene.
        /// </summary>
        public override void Escape()
        {
            if (InputManager.isKeyPressed(Keys.Escape))
            {
                mGameInstance.GetSceneManager().Transition(mStartScene);
            }
        }

        //Gets a relative root of the maps folder that works in both debug and release builds
        private string GetMapDirectory()
        {
            string currentDir = Environment.CurrentDirectory;
            string mapDirectory = Path.GetFullPath(Path.Combine(currentDir, "..", "..", "..","..","..",".."));
#if DEBUG
            mapDirectory = Path.Combine(mapDirectory, "Tankontroller", "bin", "Debug", "net6.0", "Maps");
            return mapDirectory;
#endif
            mapDirectory = Path.Combine(mapDirectory, "Tankontroller", "bin", "Release", "net6.0", "Maps");
            return mapDirectory;
        }
    }
}