﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tankontroller
{
    /*      R1     R2    R3     Off   On
            100k   47k   0      323   0
            5k6    350   53
            10k    369   91
            15k    389   132
            18k    400   154
            33k    450   252*/

    public struct ControllerColor
    {
        public byte R;
        public byte G;
        public byte B;

        public ControllerColor(byte inR, byte inG, byte inB)
        {
            R = inR;
            G = inG;
            B = inB;
        }
    }

    public struct PinState
    {
        public int RawValue;
    }

    public enum ControllerState
    {
        LEFT_TRACK_FORWARDS,
        LEFT_TRACK_FORWARDS_PRESSED,

        LEFT_TRACK_BACKWARDS,
        LEFT_TRACK_BACKWARDS_PRESSED,

        RIGHT_TRACK_FORWARDS,
        RIGHT_TRACK_FORWARDS_PRESSED,

        RIGHT_TRACK_BACKWARDS,
        RIGHT_TRACK_BACKWARDS_PRESSED,

        TURRET_LEFT,
        TURRET_LEFT_PRESSED,

        TURRET_RIGHT,
        TURRET_RIGHT_PRESSED,

        CHARGE,
        CHARGE_PRESSED,

        FIRE,
        FIRE_PRESSED,

        NO_MATCH,
        NOT_CONNECTED
    }

    public struct stateMap
    {
        public ControllerState Result;
        public int Reading;
        public stateMap( ControllerState decodedState, int readingValue)
        {
            Result=decodedState;
            Reading = readingValue;
        }
    }

    public struct PortState
    {
        public ControllerState Controller;
        public bool FirePressed;
        public override string ToString()
        {
            return Controller.ToString() + FirePressed.ToString();
        }
    }

    public class Hacktroller
    {
        SerialPort port;

        Random rand = new Random(1);

        static int numPins = 8;
        static byte[] dCommand = new[] { (byte)'D' };
        static byte[] pCommand = new[] { (byte)'P' };

        static byte[] frameBuffer = new byte[64 * 3];

        static int tolerance= 1; // SJG I changed this from 1 to 3 because some pins seemed dodgy

        PortState[] portStates = new PortState[numPins];

        static stateMap[] stateMapping = new stateMap[] 
        {
            

            new stateMap(ControllerState.RIGHT_TRACK_FORWARDS, 152),
            new stateMap(ControllerState.RIGHT_TRACK_FORWARDS_PRESSED, 000),
            
            new stateMap(ControllerState.RIGHT_TRACK_BACKWARDS, 175), 
            new stateMap(ControllerState.RIGHT_TRACK_BACKWARDS_PRESSED, 5),

            new stateMap(ControllerState.LEFT_TRACK_FORWARDS, 195),
            new stateMap(ControllerState.LEFT_TRACK_FORWARDS_PRESSED, 11),

            new stateMap(ControllerState.LEFT_TRACK_BACKWARDS, 208),
            new stateMap(ControllerState.LEFT_TRACK_BACKWARDS_PRESSED, 23),

            new stateMap(ControllerState.CHARGE, 233),
            new stateMap(ControllerState.CHARGE_PRESSED, 44),

            new stateMap(ControllerState.FIRE, 240),
            new stateMap(ControllerState.FIRE_PRESSED, 81),

            new stateMap(ControllerState.TURRET_RIGHT, 244),
            new stateMap(ControllerState.TURRET_RIGHT_PRESSED, 100),

            new stateMap(ControllerState.TURRET_LEFT, 250),
            new stateMap(ControllerState.TURRET_LEFT_PRESSED, 126),

            new stateMap(ControllerState.NOT_CONNECTED, 254)
        };

        ControllerState DecodeState(int reading)
        {
            foreach (stateMap s in stateMapping)
            {
                float diff = Math.Abs(s.Reading - reading);
                if (diff <= tolerance)
                    return s.Result;
            }
            return ControllerState.NO_MATCH;
        }

        static PinState[] pinStates;

        public Hacktroller(string portName)
        {
            port = new SerialPort(portName, 19200);//, Parity.None, 8, StopBits.One);

            port.Open();

            port.DtrEnable = true;

            port.ReadTimeout = 10;
            port.WriteTimeout = 10;

            pinStates = new PinState[numPins];
            portStates = new PortState[numPins];

            for (int i = 0; i < frameBuffer.Length; i++)
            {
                frameBuffer[i] = 20;
            }           
                
        }

        public PortState[] GetPorts()
        {
            port.DiscardInBuffer();
            port.Write(new byte[] { (byte)'R' }, 0, 1);

            System.Threading.Thread.Sleep(10);

            if (port.BytesToRead > 0)
            {
                // The comms starts with 0xff 0xff
                var b = port.ReadByte();
                if (b != 0xff) return null;

                b = port.ReadByte();
                if (b != 'D') return null;

                byte[] buffer = new byte[(numPins) + 1];

                port.Read(buffer, 0, buffer.Length);

                   for (int i = 0; i < numPins; i++)
                //for (int i = 0; i < 1; i++)
                {
                    int reading = buffer[i +1 ];
                    ControllerState readingState = DecodeState(reading);
                        if (readingState == ControllerState.FIRE_PRESSED)
                    {
                        portStates[i].Controller = ControllerState.FIRE_PRESSED;
                    }
                    else
                    {
                        portStates[i].Controller = readingState;
                    }
                }
                Console.WriteLine();
            }
            return portStates;
        }

      
        public bool SetColor(ControllerColor[] colourArray)
        {
            byte[] colourWriteCommand = new byte[] { (byte)'P', 0 };
            byte[] writeColour = new byte[3];


            // My ugly protocol does not support sending panel items larger than 254 pixels 
            if (colourArray.Length > 254)
                return false;

            colourWriteCommand[1] = (byte)colourArray.Length;

            // Write the command
            port.Write(colourWriteCommand, 0, 2);

            byte check = 0;

            foreach (ControllerColor c in colourArray)
            {
                // HACK think this is RBG instead of RGB so have swapped subscripts around
                check += c.R;
                writeColour[0] = c.R;
                check += c.G;
                writeColour[2] = c.G;
                check += c.B;
                writeColour[1] = c.B;
                port.Write(writeColour, 0, 3);
            }

            // Write out the checksum

            writeColour[0] = check;
            port.Write(writeColour, 0, 1);
            return true;
        }
    }
}