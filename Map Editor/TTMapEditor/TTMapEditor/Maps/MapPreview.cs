using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using TTMapEditor.Managers;
using TTMapEditor.Objects;

namespace TTMapEditor.Maps
{
    /// <summary>
    /// Represents a single map instance within the editor.
    /// Handles loading from JSON or legacy text format, building
    /// preview objects (walls, tanks, pickups), and saving changes
    /// back to disk as JSON <see cref="MapData"/>.
    /// </summary>
    internal class MapPreview
    {
        private Rectangle mPlayArea;

        private List<RectWall> mWalls;

        private List<Tank> mTanks;

        private List<Pickup> mPickups;

        private string mFilePath;

        private MapData mMapData;

        /// <summary>
        /// Creates a new map preview based on the supplied file path or directory.
        /// Attempts to load an existing map (JSON or legacy format) and build
        /// the preview object lists; otherwise initializes an empty map.
        /// </summary>
        /// <param name="pFilePath">
        /// Path or identifier used to resolve the map:
        /// can be absolute, relative to a <c>Maps</c> folder, or a directory name.
        /// </param>
        public MapPreview(string pFilePath)
        {
            int screenWidth = TTMapEditor.Instance().GetGraphicsDeviceManager().GraphicsDevice.Viewport.Width;
            int screenHeight = TTMapEditor.Instance().GetGraphicsDeviceManager().GraphicsDevice.Viewport.Height;
            mPlayArea = new Rectangle(screenWidth * 2 / 100, screenHeight * 25 / 100, screenWidth * 96 / 100, screenHeight * 73 / 100);
            mFilePath = pFilePath;

            // initialize lists
            mWalls = new List<RectWall>();
            mTanks = new List<Tank>();
            mPickups = new List<Pickup>();

            // Fill lists from file and keep MapData
            LoadMapPreview();
        }

        /// <summary>
        /// Gets the screen-space bounds of the playable area.
        /// </summary>
        /// <returns>The rectangle representing the play area.</returns>
        public Rectangle GetPlayArea()
        {
            return mPlayArea;
        }

        /// <summary>
        /// Returns the original path or identifier used to resolve this map.
        /// </summary>
        /// <returns>The stored map file path.</returns>
        public string GetFilePath()
        {
            return mFilePath;
        }

        /// <summary>
        /// Gets the current list of preview walls.
        /// </summary>
        /// <returns>A list of <see cref="RectWall"/> objects.</returns>
        public List<RectWall> GetWalls()
        {
            return mWalls;
        }

        /// <summary>
        /// Gets the current list of preview tanks.
        /// </summary>
        /// <returns>A list of <see cref="Tank"/> objects.</returns>
        public List<Tank> GetTanks()
        {
            return mTanks;
        }

        /// <summary>
        /// Gets the current list of preview pickups.
        /// </summary>
        /// <returns>A list of <see cref="Pickup"/> objects.</returns>
        public List<Pickup> GetPickups()
        {
            return mPickups;
        }

        /// <summary>
        /// Gets the <see cref="MapData"/> backing this preview.
        /// This object is used when saving and can be edited by tools.
        /// </summary>
        /// <returns>The current map data.</returns>
        public MapData GetMapData()
        {
            return mMapData;
        }

