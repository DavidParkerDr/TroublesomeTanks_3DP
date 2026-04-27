using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;
using Tankontroller.Managers;
using Tankontroller.Utilities;
using Tankontroller.World.Bullets;
using Tankontroller.World.Particles;
using Tankontroller.World.Shapes;
using Tankontroller.World.WorldObject;

public enum BulletType
{
    DEFAULT,
    BOUNCY_EMP,
    MINE,
    BOUNCY_BULLET
}

public enum TankStates
{ 
    ALIVE,
    DEFEATED,
    DESTROYED
}


namespace Tankontroller.World
{
    public class Tank : ICollidable
    {
        #region Variables & Constructors

        public static readonly int MAX_HEALTH = DGS.Instance.GetInt("MAX_TANK_HEALTH");
        public static readonly float TANK_SPEED = DGS.Instance.GetFloat("TANK_SPEED");
        public static readonly float BULLET_SPEED = DGS.Instance.GetFloat("BULLET_SPEED");
        public static readonly int BLAST_DELAY = DGS.Instance.GetInt("BLAST_DELAY");
        public static readonly int TANK_HEIGHT = DGS.Instance.GetInt("TANK_HEIGHT");
        public static readonly int TANK_WIDTH = DGS.Instance.GetInt("TANK_WIDTH");
        public static readonly int TANK_FRONT_BUFFER = DGS.Instance.GetInt("TANK_FRONT_BUFFER");
        public static readonly float BASE_TURRET_ROTATION_ANGLE = DGS.Instance.GetFloat("BASE_TURRET_ROTATION_ANGLE");
        public static readonly float BASE_TANK_ROTATION_ANGLE = DGS.Instance.GetFloat("BASE_TANK_ROTATION_ANGLE");
        public static readonly float TRACK_OFFSET = DGS.Instance.GetFloat("TRACK_OFFSET");
        public static readonly int MAX_DESTRUCTION_HEALTH = DGS.Instance.GetInt("MAX_TANK_DESTRUCTION_HEALTH");

        static private readonly Texture2D mBaseTexture = Tankontroller.Instance().CM().Load<Texture2D>("Tank-B-05");
        static private readonly Texture2D mBrokenTexture = Tankontroller.Instance().CM().Load<Texture2D>("BrokenTank");
        static private readonly Texture2D mRightTrackTexture = Tankontroller.Instance().CM().Load<Texture2D>("Tank track B-R");
        static private readonly Texture2D mLeftTrackTexture = Tankontroller.Instance().CM().Load<Texture2D>("Tank track B-L");
        static private readonly Texture2D mCannonTexture = Tankontroller.Instance().CM().Load<Texture2D>("cannon");
        static private readonly Texture2D mCannonFireTexture = Tankontroller.Instance().CM().Load<Texture2D>("cannonFire");

        public Transform Transform { get; private set; } = new Transform();
        public CollisionShape CollisionShape => RectangleShape;
        public RectangleOrientedShape RectangleShape { get; private set; }

        private Vector2[] TANK_CORNERS = { 
            new Vector2(TANK_HEIGHT / 2 - TANK_FRONT_BUFFER, -TANK_WIDTH / 2), 
            new Vector2(-TANK_HEIGHT / 2, -TANK_WIDTH / 2), 
            new Vector2(-TANK_HEIGHT / 2, TANK_WIDTH / 2), 
            new Vector2(TANK_HEIGHT / 2 - TANK_FRONT_BUFFER, TANK_WIDTH / 2) };

        private Vector2 mOldPosition;
        private float mOldRotation;

        private List<Bullet> mBullets;

        private int mHealth;
        private int mDestructibleHealth;
        public BulletType BulletType { get; protected set; }

        private float mCannonRotation;
        private int mFired; // Number of frames since the player fired

        private Color mColour;
        private float mResolutionScale;

        private int mLeftTrackFrame;
        private int mRightTrackFrame;

        private bool mIsInsideShockwave = false; // Needed so that Player knows to deplete charge from shockwave

        private TankStates mCurrentState = TankStates.ALIVE;

        public Tank(Vector2 pPos, float pRotation, float pScale) : this(pPos.X, pPos.Y, pRotation, pScale) { }

