using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Tankontroller.GUI
{
    //-------------------------------------------------------------------------------------------------
    // Avatar
    //
    // 
    //-------------------------------------------------------------------------------------------------

    public class Avatar
    {
        private const int LAYER_COUNT = 3;
        private Texture2D[] m_BlackAndWhiteLayer = new Texture2D[LAYER_COUNT];
        private Texture2D[] m_ColourLayer = new Texture2D[LAYER_COUNT];
        public Rectangle m_Rectangle { get; protected set; }
        private Rectangle[] m_DrawRectangles = new Rectangle[LAYER_COUNT];
        private Color m_Colour;
        private string m_Name;

        public Avatar(Texture2D pBlackAndWhiteLayer, Texture2D pColourLayer, Rectangle pRectangle, Color pColour)
        {
            m_BlackAndWhiteLayer[0] = pBlackAndWhiteLayer;
            m_ColourLayer[0] = pColourLayer;
            m_Rectangle = pRectangle;
            m_Colour = pColour;
            PrepareDrawRectangles();
        }

        public Avatar(string pName, Rectangle pRectangle, Color pColour)
        {
            Tankontroller game = (Tankontroller)Tankontroller.Instance();
            for (int i = 0; i < m_BlackAndWhiteLayer.Length; i++)
            {
                string blackAndWhiteFileName = "avatars/avatar_" + pName + "_bw_0" + (i + 1);
                string colourFileName = "avatars/avatar_" + pName + "_colour_0" + (i + 1);
                m_BlackAndWhiteLayer[i] = game.CM().Load<Texture2D>(blackAndWhiteFileName);
                m_ColourLayer[i] = game.CM().Load<Texture2D>(colourFileName);
            }
            m_Rectangle = pRectangle;
            m_Colour = pColour;
            m_Name = pName;
            PrepareDrawRectangles();
        }

        private void PrepareDrawRectangles()
        {
            int padding = 5;
            int maxAvatarWidth = m_Rectangle.Width - 2 * padding;
            int maxAvatarHeight = m_Rectangle.Height - 2 * padding;
            for (int i = 0; i < m_DrawRectangles.Length; i++)
            {
                float avatarRatio = (float)m_BlackAndWhiteLayer[i].Width / m_BlackAndWhiteLayer[i].Height;
                int avatarHeight = maxAvatarHeight;
                int avatarWidth = (int)(avatarHeight * avatarRatio);
                if (avatarWidth > maxAvatarWidth)
                {
                    avatarWidth = maxAvatarWidth;
                    avatarHeight = (int)(avatarWidth / avatarRatio);
                }
                int avatarLeft = m_Rectangle.Left + (m_Rectangle.Width - avatarWidth) / 2;
                int avatarTop = m_Rectangle.Top + (m_Rectangle.Height - avatarHeight) / 2;
                m_DrawRectangles[i] = new Rectangle(avatarLeft, avatarTop, avatarWidth, avatarHeight);
            }
        }
        public string GetName()
        {
            return m_Name;
        }
        public Color GetColour() { return m_Colour; }
        public void SetColour(Color pColour) { m_Colour = pColour; }

        public void Reposition(Rectangle pRectangle)
        {
            m_Rectangle = pRectangle;
            PrepareDrawRectangles();
        }

        public void Draw(SpriteBatch pSpriteBatch, bool pAlive, int pIndex)
        {
            pIndex = Math.Clamp(pIndex, 0, LAYER_COUNT - 1); // ensure that pIndex is within the range of the array
            if (pAlive)
            {
                pSpriteBatch.Draw(m_BlackAndWhiteLayer[pIndex], m_DrawRectangles[pIndex], Color.White);
                pSpriteBatch.Draw(m_ColourLayer[pIndex], m_DrawRectangles[pIndex], m_Colour);
            }
            else
            {
                pSpriteBatch.Draw(m_BlackAndWhiteLayer[pIndex], m_DrawRectangles[pIndex], Color.Red);
                pSpriteBatch.Draw(m_ColourLayer[pIndex], m_DrawRectangles[pIndex], Color.Red);
            }
        }
    }
}