        /// <summary>
        /// Resolves the underlying map path, loads existing content
        /// (preferring JSON, with a fallback to legacy line format),
        /// and builds the preview object lists and <see cref="MapData"/>.
        /// If no file exists, initializes an empty map.
        /// </summary>
        private void LoadMapPreview()
        {
            // Prefer the bin (runtime) output folder first
            string baseDir = AppContext.BaseDirectory; // points to bin/... where the app runs
            string mapsDir = Path.Combine(baseDir, "Maps");

            // If there's no Maps folder in the bin, try to find one upward (useful when running from IDE)
            if (!Directory.Exists(mapsDir))
            {
                DirectoryInfo? currentDirectory = new DirectoryInfo(baseDir);
                for (int i = 0; i < 6 && currentDirectory != null; i++)
                {
                    string candidateMapsDirectory = Path.Combine(currentDirectory.FullName, "Maps");
                    if (Directory.Exists(candidateMapsDirectory))
                    {
                        mapsDir = candidateMapsDirectory;
                        break;
                    }
                    currentDirectory = currentDirectory.Parent;
                }
            }

            string fullPath = null;

            // If the provided path is absolute and exists as a file, use it directly
            if (Path.IsPathRooted(mFilePath) && File.Exists(mFilePath))
            {
                fullPath = mFilePath;
            }
            else
            {
                // Remove a leading "Maps\" or "Maps/" from mFilePath if present so we don't duplicate it
                string relativePath = mFilePath;
                string mapsPrefix1 = "Maps" + Path.DirectorySeparatorChar;
                string mapsPrefix2 = "Maps" + Path.AltDirectorySeparatorChar;
                if (relativePath.StartsWith(mapsPrefix1) || relativePath.StartsWith(mapsPrefix2))
                {
                    relativePath = relativePath.Substring(5);
                }

                fullPath = Path.Combine(mapsDir, relativePath);

                // If the path resolves to a directory (e.g. a map folder name), look for "map.json" inside it
                if (Directory.Exists(fullPath))
                {
                    string candidate = Path.Combine(fullPath, "map.json");
                    if (File.Exists(candidate))
                    {
                        fullPath = candidate;
                    }
                    else
                    {
                        // directory exists but no map.json -> mark fullPath to the directory so we can treat as new map
                        // (we'll initialize empty MapData below)
                        fullPath = Path.Combine(fullPath, "map.json");
                    }
                }
            }

            // As a final attempt, if the path doesn't point to an existing file, try appending "map.json"
            if (!File.Exists(fullPath))
            {
                string alternativePath = fullPath;
                if (!fullPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    alternativePath = Path.Combine(fullPath, "map.json");
                }

                if (File.Exists(alternativePath))
                {
                    fullPath = alternativePath;
                }
                else
                {
                    // File not found — this is valid when creating a new map.
                    // Initialize empty MapData and empty preview lists and return.
                    mMapData = new MapData()
                    {
                        Walls = new List<WallData>(),
                        Tanks = new List<TankData>(),
                        Pickups = new List<PickupData>()
                    };

                    mWalls = new List<RectWall>();
                    mTanks = new List<Tank>();
                    mPickups = new List<Pickup>();

                    // Keep mFilePath as provided (full path where the file will be written when saved).
                    return;
                }
            }

            string content = File.ReadAllText(fullPath);

            // try JSON first and remember the MapData used to build the preview
            try
            {
                mMapData = JsonSerializer.Deserialize<MapData>(content);
            }
            catch (JsonException)
            {
                mMapData = null;
            }

            if (mMapData != null)
            {
                // build preview lists from mMapData
                BuildPreviewFromMapData(mMapData);
                return;
            }

            // fallback: treat file as legacy line format and parse
            string[] lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            mWalls = ParseLines(lines);

            // construct a MapData from the legacy parsing so callers can still get MapData
            mMapData = new MapData()
            {
                Walls = mWalls.Select(w =>
                {
                    string textureName = "block";

                    var rect = w.mRectangle;
                    float posX = (rect.X - mPlayArea.X) * 100.0f / mPlayArea.Width;
                    float posY = (rect.Y - mPlayArea.Y) * 100.0f / mPlayArea.Height;
                    float sizeX = rect.Width * 100.0f / mPlayArea.Width;
                    float sizeY = rect.Height * 100.0f / mPlayArea.Height;

                    return new WallData
                    {
                        Texture = textureName,
                        Position = new[] { posX.ToString(CultureInfo.InvariantCulture), posY.ToString(CultureInfo.InvariantCulture) },
                        Size = new[] { sizeX.ToString(CultureInfo.InvariantCulture), sizeY.ToString(CultureInfo.InvariantCulture) }
                    };
                }).ToList(),

                Tanks = new List<TankData>(),
                Pickups = new List<PickupData>()
            };
        }

