using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.World;
using Tankontroller.World.WorldObject;
using Tankontroller.World.Pickups;

namespace Tankontroller
{
    public static class MapManager
    {
        /// <summary>
        /// DEPRECATED method for loading maps from a custom text format. Use LoadMapFromJson instead for better performance and maintainability.
        /// </summary>
        public static TheWorld LoadMap(string filePath)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            string fullPath = Path.Combine(projectDirectory, filePath);

            string[] lines = File.ReadAllLines(fullPath);
            return ParseLines(lines);
        }

        /// <summary>
        /// DEPRECATED method for loading maps from a custom text format. Use LoadMapFromJson instead for better performance and maintainability.
        /// </summary>
        private static TheWorld ParseLines(string[] lines)
        {
            int screenWidth = Tankontroller.Instance().GDM().GraphicsDevice.Viewport.Width;
            int screenHeight = Tankontroller.Instance().GDM().GraphicsDevice.Viewport.Height;
            Rectangle playArea = new Rectangle(screenWidth * 2 / 100, screenHeight * 25 / 100, screenWidth * 96 / 100, screenHeight * 73 / 100);
            List<RectWall> Walls = new List<RectWall>();
            List<Tank> Tanks = new List<Tank>();
            List<PickupSpawnPoint> PickupSpawnPositions = new List<PickupSpawnPoint>();
            float tankScale = (float)(playArea.Width / (50 * 40)) * Tankontroller.Instance().ScaleFactor(); // Adjusted for resolution scale factor

            string texture = null;
            Vector2 position = Vector2.Zero;
            Vector2 size = Vector2.Zero;
            Dictionary<PickupType, bool> activatedPickups = new Dictionary<PickupType, bool>();
            float rotation = 0f;
            bool isWall = false;
            bool isTank = false;
            bool isPickup = false;

            foreach (string line in lines)
            {
                if (!isWall && !isTank && !isPickup)
                {
                    texture = null;
                    position = Vector2.Zero;
                    size = Vector2.Zero;
                    rotation = 0f;
                }

                if (line.StartsWith("wall"))
                {
                    isWall = true;
                    continue;
                }
                else if (line.StartsWith("tank"))
                {
                    isTank = true;
                    continue;
                }
                else if (line.StartsWith("pickup"))
                {
                    isPickup = true;
                    continue;
                }
                else if (line.Contains("texture"))
                {
                    texture = line.Split('=')[1].Trim().Trim('"');
                    continue;
                }
                else if (line.Contains("position"))
                {
                    string[] components = line.Split('=')[1].Trim().Split(',');
                    position = new Vector2(float.Parse(components[0]), float.Parse(components[1]));
                    position.X = playArea.X + ((float)playArea.Width * (position.X / 100.0f));
                    position.Y = playArea.Y + ((float)playArea.Height * (position.Y / 100.0f));
                    continue;
                }
                else if (line.Contains("size"))
                {
                    string[] components = line.Split('=')[1].Trim().Split(',');
                    size = new Vector2(float.Parse(components[0]), float.Parse(components[1]));
                    size.X = playArea.Width * (size.X / 100.0f);
                    size.Y = playArea.Height * (size.Y / 100.0f);
                    continue;
                }
                else if (line.Contains("rotation"))
                {
                    rotation = float.Parse(line.Split('=')[1].Trim());
                    continue;
                }
                else if(line.Contains("health"))
                {
                    string[] components = line.Split('=')[1].Trim().Split(',');
                    activatedPickups[PickupType.HEALTH] = bool.Parse(components[0]);
                }
                else if(line.Contains("emp"))
                {
                   string[] components = line.Split('=')[1].Trim().Split(',');
                    activatedPickups[PickupType.EMP] = bool.Parse(components[0]);
                }
                else if(line.Contains("mine"))
                {
                    string[] components = line.Split('=')[1].Trim().Split(',');
                    activatedPickups[PickupType.MINE] = bool.Parse(components[0]);
                }
                else if(line.Contains("bouncy_bullet"))
                {
                    string[] components = line.Split('=')[1].Trim().Split(',');
                    activatedPickups[PickupType.BOUNCY_BULLET] = bool.Parse(components[0]);
                }

                //check if the current object is a wall or a tank
                if (isWall)
                {
                    Texture2D wallTexture = Tankontroller.Instance().CM().Load<Texture2D>(texture);
                    Transform wallTransform = new(position, rotation);
                    RectWall currentWall = new(wallTransform, new Vector2(size.X, size.Y), wallTexture);
                    Walls.Add(currentWall);
                    isWall = false;
                }
                else if (isTank)
                {
                    Tanks.Add(new Tank(position, rotation, tankScale));
                    isTank = false;
                }
                else if (isPickup)
                {
                    PickupSpawnPositions.Add(new PickupSpawnPoint(position,activatedPickups));
                    isPickup = false;
                }
            }
            return new TheWorld(playArea, Walls, Tanks, PickupSpawnPositions);
        }

