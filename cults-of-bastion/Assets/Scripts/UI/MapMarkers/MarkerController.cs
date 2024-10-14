using System;
using System.Collections.Generic;
using CameraControllers;
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

        [SerializeField] private float scaleChangeSpeed = 5f;
        [SerializeField] private float markerScaleLowZoom;
        [SerializeField] private float markerScaleMiddleZoom;
        [SerializeField] private float markerScaleHighZoom;
        
        [SerializeField] private float alphaChangeSpeed = 0.05f;
        [SerializeField] private float markerAlphaLowZoom;
        [SerializeField] private float markerAlphaMiddleZoom;
        [SerializeField] private float markerAlphaHighZoom;
        
        [SerializeField] private float lowZoomAmount;
        [SerializeField] private float middleZoomAmount;
        [SerializeField] private float highZoomAmount;
        
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
            CityViewCameraController.OnCameraZoomUpdate += UpdateMarkerScale;
            CityViewCameraController.OnCameraZoomUpdate += UpdateMarkerAlpha;
        }

        private void UnsubscribeFromEvents()
        {
            MarkerManager.OnRequestMarkerDisplay -= CreateLocationMarker;
            MarkerManager.OnRequestMarkerToBeHidden -= RemoveLocationMarker;
            CityViewCameraController.OnCameraZoomUpdate -= UpdateMarkerScale;
            CityViewCameraController.OnCameraZoomUpdate -= UpdateMarkerAlpha;
        }

        #region LocationMarkers

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


        private void RemoveLocationMarker(int locationIndex)
        {
            var marker = _usedLocationMarkers.Find(m => m.LocationMarkerData.LocationDataEntry.LocationIndex == locationIndex);
            if(marker == null) return;
            marker.RemoveMarker();
            ReleaseMarker(marker, _availableLocationMarkers, _usedLocationMarkers, preCreatedLocationMarkers);
        }

        #endregion

        #region MarkerHandling

        private object CheckForMarkerDataExistence(int locationIndex)
        {
            var marker = _usedLocationMarkers.Find(m => m.LocationMarkerData.LocationDataEntry.LocationIndex == locationIndex);
            return marker;
        }

        private void UpdateMarkerScale(float zoomLevel)
        {
            float targetScale;

            if (zoomLevel < lowZoomAmount)
            {
                targetScale = Mathf.Lerp(markerScaleLowZoom, markerScaleMiddleZoom, zoomLevel / lowZoomAmount);
            }
            else if (zoomLevel < middleZoomAmount)
            {
                targetScale = Mathf.Lerp(markerScaleMiddleZoom, markerScaleHighZoom, (zoomLevel - lowZoomAmount) / (middleZoomAmount - lowZoomAmount));
            }
            else
            {
                targetScale = Mathf.Lerp(markerScaleHighZoom, markerScaleHighZoom, (zoomLevel - middleZoomAmount) / (highZoomAmount - middleZoomAmount));
            }
            
            _usedLocationMarkers.ForEach(m =>
            {
                m.transform.localScale = Vector3.Lerp(m.transform.localScale, Vector3.one * targetScale, Time.deltaTime * scaleChangeSpeed);
            });

            _availableLocationMarkers.ForEach(m =>
            {
                m.transform.localScale = Vector3.Lerp(m.transform.localScale, Vector3.one * targetScale, Time.deltaTime * scaleChangeSpeed);
            });
        }

        private void UpdateMarkerAlpha(float zoomLevel)
        {
            float targetAlpha;
            if (zoomLevel < lowZoomAmount)
            {
                targetAlpha = Mathf.Lerp(markerAlphaLowZoom, markerAlphaMiddleZoom, zoomLevel / lowZoomAmount);
            }
            else if (zoomLevel < middleZoomAmount)
            {
                targetAlpha = Mathf.Lerp(markerAlphaMiddleZoom, markerAlphaHighZoom, (zoomLevel - lowZoomAmount) / (middleZoomAmount - lowZoomAmount));
            }
            else
            {
                targetAlpha = Mathf.Lerp(markerAlphaHighZoom, markerAlphaHighZoom, (zoomLevel - middleZoomAmount) / (highZoomAmount - middleZoomAmount));
            }
            _usedLocationMarkers.ForEach(m =>
            {
                m.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(m.GetComponent<CanvasGroup>().alpha, targetAlpha, Time.deltaTime * alphaChangeSpeed);
            });
            _availableLocationMarkers.ForEach(m =>
            {
                m.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(m.GetComponent<CanvasGroup>().alpha, targetAlpha, Time.deltaTime * alphaChangeSpeed);
            });
        }

        #endregion


        #region GenericMarkerCreation

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

        #endregion
    }
}
