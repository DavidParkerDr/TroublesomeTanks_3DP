using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.World;
using Tankontroller.World.Bullets;
using Tankontroller.World.Particles;

namespace Tankontroller.Managers
{
    internal class CollisionManager
    {
        private static CollisionManager mInstance = new();

        static CollisionManager() { }
        private CollisionManager() { }

        public static CollisionManager Instance
        {
            get { return mInstance; }
        }

        static public bool Collide(Tank pTank, Tank pTank_2) // Tank on Tank Collision
        {
            Vector2[] Tank1Corners = new Vector2[4];
            Vector2[] Tank2Corners = new Vector2[4];
            pTank.GetCorners(Tank1Corners);
            pTank_2.GetCorners(Tank2Corners);
            for (int i = 0; i < 4; i++)
            {
                if(pTank.PointIsInTank(Tank2Corners[i]) || pTank_2.PointIsInTank(Tank1Corners[i]))
                {
                    return true;
                }
            }
            return false;
        }

        static public bool Collide(Tank pTank, Rectangle pRectangle, bool inverse) //Tank and Rectangle Collision
        {
            //account for the inverse case
            Vector2[] tankCorners = new Vector2[4];
            pTank.GetCorners(tankCorners);

            // if the inverse case is true, then we want to check if the tank is outside the rectangle
            if (inverse) {
                foreach (Vector2 corner in tankCorners)
                    if (!pRectangle.Contains(corner))
                        return true;
            }
            else {
                foreach (Vector2 corner in tankCorners)
                {
                    if (pRectangle.Contains(corner))
                        return true;
                }
                if (pTank.PointIsInTank(new Vector2(pRectangle.Left, pRectangle.Top)) ||
                   pTank.PointIsInTank(new Vector2(pRectangle.Right, pRectangle.Top)) ||
                   pTank.PointIsInTank(new Vector2(pRectangle.Left, pRectangle.Bottom)) ||
                   pTank.PointIsInTank(new Vector2(pRectangle.Right, pRectangle.Bottom)))
                    return true;
            }
            return false;
        }

        static public bool Collide(Bullet pBullet, Tank pTank) //Bullet and Tank Collision
        {
            if (pBullet is Shockwave)
            {
                if (pTank.TankInRadius(pBullet.Radius, pBullet.Position))
                {
                    return true;
                }
            }
            if (pTank.PointIsInTank(pBullet.Position))
            {
                return true;
            }
            return false;
        }
        
        static public bool Collide(Bullet pBullet, Rectangle pRectangle, bool inverse) //Bullet and Rectangle Collision
        {
            //if the inverse case is true, then we want to check if the bullet is outside the rectangle
            if (inverse) {
                if (!pRectangle.Contains(pBullet.Position))
                    return true;
            }
            else {
                if (pRectangle.Contains(pBullet.Position))
                    return true;
            }
            return false;
        }
    }
}