        public Tank(float pXPosition, float pYPosition, float pRotation, float pScale)
        {
            mHealth = MAX_HEALTH;
            BulletType = BulletType.DEFAULT;
            mDestructibleHealth = MAX_DESTRUCTION_HEALTH;

            mColour = Color.White;
            mBullets = new List<Bullet>();
            mFired = 0;
            mLeftTrackFrame = 1;
            mRightTrackFrame = 1;

            mResolutionScale = pScale;
            Transform.Position = new Vector2(pXPosition, pYPosition);
            Transform.Rotation = pRotation;
            mCannonRotation = pRotation;
            mOldPosition = Transform.Position;
            mOldRotation = Transform.Rotation;

            RectangleShape = new RectangleOrientedShape(Transform, new Vector2((TANK_HEIGHT - TANK_FRONT_BUFFER) * mResolutionScale, TANK_WIDTH * mResolutionScale), 0f, Vector2.Zero);
        }

        #endregion

        #region Getters & Setters

        public int Health() => mHealth;
        public void Heal()
        {
            mHealth++;
            if (mHealth > MAX_HEALTH)
            {
                mHealth = MAX_HEALTH;
            }
        }

        public Color Colour() => mColour;
        public void SetColour(Color pColour) => mColour = pColour;

        /// <summary> Returns true if current tank state is alive, otherwise returns false </summary>
        public bool IsAlive() => mCurrentState == TankStates.ALIVE;
        /// <summary> Returns the current state of the tank (Alive, Defeated, Destroyed). </summary>
        public TankStates GetState() => mCurrentState;

        public void SetBulletType(BulletType pBulletType) => BulletType = pBulletType;

        public float GetCannonWorldRotation() => mCannonRotation;

        public void GetCorners(Vector2[] pCorners)
        {
            if (pCorners.Length == 4)
            {
                Vector3 temp = Vector3.Zero;
                for (int i = 0; i < 4; i++)
                {
                    temp.X = TANK_CORNERS[i].X * mResolutionScale;
                    temp.Y = TANK_CORNERS[i].Y * mResolutionScale;
                    temp = Vector3.Transform(temp, Matrix.CreateRotationZ(Transform.Rotation));
                    temp += new Vector3(Transform.Position.X, Transform.Position.Y, 0);
                    pCorners[i].X = temp.X;
                    pCorners[i].Y = temp.Y;
                }
            }
        }

        #endregion

        #region Update & Draw

        public void Update(float pSeconds)
        {
            if (mFired > 0)
            {
                mFired--;
            }
            foreach (Bullet bullet in mBullets)
            {
                bullet.Update(pSeconds);
            }
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            Rectangle trackRect = new Rectangle(0, 0, mLeftTrackTexture.Width, mLeftTrackTexture.Height / 15);
            switch (mCurrentState)
            {
                case TankStates.ALIVE:
                    trackRect.Y = mLeftTrackFrame * mLeftTrackTexture.Height / 15;
                    pSpriteBatch.Draw(mLeftTrackTexture, Transform.Position, trackRect, mColour, Transform.Rotation, new Vector2(mBaseTexture.Width / 2, mBaseTexture.Height / 2), mResolutionScale, SpriteEffects.None, 0.0f);
                    trackRect.Y = mRightTrackFrame * mLeftTrackTexture.Height / 15;
                    pSpriteBatch.Draw(mRightTrackTexture, Transform.Position, trackRect, mColour, Transform.Rotation, new Vector2(mBaseTexture.Width / 2, mBaseTexture.Height / 2), mResolutionScale, SpriteEffects.None, 0.0f);
                    pSpriteBatch.Draw(mBaseTexture, Transform.Position, null, mColour, Transform.Rotation, new Vector2(mBaseTexture.Width / 2, mBaseTexture.Height / 2), mResolutionScale, SpriteEffects.None, 0.0f);
                    if (mFired == 0)
                    {
                        pSpriteBatch.Draw(mCannonTexture, Transform.Position, null, mColour, mCannonRotation, new Vector2(mCannonTexture.Width / 2, mCannonTexture.Height / 2), mResolutionScale, SpriteEffects.None, 0.0f);
                    }
                    else
                    {
                        pSpriteBatch.Draw(mCannonFireTexture, Transform.Position, null, mColour, mCannonRotation, new Vector2(mCannonTexture.Width / 2, mCannonTexture.Height / 2), mResolutionScale, SpriteEffects.None, 0.0f);
                    }
                    break;
                case TankStates.DEFEATED:
                    Color blend = Color.Lerp(mColour, Color.SlateGray, (1.0f - (float)mDestructibleHealth / (float)MAX_DESTRUCTION_HEALTH) + 0.3f); // Greys the tank out more after each shot to provide visual feedback
                    pSpriteBatch.Draw(mBrokenTexture, Transform.Position, null, blend, Transform.Rotation, new Vector2(mBrokenTexture.Width / 2, mBrokenTexture.Height / 2), mResolutionScale, SpriteEffects.None, 0.0f);
                    break;
                case TankStates.DESTROYED:
                    break;
            }

            // Draw collision shape if enabled in DGS
            if (CollisionManager.DRAW_COLLISION_SHAPES)
            {
                DrawUtilities.DrawRectangle(pSpriteBatch, RectangleShape.ToRectangle(), Color.Magenta, Transform.Rotation, Transform.Position, 1.0f);
            }
        }

