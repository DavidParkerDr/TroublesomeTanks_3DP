using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.World;
using Tankontroller.World.Particles;
using Tankontroller.Utilities;

namespace Tankontroller.World.Gameplay
{
    public interface IDeathRingTarget
    {
        Vector2 GetWorldPosition();
        void TakeDamage();
    }

    internal class DeathRing
    {
        // Config 
        private readonly float ACTIVATION_TIME;    // seconds before match end to activate
        private readonly float DURATION;          // how long shrink lasts
        private readonly float START_RADIUS;       
        private readonly float END_RADIUS;         
        private readonly float DAMAGE_PER_SECOND;   // DPS applied outside safe zone
        private readonly float GRACE_SECONDS;      // Seconds a tank can survive outside the ring before the first "tick" of damage.
        private readonly float DEATH_ZONE_MASK_SIZE = 30f;     //draw the red transparent "death zone" (outside the dafe zone)
        private readonly Vector2 CENTER;            // Center of the ring (calculated from play area)

        // State
        private float mElapsedSinceStart = 0f;      // Seconds since ring activation
        private float mSafeZoneRadius;              
        private bool mIsRingActive = false;                       
        

        // Per-tank state
        private readonly Dictionary<IDeathRingTarget, float> OUTSIDE_TIME = new();
        private readonly Dictionary<IDeathRingTarget, float> DAMAGE_ACCUMULATOR = new();


        public DeathRing(Rectangle playArea)
        {
            /// <summary>
            /// sets a float from DGS or returns default if not found or invalid
            /// </summary>
            float SafeFloat(string key, float def)
            {
                try { return DGS.Instance.GetFloat(key); } catch { return def; }
            }

            ACTIVATION_TIME = SafeFloat("DEATH_RING_ACTIVATION_TIME", 45f);
            DURATION = SafeFloat("DEATH_RING_DURATION", Math.Max(1f, ACTIVATION_TIME));
            DAMAGE_PER_SECOND = SafeFloat("DEATH_RING_DPS", 10f);
            GRACE_SECONDS = SafeFloat("DEATH_RING_GRACE", 1f);
            
            CENTER = new Vector2(playArea.X + playArea.Width / 2f, playArea.Y + playArea.Height / 2f);

            float defaultStart = MathF.Max(playArea.Width, playArea.Height) * 1.2f;
            float configuredStart = SafeFloat("DEATH_RING_START_RADIUS", defaultStart);
            START_RADIUS = configuredStart > 0 ? configuredStart : defaultStart;

            float defaultEnd = MathF.Max(playArea.Width, playArea.Height) * 0.3f;
            float configuredEnd = SafeFloat("DEATH_RING_END_RADIUS", defaultEnd);
            END_RADIUS = configuredEnd > 0 ? configuredEnd : defaultEnd;  

            mSafeZoneRadius = START_RADIUS;
        }

        /// <summary>
        /// Update ring state. Call every frame.
        /// - deltaSeconds: frame delta
        /// </summary>
        public void Update(float deltaSeconds, float remainingMatchSeconds, IEnumerable<IDeathRingTarget> tanks)
        {
            if (!mIsRingActive)
            {
                if (remainingMatchSeconds <= ACTIVATION_TIME)
                {
                    mIsRingActive = true;
                    // Align elapsed so the ring progress corresponds to time since activation
                    mElapsedSinceStart = MathF.Max(0f, ACTIVATION_TIME - remainingMatchSeconds);
                }
                else
                {
                    return;
                }
            }

            // Progress shrink (linear), clamped to not shrink below endRadius
            mElapsedSinceStart += deltaSeconds;
            float t = (DURATION <= 0f) ? 1f : MathF.Min(1f, mElapsedSinceStart / DURATION);

            // Calculates the current radius
            float baseRadius = MathHelper.Lerp(START_RADIUS, END_RADIUS, t);
            mSafeZoneRadius = MathHelper.Clamp(baseRadius, END_RADIUS, START_RADIUS);

            // Damage application: continuous DPS after grace
            foreach (var tank in tanks)
            {
                if (tank == null) continue;
                Vector2 tankPos = tank.GetWorldPosition();
                float dist = Vector2.Distance(tankPos, CENTER);
                bool outside = dist > mSafeZoneRadius;

                if (outside)
                {
                    if (!OUTSIDE_TIME.ContainsKey(tank)) OUTSIDE_TIME[tank] = 0f;
                    OUTSIDE_TIME[tank] += deltaSeconds;

                    if (OUTSIDE_TIME[tank] >= GRACE_SECONDS)
                    {
                        if (!DAMAGE_ACCUMULATOR.ContainsKey(tank)) DAMAGE_ACCUMULATOR[tank] = 0f;
                        DAMAGE_ACCUMULATOR[tank] += DAMAGE_PER_SECOND * deltaSeconds;

                        float acc = DAMAGE_ACCUMULATOR[tank];
                        int wholeHits = (int)MathF.Floor(acc);
                        if (wholeHits > 0)
                        {
                            for (int i = 0; i < wholeHits; i++)
                            {
                                tank.TakeDamage();
                            }
                            DAMAGE_ACCUMULATOR[tank] = acc - wholeHits;
                        }
                    }
                }
                else
                {
                    // Reset when tank returns inside
                    if (OUTSIDE_TIME.ContainsKey(tank)) OUTSIDE_TIME.Remove(tank);
                    if (DAMAGE_ACCUMULATOR.ContainsKey(tank)) DAMAGE_ACCUMULATOR.Remove(tank);
                }
            }

            // Prune state for tanks that no longer exist in supplied list (avoid leaks)
            var toRemove = OUTSIDE_TIME.Keys.Where(k => !tanks.Contains(k)).ToList();
            foreach (var k in toRemove)
            {
                OUTSIDE_TIME.Remove(k);
                DAMAGE_ACCUMULATOR.Remove(k);
            }
        }

        /// <summary>
        /// Draw ring visuals. Call between SpriteBatch.Begin/End.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!mIsRingActive) return;

            // color for ring (RGBA). adjust alpha to taste.
            Color ringColor = new Color(200, 30, 30, 200);

            DrawUtilities.DrawDeathZone(
                spriteBatch,
                CENTER,
                mSafeZoneRadius,
                ringColor,
                DEATH_ZONE_MASK_SIZE
            );

            //DEBUG Circle to show inner edge (safe zone boundary)
            //DrawUtilities.DrawCircle(spriteBatch, CENTER, mCurrentRadius, Color.Blue * 0.5f);
        }

        // Getters for external use
        public bool IsActive() => mIsRingActive;
        public float CurrentRadius => mSafeZoneRadius;
        public Vector2 Center => CENTER;
    }
}