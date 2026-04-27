using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.World.Shapes;
using Tankontroller.World.WorldObject;

namespace Tankontroller.World
{
    public class RectWall : ICollidable
    {
        private static readonly Color COLOUR = DGS.Instance.GetColour("COLOUR_WALLS");

        public Transform Transform { get; private set; } = new Transform();
        public CollisionShape CollisionShape => RectangleShape;
        public RectangleOrientedShape RectangleShape { get; private set; } = null;
        private RectangleOrientedShape mOutlineShape = null;

        private Texture2D mTexture;

        public RectWall(Transform pTransform, Vector2 pSize, Texture2D pTexture)
        {
            Transform = pTransform;
            RectangleShape = new RectangleOrientedShape(Transform, pSize);
            float outlineSize = 4f;
            mOutlineShape = new(Transform, RectangleShape.Size + new Vector2(outlineSize), RectangleShape.LocalRotation, RectangleShape.LocalOffset);
            mTexture = pTexture;
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            RectangleShape.Draw(pSpriteBatch, mTexture, COLOUR);
        }

        public void DrawOutlines(SpriteBatch pSpriteBatch)
        {
            mOutlineShape.Draw(pSpriteBatch, mTexture, Color.Black);
        }
    }
}
