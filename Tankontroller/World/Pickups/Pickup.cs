using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;
using Tankontroller.Managers;
using Tankontroller.Utilities;
using Tankontroller.World.Shapes;
using Tankontroller.World.WorldObject;

namespace Tankontroller.World.Pickups
{
    public abstract class Pickup : ICollidable
    {
        public Transform Transform { get; protected set; } = new Transform();
        public CollisionShape CollisionShape => RectangleShape;
        public RectangleAxisAlignedShape RectangleShape { get; protected set; } = null;
        public Texture2D Texture { get; protected set; }


        public int screenWidth = Tankontroller.Instance().GDM().GraphicsDevice.Viewport.Width;
        public int screenHeight = Tankontroller.Instance().GDM().GraphicsDevice.Viewport.Height;
        public float mScalerX;
        public float mScalerY;

        protected Pickup(Texture2D pTexture, Rectangle pRectangle, Vector2 pPosition)
        {
            mScalerX = ((float)screenWidth / 200f);
            mScalerY = ((float)screenHeight / 200f);
            Transform.Position = pPosition;
            RectangleShape = new RectangleAxisAlignedShape(Transform, pRectangle.Size.ToVector2() * Tankontroller.Instance().ScaleFactor());
            Texture = pTexture;
        }

        public virtual void Draw(SpriteBatch pSpriteBatch)
        {
            Rectangle drawRectangle = RectangleShape.ToRectangle();
            pSpriteBatch.Draw(Texture, drawRectangle, Color.White);
            if (CollisionManager.DRAW_COLLISION_SHAPES)
            {
                DrawUtilities.DrawRectangle(pSpriteBatch, drawRectangle, Color.RosyBrown, Transform.Rotation, Transform.Position, 1.0f);
            }
        }

        public abstract void TriggerEffect(Tank pTank);
    }
}
