using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text;
using System.Threading.Tasks;

namespace Tankontroller.World.Pickups
{
    public class PickupSpawnPoint
    {
        private Vector2 mPosition;
        
        private Dictionary<PickupType, bool> mActivatedPickups;

        public PickupSpawnPoint(Vector2 pPosition, Dictionary<PickupType, bool> pActivatedPickups)
        {
            mPosition = pPosition;
            mActivatedPickups = pActivatedPickups;
        }

        public bool IsPickupTypeActivated(PickupType pType)
        {
            return mActivatedPickups.ContainsKey(pType) && mActivatedPickups[pType];
        }

        public Vector2 GetPosition()
        {
            return mPosition;
        }
    }
}