        /// <summary>
        /// Builds all preview objects (walls, tanks, pickups) from a given <see cref="MapData"/>.
        /// Converts percentage-based map coordinates into screen-space rectangles and loads textures.
        /// </summary>
        /// <param name="pMapData">The map data to render in the preview.</param>
        private void BuildPreviewFromMapData(MapData pMapData)
        {
            mWalls = new List<RectWall>();
            mTanks = new List<Tank>();
            mPickups = new List<Pickup>();

            if (pMapData.Walls != null)
            {
                foreach (WallData wallData in pMapData.Walls)
                {
                    if (wallData == null) continue;
                    float posX = 0f, posY = 0f;
                    float sizeX = 0f, sizeY = 0f;
                    float rotationDeg = 0f;
                    if (wallData.Position != null && wallData.Position.Length >= 2)
                    {
                        float.TryParse(wallData.Position[0], NumberStyles.Float, CultureInfo.InvariantCulture, out posX);
                        float.TryParse(wallData.Position[1], NumberStyles.Float, CultureInfo.InvariantCulture, out posY);
                    }
                    if (wallData.Size != null && wallData.Size.Length >= 2)
                    {
                        float.TryParse(wallData.Size[0], NumberStyles.Float, CultureInfo.InvariantCulture, out sizeX);
                        float.TryParse(wallData.Size[1], NumberStyles.Float, CultureInfo.InvariantCulture, out sizeY);
                    }
                    if (wallData.Rotation != null)
                    {
                        float.TryParse(wallData.Rotation, NumberStyles.Float, CultureInfo.InvariantCulture, out rotationDeg);
                    }

                    Vector2 position = new Vector2(
                        mPlayArea.X + ((float)mPlayArea.Width * (posX / 100.0f)),
                        mPlayArea.Y + ((float)mPlayArea.Height * (posY / 100.0f))
                    );
                    Vector2 size = new Vector2(
                        mPlayArea.Width * (sizeX / 100.0f),
                        mPlayArea.Height * (sizeY / 100.0f)
                    );

                    Texture2D texture;
                    try
                    {
                        texture = TTMapEditor.Instance().GetContentManager().Load<Texture2D>(wallData.Texture);
                    }
                    catch
                    {
                        texture = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("block");
                    }

                    mWalls.Add(new RectWall(texture, new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y), rotationDeg));
                }
            }

            if (pMapData.Tanks != null)
            {
                foreach (TankData tankData in pMapData.Tanks)
                {
                    if (tankData?.Position == null || tankData.Position.Length < 2) continue;
                    float posX = 0f, posY = 0f;
                    float.TryParse(tankData.Position[0], NumberStyles.Float, CultureInfo.InvariantCulture, out posX);
                    float.TryParse(tankData.Position[1], NumberStyles.Float, CultureInfo.InvariantCulture, out posY);

                    Vector2 position = new Vector2(
                        mPlayArea.X + ((float)mPlayArea.Width * (posX / 100.0f)),
                        mPlayArea.Y + ((float)mPlayArea.Height * (posY / 100.0f))
                    );

                    // parse rotation if present (map stores degrees); default 0
                    float rotationDeg = 0f;
                    if (!string.IsNullOrEmpty(tankData.Rotation))
                    {
                        float.TryParse(tankData.Rotation, NumberStyles.Float, CultureInfo.InvariantCulture, out rotationDeg);
                    }
                    float rotationRad = MathHelper.ToRadians(rotationDeg);

                    // preview size and creation — your Tank preview constructor may differ; adapt as needed
                    int previewSize = 10;
                    Rectangle rect = new Rectangle((int)position.X - previewSize / 2, (int)position.Y - previewSize / 2, previewSize, previewSize);
                    Texture2D texture;
                    try { texture = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("block"); }
                    catch { texture = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("block"); }

                    var previewTank = new Tank(texture, rect);
                    previewTank.mRotation = rotationRad;
                    mTanks.Add(previewTank);
                }
            }

