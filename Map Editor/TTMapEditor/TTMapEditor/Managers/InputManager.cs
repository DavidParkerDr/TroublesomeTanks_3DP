using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace TTMapEditor.Managers
{
    internal class InputManager
    {
        static InputManager mInstance = new InputManager();
        private static KeyboardState mCurrentState;
        private static KeyboardState mPreviousState;
        private static MouseState mCurrentMouseState;
        private static MouseState mPreviousMouseState;


        public static InputManager Instance
        {
            get { return mInstance; }
        }

        public static void Update()
        {
            mPreviousState = mCurrentState;
            mCurrentState = Keyboard.GetState();
            mPreviousMouseState = mCurrentMouseState;
            mCurrentMouseState = Mouse.GetState();
        }

        /// <summary>
        /// Checks if a key was just pressed in the current frame (i.e., it is currently down but was up in the previous frame).
        /// </summary>
        /// <param name="key">The key to check whether it has been pressed</param>
        /// <returns></returns>
        public static bool isKeyPressed(Keys key)
        {
            return mCurrentState.IsKeyDown(key) && mPreviousState.IsKeyUp(key);
        }

        /// <summary>
        /// Checks if a key is currently being held down (i.e., it is down in the current frame, regardless of its state in the previous frame).
        /// </summary>
        /// <param name="key">The key to check whether it is down</param>
        /// <returns></returns>
        public static bool isKeyDown(Keys key)
        {
            return mCurrentState.IsKeyDown(key);
        }

        /// <summary>
        /// Checks if a key was just released in the current frame (i.e., it is currently up but was down in the previous frame).
        /// </summary>
        /// <param name="key">The key to check whether it was released</param>
        /// <returns></returns>
        public static bool isKeyReleased(Keys key)
        {
            return mCurrentState.IsKeyUp(key) && mPreviousState.IsKeyDown(key);
        }

        /// <summary>
        /// Determines whether the specified key was pressed during the previous input state.
        /// </summary>
        /// <param name="key">The key to check for a pressed state.</param>
        /// <returns>true if the specified key was pressed in the previous input state; otherwise, false.</returns>
        public static bool wasKeyPressed(Keys key)
        {
            return mPreviousState.IsKeyDown(key);
        }

        /// <summary>
        /// Determines whether the specified key was released during the previous input state.
        /// </summary>
        /// <param name="key">The key to check for a released state</param> 
        /// <returns>true if the specified key was released in previous input state; otherwise, true</returns>
        public static bool wasKeyReleased(Keys key)
        {
            return mPreviousState.IsKeyUp(key);
        }

        /// <summary>
        /// Get the position of the mouse on the screen as a vector2,
        /// </summary>
        /// <returns>Vector 2, where x is horizontal position, and y is vertical position</returns>
        public static Vector2 GetMousePosition()
        {
            return new Vector2(mCurrentMouseState.X, mCurrentMouseState.Y);
        }

        /// <summary>
        /// Checks if the left mouse button was just clicked in the current frame (i.e., it is currently pressed but was released in the previous frame).
        /// </summary>
        /// <returns>True is left mouse clicked, otherwise false</returns>
        public static bool isLeftMouseClicked()
        {
            return mCurrentMouseState.LeftButton == ButtonState.Pressed && mPreviousMouseState.LeftButton == ButtonState.Released;
        }

        /// <summary>
        /// Checks if left mouse button was released(ie if it is currently released, and on previous frame it was pressed)
        /// </summary>
        /// <returns>Return true if it was just released, false otherwise</returns>
        public static bool isLeftMouseReleased()
        {
            return mCurrentMouseState.LeftButton == ButtonState.Released && mPreviousMouseState.LeftButton == ButtonState.Pressed;
        }
    }
}
