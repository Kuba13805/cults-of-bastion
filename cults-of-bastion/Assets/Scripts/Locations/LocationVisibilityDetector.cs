using System;
using System.Collections;
using UnityEngine;

namespace Locations
{
    public class LocationVisibilityDetector : MonoBehaviour
    {
        private int _locationIndex;

        public static event Action<LocationVisibilityDetector> OnRegisterLocation;
        public static event Action<LocationVisibilityDetector> OnUnregisterLocation;

        public static event Action<int, Vector3> OnLocationVisible;
        public static event Action<int> OnLocationHidden;

        private void Start()
        {
            _locationIndex = GetComponent<Location>().locationIndex;
            OnRegisterLocation?.Invoke(this);
        }

        private void OnDestroy()
        {
            OnUnregisterLocation?.Invoke(this);
        }
        public void InvokeLocationVisible()
        {
            OnLocationVisible?.Invoke(_locationIndex, transform.position);
            Debug.Log($"Location with id {_locationIndex} visible at {transform.position}");
        }

        public void InvokeLocationHidden()
        {
            OnLocationHidden?.Invoke(_locationIndex);
        }
    }
}
