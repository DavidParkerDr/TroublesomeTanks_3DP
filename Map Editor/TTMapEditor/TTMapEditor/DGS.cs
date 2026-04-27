using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace TTMapEditor
{
    /// <summary>
    /// Global data store (DGS) for simple typed configuration values
    /// loaded from a text file at startup.
    /// </summary>
    internal class DGS
    {
        /// <summary>
        /// Singleton instance backing field.
        /// </summary>
        private static DGS mInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="DGS"/> class.
        /// Constructs the internal dictionaries and loads the default
        /// configuration file <c>Content/DGS.txt</c>.
        /// </summary>
        private DGS()
        {
            mBools = new Dictionary<string, bool>();
            mInts = new Dictionary<string, int>();
            mFloats = new Dictionary<string, float>();
            mColours = new Dictionary<string, Color>();
            mStrings = new Dictionary<string, string>();

            // Load initial values from the default configuration file.
            LoadFile("Content/DGS.txt");
        }

        /// <summary>
        /// Gets the single shared instance of the <see cref="DGS"/> class.
        /// The instance is created on first access.
        /// </summary>
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

        /// <summary>
        /// Adds a float value with the specified variable name.
        /// </summary>
        /// <param name="pVariableName">The key used to identify the value.</param>
        /// <param name="pValue">The value to store.</param>
        public void AddFloat(string pVariableName, float pValue)
        {
            mFloats.Add(pVariableName, pValue);
        }

        /// <summary>
        /// Adds an int value with the specified variable name.
        /// </summary>
        /// <param name="pVariableName">The key used to identify the value.</param>
        /// <param name="pValue">The value to store.</param>
        public void AddInt(string pVariableName, int pValue)
        {
            mInts.Add(pVariableName, pValue);
        }

        /// <summary>
        /// Adds a bool value with the specified variable name.
        /// </summary>
        /// <param name="pVariableName">The key used to identify the value.</param>
        /// <param name="pValue">The value to store.</param>
        public void AddBool(string pVariableName, bool pValue)
        {
            mBools.Add(pVariableName, pValue);
        }

        /// <summary>
        /// Adds a string value with the specified variable name.
        /// </summary>
        /// <param name="pVariableName">The key used to identify the value.</param>
        /// <param name="pValue">The value to store.</param>
        public void AddString(string pVariableName, string pValue)
        {
            mStrings.Add(pVariableName, pValue);
        }

        /// <summary>
        /// Adds a color value with the specified variable name.
        /// </summary>
        /// <param name="pVariableName">The key used to identify the value.</param>
        /// <param name="pValue">The value to store.</param>
        public void AddColour(string pVariableName, Color pValue)
        {
            mColours.Add(pVariableName, pValue);
        }

        /// <summary>
        /// Loads and parses a configuration file, adding all values
        /// it contains to the internal dictionaries.
        /// </summary>
        /// <param name="pFilePath">Path to the configuration file.</param>
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

                    // Trim whitespace and skip comment lines starting with "//".
                    line = line.Trim();
                    if (line.Substring(0, 2) == "//")
                    {
                        continue;
                    }

                    LoadLine(line);
                }
            }
        }

        /// <summary>
        /// Parses a single line of configuration text and stores the value
        /// in the appropriate type dictionary.
        /// </summary>
        /// <param name="pLine">
        /// A line in the form: <c>&lt;type&gt; &lt;name&gt; = &lt;value&gt;;</c>.
        /// Supported types: <c>float</c>, <c>int</c>, <c>Color</c>, <c>bool</c>, <c>string</c>.
        /// </param>
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
                if (string.IsNullOrEmpty(token))
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
                // Expect a 6-character hex RGB string (RRGGBB).
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

        /// <summary>
        /// Stores boolean values by variable name.
        /// </summary>
        private Dictionary<string, bool> mBools;

        /// <summary>
        /// Stores float values by variable name.
        /// </summary>
        private Dictionary<string, float> mFloats;

        /// <summary>
        /// Stores integer values by variable name.
        /// </summary>
        private Dictionary<string, int> mInts;

        /// <summary>
        /// Stores string values by variable name.
        /// </summary>
        private Dictionary<string, string> mStrings;

        /// <summary>
        /// Stores color values by variable name.
        /// </summary>
        private Dictionary<string, Color> mColours;

        /// <summary>
        /// Gets a stored string value or an empty string if the key does not exist.
        /// </summary>
        /// <param name="pKey">The variable name.</param>
        /// <returns>The stored value, or <see cref="string.Empty"/> when missing.</returns>
        public string GetString(string pKey)
        {
            if (mStrings.ContainsKey(pKey))
            {
                return mStrings[pKey];
            }

            return "";
        }

        /// <summary>
        /// Gets a stored int value or 0 if the key does not exist.
        /// </summary>
        /// <param name="pKey">The variable name.</param>
        /// <returns>The stored value, or 0 when missing.</returns>
        public int GetInt(string pKey)
        {
            if (mInts.ContainsKey(pKey))
            {
                return mInts[pKey];
            }

            return 0;
        }

        /// <summary>
        /// Gets a stored float value or 0.0f if the key does not exist.
        /// </summary>
        /// <param name="pKey">The variable name.</param>
        /// <returns>The stored value, or 0.0f when missing.</returns>
        public float GetFloat(string pKey)
        {
            if (mFloats.ContainsKey(pKey))
            {
                return mFloats[pKey];
            }

            return 0.0f;
        }

        /// <summary>
        /// Gets a stored bool value or false if the key does not exist.
        /// </summary>
        /// <param name="pKey">The variable name.</param>
        /// <returns>The stored value, or <c>false</c> when missing.</returns>
        public bool GetBool(string pKey)
        {
            if (mBools.ContainsKey(pKey))
            {
                return mBools[pKey];
            }

            return false;
        }

        /// <summary>
        /// Gets a stored color value or <see cref="Color.Black"/> if the key does not exist.
        /// </summary>
        /// <param name="pKey">The variable name.</param>
        /// <returns>The stored value, or <see cref="Color.Black"/> when missing.</returns>
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