        public void DrawBullets(SpriteBatch pSpriteBatch, Texture2D pTexture)
        {
            foreach (Bullet bullet in mBullets)
            {
                bullet.Draw(pSpriteBatch, pTexture);
            }
        }

        #endregion

        #region Movement Methods

        public void Rotate(float pRotate)
        {
            mOldRotation = Transform.Rotation;
            Transform.Rotation += pRotate;
        }

        public void RotateCannonLeft(float pSeconds) => mCannonRotation -= BASE_TURRET_ROTATION_ANGLE * pSeconds;
        public void RotateCannonRight(float pSeconds) => mCannonRotation += BASE_TURRET_ROTATION_ANGLE * pSeconds;

        public void PutBack()
        {
            Transform.Position = mOldPosition;
            Transform.Rotation = mOldRotation;
        }

        public void OffsetPosition(Vector2 delta)
        {
            Transform.Position += delta;
        }

        public void Translate(float distance)
        {
            Vector3 translationVector = new Vector3(distance, 0, 0);
            translationVector = Vector3.Transform(translationVector, Matrix.CreateRotationZ(Transform.Rotation));
            translationVector *= mResolutionScale; // Scale the translation according to the tank's scale
            mOldPosition = Transform.Position;
            Transform.Position += new Vector2(translationVector.X, translationVector.Y);
        }

        #endregion

        #region Effects

        public void Fire(BulletType bullet)
        {
            if (mCurrentState == TankStates.DEFEATED || mCurrentState == TankStates.DESTROYED)
            {
                return;
            }

            mFired = BLAST_DELAY;
            float cannonRotation = GetCannonWorldRotation();
            Vector2 cannonDirection = new Vector2((float)Math.Cos(cannonRotation), (float)Math.Sin(cannonRotation));
            float cannonOffset = 50.0f * mResolutionScale;
            Vector2 endOfCannon = Transform.Position + cannonDirection * cannonOffset;
            if (bullet == BulletType.BOUNCY_EMP)
            {
                mBullets.Add(new BouncyEMPBullet(endOfCannon, cannonDirection * BULLET_SPEED * 1.5f, mColour, 20.0f));
                BulletType = BulletType.DEFAULT;
            }
            else if (bullet == BulletType.MINE)
            {
                float backwardRotation = Transform.Rotation + MathHelper.ToRadians(180);
                Vector2 backwardDirection = new Vector2((float)Math.Cos(backwardRotation), (float)Math.Sin(backwardRotation));
                float behindOffset = 65.0f * mResolutionScale;
                Vector2 behindTheTank = Transform.Position + backwardDirection * behindOffset;
                mBullets.Add(new MineBullet(behindTheTank, Vector2.Zero, mColour, 600.0f));
                BulletType = BulletType.DEFAULT;
            }
            else if (bullet == BulletType.BOUNCY_BULLET)
            {
                mBullets.Add(new BouncyBullet(endOfCannon, cannonDirection * BULLET_SPEED, mColour, 2.0f));
                BulletType = BulletType.DEFAULT;
            }
            else
            {
                mBullets.Add(new DefaultBullet(endOfCannon, cannonDirection * BULLET_SPEED, mColour, 1.0f));
            }
        }

