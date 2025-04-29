using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Tankontroller.Controller;

namespace Tankontroller.Managers
{
    public class ControllerManager
    {
        public static Texture2D CircleTex;
        public static Texture2D PixelTex;
        public static SpriteFont TextFont;

        private Task mDetectControllerTask = null;
        private Mutex mControllerListLock = new Mutex();

        private Dictionary<string, IController> mControllers = new Dictionary<string, IController>();
        static ControllerManager mInstance = new ControllerManager();
        public static ControllerManager Instance
        {
            get { return mInstance; }
        }
        private ControllerManager() { }

        public IController GetController(int pIndex) { return GetControllers().ElementAt(pIndex); }
        public int GetControllerCount() { return GetControllers().Count; }
        public List<IController> GetControllers()
        {
            try
            {
                mControllerListLock.WaitOne();
                return mControllers.Values.ToList();
            }
            finally
            {
                mControllerListLock.ReleaseMutex();
            }
        }

        public void UpdateAllControllers()
        {
            foreach (IController controller in GetControllers())
            {
                controller.UpdateController();
            }
        }

        public void SetAllTheLEDsWhite()
        {
            foreach (IController controller in GetControllers())
            {
                controller.SetColour(Color.White);
                if (controller is ModularController modularController)
                {
                    modularController.SetAllJackLEDs(new Color(0, 0, 0));
                }
            }
        }

        public void DisconnectAllControllers()
        {
            if (mDetectControllerTask != null)
                mDetectControllerTask.Wait(); // Wait for any controller detection to finish otherwise it may detect a controller that is disconnected
            foreach (IController controller in GetControllers())
            {
                controller.Disconnect();
            }
        }

        public void AddKeyboardController(Dictionary<Keys, Control> pKeyMap, Dictionary<Keys, int> pPortMap)
        {
            try
            {
                mControllerListLock.WaitOne();
                mControllers.Add("Keyboard" + (mControllers.Count + 1).ToString("D2"), new KeyboardController(pKeyMap, pPortMap));
            }
            finally
            {
                mControllerListLock.ReleaseMutex();
            }
        }

        public void DetectControllers()
        {
            if (mDetectControllerTask == null || mDetectControllerTask.IsCompleted)
            {
                mDetectControllerTask = Task.Run(async () => await DetectControllersAsync());
            }
        }

        private bool COMPortInUse(string pPortName)
        {
            foreach (IController item in GetControllers())
            {
                if (item.IsConnected() && item is ModularController controller)
                {
                    if (controller.COMPortName == pPortName)
                        return true;
                }
            }
            return false;
        }

        private async Task DetectControllersAsync()
        {
            string[] portNames = SerialPort.GetPortNames();
            foreach (string portName in portNames)
            {
                if (!COMPortInUse(portName))
                {
                    SerialPort port;
                    try
                    {
                        port = new SerialPort(portName, 19200);
                        port.Open();
                    }
                    catch
                    {
                        continue;
                    }

                    port.DtrEnable = true;

                    port.ReadTimeout = 10;
                    port.WriteTimeout = 10;

                    port.DiscardInBuffer();

                    await port.BaseStream.WriteAsync(new byte[] { (byte)'I' }, 0, 1);

                    System.Threading.Thread.Sleep(10);

                    if (port.BytesToRead > 0)
                    {
                        // The comms starts with 0xff 0xff
                        try
                        {
                            string response = port.ReadLine();
                            if (response == "Tankontroller")
                            {
                                Hacktroller hacktroller = new Hacktroller(port);
                                string id = port.ReadLine();
                                if (mControllers.ContainsKey(id))
                                {
                                    if (mControllers[id] is ModularController controller)
                                    {
                                        controller.SetHacktroller(hacktroller);
                                    }
                                }
                                else
                                {
                                    string newID = Guid.NewGuid().ToString("N").Substring(0, 10);
                                    hacktroller.SetID(newID);
                                    ModularController controller = new ModularController(hacktroller);
                                    mControllerListLock.WaitOne();
                                    mControllers.Add(newID, controller);
                                    mControllerListLock.ReleaseMutex();
                                }
                            }
                            else
                            {
                                port.Close();
                            }
                        }
                        catch
                        {
                            //possible timeout - ignore
                        }
                    }
                    else { port.Close(); }
                }
            }
        }

        public void Draw(SpriteBatch pSpriteBatch, Rectangle pDrawArea)
        {
            if (mControllers.Count == 0)
            {
                pSpriteBatch.DrawString(TextFont, "No controllers detected", new Vector2(pDrawArea.X, pDrawArea.Y), Color.Black);
                return;
            }
            Rectangle conRect = new Rectangle(pDrawArea.X, pDrawArea.Y, pDrawArea.Width, Math.Min(pDrawArea.Height / mControllers.Count, pDrawArea.Height / 20));
            pDrawArea.Height = conRect.Height * mControllers.Count;
            pSpriteBatch.Draw(PixelTex, pDrawArea, new Color(Color.Black, 128));

            List<KeyValuePair<string, IController>> controllers;
            try
            {
                mControllerListLock.WaitOne();
                controllers = mControllers.ToList();
            }
            finally { mControllerListLock.ReleaseMutex(); }

            foreach (KeyValuePair<string, IController> controller in controllers)
            {
                Rectangle circleRect = new Rectangle(conRect.X + conRect.Height / 4, conRect.Y + conRect.Height / 4, conRect.Height / 2, conRect.Height / 2);
                pSpriteBatch.Draw(CircleTex, circleRect, controller.Value.IsConnected() ? Color.Green : Color.Red);
                Vector2 textSize = TextFont.MeasureString(controller.Key);
                pSpriteBatch.DrawString(TextFont, controller.Key, new Vector2(conRect.X + conRect.Width / 5, conRect.Y + (conRect.Height / 2) - (textSize.Y / 2)), Color.White);
                conRect.Y += conRect.Height;
            }
        }
    }
}