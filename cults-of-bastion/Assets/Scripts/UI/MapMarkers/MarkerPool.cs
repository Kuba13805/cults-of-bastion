using System.Collections.Generic;
using UnityEngine;

namespace UI.MapMarkers
{
    public class MarkerPool<T> where T : MonoBehaviour
    {
        public readonly List<T> AvailableMarkers = new();
        public readonly List<T> UsedMarkers = new();
        private readonly GameObject _markerPrefab;
        private readonly Transform _markerParent;
        private readonly int _poolSize;

        public MarkerPool(GameObject markerPrefab, Transform markerParent, int poolSize)
        {
            _markerPrefab = markerPrefab;
            _markerParent = markerParent;
            _poolSize = poolSize;

            for (int i = 0; i < poolSize; i++)
            {
                var marker = CreateMarker();
                AvailableMarkers.Add(marker);
            }
        }

        public T GetMarker()
        {
            T marker;
            if (AvailableMarkers.Count > 0)
            {
                marker = AvailableMarkers[0];
                AvailableMarkers.RemoveAt(0);
                Debug.Log($"Marker retrieved from available pool. {AvailableMarkers.Count} markers left in the pool.");
            }
            else
            {
                marker = CreateMarker();
                Debug.Log("No available markers in pool. Created a new marker.");
            }

            UsedMarkers.Add(marker);
            marker.gameObject.SetActive(true);
            Debug.Log($"Marker activated and added to UsedMarkers. Total used markers: {UsedMarkers.Count}");
            return marker;
        }

        public void ReleaseMarker(T marker)
        {
            UsedMarkers.Remove(marker);
            Debug.Log($"Marker released. Total used markers: {UsedMarkers.Count}");

            if (AvailableMarkers.Count >= _poolSize)
            {
                Object.Destroy(marker.gameObject);
                Debug.Log("Marker pool is full, marker destroyed.");
            }
            else
            {
                AvailableMarkers.Add(marker);
                marker.gameObject.SetActive(false);
                Debug.Log($"Marker deactivated and returned to the pool. Pool size: {AvailableMarkers.Count}");
            }
        }

        private T CreateMarker()
        {
            var markerInstance = Object.Instantiate(_markerPrefab, _markerParent).GetComponent<T>();
            markerInstance.gameObject.SetActive(false);
            Debug.Log("New marker created and added to the pool.");
            return markerInstance;
        }
    }
}
