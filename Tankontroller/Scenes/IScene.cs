using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

﻿namespace Tankontroller.Scenes
{
    //-------------------------------------------------------------------------------------------------
    // IScene
    //
    // This interface is used to define the methods that a scene must implement. The interface contains
    // methods to draw and update the scene.
    //-------------------------------------------------------------------------------------------------
    public abstract class IScene
    {
        //private SpriteBatch SpriteBatch;
        public SpriteBatch spriteBatch { get; set; }
        public abstract void Draw(float pSeconds);
        public abstract void Update(float pSeconds);
        public virtual void Escape()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Tankontroller.Instance().SM().Transition(null);
            }
        }
    }
}
