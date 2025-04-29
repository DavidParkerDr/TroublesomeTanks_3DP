using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tankontroller.Scenes
{
    //-------------------------------------------------------------------------------------------------
    // This class is used to transition one scene into another scene. It is a drop down
    // transition. An example of how it functions is the main menu where the main menu texture over the
    // intro texture and slowly replaces it, frame by frame, with a larger part of the main menu texture
    // Once it has been fully replaced, the scene changes to the main menu via the scene manager.
    //-------------------------------------------------------------------------------------------------
    public class TransitionScene : IScene
    {
        GraphicsDevice mGraphicsDevice;
        RenderTarget2D mPreviousTexture = null;
        RenderTarget2D mNextTexture = null;
        Rectangle mRectangle;
        IScene mNextScene;
        Vector2 mNextPosition = new Vector2(0, -(Tankontroller.Instance().GDM().GraphicsDevice.Viewport.Height));
        Vector2 mVelocity = new Vector2(0, 0);
        Vector2 mAcceleration = new Vector2(0, 1);

        public TransitionScene(IScene pPreviousScene, IScene pNextScene)
        {
            mGraphicsDevice = Tankontroller.Instance().GDM().GraphicsDevice;
            mNextScene = pNextScene;
            spriteBatch = new SpriteBatch(mGraphicsDevice);
            mRectangle = new Rectangle(0, 0, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
            mPreviousTexture = GenerateSceneTexture(pPreviousScene);
            mNextTexture = GenerateSceneTexture(pNextScene);
        }

        public RenderTarget2D GenerateSceneTexture(IScene pScene)
        {
            RenderTarget2D output = new RenderTarget2D(mGraphicsDevice, mGraphicsDevice.PresentationParameters.BackBufferWidth, mGraphicsDevice.PresentationParameters.BackBufferHeight, false, mGraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            mGraphicsDevice.SetRenderTarget(output);
            mGraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullCounterClockwise);
            pScene.Draw(0);
            spriteBatch.End();
            mGraphicsDevice.SetRenderTarget(null);
            return output;
        }

        public override void Update(float pSeconds)
        {
            IGame gameInstance = Tankontroller.Instance();
            mVelocity += mAcceleration;
            mNextPosition += mVelocity;
            if (mNextPosition.Y > 0)
            {
                gameInstance.SM().Pop();

                if (mNextScene != gameInstance.SM().Top)
                {
                    gameInstance.SM().Push(mNextScene);
                }
            }
        }

        public override void Draw(float pSeconds)
        {
            mGraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            spriteBatch.Draw(mPreviousTexture, mRectangle, Color.White);
            spriteBatch.Draw(mNextTexture, mNextPosition, mRectangle, Color.White);
            spriteBatch.End();
        }
    }
}
