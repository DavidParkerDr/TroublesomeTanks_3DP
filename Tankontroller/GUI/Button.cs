using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tankontroller.GUI
{
    //-------------------------------------------------------------------------------------------------
    // Button Class
    //
    // Purpose: This class is used to create buttons for the game
    // It contains the texture, rectangle, colour, selected colour, selected state and action for the button
    // It also contains methods to select and press the button
    // The button can be pressed with or without a touch
    //-------------------------------------------------------------------------------------------------
    public class Button
    {
        public Texture2D Texture { get; private set; } 
        public Texture2D TexturePressed { get; private set; }
        public Color SelectedColour { get; private set; }
        public Rectangle Rect { get; private set; }
        public bool Selected { get; set; } 
        public delegate void Action(); 
        private Action doButton;

        public Button(Texture2D pTexture, Texture2D pTexturePressed, Rectangle pRect, Color pColour, Action pDoButton)
        {
            Texture = pTexture;
            TexturePressed = pTexturePressed;
            Rect = pRect;
            SelectedColour = pColour;
            doButton = pDoButton;
        }
        public bool PressButton() 
        {
            if (doButton != null) {
                doButton(); 
                return true;
            }
            return false;
        }
    }

}
