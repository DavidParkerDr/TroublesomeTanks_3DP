using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using Tankontroller.Controller;
using Tankontroller.GUI;
using Tankontroller.World;

namespace Tankontroller
{
    public class Player
    {
        public static readonly float TRACK_DEPLETION_RATE = DGS.Instance.GetFloat("TRACK_DEPLETION_RATE");
        public static readonly float BULLET_CHARGE_DEPLETION = DGS.Instance.GetFloat("BULLET_CHARGE_DEPLETION");
        public static readonly float CHARGE_AMOUNT = DGS.Instance.GetFloat("CHARGE_AMOUNT");

        public TeamGUI GUI { get; private set; }
        public Tank Tank { get; private set; }
        public IController Controller { get; private set; }
        public Color Colour { get; private set; }

        private int mCannonJackIndex = -1; // Due to flickering of the controller, we need to keep track of the last jack used to fire the cannon

        // Variables to control Controller LED flashing when in EMP shockwave
        private const float FLASH_TIME = 0.5f;
        private readonly Color FLASH_COLOUR = new Color(0, 0, 0);
        private float mFlashTimer = FLASH_TIME;
        private bool mFlashSet = false;

        public Player(IController pController, Avatar pAvatar)
        {
            Controller = pController;
            GUI = new TeamGUI(pAvatar, new Rectangle(), Controller);
            Colour = pAvatar.GetColour();
        }

        public void SetController(IController pController)
        {
            Controller = pController;
        }

        public void GamePreparation(Tank pTank, Rectangle pRectangle)
        {
            Tank = pTank;
            Controller.SetColour(Colour);
            Tank.SetColour(Colour);
            GUI.Reposition(pRectangle);
        }

        public void Reset()
        {
            Controller.ResetJacks();
        }

        public bool DoTankControls(float pSeconds)
        {
            mFlashTimer -= pSeconds;
            if (mFlashSet && mFlashTimer <= 0.0f)
            {
                mFlashSet = false;
                mFlashTimer = FLASH_TIME;
                Controller.SetColour(Colour);
            }

            // Deplete charge if player is inside EMP shockwave
            bool insideShockwave = Tank.IsInsideShockwave();
            if (insideShockwave)
            {
                if (!mFlashSet && mFlashTimer <= 0.0f)
                {
                    mFlashSet = true;
                    mFlashTimer = FLASH_TIME;
                    Controller.SetColour(FLASH_COLOUR);
                }
                foreach (Control control in Enum.GetValues<Control>())
                {
                    if (control == Control.NONE) continue; // Shockwave should only affect controls that plugged in
                    Controller.DepleteCharge(control, TRACK_DEPLETION_RATE * 3.0f * pSeconds);
                }
            }

            bool tankMoved = false;
            if (Tank.Health() > 0)
            {
                bool leftTrackForward = Controller.IsPressedWithCharge(Control.LEFT_TRACK_FORWARDS);
                bool leftTrackBackward = Controller.IsPressedWithCharge(Control.LEFT_TRACK_BACKWARDS);
                bool rightTrackForward = Controller.IsPressedWithCharge(Control.RIGHT_TRACK_FORWARDS);
                bool rightTrackBackward = Controller.IsPressedWithCharge(Control.RIGHT_TRACK_BACKWARDS);

                bool leftMoved = (leftTrackForward || leftTrackBackward) && !(leftTrackForward && leftTrackBackward);
                bool rightMoved = (rightTrackForward || rightTrackBackward) && !(rightTrackForward && rightTrackBackward);

                tankMoved = leftMoved || rightMoved;

                if (tankMoved)
                {
                    // Move the tank
                    if (leftTrackForward && rightTrackForward)
                    {
                        Tank.BothTracksForward(pSeconds);
                    }
                    else if (leftTrackBackward && rightTrackBackward)
                    {
                        Tank.BothTracksBackward(pSeconds);
                    }
                    else if (leftTrackForward && rightTrackBackward)
                    {
                        Tank.BothTracksOpposite(true, pSeconds);
                    }
                    else if (leftTrackBackward && rightTrackForward)
                    {
                        Tank.BothTracksOpposite(false, pSeconds);
                    }
                    else if (leftTrackForward)
                    {
                        Tank.LeftTrackForward(pSeconds);
                    }
                    else if (leftTrackBackward)
                    {
                        Tank.LeftTrackBackward(pSeconds);
                    }
                    else if (rightTrackForward)
                    {
                        Tank.RightTrackForward(pSeconds);
                    }
                    else if (rightTrackBackward)
                    {
                        Tank.RightTrackBackward(pSeconds);
                    }

                    // Deplete the charge for the tracks
                    Control control = leftTrackForward ? Control.LEFT_TRACK_FORWARDS : Control.LEFT_TRACK_BACKWARDS;
                    if (leftMoved)
                    {
                        Controller.DepleteCharge(control, TRACK_DEPLETION_RATE * pSeconds);
                    }
                    control = rightTrackForward ? Control.RIGHT_TRACK_FORWARDS : Control.RIGHT_TRACK_BACKWARDS;
                    if (rightMoved)
                    {
                        Controller.DepleteCharge(control, TRACK_DEPLETION_RATE * pSeconds);
                    }
                }

                if (Controller.IsPressedWithCharge(Control.TURRET_LEFT))
                {
                    Tank.CannonLeft(pSeconds);
                    Controller.DepleteCharge(Control.TURRET_LEFT, TRACK_DEPLETION_RATE * pSeconds);
                }
                else if (Controller.IsPressedWithCharge(Control.TURRET_RIGHT))
                {
                    Tank.CannonRight(pSeconds);
                    Controller.DepleteCharge(Control.TURRET_RIGHT, TRACK_DEPLETION_RATE * pSeconds);
                }

                int currentJackIndex = Controller.GetJackIndex(Control.FIRE);
                if (Controller.IsPressedWithCharge(Control.FIRE))
                {
                    if (mCannonJackIndex == -1) mCannonJackIndex = currentJackIndex;
                    else if (mCannonJackIndex == currentJackIndex) Tank.PrimingWeapon(pSeconds);
                }
                else
                {
                    if (mCannonJackIndex == currentJackIndex && Tank.IsFirePrimed())
                    {
                        if (Controller.DepleteCharge(Control.FIRE, BULLET_CHARGE_DEPLETION))
                        {
                            Tank.Fire(Tank.mbulletType);
                            SoundEffectInstance bulletShot = Tankontroller.Instance().GetSoundManager().GetSoundEffectInstance("Sounds/Tank_Gun");
                            bulletShot.Play();
                        }
                    }
                    mCannonJackIndex = -1;
                }

                if (Controller.IsPressed(Control.RECHARGE) && !Controller.WasPressed(Control.RECHARGE))
                {
                    Controller.AddCharge(Control.RECHARGE, CHARGE_AMOUNT);
                }
            }
            return tankMoved;
        }
    }
}
