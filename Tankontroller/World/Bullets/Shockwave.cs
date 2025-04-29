using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.World.Particles;

namespace Tankontroller.World.Bullets
{
    class Shockwave : Bullet
    {
        private static readonly Texture2D ShockWaveTexture = Tankontroller.Instance().CM().Load<Texture2D>("Shockwave");
        public Shockwave(Vector2 pPosition, Vector2 pVelocity, Color pColour, float pLifeTime) : base(pPosition, pVelocity, pColour, pLifeTime) { }
        public override void Update(float pSeconds)
        {
            Radius += 0.5f;
            LifeTime -= pSeconds;
            base.Update(pSeconds);
        }
        public override bool DoCollision(Rectangle pRectangle)
        {
            return false;
        }
        public override bool DoCollision(RectWall pWall)
        {
            return false;
        }

        public override bool DoCollision(Tank pTank)
        {
            return false;
        }
        public override bool DoCollision(Bullet pBullet)
        {
            return false;
        }
        public override bool LifeTimeExpired()
        {
            return (LifeTime <= 0);
        }
        public override void Draw(SpriteBatch pBatch, Texture2D pTexture)
        {
            Color ShockWaveColour = Color.Yellow;
            ShockWaveColour.A = (byte)(0.0f);
            Particle.DrawCircle(pBatch, ShockWaveTexture, (int)Radius, Position, ShockWaveColour);
        }
    }
}
