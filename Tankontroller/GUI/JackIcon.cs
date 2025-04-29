using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.Controller;

namespace Tankontroller.GUI
{
    //-------------------------------------------------------------------------------------------------
    // JackIcon
    //
    // This class is used to represent the icons of the controls. It displays the icons in the game.
    //-------------------------------------------------------------------------------------------------
    class JackIcon
    {
        private static readonly Texture2D[] mIcons = new Texture2D[] {
            Tankontroller.Instance().CM().Load<Texture2D>("leftTrackForward"),
            Tankontroller.Instance().CM().Load<Texture2D>("leftTrackBackwards"),
            Tankontroller.Instance().CM().Load<Texture2D>("rightTrackForward"),
            Tankontroller.Instance().CM().Load<Texture2D>("rightTrackBackwards"),
            Tankontroller.Instance().CM().Load<Texture2D>("fire"),
            Tankontroller.Instance().CM().Load<Texture2D>("charge"),
            Tankontroller.Instance().CM().Load<Texture2D>("none"),
            Tankontroller.Instance().CM().Load<Texture2D>("turretLeft"),
            Tankontroller.Instance().CM().Load<Texture2D>("turretRight")
        };

        private Rectangle m_Rectangle;

        public JackIcon(Vector2 pPos, int pWidth, int pHeight)
        {
            m_Rectangle = new Rectangle((int)pPos.X, (int)pPos.Y, pWidth, pHeight);
        }

        public void Draw(SpriteBatch pSpriteBatch, Control pControl)
        {
            Texture2D icon = mIcons[((int)pControl)];
            pSpriteBatch.Draw(icon, m_Rectangle, Color.White);
        }
    }
}
