using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.IO;

namespace Tankontroller
{
    public class DGS
    {
        private static DGS mInstance;
        private DGS()
        {
            mBools = new Dictionary<string, bool>();
            mInts = new Dictionary<string, int>();
            mFloats = new Dictionary<string, float>();
            mColours = new Dictionary<string, Color>();
            mStrings = new Dictionary<string, string>();

            LoadFile("Content/DGS.txt");
        }
        public static DGS Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new DGS();
                }
                return mInstance;
            }
        }

        public void AddFloat(string pVariableName, float pValue)
        {
            mFloats.Add(pVariableName, pValue);
        }
        public void AddInt(string pVariableName, int pValue)
        {
            mInts.Add(pVariableName, pValue);
        }
        public void AddBool(string pVariableName, bool pValue)
        {
            mBools.Add(pVariableName, pValue);
        }
        public void AddString(string pVariableName, string pValue)
        {
            mStrings.Add(pVariableName, pValue);
        }
        public void AddColour(string pVariableName, Color pValue)
        {
            mColours.Add(pVariableName, pValue);
        }

        public void LoadFile(string pFilePath)
        {
            using (StreamReader reader = new StreamReader(pFilePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    // line contains text
                    line = line.Trim();
                    if (line.Substring(0, 2) == "//")
                    {
                        //line is a comment, skip
                        continue;
                    }
                    LoadLine(line);
                }
            }
        }
        private void LoadLine(string pLine)
        {
            char[] splitters = { ' ', '=', ';' };
            string[] tokens = pLine.Split(splitters);
            string typeString = "";
            string variableString = "";
            string valueString = "";
            int count = 0;
            foreach (string token in tokens)
            {
                if (String.IsNullOrEmpty(token))
                {
                    continue;
                }
                if (count == 0)
                {
                    typeString = token.Trim();
                }
                else if (count == 1)
                {
                    variableString = token.Trim();
                }
                else if (count == 2)
                {
                    valueString = token.Trim();
                }
                count++;
            }
            if (typeString == "float")
            {
                float value = float.Parse(valueString);
                AddFloat(variableString, value);
            }
            else if (typeString == "int")
            {
                int value = int.Parse(valueString);
                AddInt(variableString, value);
            }
            else if (typeString == "Color")
            {
                string rString = valueString.Substring(0, 2);
                string gString = valueString.Substring(2, 2);
                string bString = valueString.Substring(4, 2);

                int r = int.Parse(rString, System.Globalization.NumberStyles.HexNumber);
                int g = int.Parse(gString, System.Globalization.NumberStyles.HexNumber);
                int b = int.Parse(bString, System.Globalization.NumberStyles.HexNumber);

                Color value = new Color(r, g, b);
                AddColour(variableString, value);
            }
            else if (typeString == "bool")
            {
                bool value = bool.Parse(valueString);
                AddBool(variableString, value);
            }
            else if (typeString == "string")
            {
                AddString(variableString, valueString);
            }
        }

        private Dictionary<string, bool> mBools;
        private Dictionary<string, float> mFloats;
        private Dictionary<string, int> mInts;
        private Dictionary<string, string> mStrings;
        private Dictionary<string, Color> mColours;

        public string GetString(string pKey)
        {
            if (mStrings.ContainsKey(pKey))
            {
                return mStrings[pKey];
            }
            return "";
        }
        public int GetInt(string pKey)
        {
            if (mInts.ContainsKey(pKey))
            {
                return mInts[pKey];
            }
            return 0;
        }
        public float GetFloat(string pKey)
        {
            if (mFloats.ContainsKey(pKey))
            {
                return mFloats[pKey];
            }
            return 0.0f;
        }
        public bool GetBool(string pKey)
        {
            if (mBools.ContainsKey(pKey))
            {
                return mBools[pKey];
            }
            return false;
        }
        public Color GetColour(string pKey)
        {
            if (mColours.ContainsKey(pKey))
            {
                return mColours[pKey];
            }
            return Color.Black;
        }
    }
}