        public void TakeDamage()
        {
            switch (mCurrentState)
            {
                case TankStates.ALIVE:
                    mHealth--;
                    if (mHealth <= 0)
                    {
                        mHealth = 0;
                        mCurrentState = TankStates.DEFEATED;
                    }
                    break;
                case TankStates.DEFEATED:
                    mDestructibleHealth--;
                    if (mDestructibleHealth <= 0)
                    {
                        mDestructibleHealth = 0;
                        mCurrentState = TankStates.DESTROYED;
                        Explode(100, 36);
                    }
                    break;
                case TankStates.DESTROYED:
                    break;
            }

        }

        /// <summary>
        /// Creates an explosion effect at the tank's position using particles, directions is the number of directions to emit particles in a circle.
        /// </summary>
        /// <param name="pTotalParticles"></param>
        /// <param name="pDirections"></param>
        public void Explode(int pTotalParticles, int pDirections)
        {
            Vector2 center = Transform.Position;
            int particlesPerDirection = pTotalParticles / pDirections;

            for (int i = 0; i < pDirections; i++)
            {
                float angle = i * (MathHelper.TwoPi / pDirections);
                Vector2 normal = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                ExplosionInitialisationPolicy explosion = new ExplosionInitialisationPolicy(center, normal, mColour);
                ParticleManager.Instance().InitialiseParticles(explosion, particlesPerDirection);
            }
        }

        #endregion

        #region Collisions

