using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.Managers;
using Tankontroller.Utilities;
using Tankontroller.World.Bullets;
using Tankontroller.World.Particles;
using Tankontroller.World.Gameplay; // Add this using directive

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
    // Apply the IDeathRingTarget interface here
    public class Tank : IDeathRingTarget
    {
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



        private Vector2[] TANK_CORNERS = { 
            new Vector2(TANK_HEIGHT / 2 - TANK_FRONT_BUFFER, -TANK_WIDTH / 2), 
            new Vector2(-TANK_HEIGHT / 2, -TANK_WIDTH / 2), 
            new Vector2(-TANK_HEIGHT / 2, TANK_WIDTH / 2), 
            new Vector2(TANK_HEIGHT / 2 - TANK_FRONT_BUFFER, TANK_WIDTH / 2) };

        private List<Bullet> m_Bullets;

        private int m_Health;
        private int m_DestructibleHealth;
        public BulletType mbulletType { get; protected set; }

        private float mRotation;
        private float mOldRotation;
        private Vector3 mPosition;
        private Vector3 mOldPosition;

        private float mCannonRotation;
        private int mFired; // Number of frames since the player fired

        private Color mColour;
        private float m_Scale;
        private int m_LeftTrackFrame;
        private int m_RightTrackFrame;

        private bool mIsInsideShockwave = false; // Needed so that Player knows to deplete charge from shockwave

        private TankStates m_CurrentState = TankStates.ALIVE;

        public Tank(Vector2 pPos, float pRotation, float pScale) : this(pPos.X, pPos.Y, pRotation, pScale) { }

        public Tank(float pXPosition, float pYPosition, float pRotation, float pScale)
        {
            m_Health = MAX_HEALTH;
            mbulletType = BulletType.DEFAULT;
            m_DestructibleHealth = MAX_DESTRUCTION_HEALTH;

            mColour = Color.White;
            m_Bullets = new List<Bullet>();
            mFired = 0;
            m_LeftTrackFrame = 1;
            m_RightTrackFrame = 1;

            m_Scale = pScale;
            mPosition = new Vector3(pXPosition, pYPosition, 0);
            mRotation = pRotation;
            mCannonRotation = pRotation;
            mOldPosition = mPosition;
            mOldRotation = mRotation;
        }

        public void SetColour(Color pColour)
        {
            mColour = pColour;
        }

        private void ChangeLeftTrackFrame(int pAmount)
        {
            m_LeftTrackFrame += pAmount;
            if (m_LeftTrackFrame < 1)
            {
                m_LeftTrackFrame = 14;
            }
            else if (m_LeftTrackFrame > 14)
            {
                m_LeftTrackFrame = 1;
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
            m_RightTrackFrame += pAmount;
            if (m_RightTrackFrame < 1)
            {
                m_RightTrackFrame = 14;
            }
            else if (m_RightTrackFrame > 14)
            {
                m_RightTrackFrame = 1;
            }

            Vector2[] tankCorners = new Vector2[4];
            GetCorners(tankCorners);
            Vector2 rightTopCorner = tankCorners[2];
            Vector2 rightBottomCorner = tankCorners[3];

            DustInitialisationPolicy dust = new DustInitialisationPolicy(rightTopCorner, rightBottomCorner);
            ParticleManager.Instance().InitialiseParticles(dust, 4);
        }

        public int Health()
        {
            return m_Health;
        }
        public Color Colour()
        {
            return mColour;
        }

        public bool IsInsideShockwave()
        {
            if (mIsInsideShockwave)
            {
                mIsInsideShockwave = false;
                return true;
            }
            return false;
        }

        public void Rotate(float pRotate)
        {
            mOldRotation = mRotation;
            mRotation += pRotate;
        }

        public float GetRotation()
        {
            return mRotation;
        }

        public Vector2 GetWorldPosition()
        {
            Matrix localTransformation = Matrix.CreateRotationZ(mRotation) * Matrix.CreateTranslation(mPosition);
            Vector3 v = localTransformation.Translation;
            return new Vector2(v.X, v.Y);
        }

        public void Translate(float distance)
        {
            Vector3 translationVector = new Vector3(distance, 0, 0);
            translationVector = Vector3.Transform(translationVector, Matrix.CreateRotationZ(mRotation));
            translationVector *= m_Scale; // Scale the translation according to the tank's scale
            mOldPosition = mPosition;
            mPosition += translationVector;
        }

        public Vector2 GetIndexedCorner(int pIndex)
        {
            Vector3 temp = Vector3.Zero;
            temp.X = TANK_CORNERS[pIndex].X * m_Scale;
            temp.Y = TANK_CORNERS[pIndex].Y * m_Scale;
            temp = Vector3.Transform(temp, Matrix.CreateRotationZ(mRotation));
            temp = temp + mPosition;
            return new Vector2(temp.X, temp.Y);
        }

        public Vector2 GetRightFrontCorner()
        {
            return GetIndexedCorner(3);
        }

        public Vector2 GetLeftTrackMidpoint()
        {
            return GetLeftBackCorner();
        }

        public Vector2 GetRightTrackMidpoint()
        {
            return GetRightBackCorner();
        }

        public Vector2 GetRightBackCorner()
        {
            return GetIndexedCorner(2);
        }

        public Vector2 GetLeftFrontCorner()
        {
            return GetIndexedCorner(0);
        }

        public Vector2 GetLeftBackCorner()
        {
            return GetIndexedCorner(1);
        }

        public void GetCorners(Vector2[] pCorners)
        {
            if (pCorners.Length == 4)
            {
                Vector3 temp = Vector3.Zero;
                for (int i = 0; i < 4; i++)
                {
                    temp.X = TANK_CORNERS[i].X * m_Scale;
                    temp.Y = TANK_CORNERS[i].Y * m_Scale;
                    temp = Vector3.Transform(temp, Matrix.CreateRotationZ(mRotation));
                    temp = temp + mPosition;
                    pCorners[i].X = temp.X;
                    pCorners[i].Y = temp.Y;
                }
            }
        }

        public float GetCannonWorldRotation() { return mCannonRotation; }
        public Vector2 GetCannonWorldPosition() { return GetWorldPosition(); }

        public void CannonLeft(float pSeconds) { mCannonRotation -= BASE_TURRET_ROTATION_ANGLE * pSeconds; }
        public void CannonRight(float pSeconds) { mCannonRotation += BASE_TURRET_ROTATION_ANGLE * pSeconds; }


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
            translationVector = Vector3.Transform(translationVector, Matrix.CreateRotationZ(mRotation));
            mOldPosition = mPosition;
            mPosition += translationVector;
        }

        public bool PointIsInTank(Vector2 pPoint)
        {
            Vector2[] corners = new Vector2[4];
            GetCorners(corners);
            int i;
            int j;
            bool result = false;
            for (i = 0, j = corners.Length - 1; i < corners.Length; j = i++)
            {
                if ((corners[i].Y > pPoint.Y) != (corners[j].Y > pPoint.Y) &&
                    (pPoint.X < (corners[j].X - corners[i].X) * (pPoint.Y - corners[i].Y) / (corners[j].Y - corners[i].Y) + corners[i].X))
                {
                    result = !result;
                }
            }
            return result;
        }

        /// <summary>
        /// Checks if the tank's corners are within a certain radius of a point.
        /// Transforms the point into tank-local space and checks against an AABB.
        /// </summary>
        /// <returns> True if the bullet radius intersects the tank's collision shape (corners) </returns>
        public bool TankInRadius(float pRadius, Vector2 pPoint)
        {
            // Transform the world point into tank-local coordinates (centered on tank, unrotated).
            Vector2 tankCenter = new(mPosition.X, mPosition.Y);
            Vector2 localPoint = pPoint - tankCenter;
            localPoint = Vector2.Transform(localPoint, Matrix.CreateRotationZ(-mRotation));

            // Build the tank's local AABB from the unrotated, scaled corner definitions.
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            for (int i = 0; i < TANK_CORNERS.Length; ++i)
            {
                // TODO: Cache scaled tank corners for efficiency and update cache whenever tank is scaled
                float x = TANK_CORNERS[i].X * m_Scale;
                float y = TANK_CORNERS[i].Y * m_Scale;
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }

            // Find closest point on the AABB to the localPoint.
            float closestX = Math.Clamp(localPoint.X, minX, maxX);
            float closestY = Math.Clamp(localPoint.Y, minY, maxY);
            Vector2 closest = new(closestX, closestY);
            float effectiveRadius = Math.Max(0f, pRadius);

            // Compare squared distances
            float distanceSquared = Vector2.DistanceSquared(localPoint, closest);
            return distanceSquared <= (effectiveRadius * effectiveRadius);
        }

        public void Fire(BulletType bullet)
        {
            if(m_CurrentState == TankStates.DEFEATED || m_CurrentState == TankStates.DESTROYED)
            {
                return;
            }

            mFired = BLAST_DELAY;
            float cannonRotation = GetCannonWorldRotation();
            Vector2 cannonDirection = new Vector2((float)Math.Cos(cannonRotation), (float)Math.Sin(cannonRotation));
            float cannonOffset = 50.0f * m_Scale;
            Vector2 endOfCannon = GetCannonWorldPosition() + cannonDirection * cannonOffset;
            if (bullet == BulletType.BOUNCY_EMP)
            {
                m_Bullets.Add(new BouncyEMPBullet(endOfCannon, cannonDirection * BULLET_SPEED * 1.5f, mColour, 20.0f));
                mbulletType = BulletType.DEFAULT;
            }
            else if (bullet == BulletType.MINE)
            {
                float backwardRotation = mRotation + MathHelper.ToRadians(180);
                Vector2 backwardDirection = new Vector2((float)Math.Cos(backwardRotation), (float)Math.Sin(backwardRotation));
                float behindOffset = 50.0f * m_Scale;
                Vector2 behindTheTank = GetCannonWorldPosition() + backwardDirection * behindOffset;
                m_Bullets.Add(new MineBullet(behindTheTank, Vector2.Zero, mColour, 600.0f));
                mbulletType = BulletType.DEFAULT;
            }
            else if (bullet == BulletType.BOUNCY_BULLET)
            {
                m_Bullets.Add(new BouncyBullet(endOfCannon, cannonDirection * BULLET_SPEED, mColour, 2.0f));
                mbulletType = BulletType.DEFAULT;
            }
            else
            {
                m_Bullets.Add(new DefaultBullet(endOfCannon, cannonDirection * BULLET_SPEED, mColour, 1.0f));
            }
        }

        public void SetBulletType(BulletType pBulletType)
        {
            mbulletType = pBulletType;
        }

        public void PutBack()
        {
            mPosition = mOldPosition;
            mRotation = mOldRotation;
        }

        public void OffsetPosition(Vector2 delta)
        {
            mPosition += new Vector3(delta, 0);
        }

        public void CheckBullets(List<Tank> pTanks, Rectangle pPlayArea, List<RectWall> pWalls)
        {
            
            for (int i = 0; i < m_Bullets.Count; ++i)
            {
                bool bulletRemoved = false;

                // Check collision with tanks
                for (int j = 0; j < pTanks.Count; ++j)
                {
                    if (pTanks[j].GetState() == TankStates.DESTROYED) continue; // Skips bullet collision with destroyed tanks
                    
                    if (CollisionManager.Collide(m_Bullets[i], pTanks[j]))
                    {
                        if (m_Bullets[i] is BouncyEMPBullet)
                        {
                            m_Bullets[i].DoCollision(pTanks[j]);
                            m_Bullets.Add(new Shockwave(m_Bullets[i].Position, Vector2.Zero, Color.Aqua, 5.0f));
                            m_Bullets.RemoveAt(i);
                            bulletRemoved = true;
                            break;
                        }
                        if (m_Bullets[i] is Shockwave)
                        {
                            pTanks[j].mIsInsideShockwave = true;
                        }
                        if (m_Bullets[i].DoCollision(pTanks[j]))
                        {
                            m_Bullets.RemoveAt(i);
                            pTanks[j].TakeDamage();
                            bulletRemoved = true;
                            break;
                        }
                    }
                }

                if (bulletRemoved) continue;

                // Check collision with play area
                if (CollisionManager.Collide(m_Bullets[i], pPlayArea, true))
                {
                    if (m_Bullets[i].DoCollision(pPlayArea))
                    {
                        m_Bullets.RemoveAt(i);
                        continue;
                    }
                }

                // Check collision with walls
                for (int j = 0; j < pWalls.Count; ++j)
                {
                    if (CollisionManager.Collide(m_Bullets[i], pWalls[j].Rectangle, false))
                    {
                        if (m_Bullets[i].DoCollision(pWalls[j]))
                        {
                            m_Bullets.RemoveAt(i);
                            bulletRemoved = true;
                            break;
                        }
                    }
                }

                if (bulletRemoved) continue;

                // Check bullet lifetime
                if (m_Bullets[i].LifeTimeExpired())
                {
                    m_Bullets.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Determines whether the tank is currently in the alive state.
        /// </summary>
        /// <returns> Returns true if current tank state is alive, otherwise returns false</returns>
        public bool IsAlive()
        {
            return m_CurrentState == TankStates.ALIVE;
        }


        
        public void TakeDamage()
        {
            switch(m_CurrentState)
            {
                case TankStates.ALIVE:
                    m_Health--;
                    if (m_Health <= 0)
                    {
                        m_Health = 0;
                        m_CurrentState = TankStates.DEFEATED;
                    }
                    break;
                case TankStates.DEFEATED:
                    m_DestructibleHealth--;
                    if(m_DestructibleHealth <= 0)
                    {
                        m_DestructibleHealth = 0;
                        m_CurrentState = TankStates.DESTROYED;
                        Explode(100,36);
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
            Vector2 center = GetWorldPosition();
            int particlesPerDirection = pTotalParticles / pDirections;

            for (int i = 0; i < pDirections; i++)
            {
                float angle = i * (MathHelper.TwoPi / pDirections);
                Vector2 normal = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                ExplosionInitialisationPolicy explosion = new ExplosionInitialisationPolicy(center, normal, mColour);
                ParticleManager.Instance().InitialiseParticles(explosion, particlesPerDirection);
            }
        }

        /// <summary>
        /// Gets the current state of the tank (Alive, Defeated, Destroyed).
        /// </summary>
        /// <returns>Returns the enum value related to the tanks current state</returns>
        public TankStates GetState()
        {
            return m_CurrentState;
        }

        public void Heal()
        {
            m_Health++;
            if (m_Health > MAX_HEALTH)
            {
                m_Health = MAX_HEALTH;
            }
        }


        public void Update(float pSeconds)
        {
            if (mFired > 0)
            {
                mFired--;
            }
            foreach (Bullet bullet in m_Bullets)
            {
                bullet.Update(pSeconds);
            }
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            Rectangle trackRect = new Rectangle(0, 0, mLeftTrackTexture.Width, mLeftTrackTexture.Height / 15);
            switch(m_CurrentState)
            {
                case TankStates.ALIVE:
                    trackRect.Y = m_LeftTrackFrame * mLeftTrackTexture.Height / 15;
                    pSpriteBatch.Draw(mLeftTrackTexture, GetWorldPosition(), trackRect, mColour, mRotation, new Vector2(mBaseTexture.Width / 2, mBaseTexture.Height / 2), m_Scale, SpriteEffects.None, 0.0f);
                    trackRect.Y = m_RightTrackFrame * mLeftTrackTexture.Height / 15;
                    pSpriteBatch.Draw(mRightTrackTexture, GetWorldPosition(), trackRect, mColour, mRotation, new Vector2(mBaseTexture.Width / 2, mBaseTexture.Height / 2), m_Scale, SpriteEffects.None, 0.0f);
                    pSpriteBatch.Draw(mBaseTexture, GetWorldPosition(), null, mColour, mRotation, new Vector2(mBaseTexture.Width / 2, mBaseTexture.Height / 2), m_Scale, SpriteEffects.None, 0.0f);
                    if (mFired == 0)
                    {
                        pSpriteBatch.Draw(mCannonTexture, GetCannonWorldPosition(), null, mColour, mCannonRotation, new Vector2(mCannonTexture.Width / 2, mCannonTexture.Height / 2), m_Scale, SpriteEffects.None, 0.0f);
                    }
                    else
                    {
                        pSpriteBatch.Draw(mCannonFireTexture, GetCannonWorldPosition(), null, mColour, mCannonRotation, new Vector2(mCannonTexture.Width / 2, mCannonTexture.Height / 2), m_Scale, SpriteEffects.None, 0.0f);
                    }
                    break;
                case TankStates.DEFEATED:
                    Color blend = Color.Lerp(mColour, Color.SlateGray, (1.0f - (float)m_DestructibleHealth/(float)MAX_DESTRUCTION_HEALTH) + 0.3f); // Greys the tank out more after each shot to provide visual feedback
                    pSpriteBatch.Draw(mBrokenTexture, GetWorldPosition(), null, blend, GetRotation(), new Vector2(mBrokenTexture.Width / 2, mBrokenTexture.Height / 2), m_Scale, SpriteEffects.None, 0.0f);
                    break;
                case TankStates.DESTROYED: 
                    break;
            }

            // Draw collision shape if enabled in DGS
            if (CollisionManager.DRAW_COLLISION_SHAPES)
            {
                DrawUtilities.DrawRectangle(pSpriteBatch, ConvertUtilities.ToRectangle(TANK_CORNERS), Color.Magenta, mRotation, GetWorldPosition(), m_Scale);
            }
        }

        public void DrawBullets(SpriteBatch pSpriteBatch, Texture2D pTexture)
        {
            foreach (Bullet bullet in m_Bullets)
            {
                bullet.Draw(pSpriteBatch, pTexture);
            }
        }
    }
}
