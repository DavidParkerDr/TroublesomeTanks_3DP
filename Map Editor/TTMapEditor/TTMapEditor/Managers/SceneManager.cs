using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using TTMapEditor.Scenes;

namespace TTMapEditor.Managers
{
    /// <summary>
    /// Centralized stack-based scene controller for the map editor.
    /// 
    /// Maintains a stack of <see cref="IScene"/> instances and exposes
    /// operations to push, pop, transition between scenes, and to update/draw
    /// the currently active scene. Also provides basic error handling by
    /// replacing the current scene stack with an <see cref="ErrorMessageScene"/>
    /// if an exception is thrown during update.
    /// 
    /// Implemented as a singleton; use <see cref="Instance"/> to access it.
    /// </summary>
    internal class SceneManager
    {
        private List<IScene> mScenes = new List<IScene>();

        static SceneManager mInstance = new SceneManager();

        private SceneManager() { }

        /// <summary>
        /// Gets the singleton instance of the <see cref="SceneManager"/>.
        /// </summary>
        public static SceneManager Instance
        {
            get { return mInstance; }
        }

        /// <summary>
        /// Pushes a new scene onto the scene stack, making it the active scene.
        /// </summary>
        /// <param name="pScene">Scene to push on top of the stack.</param>
        public void Push(IScene pScene)
        {
            mScenes.Add(pScene);
        }

        /// <summary>
        /// Transitions from the current scene to a new scene using a
        /// <see cref="TransitionScene"/> wrapper.
        /// 
        /// If the current top scene is already a <see cref="TransitionScene"/>,
        /// the call is ignored to avoid nested transitions.
        /// If <paramref name="pNextScene"/> is <c>null</c>, the previous
        /// scene on the stack (if any) is used as the target.
        /// If <paramref name="pReplaceCurrent"/> is <c>true</c>, the current
        /// top scene is removed before adding the transition.
        /// If there is no next scene available, the game is exited.
        /// </summary>
        /// <param name="pNextScene">
        /// The scene to transition to. If <c>null</c>, uses
        /// <see cref="Previous"/> as the target scene.
        /// </param>
        /// <param name="pReplaceCurrent">
        /// Whether to remove the current top scene before adding the
        /// transition scene. Defaults to <c>true</c>.
        /// </param>
        public void Transition(IScene pNextScene, bool pReplaceCurrent = true)
        {
            IScene currentScene = Top;
            if (Top is TransitionScene)
            {
                return;
            }

            if (pNextScene == null)
            {
                pNextScene = Previous;
            }

            if (pReplaceCurrent)
            {
                Pop();
            }

            if (pNextScene != null)
            {
                IScene transitionScene = new TransitionScene(currentScene, pNextScene);
                mScenes.Add(transitionScene);
            }
            else
            {
                TTMapEditor game = (TTMapEditor)TTMapEditor.Instance();
                game.Exit();
            }
        }

        /// <summary>
        /// Pops the current top scene off the stack, if any exists.
        /// </summary>
        public void Pop()
        {
            if (mScenes.Count > 0)
            {
                mScenes.RemoveAt(mScenes.Count - 1);
            }
        }

        /// <summary>
        /// Gets the scene at the top of the stack, or <c>null</c> if
        /// there are no scenes.
        /// </summary>
        public IScene Top
        {
            get
            {
                if (mScenes.Count > 0)
                {
                    return mScenes.Last();
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the scene directly beneath the top of the stack, or
        /// <c>null</c> if there are fewer than two scenes.
        /// </summary>
        public IScene Previous
        {
            get
            {
                if (mScenes.Count > 1)
                {
                    return mScenes[mScenes.Count - 2];
                }

                return null;
            }
        }

        /// <summary>
        /// Updates the active scene.
        /// 
        /// If an exception occurs during update, the scene stack is cleared
        /// and replaced with a single <see cref="ErrorMessageScene"/> that
        /// displays the exception details.
        /// </summary>
        /// <param name="pSeconds">Elapsed time in seconds since the last update.</param>
        public void Update(float pSeconds)
        {
            try
            {
                if (mScenes.Count > 0)
                {
                    Top.Update(pSeconds);
                }
            }
            catch (System.Exception e)
            {
                mScenes.Clear();
                TTMapEditor game = (TTMapEditor)TTMapEditor.Instance();
                game.Exit();
            }
        }

        /// <summary>
        /// Draws the active scene, if one exists.
        /// </summary>
        /// <param name="pSeconds">Elapsed time in seconds since the last draw.</param>
        public void Draw(float pSeconds)
        {
            if (mScenes.Count > 0)
            {
                Top.Draw(pSeconds);
            }
        }
    }
}
