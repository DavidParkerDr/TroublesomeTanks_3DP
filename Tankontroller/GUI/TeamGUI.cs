using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.Controller;
using Tankontroller.World;

namespace Tankontroller.GUI
{
    //-------------------------------------------------------------------------------------------------
    // TeamGUI
    //
    //-------------------------------------------------------------------------------------------------
    public class TeamGUI
    {
        private HealthBar m_HealthBar;
        private Avatar m_Avatar;
        private PowerBar[] m_PowerBars = new PowerBar[8];
        private JackIcon[] m_JackIcons = new JackIcon[8];
        private PortNumLabel[] m_PortNumLabels = new PortNumLabel[8];
        private IController mController;
        private Color m_Color { get; set; }
        private BulletType m_BulletType { get; set; }

        private int screenWidth = Tankontroller.Instance().GDM().GraphicsDevice.Viewport.Width;
        private int screenHeight = Tankontroller.Instance().GDM().GraphicsDevice.Viewport.Height;

        public TeamGUI(Avatar pAvatar, Rectangle pRectangle, IController pController)
        {
            mController = pController;
            m_Avatar = pAvatar;
            m_Color = pAvatar.GetColour();
            m_Avatar.Reposition(GetAvatarRect(pRectangle));
            m_HealthBar = new HealthBar(Tank.MAX_HEALTH, GetHealthBarRect(pRectangle));
            RepositionPowerBar(pRectangle);
        }

        private void RepositionPowerBar(Rectangle pRectangle)
        {
            int powerBarWidth = screenWidth / 4  * 9 / 16  / 8;
            int powerBarHeight = screenHeight / 100 * 14;

            int powerBar_yValueOffset = -10 + screenWidth / 100 * 2;
            int jackIcons_yValueOffset = powerBar_yValueOffset + (int)(powerBarHeight * 1.01f) - powerBarWidth;
            int labels_yValueOffset = powerBar_yValueOffset + (int)(powerBarWidth * 1.01f);

            bool isOnLeft = true;
            int xValue = screenWidth / 80 * 3;
            int xIncrement = Convert.ToInt32(powerBarWidth * 1.2);
            int xOffset = isOnLeft ? pRectangle.X + pRectangle.Width - 8 * xIncrement - powerBarWidth : pRectangle.X;
            for (int j = 0; j < 7; j++)
            {
                m_PowerBars[j] = new PowerBar(new Vector2(xOffset + xValue, powerBar_yValueOffset), powerBarWidth, powerBarHeight);
                m_JackIcons[j] = new JackIcon(new Vector2(xOffset + xValue, jackIcons_yValueOffset), powerBarWidth, powerBarWidth);
                m_PortNumLabels[j] = new PortNumLabel(new Vector2(xOffset + xValue, labels_yValueOffset), powerBarWidth, powerBarWidth);
                xValue += xIncrement;
            }
        }

        private Rectangle GetAvatarRect(Rectangle pRectangle)
        {
            int avatarWidth = (int)(pRectangle.Width * 0.4);
            int avatarHeight = pRectangle.Height;
            return new Rectangle(pRectangle.X, pRectangle.Y, avatarWidth, avatarHeight);
        }
        private Rectangle GetHealthBarRect(Rectangle pRectangle)
        {
            int healthBarWidth = (int)(pRectangle.Width * 0.6);
            int healthBarHeight = (int)(pRectangle.Height * 0.25);
            int healthBarLeft = pRectangle.Left + pRectangle.Width - healthBarWidth;
            int healthBarTop = pRectangle.Top + pRectangle.Height - healthBarHeight;
            return new Rectangle(healthBarLeft, healthBarTop, healthBarWidth, healthBarHeight);
        }

        public void DrawHealthBar(SpriteBatch pSpriteBatch, int pHealth)
        {
            m_HealthBar.Draw(pSpriteBatch, pHealth);
        }

        public void Reposition(Rectangle pRectangle)
        {
            m_Avatar.Reposition(GetAvatarRect(pRectangle));
            m_HealthBar.Reposition(GetHealthBarRect(pRectangle));
            RepositionPowerBar(pRectangle);
        }

