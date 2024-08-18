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
        [SerializeField] private TextMeshProUGUI locationNameBox;
        [SerializeField] private TextMeshProUGUI locationTypeBox;

        public static event Action<Character> OnInvokeLocationOwnerInspector;

        public void InitializeInspector(LocationData locationData)
        {
            _locationData = locationData;
            
            locationNameBox.text = _locationData.locationName;
            locationTypeBox.text = _locationData.LocationType.typeName;
        }

        public void InvokeLocationOwnerInspector() =>
            OnInvokeLocationOwnerInspector?.Invoke(_locationData.LocationOwner);
    }
}
