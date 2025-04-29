using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Tankontroller.Controller;
using Tankontroller.GUI;
using static Tankontroller.MapManager;

namespace Tankontroller.Scenes
{
    public class LevelSelectionScene : IScene
    {
        private static readonly bool ONLY_KEYBOARD_ON_MAP_SELECT = DGS.Instance.GetBool("ONLY_KEYBOARD_ON_MAP_SELECT");
        private static readonly Texture2D mBackgroundTexture = Tankontroller.Instance().CM().Load<Texture2D>("background_01");
        private static readonly SpriteFont mSpriteFont = Tankontroller.Instance().CM().Load<SpriteFont>("TitleFont");
        private Rectangle mBackgroundRectangle;
        private Vector2 mTitlePos;
        private List<string> mMapFiles;
        private Tankontroller mGameInstance;
        private int mCurrentScrollPosition;
        private MainMenuScene mStartScene;

        private List<Texture2D> mThumbnailTextures = new List<Texture2D>();
        Rectangle currentRect;
        Rectangle prevRect;
        Rectangle nextRect;
        int mThumbnailWidth;
        int mThumbnailHeight;

        public LevelSelectionScene(MainMenuScene startScene)
        {
            mStartScene = startScene;
            mGameInstance = (Tankontroller)Tankontroller.Instance();
            spriteBatch = new SpriteBatch(mGameInstance.GDM().GraphicsDevice);
            int screenWidth = mGameInstance.GDM().GraphicsDevice.Viewport.Width;
            int screenHeight = mGameInstance.GDM().GraphicsDevice.Viewport.Height;

            mBackgroundRectangle = new Rectangle(0, 0, screenWidth, screenHeight);
            mTitlePos = new Vector2(screenWidth / 2, screenHeight / 5);
            mThumbnailWidth = screenWidth * 96 / 100 / 4;
            mThumbnailHeight = screenHeight * 73 / 100 / 4;

            // Get the list of map files
            string mapsDirectory = Path.Combine(Environment.CurrentDirectory, "Maps");
            if (!Directory.Exists(mapsDirectory))
            {
                Directory.CreateDirectory(mapsDirectory);
            }
            string[] filePaths = Directory.GetFiles(mapsDirectory, "*.json", SearchOption.AllDirectories);
            for (int i = 0; i < filePaths.Length; i++)
            {
                filePaths[i] = filePaths[i].Replace(mapsDirectory + "\\", "");
            }
            mMapFiles = new List<string>(filePaths);

            // Load thumbnail textures for each map
            foreach (string mapFile in mMapFiles)
            {
                string thumbnailFile = mapFile.Replace(".json", "_thumbnail.png");
                if (!File.Exists(thumbnailFile))
                {
                    MakeThumbnailTextureFromMapFile(mapFile);
                }
                else
                {
                    using (FileStream fileStream = new FileStream(thumbnailFile, FileMode.Open))
                    {
                        mThumbnailTextures.Add(Texture2D.FromStream(mGameInstance.GDM().GraphicsDevice, fileStream));
                    }
                }
            }

            // Calculate positions and sizes for the thumbnails
            prevRect = new Rectangle(
                (screenWidth / 2) - (mThumbnailWidth / 2) - mThumbnailWidth,
                (screenHeight / 2) - (mThumbnailHeight / 2),
                mThumbnailWidth,
                mThumbnailHeight
            );

            currentRect = new Rectangle(
                (screenWidth / 2) - (mThumbnailWidth),
                (screenHeight / 2) - (mThumbnailHeight),
                mThumbnailWidth * 2,
                mThumbnailHeight * 2
            );

            nextRect = new Rectangle(
                (screenWidth / 2) - (mThumbnailWidth / 2) + mThumbnailWidth,
                (screenHeight / 2) - (mThumbnailHeight / 2),
                mThumbnailWidth,
                mThumbnailHeight
            );

            mCurrentScrollPosition = 0;
        }

        private void SelectMap(string mapFile)
        {
            mStartScene.SetDefaultMapFile(mapFile);
            mGameInstance.SM().Transition(mStartScene, true);
        }

        public override void Update(float pSeconds)
        {
            Escape();
            mGameInstance.GetControllerManager().DetectControllers();

            foreach (IController controller in mGameInstance.GetControllerManager().GetControllers())
            {
                controller.UpdateController();

                if (ONLY_KEYBOARD_ON_MAP_SELECT && !(controller is KeyboardController))
                {
                    continue; // If enabled in DSG skip any controller that isn't a keyboard
                }

                if (controller.IsPressed(Control.TURRET_LEFT) && !controller.WasPressed(Control.TURRET_LEFT))
                {
                    mCurrentScrollPosition = (mCurrentScrollPosition - 1 + mMapFiles.Count) % mMapFiles.Count;
                }
                if (controller.IsPressed(Control.TURRET_RIGHT) && !controller.WasPressed(Control.TURRET_RIGHT))
                {
                    mCurrentScrollPosition = (mCurrentScrollPosition + 1) % mMapFiles.Count;
                }
                if (controller.IsPressed(Control.FIRE) && !controller.WasPressed(Control.FIRE) ||
                    controller.IsPressed(Control.RECHARGE) && !controller.WasPressed(Control.RECHARGE))
                {
                    SelectMap(mMapFiles[mCurrentScrollPosition]);
                }
            }
        }