        public void RepositionForGameOver(Rectangle pRectangle)
        {
            Rectangle healthRect = new Rectangle(new Point(pRectangle.X + (pRectangle.Width / 3), pRectangle.Y + (pRectangle.Height - 15)), new Point(pRectangle.Width, pRectangle.Height / 6));
            m_HealthBar.Reposition(healthRect);
            m_Avatar.Reposition(pRectangle);
        }

        public void DrawAvatar(SpriteBatch pSpriteBatch, int pHealth)
        {
            float avatarIndex = Tank.MAX_HEALTH / 2.0f;
            int roundedUp = (int)Math.Ceiling(avatarIndex); // rounds up
            //m_Avatar.Draw(pSpriteBatch, pHealth > 0, avatarIndex);
            if (pHealth == 1)
            {
                m_Avatar.Draw(pSpriteBatch, pHealth > 0, 2);
            }
            else if (pHealth <= roundedUp) //below half but above 1 
            {
                m_Avatar.Draw(pSpriteBatch, pHealth > 0, 1);
            }
            else if (pHealth > roundedUp) //above half
            {
                m_Avatar.Draw(pSpriteBatch, pHealth > 0, 0);
            }
        }

        public void DrawHeldBullet(SpriteBatch pSpriteBatch, BulletType pBulletType)
        {
            //TODO: fix the positons of the circle and the bullet
            float scaler = ((float)screenWidth / 200f);
            Vector2 pos = new Vector2(m_Avatar.m_Rectangle.X + m_Avatar.m_Rectangle.Width - (3.5f * scaler), m_Avatar.m_Rectangle.Y + m_Avatar.m_Rectangle.Height - (3.5f * scaler));
            float radius = 6.5f * scaler;
            DrawCircle(pSpriteBatch, Tankontroller.Instance().CM().Load<Texture2D>("BulletSlot"), (int)radius, pos, Color.White);
            if (pBulletType == BulletType.BOUNCY_EMP)
            {
                DrawCircle(pSpriteBatch, Tankontroller.Instance().CM().Load<Texture2D>("EMP"), (int)radius, pos, Color.White);
            }
            else if (pBulletType == BulletType.MINE)
            {
                DrawCircle(pSpriteBatch, Tankontroller.Instance().CM().Load<Texture2D>("MinePickup"), (int)radius, pos, Color.White);
            }
            else if (pBulletType == BulletType.BOUNCY_BULLET)
            {
                DrawCircle(pSpriteBatch, Tankontroller.Instance().CM().Load<Texture2D>("BouncyBulletPickup"), (int)radius, pos, Color.White);
            }
            else
            {
                DrawCircle(pSpriteBatch, Tankontroller.Instance().CM().Load<Texture2D>("Empty"), (int)radius, pos, Color.White);
            }
        }
        public void DrawCircle(SpriteBatch pBatch, Texture2D pTexture, int pRadius, Vector2 pPos, Color pColour)
        {
            Rectangle rectangle = new Rectangle();
            rectangle.Width = pRadius;
            rectangle.Height = pRadius;
            rectangle.X = (int)pPos.X - pRadius / 2;
            rectangle.Y = (int)pPos.Y - pRadius / 2;
            pBatch.Draw(pTexture, rectangle, pColour);
        }

        public void Draw(SpriteBatch pSpriteBatch, int pHealth, BulletType pBulletType)
        {
            DrawAvatar(pSpriteBatch, pHealth);
            DrawHealthBar(pSpriteBatch, pHealth);
            for (int port = 0; port < 7; port++)
            {
                float currentCharge = mController.GetJackCharge(port);
                Control currentControl = mController.GetJackControl(port);
                bool hasCharge = currentControl == Control.FIRE ? currentCharge >= Player.BULLET_CHARGE_DEPLETION : currentCharge > 0;

                m_PowerBars[port].Draw(pSpriteBatch, currentCharge, hasCharge);
                m_JackIcons[port].Draw(pSpriteBatch, currentControl);
                m_PortNumLabels[port].Draw(pSpriteBatch, port, m_Color);
            }
            DrawHeldBullet(pSpriteBatch, pBulletType);
        }
    }
}