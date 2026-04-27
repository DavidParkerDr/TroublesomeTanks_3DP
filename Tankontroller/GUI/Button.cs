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
        public Texture2D TextureHighlighted { get; private set; }
        public Texture2D TextureOnState { get; private set; } // Exception for buttons that have an on/off state
        public Texture2D TextureOnStateHighlighted { get; private set; } // Exception for buttons that have an on/off state
        public Color SelectedColour { get; private set; }
        public Rectangle Rect { get; private set; }
        public bool Selected { get; set; } 
        public bool OnOffState { get; set; } = false; // Exception for buttons that have an on/off state
        public delegate void Action(); 
        private Action doButton;

        public Button(Texture2D pTexture, Texture2D pTextureHighlighted, Rectangle pRect, Color pColour, Action pDoButton)
        {
            Texture = pTexture;
            TextureHighlighted = pTextureHighlighted;
            Rect = pRect;
            SelectedColour = pColour;
            doButton = pDoButton;
            OnOffState = false;
        }

        /// <summary>
        /// Exception for buttons that have an on/off state.
        /// </summary>
        public Button(Texture2D pTexture, Texture2D pTextureHighlighted, Texture2D pTextureOn, Texture2D pTextureOnHighlighted, Rectangle pRect, Color pColour, Action pDoButton, bool pOnOffState)
        {
            Texture = pTexture;
            TextureHighlighted = pTextureHighlighted;
            TextureOnState = pTextureOn;
            TextureOnStateHighlighted = pTextureOnHighlighted;
            Rect = pRect;
            SelectedColour = pColour;
            doButton = pDoButton;
            OnOffState = pOnOffState;
        }
        public bool PressButton() 
        {
            // Exception for buttons that have an on/off state.
            if (TextureOnState != null) {
                OnOffState = !OnOffState;
            }
            if (doButton != null) {
                doButton(); 
                return true;
            }
            return false;
        }
    }

}