        public override void Draw(float pSeconds)
        {
            mGameInstance.GDM().GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            spriteBatch.Draw(mBackgroundTexture, mBackgroundRectangle, Color.White);
            string mapName = mMapFiles[mCurrentScrollPosition].Substring(0, mMapFiles[mCurrentScrollPosition].Length - 5);
            Vector2 titlePos = mTitlePos - (mSpriteFont.MeasureString(mapName) / 2);
            //spriteBatch.DrawString(mSpriteFont, mapName, titlePos, Color.White, 0.0f, Vector2.One, 2.0f, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(mSpriteFont, mapName, titlePos, Color.White);
            // Calculate the indices of the previous, current, and next thumbnails
            int prevIndex = (mCurrentScrollPosition - 1 + mMapFiles.Count) % mMapFiles.Count;
            int nextIndex = (mCurrentScrollPosition + 1) % mMapFiles.Count;

            spriteBatch.Draw(mThumbnailTextures[prevIndex], prevRect, Color.White);
            spriteBatch.Draw(mThumbnailTextures[nextIndex], nextRect, Color.White);
            spriteBatch.Draw(mThumbnailTextures[mCurrentScrollPosition], currentRect, Color.White);
            spriteBatch.End();
        }

        void MakeThumbnailTextureFromMapFile(string pMapFile)
        {
            pMapFile = "Maps\\" + pMapFile;
            string mapContent = File.ReadAllText(pMapFile);
            MapData mapData = JsonSerializer.Deserialize<MapData>(mapContent);

            int thumbnailWidth = mThumbnailWidth * 2;
            int thumbnailHeight = mThumbnailHeight * 2;
            RenderTarget2D renderTarget = new RenderTarget2D(mGameInstance.GDM().GraphicsDevice, thumbnailWidth, thumbnailHeight);

            mGameInstance.GDM().GraphicsDevice.SetRenderTarget(renderTarget);
            mGameInstance.GDM().GraphicsDevice.Clear(Color.Transparent);

            int screenWidth = Tankontroller.Instance().GDM().GraphicsDevice.Viewport.Width;
            int screenHeight = Tankontroller.Instance().GDM().GraphicsDevice.Viewport.Height;
            Rectangle playArea = new Rectangle(0, 0, thumbnailWidth, thumbnailHeight);

            spriteBatch.Begin();

            // Draw black outline
            Rectangle outlineRect = new Rectangle(0, 0, thumbnailWidth, thumbnailHeight);
            spriteBatch.Draw(mGameInstance.CM().Load<Texture2D>("block"), outlineRect, Color.Black);

            // Draw inner rectangle with map background color
            Rectangle innerRect = new Rectangle(2, 2, thumbnailWidth - 4, thumbnailHeight - 4);
            spriteBatch.Draw(mGameInstance.CM().Load<Texture2D>("block"), innerRect, DGS.Instance.GetColour("COLOUR_GROUND"));

            // Draw outlines for walls
            foreach (var wall in mapData.Walls)
            {
                Vector2 pos = new Vector2(float.Parse(wall.Position[0]), float.Parse(wall.Position[1]));
                Vector2 size = new Vector2(float.Parse(wall.Size[0]), float.Parse(wall.Size[1]));
                Rectangle wallRect = GetRect(playArea, pos, size);
                DrawOutline(wallRect, wall.Texture);
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

            spriteBatch.End();

            spriteBatch.Begin();

            // Draw walls
            foreach (var wall in mapData.Walls)
            {
                Vector2 pos = new Vector2(float.Parse(wall.Position[0]), float.Parse(wall.Position[1]));
                Vector2 size = new Vector2(float.Parse(wall.Size[0]), float.Parse(wall.Size[1]));
                Rectangle wallRect = GetRect(playArea, pos, size);
                spriteBatch.Draw(mGameInstance.CM().Load<Texture2D>(wall.Texture), wallRect, DGS.Instance.GetColour("COLOUR_WALLS"));
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
                spriteBatch.Draw(mGameInstance.CM().Load<Texture2D>("block"), tankRect, Color.Blue);
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
                spriteBatch.Draw(mGameInstance.CM().Load<Texture2D>("circle"), pickupRect, Color.Red);
            }

            spriteBatch.End();

            mGameInstance.GDM().GraphicsDevice.SetRenderTarget(null);

            Texture2D thumbnailTexture = renderTarget;

            using (FileStream stream = new FileStream(pMapFile.Replace(".json", "_thumbnail.png"), FileMode.Create))
            {
                thumbnailTexture.SaveAsPng(stream, thumbnailWidth, thumbnailHeight);
            }

            //add the thumbnail texture to the list
            mThumbnailTextures.Add(thumbnailTexture);
        }

        Rectangle GetRect(Rectangle pPlayArea, Vector2 pPos, Vector2 pSize)
        {
            return new Rectangle(
                   (int)(pPlayArea.X + (pPlayArea.Width * (pPos.X / 100.0))),
                   (int)(pPlayArea.Y + (pPlayArea.Height * (pPos.Y / 100.0))),
                   (int)(pPlayArea.Width * (pSize.X / 100.0)),
                   (int)(pPlayArea.Height * (pSize.Y / 100.0))
               );
        }

        void DrawOutline(Rectangle rect, string textureName)
        {
            int offset = 2; // Outline thickness
            Texture2D texture = mGameInstance.CM().Load<Texture2D>(textureName);
            spriteBatch.Draw(texture, new Rectangle(rect.X - offset, rect.Y - offset, rect.Width + (offset * 2), rect.Height + (offset * 2)), Color.Black);
        }

        public override void Escape()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                mGameInstance.SM().Transition(mStartScene, true);
            }
        }
    }
}
