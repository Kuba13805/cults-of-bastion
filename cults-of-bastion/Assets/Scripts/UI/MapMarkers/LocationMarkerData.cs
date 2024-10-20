using System.Collections.Generic;
using Characters;
using Managers;
using PlayerInteractions;

namespace UI.MapMarkers
{
    public class LocationMarkerData
    {
        public LocationDataEntry LocationDataEntry;
        public List<Character> CharacterMarker = new();
        public List<BaseAction> ActionMarker = new();
    }
}