            if (pMapData.Pickups != null)
            {
                foreach (PickupData pickupData in pMapData.Pickups)
                {
                    if (pickupData?.Position == null || pickupData.Position.Length < 2) continue;
                    float posX = 0f, posY = 0f;
                    float.TryParse(pickupData.Position[0], NumberStyles.Float, CultureInfo.InvariantCulture, out posX);
                    float.TryParse(pickupData.Position[1], NumberStyles.Float, CultureInfo.InvariantCulture, out posY);

                    Vector2 position = new Vector2(
                        mPlayArea.X + ((float)mPlayArea.Width * (posX / 100.0f)),
                        mPlayArea.Y + ((float)mPlayArea.Height * (posY / 100.0f))
                    );

                    int previewSize = 9;
                    Rectangle rect = new Rectangle((int)position.X - previewSize / 2, (int)position.Y - previewSize / 2, previewSize, previewSize);
                    Texture2D texture;
                    try { texture = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("circle"); }
                    catch { texture = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("circle"); }

                    // Create preview pickup and apply activation map if present in the MapData
                    var previewPickup = new Pickup(texture, rect);
                    try
                    {
                        // if the deserialized PickupData contains ActivatedPickups (Dictionary<PickupType,bool>), apply it
                        if (pickupData.GetType().GetProperty("ActivatedPickups") != null)
                        {
                            var activatedProp = pickupData.GetType().GetProperty("ActivatedPickups")!.GetValue(pickupData);
                            if (activatedProp is Dictionary<PickupType, bool> enumMap)
                            {
                                previewPickup.SetActivatedPickups(enumMap);
                            }
                            else if (activatedProp is Dictionary<string, bool> stringMap)
                            {
                                // convert string keys to enum where possible
                                var converted = new Dictionary<PickupType, bool>();
                                foreach (var kv in stringMap)
                                {
                                    if (Enum.TryParse<PickupType>(kv.Key, true, out var keyEnum))
                                    {
                                        converted[keyEnum] = kv.Value;
                                    }
                                }
                                previewPickup.SetActivatedPickups(converted);
                            }
                        }
                    }
                    catch
                    {
                        // ignore if ActivatedPickups property isn't present or conversion fails
                    }

                    mPickups.Add(previewPickup);
                }
            }
        }

        /// <summary>
        /// Parses a legacy, line-based map format and produces a collection of walls.
        /// This is kept for backwards compatibility with older map files.
        /// </summary>
        /// <param name="pLines">All lines read from the legacy map file.</param>
        /// <returns>A list of walls reconstructed from the legacy format.</returns>
        private List<RectWall> ParseLines(string[] pLines)
        {
            List<RectWall> walls = new List<RectWall>();

            string texture = null;
            Vector2 positionFallback = Vector2.Zero;
            Vector2 sizeFallback = Vector2.Zero;
            bool isWall = false;

            foreach (string line in pLines)
            {
                if (!isWall)
                {
                    texture = null;
                    positionFallback = Vector2.Zero;
                    sizeFallback = Vector2.Zero;
                }

                if (line.Contains("Walls") || line.StartsWith("wall"))
                {
                    isWall = true;
                    continue;
                }
                else if (line.Contains("Texture") || line.Contains("texture"))
                {
                    texture = line.Split('=')[1].Trim().Trim('"');
                    continue;
                }
                else if (line.Contains("Position") || line.Contains("position"))
                {
                    string[] components = line.Split('=')[1].Trim().Split(',');
                    positionFallback = new Vector2(float.Parse(components[0], CultureInfo.InvariantCulture), float.Parse(components[1], CultureInfo.InvariantCulture));
                    positionFallback.X = mPlayArea.X + ((float)mPlayArea.Width * (positionFallback.X / 100.0f));
                    positionFallback.Y = mPlayArea.Y + ((float)mPlayArea.Height * (positionFallback.Y / 100.0f));
                    continue;
                }
                else if (line.Contains("Size") || line.Contains("size"))
                {
                    string[] components = line.Split('=')[1].Trim().Split(',');
                    sizeFallback = new Vector2(float.Parse(components[0], CultureInfo.InvariantCulture), float.Parse(components[1], CultureInfo.InvariantCulture));
                    sizeFallback.X = mPlayArea.Width * (sizeFallback.X / 100.0f);
                    sizeFallback.Y = mPlayArea.Height * (sizeFallback.Y / 100.0f);
                    continue;
                }

                if (isWall)
                {
                    RectWall currentWall = new RectWall(
                        TTMapEditor.Instance().GetContentManager().Load<Texture2D>(texture),
                        new Rectangle((int)positionFallback.X, (int)positionFallback.Y, (int)sizeFallback.X, (int)sizeFallback.Y));
                    walls.Add(currentWall);
                    isWall = false;
                }
            }
            return walls;
        }