        public static TheWorld LoadMapFromJson(string filePath)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string fullPath = Path.Combine(workingDirectory, "Maps", filePath);

            string jsonString = File.ReadAllText(fullPath);
            return ParseJson(jsonString);
        }

        private static TheWorld ParseJson(string jsonString)
        {
            var mapData = JsonSerializer.Deserialize<MapData>(jsonString);
            if (mapData == null)
            {
                throw new Exception("Failed to deserialize JSON to MapData.");
            }

            int screenWidth = Tankontroller.Instance().GDM().GraphicsDevice.Viewport.Width;
            int screenHeight = Tankontroller.Instance().GDM().GraphicsDevice.Viewport.Height;
            Rectangle playArea = new Rectangle(screenWidth * 2 / 100, screenHeight * 25 / 100, screenWidth * 96 / 100, screenHeight * 73 / 100);
            List<RectWall> Walls = new List<RectWall>();
            List<Tank> Tanks = new List<Tank>();
            List<PickupSpawnPoint> PickupSpawnPositions = new List<PickupSpawnPoint>();
            float tankScale = (float)playArea.Width / (50 * 40);

            foreach (var wall in mapData.Walls)
            {
                try
                {
                    var texture = wall.Texture;
                    var positionPercent = new Vector2(float.Parse(wall.Position[0]), float.Parse(wall.Position[1]));
                    var sizePercent = new Vector2(float.Parse(wall.Size[0]), float.Parse(wall.Size[1]));
                    Rectangle wallRect = GetWorldRect(playArea, positionPercent, sizePercent);
                    Vector2 wallCenter = new Vector2(wallRect.X + wallRect.Width / 2f, wallRect.Y + wallRect.Height / 2f);
                    Vector2 wallSize = new Vector2(wallRect.Width, wallRect.Height);
                    float rotationDegrees = 0f;
                    float.TryParse(wall.Rotation, out rotationDegrees);
                    float rotationRadians = rotationDegrees;
                    Transform wallTransform = new(wallCenter, rotationRadians);

                    RectWall currentWall = new RectWall(wallTransform,wallSize,Tankontroller.Instance().CM().Load<Texture2D>(texture));

                    Walls.Add(currentWall);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing wall data: {ex.Message}");
                }
            }

            foreach (var tank in mapData.Tanks)
            {
                try
                {
                    var position = new Vector2(float.Parse(tank.Position[0]), float.Parse(tank.Position[1]));
                    var rotation = MathHelper.ToRadians(float.Parse(tank.Rotation));
                    position.X = playArea.X + (playArea.Width * (position.X / 100.0f));
                    position.Y = playArea.Y + (playArea.Height * (position.Y / 100.0f));

                    Tanks.Add(new Tank(position, rotation, tankScale));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing tank data: {ex.Message}");
                }
            }

            foreach (var pickup in mapData.Pickups)
            {
                try
                {
                    var position = new Vector2(float.Parse(pickup.Position[0]), float.Parse(pickup.Position[1]));
                    position.X = playArea.X + (playArea.Width * (position.X / 100.0f));
                    position.Y = playArea.Y + (playArea.Height * (position.Y / 100.0f));
                    Dictionary <PickupType,bool> activatedPickups = pickup.ActivatedPickups;

                    PickupSpawnPositions.Add(new PickupSpawnPoint(position,activatedPickups));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing pickup data: {ex.Message}");
                }
            }

            return new TheWorld(playArea, Walls, Tanks, PickupSpawnPositions);
        }

        private static Rectangle GetWorldRect(Rectangle pPlayArea, Vector2 pPosPercent, Vector2 pSizePercent)
        {
            return new Rectangle(
                (int)(pPlayArea.X + (pPlayArea.Width * (pPosPercent.X / 100.0f))),
                (int)(pPlayArea.Y + (pPlayArea.Height * (pPosPercent.Y / 100.0f))),
                (int)(pPlayArea.Width * (pSizePercent.X / 100.0f)),
                (int)(pPlayArea.Height * (pSizePercent.Y / 100.0f))
            );
        }

        public class MapData
        {
            public List<WallData> Walls { get; set; }
            public List<TankData> Tanks { get; set; }
            public List<PickupData> Pickups { get; set; }
        }

        public class WallData
        {
            public string Texture { get; set; }
            public string[] Position { get; set; }
            public string[] Size { get; set; }
            public string Rotation { get; set; }
        }

        public class TankData
        {
            public string[] Position { get; set; }
            public string Rotation { get; set; }
        }

        public class PickupData
        {
            public string[] Position { get; set; }
            public Dictionary<PickupType, bool> ActivatedPickups { get; set; }
        }


    }
}
