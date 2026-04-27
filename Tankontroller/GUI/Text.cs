using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Tankontroller.GUI
{
    //-------------------------------------------------------------------------------------------------
    // Text Class
    //
    // Purpose: This class is used to create text for the game
    // It contains the string, font, colour, position, and size for the text
    // It also contains methods to allow for localization and scaling of the text.
    //-------------------------------------------------------------------------------------------------

    public class Text
    {            
        public SpriteFont Font { get; private set; }
        public string Message { get; private set; }
        public Vector2 Position { get; private set; }
        public Vector2 Centre { get; private set; }
        public Vector2 FontSize { get; private set; }
        public Color TextColour { get; private set; }
        public float Scale { get; private set; }
        //TODO: Add localization support for the text.

        public Text(SpriteFont pFont, string pMessage, Vector2 pPos, Color pTextColour, float pScale)
        {
            Scale = pScale;
            Position = pPos;
            Font = pFont;
            Message = pMessage;
            TextColour = pTextColour;
            FontSize = Font.MeasureString(Message);
            Centre = new Vector2(Position.X + FontSize.X / 2, Position.Y + FontSize.Y / 2);
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            pSpriteBatch.DrawString(Font, Message, new Vector2(Position.X, Position.Y), TextColour, 0.0f, new Vector2(0.0f, 0.0f), Scale, SpriteEffects.None, 0.0f);
        }
    }
}
