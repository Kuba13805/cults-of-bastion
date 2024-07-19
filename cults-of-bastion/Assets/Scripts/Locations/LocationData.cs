using System;

namespace Locations
{
    [System.Serializable]
    public class LocationData
    {
        public int locationID;
        public string locationName;
        public string locationTypeName;
        
        [NonSerialized] public LocationType LocationType;
    }
}
