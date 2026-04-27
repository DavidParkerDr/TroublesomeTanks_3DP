using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Tankontroller.GUI
{
    //-------------------------------------------------------------------------------------------------
    // TextList
    //
    // This class is used to manage a list of texts. 
    //
    // The class contains a list of text, that can be drawn to the screen. The class provides a method to draw the text.
    //-------------------------------------------------------------------------------------------------
    public class TextList
    {
        List<Text> mTexts = null;

        public TextList()
        {
            mTexts = new List<Text>();
        }

        public void Add(Text pText)
        {
            mTexts.Add(pText);
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            foreach (Text text in mTexts)
            {
                text.Draw(pSpriteBatch);
            }
        }
    }
}
