using System;
using Characters;
using Locations;
using TMPro;
using UnityEngine;

namespace UI.PlayerInspector
{
    public class LocationInspector : MonoBehaviour
    {
        private LocationData _locationData;
        [SerializeField] private TextMeshProUGUI locationName;
        [SerializeField] private TextMeshProUGUI locationType;

        public static event Action<Character> OnInvokeLocationOwnerInspector;

        public void InitializeInspector(LocationData locationData)
        {
            _locationData = locationData;
            
            locationName.text = _locationData.locationName;
            locationType.text = _locationData.LocationType.typeName;
        }

        public void InvokeLocationOwnerInspector() =>
            OnInvokeLocationOwnerInspector?.Invoke(_locationData.LocationOwner);
    }
}
