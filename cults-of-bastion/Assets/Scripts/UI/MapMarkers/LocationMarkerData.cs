using System.Collections.Generic;
using Characters;
using Locations;
using Managers;
using PlayerInteractions;
using UnityEngine;

namespace UI.MapMarkers
{
    public class LocationMarkerData
    {
        public LocationDataEntry LocationDataEntry;
        public List<Character> CharacterMarker = new();
        public List<BaseAction> ActionMarker = new();
    }
}
