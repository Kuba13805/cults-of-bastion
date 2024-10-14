using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.MapMarkers
{
    public class MarkerController : MonoBehaviour
    {
        [SerializeField] private int preCreatedLocationMarkers;
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
            MarkerManager.OnRequestMarkerDisplay += CreateLocationMarker;
            MarkerManager.OnRequestMarkerToBeHidden += RemoveLocationMarker;
        }

        private void UnsubscribeFromEvents()
        {
            MarkerManager.OnRequestMarkerDisplay -= CreateLocationMarker;
            MarkerManager.OnRequestMarkerToBeHidden -= RemoveLocationMarker;
        }
        private void InitializeLocationMarkers()
        {
            for (int i = 0; i < preCreatedLocationMarkers; i++)
            {
                Debug.Log($"Marker number {i} created");
                var newMarker = CreateMarker<LocationMarker>(locationMarkerPrefab, locationMarkerParent);
                _availableLocationMarkers.Add(newMarker);
            }
        }
        private void CreateLocationMarker(LocationMarkerData locationMarkerData, Vector3 markerPosition)
        {
            var markerExists = CheckForMarkerDataExistence(locationMarkerData.LocationDataEntry.LocationIndex) ??
                               GetMarker(_availableLocationMarkers, _usedLocationMarkers, locationMarkerPrefab,
                locationMarkerParent);

            var newMarker = markerExists as LocationMarker;
            if (newMarker != null) newMarker.InitializeMarker(locationMarkerData, markerPosition);
        }

        private object CheckForMarkerDataExistence(int locationIndex)
        {
            var marker = _usedLocationMarkers.Find(m => m.LocationMarkerData.LocationDataEntry.LocationIndex == locationIndex);
            return marker;
        }

        private void RemoveLocationMarker(int locationIndex)
        {
            var marker = _usedLocationMarkers.Find(m => m.LocationMarkerData.LocationDataEntry.LocationIndex == locationIndex);
            if(marker == null) return;
            ReleaseMarker(marker, _availableLocationMarkers, _usedLocationMarkers, preCreatedLocationMarkers);
        }
        
        
        private static T GetMarker<T>(IList<T> availableMarkerList, ICollection<T> usedMarkerList, GameObject markerPrefab, Transform prefabParent) where T : MonoBehaviour
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

        private static void ReleaseMarker<T>(T marker, ICollection<T> availableMarkerList, ICollection<T> usedMarkerList, int poolSize) where T : MonoBehaviour
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
