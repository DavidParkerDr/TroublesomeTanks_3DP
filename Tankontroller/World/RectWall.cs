using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tankontroller.World
{
    public class RectWall
    {
        private static readonly Color COLOUR = DGS.Instance.GetColour("COLOUR_WALLS");
        public Rectangle Rectangle { get; private set; }
        private Rectangle m_OutlineRectangle;
        private Texture2D m_Texture;

        public RectWall(Texture2D pTexture, Rectangle pRectangle)
        {
            Rectangle = pRectangle;
            m_OutlineRectangle = new Rectangle(pRectangle.X - 2, pRectangle.Y - 2, pRectangle.Width + 4, pRectangle.Height + 4);
            m_Texture = pTexture;
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            pSpriteBatch.Draw(m_Texture, Rectangle, COLOUR);
        }

        public void DrawOutlines(SpriteBatch pSpriteBatch)
        {
            pSpriteBatch.Draw(m_Texture, m_OutlineRectangle, Color.Black);
        }
    }
}
