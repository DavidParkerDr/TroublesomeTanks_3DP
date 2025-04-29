using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tankontroller.Controller
{
    public class PortNumLabel
    {
        private static readonly Texture2D[] m_PortNumbers = new Texture2D[]
           {
            Tankontroller.Instance().CM().Load<Texture2D>("port1"),
            Tankontroller.Instance().CM().Load<Texture2D>("port2"),
            Tankontroller.Instance().CM().Load<Texture2D>("port3"),
            Tankontroller.Instance().CM().Load<Texture2D>("port4"),
            Tankontroller.Instance().CM().Load<Texture2D>("port5"),
            Tankontroller.Instance().CM().Load<Texture2D>("port6"),
            Tankontroller.Instance().CM().Load<Texture2D>("port7"),
            Tankontroller.Instance().CM().Load<Texture2D>("port8"),
         };
        private Rectangle m_Rectangle;

        public PortNumLabel(Vector2 pPos, int pWidth, int pHeight)
        {
            m_Rectangle = new Rectangle((int)pPos.X, (int)pPos.Y, pWidth, pHeight);
        }

        public void Draw(SpriteBatch pSpriteBatch, int pNumToDisplay, Color pColor)
        {
            pSpriteBatch.Draw(m_PortNumbers[pNumToDisplay], m_Rectangle, pColor);

        }
    }
}
