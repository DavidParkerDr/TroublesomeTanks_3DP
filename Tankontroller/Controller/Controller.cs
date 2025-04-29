using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Tankontroller.Controller
{
    public enum Control { LEFT_TRACK_FORWARDS = 0, LEFT_TRACK_BACKWARDS = 1, RIGHT_TRACK_FORWARDS = 2, RIGHT_TRACK_BACKWARDS = 3, FIRE = 4, RECHARGE = 5, NONE = 6, TURRET_LEFT = 7, TURRET_RIGHT = 8 };

    //---------------------------------------------------------------------------------------------------
    // IController Interface:
    //
    // This interface is used to define the methods that all controllers will need to implement.
    //---------------------------------------------------------------------------------------------------
    public interface IController
    {
        bool IsPressedWithCharge(Control pControl);
        bool DepleteCharge(Control pControl, float amount);
        bool AddCharge(Control pControl, float amount);
        float GetJackCharge(int pJackIndex);
        Control GetJackControl(int pJackIndex);
        int GetJackIndex(Control pControl);
        void TransferJackCharge(IController pController);
        bool IsPressed(Control pControl);
        bool WasPressed(Control pControl);
        void ResetJacks();

        void Disconnect();
        bool IsConnected();

        void SetColour(Color pColour);

        void UpdateController();
    }

    //---------------------------------------------------------------------------------------------------
    // Controller Class:
    //
    // This class is the base class for all controllers. It contains the basic functionality that all controllers will need.
    // It contains the following:
    // - A list of Jacks, which are the inputs for the controller. Each Jack has a Control, a charge, and a list of LED IDs.
    // - A list of LED IDs, which are the LEDs on the controller.
    // - A boolean for whether the lights are on or off.
    // - A boolean for whether the controller is connected.
    // - A method to set the colour of the controller.
    // - A method to reset the charges of all the Jacks.
    // - A method to turn off and on the lights.
    // - A method to get the control and charge of a Jack.
    // - A method to deplete, add charge and transfer the charge of a Jack.
    // - A method to check if a control is pressed or pressed with charge.
    // - A method to check if the controller is connected and another to update it.
    //---------------------------------------------------------------------------------------------------
    public abstract class Controller : IController
    {
        public static readonly float MAX_CHARGE = DGS.Instance.GetFloat("MAX_CHARGE");

        protected class Jack
        {
            private bool mIsDown;

            public Control Control;
            public float charge;
            public bool WasDown { get; private set; }
            public bool IsDown
            {
                get { return mIsDown; }
                set
                {
                    WasDown = mIsDown;
                    mIsDown = value;
                }
            }

            public Jack()
            {
                mIsDown = WasDown = false;
                Control = Control.NONE;
                ResetCharge();
            }
            public void ResetCharge()
            {
                charge = MAX_CHARGE;
            }
        };
        protected Color mColour;
        protected Jack[] mJacks;
        protected volatile bool mConnected;

        protected Controller()
        {
            mJacks = new Jack[7] { new Jack(), new Jack(), new Jack(), new Jack(), new Jack(), new Jack(), new Jack() };
            mConnected = true;
        }

        public virtual void SetColour(Color pColour)
        {
            mColour = pColour;
        }
        public virtual void ResetJacks()
        {
            foreach (Jack j in mJacks)
            {
                j.ResetCharge();
            }
        }

        public Control GetJackControl(int pJackIndex)
        {
            return mJacks[pJackIndex].Control;
        }

        public float GetJackCharge(int pJackIndex)
        {
            return mJacks[pJackIndex].charge;
        }

        public abstract void UpdateController();

        public int GetJackIndex(Control pControl)
        {
            for (int i = 0; i < 7; ++i)
            {
                if (mJacks[i].Control == pControl)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool IsPressedWithCharge(Control pControl)
        {
            for (int i = 0; i < 7; ++i)
            {
                if (mJacks[i].Control == pControl)
                {
                    bool hasCharge = pControl == Control.FIRE ? mJacks[i].charge >= Player.BULLET_CHARGE_DEPLETION : mJacks[i].charge > 0;
                    return mJacks[i].IsDown && hasCharge;
                }
            }
            return false;
        }

        public virtual bool AddCharge(Control pControl, float pAmount)
        {
            for (int i = 0; i < 7; ++i)
            {
                if (mJacks[i].Control == pControl)
                {
                    mJacks[i].charge += pAmount;
                    mJacks[i].charge = MathHelper.Clamp(mJacks[i].charge, 0, MAX_CHARGE);
                    return true;
                }
            }
            return false;
        }

        public bool DepleteCharge(Control pControl, float amount)
        {
            return AddCharge(pControl, -amount);
        }

        public void TransferJackCharge(IController pController)
        {
            for (int i = 0; i < 7; ++i)
            {
                mJacks[i].charge = pController.GetJackCharge(i);
            }
        }

        public bool IsPressed(Control pControl)
        {
            for (int i = 0; i < 7; ++i)
            {
                if (mJacks[i].Control == pControl)
                {
                    return mJacks[i].IsDown;
                }
            }
            return false;
        }

        public bool WasPressed(Control pControl)
        {
            for (int i = 0; i < 7; ++i)
            {
                if (mJacks[i].Control == pControl)
                {
                    return mJacks[i].WasDown;
                }
            }
            return false;
        }

        public bool IsConnected()
        {
            return mConnected;
        }

        public void Disconnect()
        {
            SetColour(new Color(0, 0, 0));
            mConnected = false;
        }
    }

    //---------------------------------------------------------------------------------------------------
    // KeyboardController Class:
    //
    // This class is a subclass of Controller. It is used to control the tank with the keyboard.
    // It contains the following:
    // - A dictionary of Keys and Controls, which is used to map the keys to the controls.
    // - A dictionary of Keys and ints, which is used to map the keys to the ports.
    // - A method to update the controller.
    //---------------------------------------------------------------------------------------------------
    public class KeyboardController : Controller
    {
        private Dictionary<Keys, Control> m_KeyMap;
        private Dictionary<Keys, int> m_PortMap;
        private Control m_SpareControl;
        private KeyboardState mPreviousKeyboardState = Keyboard.GetState();

        public KeyboardController(Dictionary<Keys, Control> pKeyMap, Dictionary<Keys, int> pPortMap) : base()
        {
            m_KeyMap = pKeyMap;
            m_PortMap = pPortMap;

            mJacks[0].Control = Control.LEFT_TRACK_BACKWARDS;
            mJacks[1].Control = Control.LEFT_TRACK_FORWARDS;
            mJacks[2].Control = Control.RIGHT_TRACK_FORWARDS;
            mJacks[3].Control = Control.RIGHT_TRACK_BACKWARDS;
            mJacks[4].Control = Control.FIRE;
            mJacks[5].Control = Control.TURRET_LEFT;
            mJacks[6].Control = Control.TURRET_RIGHT;
            m_SpareControl = Control.RECHARGE;
        }

        public override void UpdateController()
        {
            KeyboardState keyboardState = Keyboard.GetState();

            foreach (KeyValuePair<Keys, int> kvp in m_PortMap)
            {
                if (keyboardState.IsKeyDown(kvp.Key) && !mPreviousKeyboardState.IsKeyDown(kvp.Key))
                {
                    Control temp = mJacks[kvp.Value].Control;
                    mJacks[kvp.Value].Control = m_SpareControl;
                    m_SpareControl = temp;
                    break;
                }
            }

            foreach (KeyValuePair<Keys, Control> kvp in m_KeyMap)
            {
                for (int i = 0; i < mJacks.Length; i++)
                {
                    if (mJacks[i].Control == kvp.Value)
                    {
                        mJacks[i].IsDown = keyboardState.IsKeyDown(kvp.Key);
                    }
                }
            }

            mPreviousKeyboardState = keyboardState;
        }
    }

    //---------------------------------------------------------------------------------------------------
    // ModularController Class:
    //
    // This class is a subclass of Controller. It is used to control the tank with the Hacktroller (3D Printed Controllers).
    // It contains the following:
    // - A Hacktroller object, which is used to communicate with the Hacktroller.
    // - A method to pull the data from the Hacktroller.
    // - A method to update the colours of the LEDs.
    // - A method to update the controller.
    //---------------------------------------------------------------------------------------------------
    public class ModularController : Controller
    {
        private static readonly float LED_BRIGHTNESS = DGS.Instance.GetFloat("LED_BRIGHTNESS");

        private Hacktroller mHacktroller;
        public string COMPortName { get { return mHacktroller.PortName; } }

        private Thread UpdateThread;
        private Mutex mColourUpdateListLock = new();
        private List<Tuple<byte, ControllerColor>> mColourUpdates = new();

        public ModularController(Hacktroller pHackTroller) : base()
        {
            SetHacktroller(pHackTroller);
        }

        public void SetHacktroller(Hacktroller pHackTroller)
        {
            mHacktroller = pHackTroller;
            mConnected = true;
            mColour = Color.Black; // Incase the colour is already white
            SetColour(Color.White);
            PullDataThread();
        }

        private void PullDataThread()
        {
            if (UpdateThread == null || !UpdateThread.IsAlive)
            {
                UpdateThread = new Thread(new ThreadStart(PullData));
                UpdateThread.Start();
            }
        }

        public override void SetColour(Color pColour)
        {
            if (pColour != mColour)
            {
                mColour = pColour;
                try
                {
                    mColourUpdateListLock.WaitOne();
                    for (byte i = 7; i < 10; i++) // Set Centre LED colour
                    {
                        ControllerColor colour = new ControllerColor((byte)(mColour.R * LED_BRIGHTNESS), (byte)(mColour.G * LED_BRIGHTNESS), (byte)(mColour.B * LED_BRIGHTNESS));
                        mColourUpdates.Add(new Tuple<byte, ControllerColor>(i, colour));
                    }
                }
                finally { mColourUpdateListLock.ReleaseMutex(); }
            }
        }

        public void SetAllJackLEDs(Color pColour)
        {
            try
            {
                mColourUpdateListLock.WaitOne();
                for (byte i = 0; i < 7; i++)
                {
                    mColourUpdates.Add(new Tuple<byte, ControllerColor>(i, new ControllerColor(pColour.R, pColour.G, pColour.B)));
                }
            }
            finally { mColourUpdateListLock.ReleaseMutex(); }
        }

        public override bool AddCharge(Control pControl, float amount)
        {
            bool result = base.AddCharge(pControl, amount);
            if (result) UpdateJackLED(pControl); // Update the LED for the jack that was depleted
            return result;
        }

        private void UpdateJackLED(Control pControl)
        {
            int jackIndex = GetJackIndex(pControl);
            if (jackIndex != -1)
            {
                SetJackLED(jackIndex, GetJackCharge(jackIndex) / MAX_CHARGE);
            }
        }

        public void SetJackLED(int pJackIndex, float pChargeRatio)
        {
            pChargeRatio = MathHelper.Clamp(pChargeRatio, 0.0f, 1.0f);

            // Calculate colour gradient for charge
            // Green (1.0) -> Yellow (0.5) -> Red (0.0)
            byte red, green; // blue is always 0
            if (pChargeRatio > 0.5f)
            {
                // Transition from green to yellow
                red = (byte)(255 * (1f - 2f * (pChargeRatio - 0.5f)));
                green = 255;
            }
            else
            {
                // Transition from yellow to red
                red = 255;
                green = (byte)(255 * (2f * pChargeRatio));
            }

            // Apply brightness
            red = (byte)(red * LED_BRIGHTNESS);
            green = (byte)(green * LED_BRIGHTNESS);
            ControllerColor result = new ControllerColor(red, green, 0);
            try
            {
                mColourUpdateListLock.WaitOne();
                mColourUpdates.Add(new Tuple<byte, ControllerColor>((byte)pJackIndex, result));
            }
            finally { mColourUpdateListLock.ReleaseMutex(); }
        }

        public override void ResetJacks()
        {
            SetAllJackLEDs(Color.Green); // Green means full charge
            base.ResetJacks();
        }

        public override void UpdateController() { }

        private static Control GetControlFromState(ControllerState pState)
        {
            switch (pState)
            {
                case ControllerState.LEFT_TRACK_FORWARDS:
                case ControllerState.LEFT_TRACK_FORWARDS_PRESSED:
                    return Control.LEFT_TRACK_FORWARDS;

                case ControllerState.LEFT_TRACK_BACKWARDS:
                case ControllerState.LEFT_TRACK_BACKWARDS_PRESSED:
                    return Control.LEFT_TRACK_BACKWARDS;

                case ControllerState.RIGHT_TRACK_FORWARDS:
                case ControllerState.RIGHT_TRACK_FORWARDS_PRESSED:
                    return Control.RIGHT_TRACK_FORWARDS;

                case ControllerState.RIGHT_TRACK_BACKWARDS:
                case ControllerState.RIGHT_TRACK_BACKWARDS_PRESSED:
                    return Control.RIGHT_TRACK_BACKWARDS;

                case ControllerState.TURRET_LEFT:
                case ControllerState.TURRET_LEFT_PRESSED:
                    return Control.TURRET_LEFT;

                case ControllerState.TURRET_RIGHT:
                case ControllerState.TURRET_RIGHT_PRESSED:
                    return Control.TURRET_RIGHT;

                case ControllerState.CHARGE:
                case ControllerState.CHARGE_PRESSED:
                    return Control.RECHARGE;

                case ControllerState.FIRE:
                case ControllerState.FIRE_PRESSED:
                    return Control.FIRE;

                default:
                    return Control.NONE;
            }
        }

        private static bool GetIsDownFromControl(ControllerState pState)
        {
            switch (pState)
            {
                case ControllerState.LEFT_TRACK_FORWARDS_PRESSED:
                case ControllerState.LEFT_TRACK_BACKWARDS_PRESSED:
                case ControllerState.RIGHT_TRACK_FORWARDS_PRESSED:
                case ControllerState.RIGHT_TRACK_BACKWARDS_PRESSED:
                case ControllerState.FIRE_PRESSED:
                case ControllerState.CHARGE_PRESSED:
                case ControllerState.TURRET_LEFT_PRESSED:
                case ControllerState.TURRET_RIGHT_PRESSED:
                    return true;
                default:
                    return false;
            }
        }

        private void PullData()
        {
            Thread.Sleep(10); // Give the Hacktroller time to connect (otherwise there's sometimes a semaphore timeout when writing)
            while (mConnected && mHacktroller.PortConnected)
            {
                ControllerState[] ports = mHacktroller.GetPorts();
                if (ports == null)
                {
                    continue;
                }

                for (int i = 0; i < ports.Length; ++i)
                {
                    Control control = GetControlFromState(ports[i]);
                    bool isDown = GetIsDownFromControl(ports[i]);

                    if (mJacks[i].Control == Control.NONE)
                    {
                        mJacks[i].Control = control;
                        mJacks[i].IsDown = isDown;
                    }
                    else
                    {
                        //if (control == mJacks[i].Control || control == Control.NONE)
                        {
                            mJacks[i].Control = control;
                            mJacks[i].IsDown = isDown;
                        }
                    }
                }

                if (mColourUpdates.Count > 0)
                {
                    List<Tuple<byte, ControllerColor>> data = new();
                    try
                    {
                        mColourUpdateListLock.WaitOne();
                        data = mColourUpdates.GetRange(0, mColourUpdates.Count);
                        mColourUpdates.Clear();
                    }
                    finally { mColourUpdateListLock.ReleaseMutex(); }
                    mHacktroller.SetColor(data);
                }
            }
            if (mHacktroller != null && mHacktroller.PortConnected)
            {
                mHacktroller.ClosePort();
            }
            mColour = new Color(0, 0, 0);
            mConnected = false;
        }
    }
}
