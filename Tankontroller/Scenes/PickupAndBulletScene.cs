using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tankontroller.Controller;
using Tankontroller.GUI;
using Tankontroller.World;
using static System.Formats.Asn1.AsnWriter;

namespace Tankontroller.Scenes
{
    public class PickupAndBulletScene : IScene
    {
        private static readonly Texture2D mBackgroundTexture = Tankontroller.Instance().CM().Load<Texture2D>("background_01");
        private static readonly SpriteFont m_SpriteFont = Tankontroller.Instance().CM().Load<SpriteFont>("handwritingfont");
        private Rectangle mBackgroundRectangle;
        private Tankontroller mGameInstance;
        private MainMenuScene mStartScene;

        private ButtonList mButtonList = null;
        private TextList mTextList = null;

        // Cooldown timers to prevent the menu selection from being too fast
        private float mSelectionCooldown = 0.0f;
        private readonly float SELECTION_COOLDOWN_TIME = 0.2f;

        //Selection button frequency values
        private readonly float SELECTION_LOW_VALUE = DGS.Instance.GetFloat("PICKUP_SPAWN_RATE") * 1.2f;
        private readonly float SELECTION_MED_VALUE = DGS.Instance.GetFloat("PICKUP_SPAWN_RATE");
        private readonly float SELECTION_HIGH_VALUE = DGS.Instance.GetFloat("PICKUP_SPAWN_RATE") * 0.2f;

        public PickupAndBulletScene(MainMenuScene startScene)
        {
            mStartScene = startScene;
            mGameInstance = (Tankontroller)Tankontroller.Instance();
            spriteBatch = new SpriteBatch(mGameInstance.GDM().GraphicsDevice);
            int screenWidth = mGameInstance.GDM().GraphicsDevice.Viewport.Width;
            int screenHeight = mGameInstance.GDM().GraphicsDevice.Viewport.Height;
            Tankontroller game = (Tankontroller)Tankontroller.Instance();
            mBackgroundRectangle = new Rectangle(0, 0, screenWidth, screenHeight);            
            mSelectionCooldown = SELECTION_COOLDOWN_TIME;

            GenerateButtons();
        }

        public override void Draw(float pSeconds)
        {
            mGameInstance.GDM().GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            spriteBatch.Draw(mBackgroundTexture, mBackgroundRectangle, Color.White);
            mButtonList.Draw(spriteBatch);
            mTextList.Draw(spriteBatch);
            spriteBatch.End();
        }

