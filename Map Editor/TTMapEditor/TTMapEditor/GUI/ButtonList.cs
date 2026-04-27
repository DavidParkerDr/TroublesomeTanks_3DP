using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;


namespace TTMapEditor.GUI
{
    internal class ButtonList
    {
        List<Button> mButtons = null;
        int currentSelectedButtonIndex = 0;

        public ButtonList()
        {
            mButtons = new List<Button>();
        }

        /// <summary>
        /// Adds a button to the list of buttons. The first button added will be selected by default.
        /// </summary>
        /// <param name="pButton"></param>
        public void AddButton(Button pButton)
        {
            mButtons.Add(pButton);
        }

        /// <summary>
        /// Selects the next button in the collection, updating the selection state accordingly.
        /// </summary>
        /// <remarks>If the currently selected button is the last in the collection, selection wraps
        /// around to the first button. Only one button is selected at a time.</remarks>
        public void SelectNextButton()
        {
            Console.WriteLine("NextButton start: " + currentSelectedButtonIndex);
            int nextSelectedButtonIndex = currentSelectedButtonIndex + 1;
            if (nextSelectedButtonIndex >= mButtons.Count)
            {
                nextSelectedButtonIndex = 0;
            }
            mButtons[nextSelectedButtonIndex].mSelected = true;
            mButtons[currentSelectedButtonIndex].mSelected = false;
            currentSelectedButtonIndex = nextSelectedButtonIndex;
            Console.WriteLine("NextButton finish: " + currentSelectedButtonIndex);
        }

        /// <summary>
        /// Selects the previous button in the collection, updating the selection state accordingly.
        /// </summary>
        /// <remarks>If the currently selected button is the first in the collection, selection wraps
        public void SelectPreviousButton()
        {
            Console.WriteLine("PreviousButton start: " + currentSelectedButtonIndex);
            int previousSelectedButtonIndex = currentSelectedButtonIndex - 1;
            if (previousSelectedButtonIndex < 0)
            {
                previousSelectedButtonIndex = mButtons.Count - 1;
            }
            mButtons[previousSelectedButtonIndex].mSelected = true;
            mButtons[currentSelectedButtonIndex].mSelected = false;
            currentSelectedButtonIndex = previousSelectedButtonIndex;
            Console.WriteLine("PreviousButton finish: " + currentSelectedButtonIndex);
        }

        /// <summary>
        /// Carries out the action of the currently selected button.
        /// </summary>
        public void PressSelectedButton()
        {
            mButtons[currentSelectedButtonIndex].PressButton();
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            foreach (Button button in mButtons)
            {
                Color buttonColour = Color.White;
                if (button.mSelected)
                    pSpriteBatch.Draw(button.mTexturePressed, button.mRectangle, Color.Lerp(buttonColour, Color.Black, 0.2f));
                else
                    pSpriteBatch.Draw(button.mTexture, button.mRectangle, buttonColour);
            }
        }

    }
}
