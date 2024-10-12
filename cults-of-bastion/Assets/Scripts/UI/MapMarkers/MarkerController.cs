using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;

namespace UI.MapMarkers
{
    public class MarkerController : MonoBehaviour
    {
        [SerializeField] private int precreatedMarkers;
        [SerializeField] private GameObject locationMarkerPrefab;
        [SerializeField] private Transform locationMarkerParent;
        
        private List<LocationMarker> _availableLocationMarkers = new();
        private List<LocationMarker> _usedLocationMarkers = new();

        private void Start()
        {
            SubscribeToEvents();
            InitializeLocationMarkers();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            MarkerManager.OnLocationMarkerCreated += CreateLocationMarker;
        }

        private void UnsubscribeFromEvents()
        {
            MarkerManager.OnLocationMarkerCreated -= CreateLocationMarker;
        }
        
        private void CreateLocationMarker(LocationMarkerData locationMarkerData)
        {
            var newMarker = GetMarker(_availableLocationMarkers, _usedLocationMarkers, locationMarkerPrefab,
                locationMarkerParent);
        }
        
        private void InitializeLocationMarkers()
        {
            for (int i = 0; i < precreatedMarkers; i++)
            {
                var newMarker = CreateMarker<LocationMarker>(locationMarkerPrefab, locationMarkerParent);
                _availableLocationMarkers.Add(newMarker);
            }
        }
        
        private static T GetMarker<T>(List<T> availableMarkerList, List<T> usedMarkerList, GameObject markerPrefab, Transform prefabParent) where T : MonoBehaviour
        {
            T marker;
            if (availableMarkerList.Count > 0)
            {
                marker = availableMarkerList[0];
                availableMarkerList.RemoveAt(0);
            }
            else
            {
                marker = CreateMarker<T>(markerPrefab, prefabParent);
            }
            usedMarkerList.Add(marker);
            return marker;
        }

        private void ReleaseMarker<T>(T marker, List<T> usedMarkerList, List<T> availableMarkerList, int poolSize) where T : MonoBehaviour
        {
            usedMarkerList.Remove(marker);
            if (availableMarkerList.Count >= poolSize)
            {
                Destroy(marker.gameObject);
            }
            else
            {
                availableMarkerList.Add(marker);
                marker.gameObject.SetActive(false);
            }
        }
        
        private static T CreateMarker<T>(GameObject prefab, Transform prefabParent) where T : MonoBehaviour
        {
            var markerInstance = Instantiate(prefab, prefabParent).GetComponent<T>();
            markerInstance.gameObject.SetActive(false);
            return markerInstance;
        }
    }
}
