using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace TTMapEditor.Saving
{
    /// <summary>
    /// Provides a simple on-screen text input widget for entering a file name.
    /// Handles keyboard input, character filtering, repeat rate, and drawing
    /// of the text entry background and prompt.
    /// </summary>
    internal class FileNamer
    {
        private string mCurrentName;

        private float mTimeBetweenKeyPresses = 0.2f;

        private float mTimeSinceLastKeyPress = 0f;

        private static readonly SpriteFont mFont = TTMapEditor.Instance().GetContentManager().Load<SpriteFont>("FolderPickerText");

        private Rectangle mTypingRectangle;

        private Texture2D mTypingBackground = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("block");

        private bool mIsActive = false;

        private Keys mPreviousKey;

        private Keys mCurrentKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileNamer"/> class
        /// with an empty name and a default typing rectangle.
        /// </summary>
        public FileNamer()
        {
            mCurrentName = string.Empty;
            mTypingRectangle = new Rectangle(100, 100, 400, 50);
        }

        /// <summary>
        /// Updates the file name input logic.
        /// Processes keyboard state and appends/removes characters
        /// according to the allowed keys and repeat timing.
        /// </summary>
        /// <param name="pDeltaTime">Elapsed time in seconds since the last update call.</param>
        public void Update(float pDeltaTime)
        {
            // Determine Caps Lock state to decide character casing.
            bool capsLock = Keyboard.GetState().CapsLock;

            // Skip processing if the input UI is not active.
            if (!mIsActive)
            {
                return;
            }

            mTimeSinceLastKeyPress += pDeltaTime;

            Keys[] pressedKeys = Keyboard.GetState().GetPressedKeys();
            int numPressedKeys = pressedKeys.Length;

            // No keys pressed, reset repeat timer to allow immediate next key.
            if (numPressedKeys == 0)
            {
                mTimeSinceLastKeyPress = mTimeBetweenKeyPresses;
                return;
            }

            Keys key = pressedKeys[0];

            // Ignore keys that are not part of the allowed input set.
            if (!IsValidKey(key))
            {
                return;
            }

            // If this is a new key, reset the repeat timer.
            if (key != mPreviousKey)
            {
                mTimeSinceLastKeyPress = mTimeBetweenKeyPresses;
            }
            // If the key is being held, only process it when enough time has elapsed.
            else if (mTimeSinceLastKeyPress < mTimeBetweenKeyPresses)
            {
                return;
            }

            // Handle backspace to delete the last character, if any.
            if (key == Keys.Back && mCurrentName.Length > 0)
            {
                mCurrentName = mCurrentName.Substring(0, mCurrentName.Length - 1);
                mTimeSinceLastKeyPress = 0f;
                mPreviousKey = key;
                return;
            }

            // Prevent overflow: do not add more characters if the text exceeds the box width.
            if (mFont.MeasureString(mCurrentName).X > mTypingRectangle.Width - 20)
            {
                return;
            }

            // Space key is handled explicitly.
            if (key == Keys.Space)
            {
                mCurrentName += " ";
                mPreviousKey = pressedKeys[0];
                mTimeSinceLastKeyPress = 0f;
                return;
            }

            // Convert key to a character (letters, digits) and append it.
            char? ch = KeyToChar(key, capsLock);
            if (ch.HasValue)
            {
                mCurrentName += ch.Value;
            }

            mPreviousKey = key;
            mTimeSinceLastKeyPress = 0f;
        }

        /// <summary>
        /// Draws the file naming UI, including background, prompt, current name,
        /// and a short instruction message.
        /// </summary>
        /// <param name="pSpriteBatch">Sprite batch used for drawing.</param>
        public void Draw(SpriteBatch pSpriteBatch)
        {
            if (!mIsActive)
            {
                return;
            }

            pSpriteBatch.Draw(mTypingBackground, mTypingRectangle, Color.Black);
            pSpriteBatch.DrawString(mFont, "Enter name:", new Vector2(100, 100), Color.White);
            pSpriteBatch.DrawString(mFont, mCurrentName, new Vector2(100, 115), Color.White);
            pSpriteBatch.DrawString(mFont, "Press Enter to confirm", new Vector2(100, 130), Color.White);
        }

        /// <summary>
        /// Returns the currently typed name and deactivates the input UI.
        /// </summary>
        /// <returns>The final file name string.</returns>
        public string ReturnName()
        {
            mIsActive = false;
            return mCurrentName;
        }

        /// <summary>
        /// Activates the file naming UI and clears any previously typed text.
        /// </summary>
        public void StartTyping()
        {
            mIsActive = true;
            mCurrentName = string.Empty;
        }

        /// <summary>
        /// Indicates whether the file naming UI is currently active.
        /// </summary>
        /// <returns><c>true</c> if active; otherwise, <c>false</c>.</returns>
        public bool IsActive()
        {
            return mIsActive;
        }

        /// <summary>
        /// Determines if a key is valid for text input (letters, digits, space, backspace).
        /// </summary>
        /// <param name="pKey">The key to validate.</param>
        /// <returns><c>true</c> if the key can be used for input; otherwise, <c>false</c>.</returns>
        private static bool IsValidKey(Keys pKey)
        {
            return (pKey >= Keys.A && pKey <= Keys.Z)
                   || (pKey >= Keys.D0 && pKey <= Keys.D9)
                   || pKey == Keys.Space
                   || pKey == Keys.Back;
        }

        /// <summary>
        /// Maps a keyboard key to its corresponding character, if supported.
        /// Supports letters A–Z, top-row digits 0–9, and numpad digits 0–9.
        /// Space and backspace are handled outside of this method.
        /// </summary>
        /// <param name="pKey">The key to convert.</param>
        /// <param name="pCapsLock">Whether Caps Lock is currently enabled.</param>
        /// <returns>
        /// The corresponding character, or <c>null</c> if the key does not map to a printable character.
        /// </returns>
        private static char? KeyToChar(Keys pKey, bool pCapsLock)
        {
            // Letters A–Z
            if (pKey >= Keys.A && pKey <= Keys.Z)
            {
                char c = (char)('A' + (pKey - Keys.A));
                return pCapsLock ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c);
            }

            // Top-row digits D0–D9 -> '0'–'9'
            if (pKey >= Keys.D0 && pKey <= Keys.D9)
            {
                return (char)('0' + (pKey - Keys.D0));
            }

            // Numpad digits -> '0'–'9'
            if (pKey >= Keys.NumPad0 && pKey <= Keys.NumPad9)
            {
                return (char)('0' + (pKey - Keys.NumPad0));
            }

            // Space handled earlier, Backspace returns null (no char)
            return null;
        }
    }
}