        public override void Update(float pSeconds)
        {
            Escape();
            mGameInstance.GetControllerManager().DetectControllers();

            mSelectionCooldown -= pSeconds;

            foreach (IController controller in mGameInstance.GetControllerManager().GetControllers())
            {
                controller.UpdateController();

                if (controller.IsPressed(Control.TURRET_LEFT) && mSelectionCooldown <= 0.0f)
                {
                    mButtonList.SelectPreviousButton();
                    mSelectionCooldown = SELECTION_COOLDOWN_TIME;
                }
                if (controller.IsPressed(Control.TURRET_RIGHT) && mSelectionCooldown <= 0.0f)
                {
                    mButtonList.SelectNextButton();
                    mSelectionCooldown = SELECTION_COOLDOWN_TIME;
                }

                if (controller.IsPressed(Control.FIRE) && !controller.WasPressed(Control.FIRE) ||
                    controller.IsPressed(Control.RECHARGE) && !controller.WasPressed(Control.RECHARGE))
                {
                    //Exception to stop the player from pressing selection button that is already on.
                    if (mButtonList.GetSelectedButtonIndex() < 4 && mButtonList.IsButtonOnOff())
                        return;

                    SoundEffectInstance buttonPress = mGameInstance.GetSoundManager().GetSoundEffectInstance("Sounds/Button_Push");
                    buttonPress.Play();
                    mButtonList.PressSelectedButton();
                }
            }
        }
        public override void Escape()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                mGameInstance.SM().Transition(mStartScene, true);
            }
        }

        /// <summary>
        /// Turns off all the selection buttons except the one at given index.
        /// </summary>
        /// <param name="index">The selection button that won't turn off.</param>
        private void TurnOffAllSelectionExcept(int index)
        {
            for (int i = 0; i < 4; i++)
            {
                if (i != index)
                {
                    mButtonList.TurnOffButton(i);
                }
            }
        }

        private void ButtonSelectionNone()
        {
            TheWorld.PICKUP_SPAWN = false;
            TheWorld.PICKUP_SPAWN_TIME = 0;
            TurnOffAllSelectionExcept(0);
        }

        private void ButtonSelectionLow()
        {
            TheWorld.PICKUP_SPAWN = true;
            TheWorld.PICKUP_SPAWN_TIME = SELECTION_LOW_VALUE;
            TurnOffAllSelectionExcept(1);
        }

        private void ButtonSelectionMed()
        {
            TheWorld.PICKUP_SPAWN = true;
            TheWorld.PICKUP_SPAWN_TIME = SELECTION_MED_VALUE;
            TurnOffAllSelectionExcept(2);
        }

        private void ButtonSelectionHigh()
        {
            TheWorld.PICKUP_SPAWN = true;
            TheWorld.PICKUP_SPAWN_TIME = SELECTION_HIGH_VALUE;
            TurnOffAllSelectionExcept(3);
        }

        private void ButtonPickupHealth()
        {
            TheWorld.HEALTH_PICKUP = !TheWorld.HEALTH_PICKUP;
        }

        private void ButtonPickupBall()
        {
            TheWorld.BOUNCY_BULLET_PICKUP = !TheWorld.BOUNCY_BULLET_PICKUP;
        }

        private void ButtonPickupEMP()
        {
            TheWorld.EMP_PICKUP = !TheWorld.EMP_PICKUP;
        }

        private void ButtonPickupMine()
        {
            TheWorld.MINE_PICKUP = !TheWorld.MINE_PICKUP;
        }

        private void ButtonBack()
        {
            IGame game = Tankontroller.Instance();
            game.GetControllerManager().SetAllTheLEDsWhite();
            game.SM().Transition(null);
        }

        /// <summary>
        /// Creates the pickup and pickup spawning frequency buttons for the scene and assigns their actions.
        /// </summary>
        private void GenerateButtons()
        {
            float scaleFactor = Tankontroller.Instance().ScaleFactor();
            int screenWidth = mGameInstance.GDM().GraphicsDevice.Viewport.Width;
            int screenHeight = mGameInstance.GDM().GraphicsDevice.Viewport.Height;

            //y offset for 32:9 aspect ratio
            if ((float)screenWidth / (float)screenHeight == 32f / 9f)
            {
                scaleFactor = 1f;
            }

            mButtonList = new ButtonList();
            mTextList = new TextList();

            // Textures for the pickup spawn frequency selection buttons
            Texture2D pickupSelectionOffTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_selection_off");
            Texture2D pickupSelectionOffHighlightTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_selection_off_highlight");
            Texture2D pickupSelectionOnTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_selection_on");
            Texture2D pickupSelectionOnHighlightTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_selection_on_highlight");

            // Create button and text univeral positions and dimensions for the pickup spawn frequency selection buttons
            int buttonWidth = (int)(pickupSelectionOffTexture.Width * scaleFactor);
            int buttonHeight = (int)(pickupSelectionOffTexture.Height * scaleFactor);
            int buttonX = Convert.ToInt32((screenWidth - buttonWidth) / 5);
            int buttonY = (int)(screenHeight / 35.55);
            int buttonXPadding = (int)(screenWidth / 6);
            Rectangle buttonRect = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);
            int buttonTextX = Convert.ToInt32(buttonRect.X + buttonRect.Width / 2.5);
            int buttonTextY = Convert.ToInt32(buttonRect.Y + buttonRect.Height / 2.5);
            Vector2 buttonTextPosition = new Vector2(buttonTextX, buttonTextY);
            bool buttonState = false;

            // Selection "None" button
            buttonState = !TheWorld.PICKUP_SPAWN;
            Button selectionNoneButton = new Button(pickupSelectionOffTexture, pickupSelectionOffHighlightTexture, pickupSelectionOnTexture, pickupSelectionOnHighlightTexture,buttonRect, Color.White, ButtonSelectionNone, buttonState);
            selectionNoneButton.Selected = true;
            mButtonList.Add(selectionNoneButton);
            Text selectionNoneButtonText = new Text(m_SpriteFont, "None", buttonTextPosition, Color.White, scaleFactor);
            mTextList.Add(selectionNoneButtonText);

            // Selection "Low" button
            buttonState = TheWorld.PICKUP_SPAWN_TIME == SELECTION_LOW_VALUE;
            buttonRect.X += buttonXPadding;
            buttonTextX = Convert.ToInt32(buttonRect.X + buttonRect.Width / 2.3);
            buttonTextPosition = new Vector2(buttonTextX, buttonTextY);
            Button selectionLowButton = new Button(pickupSelectionOffTexture, pickupSelectionOffHighlightTexture, pickupSelectionOnTexture, pickupSelectionOnHighlightTexture, buttonRect, Color.White, ButtonSelectionLow, buttonState);
            selectionLowButton.Selected = false;
            mButtonList.Add(selectionLowButton);
            Text selectionLowButtonText = new Text(m_SpriteFont, "Low", buttonTextPosition, Color.White, scaleFactor);
            mTextList.Add(selectionLowButtonText);

            // Selection "Medium" button
            buttonState = TheWorld.PICKUP_SPAWN_TIME == SELECTION_MED_VALUE;
            buttonRect.X += buttonXPadding;
            buttonTextX = Convert.ToInt32(buttonRect.X + buttonRect.Width / 2.4);
            buttonTextPosition = new Vector2(buttonTextX, buttonTextY);
            Button selectionMedButton = new Button(pickupSelectionOffTexture, pickupSelectionOffHighlightTexture, pickupSelectionOnTexture, pickupSelectionOnHighlightTexture, buttonRect, Color.White, ButtonSelectionMed, buttonState);
            selectionMedButton.Selected = false;
            mButtonList.Add(selectionMedButton);
            Text selectionMedButtonText = new Text(m_SpriteFont, "Med", buttonTextPosition, Color.White, scaleFactor);
            mTextList.Add(selectionMedButtonText);

            // Selection "High" button
            buttonState = TheWorld.PICKUP_SPAWN_TIME == SELECTION_HIGH_VALUE;
            buttonRect.X += buttonXPadding;
            buttonTextX = Convert.ToInt32(buttonRect.X + buttonRect.Width / 2.4);
            buttonTextPosition = new Vector2(buttonTextX, buttonTextY);
            Button selectionHighButton = new Button(pickupSelectionOffTexture, pickupSelectionOffHighlightTexture, pickupSelectionOnTexture, pickupSelectionOnHighlightTexture, buttonRect, Color.White, ButtonSelectionHigh, buttonState);
            selectionHighButton.Selected = false;
            mButtonList.Add(selectionHighButton);
            Text selectionHighButtonText = new Text(m_SpriteFont, "High", buttonTextPosition, Color.White, scaleFactor);
            mTextList.Add(selectionHighButtonText);

            // Textures for the pickup type buttons
            Texture2D pickupHealthOffTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_health_off");
            Texture2D pickupHealthOffHighlightTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_health_off_highlight");
            Texture2D pickupHealthOnTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_health_on");
            Texture2D pickupHealthOnHighlightTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_health_on_highlight");
            Texture2D pickupBallOffTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_ball_off");
            Texture2D pickupBallOffHighlightTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_ball_off_highlight");
            Texture2D pickupBallOnTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_ball_on");
            Texture2D pickupBallOnHighlightTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_ball_on_highlight");
            Texture2D pickupEMPOffTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_emp_off");
            Texture2D pickupEMPOffHighlightTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_emp_off_highlight");
            Texture2D pickupEMPOnTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_emp_on");
            Texture2D pickupEMPOnHighlightTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_emp_on_highlight");
            Texture2D pickupMineOffTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_mine_off");
            Texture2D pickupMineOffHighlightTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_mine_off_highlight");
            Texture2D pickupMineOnTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_mine_on");
            Texture2D pickupMineOnHighlightTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_mine_on_highlight");

            // Create button and text univeral positions and dimensions for the pickup type buttons
            buttonWidth = (int)(pickupBallOffTexture.Width * scaleFactor);
            buttonHeight = (int)(pickupBallOffTexture.Height * scaleFactor);
            buttonX = Convert.ToInt32((screenWidth-buttonWidth) / 2);
            buttonY = (int)(screenHeight / 4.23);
            int buttonYPadding = (int)(screenHeight / 5.68);
            buttonRect = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);
            buttonTextX = Convert.ToInt32(buttonRect.X + buttonRect.Width / 3.8);
            buttonTextY = Convert.ToInt32(buttonRect.Y + buttonRect.Height / 2.5);
            buttonTextPosition = new Vector2(buttonTextX, buttonTextY);

            // Health Pickup button
            buttonState = TheWorld.HEALTH_PICKUP;
            Button pickupHealthButton = new Button(pickupHealthOffTexture, pickupHealthOffHighlightTexture, pickupHealthOnTexture, pickupHealthOnHighlightTexture, buttonRect, Color.White, ButtonPickupHealth, buttonState);
            pickupHealthButton.Selected = false;
            mButtonList.Add(pickupHealthButton);
            Text pickupHealthButtonText = new Text(m_SpriteFont, "The Health - Heals the tank for 1 hp", buttonTextPosition, Color.White, scaleFactor);
            mTextList.Add(pickupHealthButtonText);

            // Ball Pickup button
            buttonState = TheWorld.BOUNCY_BULLET_PICKUP;
            buttonRect.Y += buttonYPadding;
            buttonTextX = Convert.ToInt32(buttonRect.X + buttonRect.Width / 3.8);
            buttonTextY = Convert.ToInt32(buttonRect.Y + buttonRect.Height / 3.0);
            buttonTextPosition = new Vector2(buttonTextX, buttonTextY);
            Button pickupBallButton = new Button(pickupBallOffTexture, pickupBallOffHighlightTexture, pickupBallOnTexture, pickupBallOnHighlightTexture, buttonRect, Color.White, ButtonPickupBall, buttonState);
            pickupBallButton.Selected = false;
            mButtonList.Add(pickupBallButton);
            Text pickupBallButtonText = new Text(m_SpriteFont, "The Bouncy - A bullet that can bounce off walls", buttonTextPosition, Color.White, scaleFactor);
            mTextList.Add(pickupBallButtonText);

            // EMP Pickup button
            buttonState = TheWorld.EMP_PICKUP;
            buttonRect.Y += buttonYPadding;
            buttonTextX = Convert.ToInt32(buttonRect.X + buttonRect.Width / 3.8);
            buttonTextY = Convert.ToInt32(buttonRect.Y + buttonRect.Height / 2.7);
            buttonTextPosition = new Vector2(buttonTextX, buttonTextY);
            Button pickupEMPButton = new Button(pickupEMPOffTexture, pickupEMPOffHighlightTexture, pickupEMPOnTexture, pickupEMPOnHighlightTexture, buttonRect, Color.White, ButtonPickupEMP, buttonState);
            pickupEMPButton.Selected = false;
            mButtonList.Add(pickupEMPButton);
            Text pickupEMPButtonText = new Text(m_SpriteFont, "The EMP - Drains the energy of any tank in its shockwave", buttonTextPosition, Color.White, scaleFactor);
            mTextList.Add(pickupEMPButtonText);

            // Mine Pickup button
            buttonState = TheWorld.MINE_PICKUP;
            buttonRect.Y += buttonYPadding;
            buttonTextX = Convert.ToInt32(buttonRect.X + buttonRect.Width / 3.8);
            buttonTextY = Convert.ToInt32(buttonRect.Y + buttonRect.Height / 2.45);
            buttonTextPosition = new Vector2(buttonTextX, buttonTextY);
            Button pickupMineButton = new Button(pickupMineOffTexture, pickupMineOffHighlightTexture, pickupMineOnTexture, pickupMineOnHighlightTexture, buttonRect, Color.White, ButtonPickupMine, buttonState);
            pickupMineButton.Selected = false;
            mButtonList.Add(pickupMineButton);
            Text pickupMineButtonText = new Text(m_SpriteFont, "The Mine - Damages the tank who drives over it", buttonTextPosition, Color.White, scaleFactor);
            mTextList.Add(pickupMineButtonText);

            // Back button
            Texture2D fireBack = mGameInstance.CM().Load<Texture2D>("fire");
            Texture2D fireBackHighlight = mGameInstance.CM().Load<Texture2D>("PickupMenu/fire_highlight");

            buttonRect.Width = (int)(fireBack.Width * scaleFactor);
            buttonRect.Height = (int)(fireBack.Height * scaleFactor);
            buttonRect.X = Convert.ToInt32(0);
            buttonRect.Y = Convert.ToInt32(screenHeight / 2);
            buttonTextX = Convert.ToInt32(buttonRect.X + buttonRect.Width / 1.3);
            buttonTextY = Convert.ToInt32(buttonRect.Y + buttonRect.Height / 1.5);
            buttonTextPosition = new Vector2(buttonTextX, buttonTextY);
            Button backButton = new Button(fireBack, fireBackHighlight, buttonRect, Color.White, ButtonBack);
            backButton.Selected = false;
            mButtonList.Add(backButton);
            Text backButtonText = new Text(m_SpriteFont, "Back", buttonTextPosition, Color.White, scaleFactor);
            mTextList.Add(backButtonText);
        }
    }
}