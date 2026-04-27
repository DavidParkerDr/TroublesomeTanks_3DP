using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace TTMapEditor.GUI
{
    internal class Button
    {
        public Texture2D mTexture { get; private set; }

        public Texture2D mTexturePressed { get; private set; }

        public Color mSelectedColour { get; private set; }

        public Rectangle mRectangle { get; private set; }

        public bool mSelected { get; set; }

        public delegate void Action();

        private Action doButton;

        public Button(Texture2D pTexture, Texture2D pTexturePressed, Rectangle pRectangle, Color pSelectedColour, Action pAction)
        {
            mTexture = pTexture;
            mTexturePressed = pTexturePressed;
            mRectangle = pRectangle;
            mSelectedColour = pSelectedColour;
            doButton = pAction;
        }

        /// <summary>
        /// This carries out the action of a button,if it has one. Return true if button has an action, false if it doesn't.
        /// </summary>
        /// <returns></returns>
        public bool PressButton()
        {
            if(doButton != null)
            {
                doButton();
                return true;
            }
            return false;
        }

    }
}
