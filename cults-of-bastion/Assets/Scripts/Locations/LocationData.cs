using System;
using Characters;

namespace Locations
{
    [Serializable]
    public class LocationData
    {
        public int locationID;
        public string locationName;
        public string locationTypeName;
        
        [NonSerialized] public LocationType LocationType;
        [NonSerialized] public Character LocationOwner;
    }
}
