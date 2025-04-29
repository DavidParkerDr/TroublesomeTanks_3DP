using System.Collections.Generic;
using System.Linq;
using Tankontroller.Scenes;

namespace Tankontroller.Managers
{
    //-------------------------------------------------------------------------------------------------
    // SceneManager
    //
    // This class is used to manage a list of scenes. It allows the user to push a scene onto the list,
    // to transition to the next scene, to pop the current scene, to get the top scene, and to update and
    // draw the top scene.
    //
    // The class contains a list of scenes. The class provides methods to push a scene onto the list, to
    // transition to the next scene, to pop the current scene, to get the top scene, and to update and draw
    // the top scene.
    //-------------------------------------------------------------------------------------------------
    public class SceneManager
    {
        private List<IScene> mScenes = new List<IScene>();
        static SceneManager mInstance = new SceneManager();

        private SceneManager() { }

        public static SceneManager Instance
        {
            get { return mInstance; }
        }

        public void Push(IScene p_Scene)
        {
            mScenes.Add(p_Scene);
        }

        public void Transition(IScene pNextScene, bool replaceCurrent = true)
        {
            IScene currentScene = Top;
            // gain access to the scene before the current scene
            if (pNextScene == null)
            {
                pNextScene = Previous;
            }
            if (replaceCurrent)
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
                // we should exit the game here...
                Tankontroller game = (Tankontroller)Tankontroller.Instance();
                game.GetControllerManager().DisconnectAllControllers();
                game.Exit();
            }
        }

        public void Pop()
        {
            if (mScenes.Count > 0)
            {
                mScenes.RemoveAt(mScenes.Count - 1);
            }
        }

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
                mScenes.Add(new ErrorMessageScene(e));
            }
        }

        public void Draw(float pSeconds)
        {
            if (mScenes.Count > 0)
            {
                Top.Draw(pSeconds);
            }
        }
    }
}
