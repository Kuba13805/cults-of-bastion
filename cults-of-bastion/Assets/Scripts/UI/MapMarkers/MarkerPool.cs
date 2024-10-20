using System.Collections.Generic;
using UnityEngine;

namespace UI.MapMarkers
{
    public class MarkerPool<T> where T : MonoBehaviour
    {
        private readonly List<T> _availableMarkers = new();
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
                _availableMarkers.Add(marker);
            }
        }

        public T GetMarker()
        {
            T marker;
            if (_availableMarkers.Count > 0)
            {
                marker = _availableMarkers[0];
                _availableMarkers.RemoveAt(0);
            }
            else
            {
                marker = CreateMarker();
            }

            UsedMarkers.Add(marker);
            marker.gameObject.SetActive(true);
            return marker;
        }

        public void ReleaseMarker(T marker)
        {
            UsedMarkers.Remove(marker);

            if (_availableMarkers.Count >= _poolSize)
            {
                Object.Destroy(marker.gameObject);
            }
            else
            {
                _availableMarkers.Add(marker);
                marker.gameObject.SetActive(false);
            }
        }

        private T CreateMarker()
        {
            var markerInstance = Object.Instantiate(_markerPrefab, _markerParent).GetComponent<T>();
            markerInstance.gameObject.SetActive(false);
            return markerInstance;
        }
    }
}
