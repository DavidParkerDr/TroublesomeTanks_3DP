using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Tankontroller.Controller;

namespace Tankontroller.World.Particles
{
    public class Track
    {
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public Color Colour { get; set; }

        public Track(Vector2 pPosition, float pRotation, Color pColour)
        {
            Position = pPosition;
            Rotation = pRotation;
            Colour = pColour;
        }
    }
    public class TrackSystem
    {
        private static readonly Texture2D m_Texture = Tankontroller.Instance().CM().Load<Texture2D>("track");
        private static readonly TrackSystem m_Instance = new TrackSystem();
        private static readonly int EDGE_THICKNESS = DGS.Instance.GetInt("PARTICLE_EDGE_THICKNESS");

        private Track[] m_Tracks;
        private const int MAX_TRACKS = 250;
        private int m_NextTrack;

        public static TrackSystem GetInstance()
        {
            return m_Instance;
        }
        private TrackSystem()
        {
            m_Tracks = new Track[MAX_TRACKS];
            Reset();
        }

        public void Reset()
        {
            m_NextTrack = 0;
            for (int i = 0; i < MAX_TRACKS; i++)
            {
                m_Tracks[i] = new Track(Vector2.Zero, 0f, Color.Black);
            }
        }

        public void AddTrack(Vector2 pPosition, float pRotation, Color pColour)
        {
            m_Tracks[m_NextTrack].Colour = pColour;
            m_Tracks[m_NextTrack].Position = pPosition;
            m_Tracks[m_NextTrack].Rotation = pRotation;
            m_NextTrack++;
            if (m_NextTrack == MAX_TRACKS)
            {
                m_NextTrack = 0;
            }
        }

        public void Draw(SpriteBatch pBatch)
        {
            Vector2 origin = new Vector2((m_Texture.Width / 2f), (m_Texture.Height / 2f));
            float scale = Tankontroller.Instance().ScaleFactor();

            for (int i = 0; i < MAX_TRACKS; i++)
            {
                // Round positions to avoid sub-pixel sampling differences
                Vector2 basePos = new Vector2((float)Math.Round(m_Tracks[i].Position.X), (float)Math.Round(m_Tracks[i].Position.Y));

                // Draw outline by rendering the texture at pixel offsets around the base position.
                // This produces a crisp outline that lines up exactly with the foreground.
                for (int ox = -EDGE_THICKNESS; ox <= EDGE_THICKNESS; ox++)
                {
                    for (int oy = -EDGE_THICKNESS; oy <= EDGE_THICKNESS; oy++)
                    {
                        // skip the center pixel where the foreground will be drawn
                        if (ox == 0 && oy == 0) continue;

                        pBatch.Draw(m_Texture, basePos + new Vector2(ox, oy), null, Color.Black, m_Tracks[i].Rotation, origin, scale, SpriteEffects.None, 0.0f);
                    }
                }

                // Draw the foreground (main) track
                pBatch.Draw(m_Texture, basePos, null, m_Tracks[i].Colour, m_Tracks[i].Rotation, origin, scale, SpriteEffects.None,0.0f);
            }
        }
    }
}
