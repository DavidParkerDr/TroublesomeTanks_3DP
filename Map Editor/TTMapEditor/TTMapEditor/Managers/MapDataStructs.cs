using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TTMapEditor.Objects;

namespace TTMapEditor.Managers
{
    /// <summary>
    /// Serializable container for all map content (walls, tanks, and pickups).
    /// This is the format persisted to and loaded from map JSON files.
    /// </summary>
    internal class MapData
    {
        public List<WallData> Walls { get; set; }

        public List<TankData> Tanks { get; set; }

        public List<PickupData> Pickups { get; set; }
    }

    /// <summary>
    /// Describes a single wall segment in the map, including its texture,
    /// position, size, and rotation.
    /// </summary>
    internal class WallData
    {

        public string Texture { get; set; }

        public string[] Position { get; set; }

        public string[] Size { get; set; }

        public string Rotation { get; set; }
    }

    /// <summary>
    /// Represents a tank spawn point in the map, including its position
    /// and facing rotation.
    /// </summary>
    internal class TankData
    {
        public string[] Position { get; set; }

        public string Rotation { get; set; }
    }

    /// <summary>
    /// Represents a pickup instance in the map, including where it is placed
    /// and which pickup types are active at this location.
    /// </summary>
    internal class PickupData
    {
        public string[] Position { get; set; }

        public Dictionary<PickupType, bool> ActivatedPickups { get; set; }
    }
}
