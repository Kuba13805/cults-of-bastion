using System;
using Locations;
using UnityEngine;

namespace UI.PlayerInspector
{
    public class CharacterOwnedLocationButton : MonoBehaviour
    {
        private LocationData _locationData;

        public static event Action<LocationData> OnInvokeLocationInspector; 

        public void InitializeInspector(LocationData locationData)
        {
            _locationData = locationData;
        }

        public void InvokeLocationInspector() => OnInvokeLocationInspector?.Invoke(_locationData);
    }
}