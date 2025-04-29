using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Tankontroller.GUI
{
    //-------------------------------------------------------------------------------------------------
    // ButtonList
    //
    // This class is used to manage a list of buttons. It allows the user to select the next or previous
    // button in the list, and to press the currently selected button.
    //
    // The class contains a list of buttons, and an index to the currently selected button. The class
    // provides methods to select the next or previous button in the list, and to press the currently
    // selected button. The class also provides a method to draw the buttons.
    //-------------------------------------------------------------------------------------------------
    class ButtonList
    {
        List<Button> mButtons = null;
        int currentSelectedButtonIndex = 0;

        public ButtonList()
        {
            mButtons = new List<Button>();
        }
        public void Add(Button pButton)
        {
            mButtons.Add(pButton);
        }

        public void SelectNextButton()
        {
            Console.WriteLine("NextButton start: " + currentSelectedButtonIndex);
            int nextSelectedButtonIndex = currentSelectedButtonIndex + 1;
            if (nextSelectedButtonIndex >= mButtons.Count)
            {
                nextSelectedButtonIndex = 0;
            }
            mButtons[nextSelectedButtonIndex].Selected = true;
            mButtons[currentSelectedButtonIndex].Selected = false;
            currentSelectedButtonIndex = nextSelectedButtonIndex;
            Console.WriteLine("NextButton finish: " + currentSelectedButtonIndex);

        }

        public void SelectPreviousButton()
        {
            Console.WriteLine("PreviousButton start: " + currentSelectedButtonIndex);
            int previousSelectedButtonIndex = currentSelectedButtonIndex - 1;
            if (previousSelectedButtonIndex < 0)
            {
                previousSelectedButtonIndex = mButtons.Count - 1;
            }
            mButtons[previousSelectedButtonIndex].Selected = true;
            mButtons[currentSelectedButtonIndex].Selected = false;
            currentSelectedButtonIndex = previousSelectedButtonIndex;
            Console.WriteLine("PreviousButton finish: " + currentSelectedButtonIndex);
        }

        public void PressSelectedButton()
        {
            mButtons[currentSelectedButtonIndex].PressButton();
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            foreach (Button button in mButtons)
            {
                Color buttonColour = Color.White;
                if (button.Selected)
                    pSpriteBatch.Draw(button.TexturePressed, button.Rect, buttonColour);
                else
                    pSpriteBatch.Draw(button.Texture, button.Rect, buttonColour);
            }
        }
    }
}