        /// <summary>
        /// Adds a scene object (wall, tank, or pickup) to the preview.
        /// The object is routed into the appropriate internal list.
        /// </summary>
        /// <param name="pObject">Object instance to add to the map preview.</param>
        public void AddObject(SceneObject pObject)
        {
            switch (pObject)
            {
                case RectWall wall:
                    mWalls.Add(wall);
                    break;
                case Tank tank:
                    mTanks.Add(tank);
                    break;
                case Pickup pickup:
                    mPickups.Add(pickup);
                    break;
            }
        }

        /// <summary>
        /// Removes a scene object (wall, tank, or pickup) from the preview.
        /// </summary>
        /// <param name="pObject">Object instance to remove from the map preview.</param>
        public void RemoveObject(SceneObject pObject)
        {
            switch (pObject)
            {
                case RectWall wall:
                    mWalls.Remove(wall);
                    break;
                case Tank tank:
                    mTanks.Remove(tank);
                    break;
                case Pickup pickup:
                    mPickups.Remove(pickup);
                    break;
            }
        }

        /// <summary>
        /// Serializes the current preview state into <see cref="MapData"/> and writes it as JSON.
        /// Resolves the output path based on the original file path and the provided map name.
        /// </summary>
        /// <param name="pMapName">
        /// Name or path to save as:
        /// simple name -> <c>&lt;base&gt;\name.json</c>,
        /// directory or rooted path without extension -> <c>dir\map.json</c>,
        /// full file path (with extension) -> saved directly.
        /// </param>
        public void SaveMap(string pMapName)
        {
            // Ensure MapData exists and lists are initialized
            if (mMapData == null)
            {
                mMapData = new MapData
                {
                    Walls = new List<WallData>(),
                    Tanks = new List<TankData>(),
                    Pickups = new List<PickupData>()
                };
            }

            // Populate MapData from the current preview
            MapDataFromPreview();

            // Resolve where to save the map (mirror the LoadMapPreview resolution rules)
            string baseDir = AppContext.BaseDirectory;
            string mapsDir = Path.Combine(baseDir, "Maps");

            if (!Directory.Exists(mapsDir))
            {
                DirectoryInfo? currentDirectory = new DirectoryInfo(baseDir);
                for (int i = 0; i < 6 && currentDirectory != null; i++)
                {
                    string candidateMapsDirectory = Path.Combine(currentDirectory.FullName, "Maps");
                    if (Directory.Exists(candidateMapsDirectory))
                    {
                        mapsDir = candidateMapsDirectory;
                        break;
                    }
                    currentDirectory = currentDirectory.Parent;
                }
            }

            // Decide base directory to place the new file in
            string outputBaseDir;

            if (Path.IsPathRooted(mFilePath))
            {
                // If mFilePath is a directory (or looks like one), use it
                if (Directory.Exists(mFilePath) || !Path.HasExtension(mFilePath))
                {
                    outputBaseDir = mFilePath;
                }
                else
                {
                    // mFilePath is a file -> save sibling files in the same directory
                    string? parent = Path.GetDirectoryName(mFilePath);
                    outputBaseDir = !string.IsNullOrEmpty(parent) ? parent : mapsDir;
                }
            }
            else
            {
                // relative path handling: strip leading "Maps\" if present
                string relativePath = mFilePath;
                string mapsPrefix1 = "Maps" + Path.DirectorySeparatorChar;
                string mapsPrefix2 = "Maps" + Path.AltDirectorySeparatorChar;
                if (relativePath.StartsWith(mapsPrefix1) || relativePath.StartsWith(mapsPrefix2))
                {
                    relativePath = relativePath.Substring(5);
                }

                string combined = Path.Combine(mapsDir, relativePath);

                if (Directory.Exists(combined) || !Path.HasExtension(combined))
                {
                    outputBaseDir = combined;
                }
                else
                {
                    string? parent = Path.GetDirectoryName(combined);
                    outputBaseDir = !string.IsNullOrEmpty(parent) ? parent : mapsDir;
                }
            }

            string targetPath;

            // If the user typed a path (contains separator or is rooted) treat it as a path
            bool nameLooksLikePath = Path.IsPathRooted(pMapName)
                                     || pMapName.Contains(Path.DirectorySeparatorChar)
                                     || pMapName.Contains(Path.AltDirectorySeparatorChar);

            if (nameLooksLikePath)
            {
                // If typed value has an extension, treat it as the full filename
                if (Path.HasExtension(pMapName))
                {
                    targetPath = Path.GetFullPath(pMapName);
                    string? parent = Path.GetDirectoryName(targetPath);
                    if (!string.IsNullOrEmpty(parent)) Directory.CreateDirectory(parent);
                }
                else
                {
                    // No extension: treat as directory -> save map.json inside it
                    string candidateDir = Path.GetFullPath(pMapName);
                    Directory.CreateDirectory(candidateDir);
                    targetPath = Path.Combine(candidateDir, "map.json");
                }
            }
            else
            {
                // simple name: create <outputBaseDir>\<name>.json
                Directory.CreateDirectory(outputBaseDir);
                string safeName = pMapName;
                if (string.IsNullOrWhiteSpace(safeName))
                {
                    // fallback to "map" if the name is empty
                    safeName = "map";
                }
                targetPath = Path.Combine(outputBaseDir, $"{safeName}.json");
            }

            // Serialize MapData and write
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(mMapData, options);
            File.WriteAllText(targetPath, json);
        }

