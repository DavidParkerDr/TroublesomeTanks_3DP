using Microsoft.Xna.Framework.Graphics;

namespace TTMapEditor.Scenes
{
    /// <summary>
    /// IScene
    /// 
    /// This interface is used to define the methods that a scene must implement.
    /// The interface contains methods to draw and update the scene.
    /// </summary>
    internal abstract class IScene
    {
        public SpriteBatch mSpriteBatch { get; set; }

        public abstract void Draw(float pSeconds);

        public abstract void Update(float pSeconds);

        public virtual void Escape()
        {

        }

    }
}
