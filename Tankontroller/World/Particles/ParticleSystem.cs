using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Tankontroller.World.Particles
{
    public class Particle
    {
        public static readonly int EDGE_THICKNESS = DGS.Instance.GetInt("PARTICLE_EDGE_THICKNESS");
        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; private set; }
        public float Radius { get; private set; }
        public float RadiusIncreaseRate { get; private set; }
        public Color Colour { get; private set; }
        public float TimeTillDead { get; private set; }
        /// <summary>
        /// Initiates all values.
        /// </summary>
        /// <param name="pPosition">Initial position of particle</param>
        /// <param name="pVelocity">Initial velocity of particle</param>
        /// <param name="pRadius">Initial radius of particle</param>
        /// <param name="pRadiusIncreaseRate">Rate of change of radius</param>
        /// <param name="pColour">Particle colour</param>
        /// <param name="pTimeTillDead">Time until particle will be recycled</param>
        public void Initiate(Vector2 pPosition, Vector2 pVelocity, float pRadius, float pRadiusIncreaseRate, Color pColour, float pTimeTillDead)
        {
            Position = pPosition;
            Velocity = pVelocity;
            Radius = pRadius;
            RadiusIncreaseRate = pRadiusIncreaseRate;
            Colour = pColour;
            TimeTillDead = pTimeTillDead;
        }

        /// <summary>
        /// Update particle
        /// </summary>
        /// <param name="pSeconds">delta time (seconds)</param>
        /// <returns>Particle still active</returns>
        public bool Update(float pSeconds)
        {
            Position = Position + Velocity * pSeconds;
            Radius = Radius + RadiusIncreaseRate * pSeconds;
            TimeTillDead = TimeTillDead - pSeconds;
            return TimeTillDead > 0;
        }

        /// <summary>
        /// Draws a circle. Used to draw the particle.
        /// </summary>
        /// <param name="pBatch">SpriteBatch to draw to</param>
        /// <param name="pTexture">Texture to draw</param>
        /// <param name="pRadius">Radius of circle</param>
        /// <param name="pPos">Position of circle centre</param>
        /// <param name="pColour">Colour of circle</param>
        static public void DrawCircle(SpriteBatch pBatch, Texture2D pTexture, int pRadius, Vector2 pPos, Color pColour)
        {
            Rectangle rectangle = new Rectangle();
            rectangle.Width = pRadius * 2;
            rectangle.Height = pRadius * 2;
            rectangle.X = (int)pPos.X - pRadius;
            rectangle.Y = (int)pPos.Y - pRadius;
            pBatch.Draw(pTexture, rectangle, pColour);
        }

        /// <summary>
        /// Draws the background of the particle.
        /// </summary>
        /// <param name="pBatch"></param>
        /// <param name="pTexture"></param>
        public void DrawBackground(SpriteBatch pBatch, Texture2D pTexture)
        {
            DrawCircle(pBatch, pTexture, (int)Radius + 2 * EDGE_THICKNESS, Position, Color.Black);
        }

        /// <summary>
        /// Draws the foreground particle.
        /// </summary>
        /// <param name="pBatch"></param>
        /// <param name="pTexture"></param>
        public void DrawForeground(SpriteBatch pBatch, Texture2D pTexture)
        {
            DrawCircle(pBatch, pTexture, (int)Radius, Position, Colour);
        }
    }

    /// <summary>
    /// The ParticleManager singleton manages an array of particle objects.
    /// Particle systems are created by passing an InitialisationPolicy object to the InitialiseParticles method.
    /// ParticleManager also updates and draws the particles.
    /// </summary>
    public class ParticleManager
    {
        private const int MAX_PARTICLES = 1000;
        private static readonly Texture2D m_Texture = Tankontroller.Instance().CM().Load<Texture2D>("circle");
        private Particle[] m_Particles = null;
        private int m_LastParticleIndex = 0;

        private static ParticleManager m_Instance = null;
        private ParticleManager()
        {
            m_Particles = new Particle[MAX_PARTICLES];
            Reset();
        }

        public void Reset()
        {
            for (int i = 0; i < m_Particles.Length; i++)
            {
                m_Particles[i] = new Particle();
            }
        }

        /// <summary>
        /// Singleton instanct of ParticleManager
        /// </summary>
        /// <returns>The instance of ParticleManager</returns>
        public static ParticleManager Instance()
        {
            if (m_Instance == null)
            {
                m_Instance = new ParticleManager();
            }
            return m_Instance;
        }

        /// <summary>
        /// Updates all active particles. Any particle that is no longer active is recycled
        /// </summary>
        /// <param name="pSeconds">delta time (seconds)</param>
        public void Update(float pSeconds)
        {
            for (int i = 0; i < m_LastParticleIndex; i++)
            {
                if (!m_Particles[i].Update(pSeconds))
                {
                    Particle temp = m_Particles[i];
                    m_Particles[i] = m_Particles[m_LastParticleIndex - 1];
                    m_Particles[m_LastParticleIndex - 1] = temp;
                    i = i - 1;
                    m_LastParticleIndex--;
                }
            }
        }

        /// <summary>
        /// Allocates number of particles to be initialised
        /// </summary>
        /// <param name="pNumberOfParticles">Requested number of particles to be allocated</param>
        /// <returns>Particles allocated</returns>
        private Particle[] GetParticles(int pNumberOfParticles)
        {
            Particle[] particles;

            int availableParticles = m_Particles.Length - m_LastParticleIndex;

            if (availableParticles > pNumberOfParticles)
            {
                particles = new Particle[pNumberOfParticles];
            }
            else
            {
                particles = new Particle[availableParticles];
            }

            int i = 0;
            for (; i < particles.Length; i++)
            {
                particles[i] = m_Particles[m_LastParticleIndex + i];
            }
            m_LastParticleIndex += i;
            return particles;
        }



        /// <summary>
        /// Requests number of particles and initialises according to the ParticleInitialisationPolicy parameter
        /// </summary>
        /// <param name="pInitialisationPolicy">Policy used to initialise particles</param>
        /// <param name="pNumberOfParticles">Requested number of particles</param>
        public void InitialiseParticles(IParticleInitialisationPolicy pInitialisationPolicy, int pNumberOfParticles)
        {
            pInitialisationPolicy.InitiateParticles(GetParticles(pNumberOfParticles));
        }

        /// <summary>
        /// Draws all particles with black border.
        /// </summary>
        /// <param name="pBatch">SpriteBatch to use to draw with.</param>
        public void Draw(SpriteBatch pBatch)
        {
            for (int i = 0; i < m_LastParticleIndex; i++)
            {
                m_Particles[i].DrawBackground(pBatch, m_Texture);
            }
            for (int i = 0; i < m_LastParticleIndex; i++)
            {
                m_Particles[i].DrawForeground(pBatch, m_Texture);
            }
        }
    }

    /// <summary>
    /// Interface describes how initial values of particle should be set
    /// </summary>
    public interface IParticleInitialisationPolicy
    {
        void InitiateParticles(Particle[] pParticles);
    }

    public class ExplosionInitialisationPolicy : IParticleInitialisationPolicy
    {
        private Vector2 m_Position;
        private Vector2 m_Normal;
        private Color m_Colour;
        private Random m_Rng = new Random();

        /// <summary>
        /// ExplosionInitialisationPolicy creates particles at the point of explision travelling in a 
        /// semicircle around the collision normal.
        /// </summary>
        /// <param name="pPosition">Central position of explosion.</param>
        /// <param name="pNormal">Normal provides "direction" for explosion.</param>
        /// <param name="pColour">Colour of all particles.</param>
        public ExplosionInitialisationPolicy(Vector2 pPosition, Vector2 pNormal, Color pColour)
        {
            m_Position = pPosition;
            m_Normal = pNormal;
            m_Colour = pColour;
        }

        public void InitiateParticles(Particle[] pParticles)
        {
            for (int i = 0; i < pParticles.Length; i++)
            {
                float lifetime = m_Rng.Next(1000, 2001) * 0.00015f;
                Vector2 velocity = Vector2.Transform(m_Normal, Matrix.CreateRotationZ((float)(m_Rng.NextDouble() * Math.PI - Math.PI / 2))) * m_Rng.Next(90, 121);
                pParticles[i].Initiate(m_Position, velocity, m_Rng.Next(1, 7), m_Rng.Next(25, 46), m_Colour, lifetime);
            }
        }
    }

    public class DustInitialisationPolicy : IParticleInitialisationPolicy
    {
        //private static readonly Color COLOUR_DUST = DGS.Instance.GetColour("COLOUR_DUST");
        private Vector2 m_Point1;
        private Vector2 m_Point2;
        private Random m_Rng = new Random();
        private Color[] m_Colours = new Color[4];

        /// <summary>
        /// DustInitialisationPolicy creates particles at a random point on a line between two points.
        /// </summary>
        /// <param name="pPoint1">Start point of line.</param>
        /// <param name="pPoint2">End point of line.</param>
        public DustInitialisationPolicy(Vector2 pPoint1, Vector2 pPoint2)
        {
            m_Point1 = pPoint1;
            m_Point2 = pPoint2;
            m_Colours[0] = Color.LightGray;
            m_Colours[1] = new Color(200, 200, 200, 255);
            m_Colours[2] = new Color(190, 190, 190, 255);
            m_Colours[3] = new Color(175, 175, 175, 255);
        }

        public void InitiateParticles(Particle[] pParticles)
        {
            for (int i = 0; i < pParticles.Length; i++)
            {
                Vector2 position = m_Point1 + (m_Point2 - m_Point1) * (float)m_Rng.NextDouble();
                float lifetime = m_Rng.Next(250, 751) * 0.001f;
                pParticles[i].Initiate(position, Vector2.Zero, 0.5f, m_Rng.Next(5, 15), m_Colours[m_Rng.Next(4)], lifetime);
            }
        }
    }

    public class BulletInitialisationPolicy : IParticleInitialisationPolicy
    {
        private Vector2 m_Position;
        private Color m_Colour;
        private Random m_Rng = new Random();

        public BulletInitialisationPolicy(Vector2 pPosition, Color pColour)
        {
            m_Position = pPosition;
            m_Colour = pColour;
        }

        public void InitiateParticles(Particle[] pParticles)
        {
            for (int i = 0; i < pParticles.Length; i++)
            {
                float lifetime = m_Rng.Next(1000, 2001) * 0.0001f;
                Vector2 position = m_Position;
                position.X += m_Rng.Next(-2, 2);
                position.Y += m_Rng.Next(-2, 2);
                pParticles[i].Initiate(position, Vector2.Zero, m_Rng.Next(1, 2), 0, m_Colour, lifetime);
            }
        }
    }

    public class EMPBlastInitPolicy : IParticleInitialisationPolicy
    {
        private Vector2 m_Position;
        private float m_LifeTime;
        //private float m_VelocityScale;
        private Random m_Rng = new Random();
        private Color[] m_Colours = new Color[2];

        public EMPBlastInitPolicy(Vector2 pPosition, float pLifeTime)
        {
            m_Position = pPosition;
            m_LifeTime = pLifeTime;
            m_Colours[0] = Color.Yellow;
            m_Colours[1] = Color.Gold;
        }

        public void InitiateParticles(Particle[] pParticles)
        {
            for (int i = 0; i < pParticles.Length; i++)
            {
                m_LifeTime = m_LifeTime * (float)(1.0 - (m_Rng.NextDouble() * 0.01));
                Vector2 velocity = Vector2.Transform(new Vector2(0, 1), Matrix.CreateRotationZ((float)(m_Rng.NextDouble() * 2 * Math.PI))) * m_Rng.Next(90, 121);
                // normalize the velocity
                velocity.Normalize();
                // apply speed
                velocity = velocity * 30;
                pParticles[i].Initiate(m_Position, velocity, m_Rng.Next(1, 7), m_Rng.Next(0, 2), m_Colours[m_Rng.Next(2)], m_LifeTime);
            }
        }
    }
    public class MineBlastInitPolicy : IParticleInitialisationPolicy
    {
        private Vector2 m_Position;
        private float m_LifeTime;
        private Random m_Rng = new Random();
        private Color[] m_SmokeColours = new Color[2];
        private Color[] m_BlastColours = new Color[2];

        public MineBlastInitPolicy(Vector2 pPosition, float pLifeTime)
        {
            m_Position = pPosition;
            m_LifeTime = pLifeTime;
            m_SmokeColours[0] = Color.DarkSlateGray;
            m_SmokeColours[1] = new Color(47,50,50,1);
            m_BlastColours[0] = Color.Orange;
            m_BlastColours[1] = Color.MonoGameOrange;
        }

        public void InitiateParticles(Particle[] pParticles)
        {
            for (int i = 0; i < pParticles.Length; i++)
            {
                m_LifeTime = m_LifeTime * (float)(1.0 - (m_Rng.NextDouble() * 0.01));
                Vector2 velocity = Vector2.Transform(new Vector2(0, 1), Matrix.CreateRotationZ((float)(m_Rng.NextDouble() * 2 * Math.PI))) * m_Rng.Next(90, 121);
                velocity.Normalize();
               
                if (i <= (pParticles.Length / 2))
                {
                    velocity = velocity * 40;
                    pParticles[i].Initiate(m_Position, velocity, m_Rng.Next(1, 7), m_Rng.Next(15, 25), m_SmokeColours[m_Rng.Next(2)], m_LifeTime);
                }
                else
                {
                    velocity = velocity * 20;
                    pParticles[i].Initiate(m_Position, velocity, m_Rng.Next(0, 6), m_Rng.Next(10, 20), m_BlastColours[m_Rng.Next(2)], m_LifeTime + 0.4f);
                }

            }
        }
    }
}