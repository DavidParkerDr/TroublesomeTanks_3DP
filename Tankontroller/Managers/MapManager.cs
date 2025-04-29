using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.World;

namespace Tankontroller
{
    public static class MapManager
    {
        public static TheWorld LoadMap(string filePath)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            string fullPath = Path.Combine(projectDirectory, filePath);

            string[] lines = File.ReadAllLines(fullPath);
            return ParseLines(lines);
        }

        private static TheWorld ParseLines(string[] lines)
        {
            int screenWidth = Tankontroller.Instance().GDM().GraphicsDevice.Viewport.Width;
            int screenHeight = Tankontroller.Instance().GDM().GraphicsDevice.Viewport.Height;
            Rectangle playArea = new Rectangle(screenWidth * 2 / 100, screenHeight * 25 / 100, screenWidth * 96 / 100, screenHeight * 73 / 100);
            List<RectWall> Walls = new List<RectWall>();
            List<Tank> Tanks = new List<Tank>();
            List<Vector2> PickupSpawnPositions = new List<Vector2>();
            float tankScale = (float)playArea.Width / (50 * 40);

            string texture = null;
            Vector2 position = Vector2.Zero;
            Vector2 size = Vector2.Zero;
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
                    rotation = MathHelper.ToRadians(rotation);
                    continue;
                }

                //check if the current object is a wall or a tank
                if (isWall)
                {
                    RectWall currentWall = new RectWall(
                        Tankontroller.Instance().CM().Load<Texture2D>(texture),
                        new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y));
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
                    PickupSpawnPositions.Add(position);
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
            List<Vector2> PickupSpawnPositions = new List<Vector2>();
            float tankScale = (float)playArea.Width / (50 * 40);

            foreach (var wall in mapData.Walls)
            {
                try
                {
                    var texture = wall.Texture;
                    var position = new Vector2(float.Parse(wall.Position[0]), float.Parse(wall.Position[1]));
                    var size = new Vector2(float.Parse(wall.Size[0]), float.Parse(wall.Size[1]));
                    position.X = playArea.X + (playArea.Width * (position.X / 100.0f));
                    position.Y = playArea.Y + (playArea.Height * (position.Y / 100.0f));
                    size.X = playArea.Width * (size.X / 100.0f);
                    size.Y = playArea.Height * (size.Y / 100.0f);

                    RectWall currentWall = new RectWall(
                        Tankontroller.Instance().CM().Load<Texture2D>(texture),
                        new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y));
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

                    PickupSpawnPositions.Add(position);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing pickup data: {ex.Message}");
                }
            }

            return new TheWorld(playArea, Walls, Tanks, PickupSpawnPositions);
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
        }

        public class TankData
        {
            public string[] Position { get; set; }
            public string Rotation { get; set; }
        }

        public class PickupData
        {
            public string[] Position { get; set; }
        }
    }
}
