using Locations;
using UnityEngine;

namespace PlayerInteractions.LocationActions
{
    [System.Serializable]
    public class LocationAction : BaseAction
    {
        public LocationData targetLocation;

        public override void Execute()
        {
            base.Execute();
            Debug.Log($"This is base location action. Target location: {targetLocation.locationName}");
        }
    }
}