        public bool IsInsideShockwave()
        {
            if (mIsInsideShockwave)
            {
                mIsInsideShockwave = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handles collision detection and response for all bullets fired by this tank against a list of tanks and wall colliders.
        /// </summary>
        public void HandleBulletCollisions(List<Tank> pTanks, List<CollisionShape> pWallColliders)
        {
            for (int i = 0; i < mBullets.Count; ++i)
            {
                bool bulletRemoved = false;

                // Check and resolve bullet-tank collisions
                foreach (Tank tank in pTanks)
                {
                    if (tank.GetState() == TankStates.DESTROYED) continue; // Skips bullet collision with destroyed tanks

                    CollisionEvent collisionEvent = mBullets[i].CollisionShape.Intersects(tank.CollisionShape);

                    if(collisionEvent.HasCollided)
                    {
                        if (mBullets[i] is Shockwave)
                        {
                            tank.mIsInsideShockwave = true;
                        }
                        else if (mBullets[i].TankCollisionResponse(tank))
                        {
                            if (mBullets[i] is BouncyEMPBullet)
                            {
                                mBullets.Add(new Shockwave(mBullets[i].Position, Vector2.Zero, Color.Aqua, 5.0f));
                            }
                            mBullets.RemoveAt(i);
                            bulletRemoved = true;
                            break;
                        }
                    }
                }

                if (bulletRemoved) continue;

                if (DGS.Instance.GetBool("BULLET_BULLET_COLLISIONS"))
                {
                    foreach (Tank tank in pTanks)
                    {
                        bulletRemoved = CheckBulletBulletCollisions(tank, i);
                        if (bulletRemoved) break;
                    }
                }

                if (bulletRemoved) continue;

                // Check and resolve bullet-wall collisions (including play area)
                foreach (CollisionShape shape in pWallColliders)
                {
                    CollisionEvent collisionEvent = mBullets[i].CollisionShape.Intersects(shape);
                    if (collisionEvent.HasCollided)
                    {
                        if (mBullets[i].WallCollisionResponse(collisionEvent))
                        {
                            mBullets.RemoveAt(i);
                            bulletRemoved = true;
                            break;
                        }
                    }
                }

                if (bulletRemoved) continue;

                // Check bullet lifetime
                if (mBullets[i].LifeTimeExpired())
                {
                    mBullets.RemoveAt(i);
                }
            }
        }

        private bool CheckBulletBulletCollisions(Tank pOtherTank, int pBulletIndex)
        {
            for (int j = 0; j < pOtherTank.mBullets.Count; j++)
            {
                Bullet bulletA = mBullets[pBulletIndex];
                Bullet bulletB = pOtherTank.mBullets[j];
                // Skip if the bullets are the same instance (can happen when checking a tank's bullets against itself)
                if (bulletA == bulletB) continue;
                CollisionEvent bulletCollisionEvent = bulletA.CollisionShape.Intersects(bulletB.CollisionShape);
                if (bulletCollisionEvent.HasCollided)
                {
                    if (bulletA.BulletCollisionResponse(bulletB))
                    {
                        if (pOtherTank != this && bulletB.BulletCollisionResponse(bulletA))
                        {
                            pOtherTank.mBullets.RemoveAt(j);
                        }
                        mBullets.RemoveAt(pBulletIndex);
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        #region Track Movement Methods

        public void LeftTrackForward(float pSeconds)
        {
            Rotate(BASE_TANK_ROTATION_ANGLE * pSeconds);
            ChangeLeftTrackFrame(1);
            AdvancedTrackRotation(BASE_TANK_ROTATION_ANGLE * pSeconds, true);
        }
        public void RightTrackForward(float pSeconds)
        {
            Rotate(-BASE_TANK_ROTATION_ANGLE * pSeconds);
            AdvancedTrackRotation(-BASE_TANK_ROTATION_ANGLE * pSeconds, true);
            ChangeRightTrackFrame(1);
        }
        public void LeftTrackBackward(float pSeconds)
        {
            Rotate(-BASE_TANK_ROTATION_ANGLE * pSeconds);
            ChangeLeftTrackFrame(-1);
            AdvancedTrackRotation(-BASE_TANK_ROTATION_ANGLE * pSeconds, false);
        }
        public void RightTrackBackward(float pSeconds)
        {
            Rotate(BASE_TANK_ROTATION_ANGLE * pSeconds);
            ChangeRightTrackFrame(-1);
            AdvancedTrackRotation(BASE_TANK_ROTATION_ANGLE * pSeconds, false);
        }
        public void BothTracksForward(float pSeconds)
        {
            Translate(TANK_SPEED * pSeconds);
            ChangeLeftTrackFrame(1);
            ChangeRightTrackFrame(1);
        }
        public void BothTracksBackward(float pSeconds)
        {
            Translate(-TANK_SPEED * pSeconds);
            ChangeLeftTrackFrame(-1);
            ChangeRightTrackFrame(-1);
        }
        public void BothTracksOpposite(bool clockwise, float pSeconds)
        {
            float angle = 2 * BASE_TANK_ROTATION_ANGLE * pSeconds;
            angle = clockwise ? angle : -angle;
            Rotate(angle);

            ChangeLeftTrackFrame(clockwise ? 1 : -1);
            ChangeRightTrackFrame(clockwise ? -1 : 1);
            AdvancedTrackRotation(BASE_TANK_ROTATION_ANGLE * pSeconds, false);
        }

        private void AdvancedTrackRotation(float pAngle, bool pForwards)
        {
            float offsetSqrd = TRACK_OFFSET * TRACK_OFFSET;
            float arcLength = (float)Math.Sqrt(2 * offsetSqrd - 2 * offsetSqrd * Math.Cos(pAngle));
            arcLength = pForwards ? arcLength : arcLength * -1;
            Vector3 translationVector = new Vector3(arcLength, 0, 0);
            translationVector = Vector3.Transform(translationVector, Matrix.CreateRotationZ(Transform.Rotation));
            mOldPosition = Transform.Position;
            Transform.Position += new Vector2(translationVector.X, translationVector.Y);
        }

        private void ChangeLeftTrackFrame(int pAmount)
        {
            mLeftTrackFrame += pAmount;
            if (mLeftTrackFrame < 1)
            {
                mLeftTrackFrame = 14;
            }
            else if (mLeftTrackFrame > 14)
            {
                mLeftTrackFrame = 1;
            }

            Vector2[] tankCorners = new Vector2[4];
            GetCorners(tankCorners);
            Vector2 leftTopCorner = tankCorners[0];
            Vector2 leftBottomCorner = tankCorners[1];

            DustInitialisationPolicy dust = new DustInitialisationPolicy(leftTopCorner, leftBottomCorner);
            ParticleManager.Instance().InitialiseParticles(dust, 4);
        }

        private void ChangeRightTrackFrame(int pAmount)
        {
            mRightTrackFrame += pAmount;
            if (mRightTrackFrame < 1)
            {
                mRightTrackFrame = 14;
            }
            else if (mRightTrackFrame > 14)
            {
                mRightTrackFrame = 1;
            }

            Vector2[] tankCorners = new Vector2[4];
            GetCorners(tankCorners);
            Vector2 rightTopCorner = tankCorners[2];
            Vector2 rightBottomCorner = tankCorners[3];

            DustInitialisationPolicy dust = new DustInitialisationPolicy(rightTopCorner, rightBottomCorner);
            ParticleManager.Instance().InitialiseParticles(dust, 4);
        }

        #endregion
    }
}
