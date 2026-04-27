using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTMapEditor.Scenes
{
    /// <summary>
    /// Scene used to visually transition between two other scenes.
    /// Renders the previous and next scenes to textures and animates the
    /// next scene sliding in from the top until it replaces the previous one.
    /// </summary>
    internal class TransitionScene : IScene
    {

        GraphicsDevice mGraphicsDevice;
        RenderTarget2D mPreviousTexture = null;
        RenderTarget2D mNextTexture = null;
        Rectangle mRectangle;
        IScene mNextScene;
        Vector2 mNextPosition = new Vector2(
            0,
            -(TTMapEditor.Instance().GetGraphicsDeviceManager().GraphicsDevice.Viewport.Height));
        Vector2 mVelocity = new Vector2(0, 0);
        Vector2 mAcceleration = new Vector2(0, 1);

        /// <summary>
        /// Creates a new transition between two scenes.
        /// Captures both scenes into textures so they can be animated during the transition.
        /// </summary>
        /// <param name="pPreviousScene">The scene currently on screen.</param>
        /// <param name="pNextScene">The scene that should appear after the transition.</param>
        public TransitionScene(IScene pPreviousScene, IScene pNextScene)
        {
            mGraphicsDevice = TTMapEditor.Instance().GetGraphicsDeviceManager().GraphicsDevice;
            mNextScene = pNextScene;
            mSpriteBatch = new SpriteBatch(mGraphicsDevice);

            // Use the display's current resolution as the drawing rectangle.
            mRectangle = new Rectangle(
                0,
                0,
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);

            // Pre-render both scenes into textures so we can animate them efficiently.
            mPreviousTexture = GenerateSceneTexture(pPreviousScene);
            mNextTexture = GenerateSceneTexture(pNextScene);
        }

        /// <summary>
        /// Renders a scene to an off-screen render target and returns the resulting texture.
        /// </summary>
        /// <param name="pScene">Scene to render into a texture.</param>
        /// <returns>Render target containing a snapshot of the scene.</returns>
        public RenderTarget2D GenerateSceneTexture(IScene pScene)
        {
            RenderTarget2D output = new RenderTarget2D(
                mGraphicsDevice,
                mGraphicsDevice.PresentationParameters.BackBufferWidth,
                mGraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                mGraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);

            // Draw the scene into the render target.
            mGraphicsDevice.SetRenderTarget(output);
            mGraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };

            mSpriteBatch.Begin(
                SpriteSortMode.BackToFront,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullCounterClockwise);

            // Draw at time = 0 since we just need a static snapshot.
            pScene.Draw(0);
            mSpriteBatch.End();

            // Restore default render target (back buffer).
            mGraphicsDevice.SetRenderTarget(null);

            return output;
        }

        /// <summary>
        /// Draws the transition frame, showing the previous scene and the
        /// sliding next scene texture.
        /// </summary>
        /// <param name="pSeconds">Elapsed time in seconds since last frame.</param>
        public override void Draw(float pSeconds)
        {
            mGraphicsDevice.Clear(Color.Black);

            mSpriteBatch.Begin();
            // Draw the previous scene as the background.
            mSpriteBatch.Draw(mPreviousTexture, mRectangle, Color.White);
            // Draw the next scene, offset by its current animated position.
            mSpriteBatch.Draw(mNextTexture, mNextPosition, mRectangle, Color.White);
            mSpriteBatch.End();
        }

        /// <summary>
        /// Updates the transition animation and swaps scenes when complete.
        /// </summary>
        /// <param name="pSeconds">Elapsed time in seconds since last update.</param>
        public override void Update(float pSeconds)
        {
            IGame gameInstance = TTMapEditor.Instance();

            // Basic physics-style motion: v += a, pos += v.
            mVelocity += mAcceleration;
            mNextPosition += mVelocity;

            // Once the next scene has fully slid into view (Y >= 0),
            // remove this transition scene and ensure the next scene is on top.
            if (mNextPosition.Y > 0)
            {
                gameInstance.GetSceneManager().Pop();

                if (mNextScene != gameInstance.GetSceneManager().Top)
                {
                    gameInstance.GetSceneManager().Push(mNextScene);
                }
            }
        }
    }
}