        /// <summary>
        /// Regenerates <see cref="MapData"/> from the current preview objects.
        /// Converts screen-space rectangles back to percentage-based coordinates.
        /// </summary>
        public void MapDataFromPreview()
        {
            mMapData.Tanks.Clear();
            mMapData.Pickups.Clear();
            mMapData.Walls.Clear();

            // Adding walls to map data
            foreach (RectWall wall in mWalls)
            {
                float rotationDeg = wall.mRotation;
                mMapData.Walls.Add(new WallData()
                {
                    Texture = "block",
                    Position = new string[]
                    {
                        ((wall.mRectangle.X - mPlayArea.X) * 100.0f / mPlayArea.Width).ToString(CultureInfo.InvariantCulture),
                        ((wall.mRectangle.Y - mPlayArea.Y) * 100.0f / mPlayArea.Height).ToString(CultureInfo.InvariantCulture)
                    },
                    Size = new string[]
                    {
                        (wall.mRectangle.Width * 100.0f / mPlayArea.Width).ToString(CultureInfo.InvariantCulture),
                        (wall.mRectangle.Height * 100.0f / mPlayArea.Height).ToString(CultureInfo.InvariantCulture)
                    },
                    Rotation = rotationDeg.ToString(CultureInfo.InvariantCulture)
                });
            }

            // Adding tanks to map data
            foreach (Tank tank in mTanks)
            {
                float posX = (tank.mRectangle.X + tank.mRectangle.Width / 2 - mPlayArea.X) * 100.0f / mPlayArea.Width;
                float posY = (tank.mRectangle.Y + tank.mRectangle.Height / 2 - mPlayArea.Y) * 100.0f / mPlayArea.Height;
                float rotationDeg = MathHelper.ToDegrees(tank.mRotation);
                mMapData.Tanks.Add(new TankData()
                {
                    Position = new string[]
                    {
                        posX.ToString(CultureInfo.InvariantCulture),
                        posY.ToString(CultureInfo.InvariantCulture)
                    },
                    Rotation = rotationDeg.ToString(CultureInfo.InvariantCulture)
                });
            }

            // Adding pickups to map data
            foreach (Pickup pickup in mPickups)
            {
                float posX = (pickup.mRectangle.X + pickup.mRectangle.Width / 2 - mPlayArea.X) * 100.0f / mPlayArea.Width;
                float posY = (pickup.mRectangle.Y + pickup.mRectangle.Height / 2 - mPlayArea.Y) * 100.0f / mPlayArea.Height;
                mMapData.Pickups.Add(new PickupData()
                {
                    Position = new string[]
                    {
                        posX.ToString(CultureInfo.InvariantCulture),
                        posY.ToString(CultureInfo.InvariantCulture)
                    },
                    ActivatedPickups = pickup.GetActivatedPickups()
                });
            }
        }
    }
}





