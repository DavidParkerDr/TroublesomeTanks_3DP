using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.Managers;
using Tankontroller.Utilities;
using Tankontroller.World.Particles;
using Tankontroller.World.Pickups;
using Tankontroller.World.Shapes;
using Tankontroller.World.WorldObject;

public enum PickupType
{
    HEALTH,
    EMP,
    MINE,
    BOUNCY_BULLET
}

namespace Tankontroller.World
{
    public class TheWorld
    {
        private static readonly Texture2D mPixelTexture = Tankontroller.Instance().CM().Load<Texture2D>("block");
        private static readonly Texture2D m_BulletTexture = Tankontroller.Instance().CM().Load<Texture2D>("circle");
        private static readonly Color GROUND_COLOUR = DGS.Instance.GetColour("COLOUR_GROUND");
        public static bool PICKUP_SPAWN = DGS.Instance.GetBool("PICKUPS_ON");
        public static float PICKUP_SPAWN_TIME = DGS.Instance.GetFloat("PICKUP_SPAWN_RATE");
        public static bool HEALTH_PICKUP = DGS.Instance.GetBool("ADD_PICKUP_HEALTH");
        public static bool EMP_PICKUP = DGS.Instance.GetBool("ADD_PICKUP_EMP");
        public static bool MINE_PICKUP = DGS.Instance.GetBool("ADD_PICKUP_MINE");
        public static bool BOUNCY_BULLET_PICKUP = DGS.Instance.GetBool("ADD_PICKUP_BOUNCYBULLET");

        private Rectangle mPlayArea;
        private Rectangle mPlayAreaOutline;
        private RectangleAxisAlignedShape[] mPlayAreaCollisionShapes = new RectangleAxisAlignedShape[4];
        private List<Tank> mTanks = new List<Tank>();
        private List<RectWall> mWalls;
        private List<PickupSpawnPoint> mPickupSpawnPositions = new List<PickupSpawnPoint>();
        private List<Pickup> mPickups = new List<Pickup>();
        private float mPickupSpawnTimer = PICKUP_SPAWN_TIME;
        private List<PickupType> mActivatedPickups = new List<PickupType>();

        public Rectangle PlayArea { get { return mPlayArea; } }

        public TheWorld(Rectangle pPlayArea, List<RectWall> pWalls, List<Tank> pTanks, List <PickupSpawnPoint> pPickupSpawnPositions)
        {
            mWalls = pWalls;
            mTanks = pTanks;
            mPlayArea = pPlayArea;
            mPickupSpawnPositions = pPickupSpawnPositions;
            mPlayAreaOutline = new Rectangle(mPlayArea.X - 5, mPlayArea.Y - 5, mPlayArea.Width + 10, mPlayArea.Height + 10);
            CreatePlayAreaCollisionShapes();
            CheckActivatedPickups();
        }

        private void CreatePlayAreaCollisionShapes()
        {
            // Top
            mPlayAreaCollisionShapes[0] = new RectangleAxisAlignedShape(
                new Transform(new Vector2(mPlayArea.X + mPlayArea.Width / 2f, mPlayArea.Y - 5)), new Vector2(mPlayArea.Width, 10));
            // Bottom
            mPlayAreaCollisionShapes[1] = new RectangleAxisAlignedShape(
                new Transform(new Vector2(mPlayArea.X + mPlayArea.Width / 2f, mPlayArea.Y + mPlayArea.Height + 5)), new Vector2(mPlayArea.Width, 10));
            // Left
            mPlayAreaCollisionShapes[2] = new RectangleAxisAlignedShape(
                new Transform(new Vector2(mPlayArea.X - 5, mPlayArea.Y + mPlayArea.Height / 2f)), new Vector2(10, mPlayArea.Height + 20));
            // Right
            mPlayAreaCollisionShapes[3] = new RectangleAxisAlignedShape(
                new Transform(new Vector2(mPlayArea.X + mPlayArea.Width + 5, mPlayArea.Y + mPlayArea.Height / 2f)), new Vector2(10, mPlayArea.Height + 20));
        }

        public List<Tank> GetTanksForPlayers(int pPlayerCount)
        {
            if (mTanks.Count >= pPlayerCount && pPlayerCount > 0)
            {
                mTanks = mTanks.GetRange(0, (int)pPlayerCount);
                return mTanks;
            }
            return null;
        }

