using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tankontroller.World.Shapes;

namespace Tankontroller.World.WorldObject
{
    /// <summary>
    /// Interface for objects that can participate in collision detection.
    /// </summary>
    public interface ICollidable
    {
        Transform Transform { get; }
        CollisionShape CollisionShape { get; }
    }
}
