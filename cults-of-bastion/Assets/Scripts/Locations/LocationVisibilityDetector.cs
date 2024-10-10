using System;
using System.Collections;
using UnityEngine;

namespace Locations
{
    public class LocationVisibilityDetector : MonoBehaviour
    {
        private int _locationIndex;
        
        public static event Action<int> OnLocationVisible;
        public static event Action<int> OnLocationHidden;

        private void Start()
        {
            _locationIndex = GetComponent<Location>().locationIndex;
        }

        private void OnBecameVisible()
        {
            OnLocationVisible?.Invoke(_locationIndex);
        }
        private void OnBecameInvisible()
        {
            OnLocationHidden?.Invoke(_locationIndex);
        }
    }
}