        public void AddPickup()
        {
            mPickupSpawnTimer = PICKUP_SPAWN_TIME;
            if (PICKUP_SPAWN && mActivatedPickups.Count() > 0 && mPickupSpawnPositions.Count() > 0)
            {
                int randPos = new Random().Next(0, mPickupSpawnPositions.Count());
                //Checks for any pickups at this position to prevent spawn overlap
                foreach (Pickup p in mPickups)
                {
                    if (p.Transform.Position == mPickupSpawnPositions[randPos].GetPosition())
                    {
                        return;
                    }
                }

                int randPickup = new Random().Next(0, mActivatedPickups.Count());
                if (mActivatedPickups[randPickup] == PickupType.HEALTH && mPickupSpawnPositions[randPos].IsPickupTypeActivated(PickupType.HEALTH))
                {
                    HealthPickup mHealthPickup = new HealthPickup(mPickupSpawnPositions[randPos].GetPosition());
                    mPickups.Add(mHealthPickup);
                }
                else if (mActivatedPickups[randPickup] == PickupType.EMP && mPickupSpawnPositions[randPos].IsPickupTypeActivated(PickupType.EMP))
                {
                    EMPPickup mEMPPickup = new EMPPickup(mPickupSpawnPositions[randPos].GetPosition());
                    mPickups.Add(mEMPPickup);
                }
                else if (mActivatedPickups[randPickup] == PickupType.MINE && mPickupSpawnPositions[randPos].IsPickupTypeActivated(PickupType.MINE))
                {
                    MinePickup mMinePickup = new MinePickup(mPickupSpawnPositions[randPos].GetPosition());
                    mPickups.Add(mMinePickup);
                }
                else if (mActivatedPickups[randPickup] == PickupType.BOUNCY_BULLET && mPickupSpawnPositions[randPos].IsPickupTypeActivated(PickupType.BOUNCY_BULLET))
                {
                    BouncyBulletPickup mBouncyBulletPickup = new BouncyBulletPickup(mPickupSpawnPositions[randPos].GetPosition());
                    mPickups.Add(mBouncyBulletPickup);
                }
            }
        }

        public void CheckActivatedPickups()
        {
            foreach (PickupType p in Enum.GetValues(typeof(PickupType)))
            {
                if(p == PickupType.HEALTH && HEALTH_PICKUP)
                {
                    mActivatedPickups.Add(p);
                }
                else if (p == PickupType.EMP && EMP_PICKUP)
                {
                    mActivatedPickups.Add(p);
                }
                else if (p == PickupType.MINE && MINE_PICKUP)
                {
                    mActivatedPickups.Add(p);
                }
                else if (p == PickupType.BOUNCY_BULLET && BOUNCY_BULLET_PICKUP)
                {
                    mActivatedPickups.Add(p);
                }
            }
        }

        public void Update(float pSeconds)
        {
            mPickupSpawnTimer -= pSeconds;
            if(mPickupSpawnTimer <= 0) { AddPickup(); }
            Particles.ParticleManager.Instance().Update(pSeconds);

            // Check collisions for each tank
            for (int tankIndex = 0; tankIndex < mTanks.Count; tankIndex++)
            {

                if (mTanks[tankIndex].GetState() == TankStates.DESTROYED)
                {
                    continue;
                }
                else
                {
                    mTanks[tankIndex].Update(pSeconds);

                    // Create a combined list of wall colliders and play area colliders
                    List<CollisionShape> wallColliders = new();
                    mWalls.ForEach(wall => wallColliders.Add(wall.CollisionShape));
                    wallColliders.AddRange(mPlayAreaCollisionShapes);

                    // Bullet collisions
                    mTanks[tankIndex].HandleBulletCollisions(mTanks, wallColliders);

                    // Pickup collisions
                    foreach (Pickup pickup in mPickups)
                    {
                        // This is to avoid any dead tanks from picking up a pickup
                        if (mTanks[tankIndex].GetState() != TankStates.ALIVE)
                        {
                            continue;
                        }
                        else if (pickup.CollisionShape.Intersects(mTanks[tankIndex].CollisionShape).HasCollided)
                        {
                            pickup.TriggerEffect(mTanks[tankIndex]);
                            mPickups.Remove(pickup);
                            break;
                        }
                    }

                    // Wall collisions
                    foreach (CollisionShape wall in wallColliders)
                    {
                        CollisionManager.ResolveTankWallCollision(mTanks[tankIndex], wall);
                    }

                    // Collisions with other tanks
                    for (int i = 0; i < mTanks.Count; i++)
                    {
                        if (tankIndex == i || mTanks[i].GetState() == TankStates.DESTROYED) // Skip collision with self and if the tanks is destroyed
                        {
                            continue;
                        }
                        if (mTanks[tankIndex].CollisionShape.Intersects(mTanks[i].CollisionShape).HasCollided)
                        {
                            mTanks[tankIndex].PutBack();
                        }
                    }
                }
            }
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            pSpriteBatch.Draw(mPixelTexture, mPlayAreaOutline, Color.Black);
            pSpriteBatch.Draw(mPixelTexture, mPlayArea, GROUND_COLOUR);

            TrackSystem.GetInstance().Draw(pSpriteBatch);

            foreach (Pickup p in mPickups)
            {
                p.Draw(pSpriteBatch);
            }

            ParticleManager.Instance().Draw(pSpriteBatch);

            //Draws the tanks (on top of tracks but below particles)
            foreach (Tank t in mTanks)
            {
                t.DrawBullets(pSpriteBatch, m_BulletTexture);
            }

            //Draws the tanks
            foreach (Tank t in mTanks)
            {
                t.Draw(pSpriteBatch);
            }

            //Draws the walls
            foreach (RectWall w in mWalls)
            {
                w.DrawOutlines(pSpriteBatch);
            }
            foreach (RectWall w in mWalls)
            {
                w.Draw(pSpriteBatch);
            }

            if (CollisionManager.DRAW_COLLISION_SHAPES)
            {
                foreach (RectangleAxisAlignedShape shape in mPlayAreaCollisionShapes)
                {
                    DrawUtilities.DrawRectangle(pSpriteBatch, shape.ToRectangle(), Color.Red, 0, shape.WorldPosition, 1);
                }
            }
        }
    }
}
