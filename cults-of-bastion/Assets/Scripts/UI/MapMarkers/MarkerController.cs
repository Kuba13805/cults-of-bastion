using System.Collections.Generic;
using CameraControllers;
using Managers;
using PlayerInteractions;
using UnityEngine;

namespace UI.MapMarkers
{
    public class MarkerController : MonoBehaviour
    {
        [SerializeField] private int preCreatedLocationMarkers;
        [SerializeField] private int preCreatedActionMarkers;
        [SerializeField] private int preCreatedCharacterMarkers;
        
        [SerializeField] private GameObject characterInLocationMarkerPrefab;
        [SerializeField] private GameObject actionInLocationMarkerPrefab;
        [SerializeField] private GameObject locationMarkerPrefab;
        
        [SerializeField] private Transform locationMarkerParent;
        [SerializeField] private Transform characterInLocationMarkerParent;
        [SerializeField] private Transform actionInLocationMarkerParent;

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
        
        private MarkerPool<LocationMarker> _locationMarkerPool;
        private MarkerPool<ActionInLocationMarker> _actionMarkerPool;
        private MarkerPool<CharacterInLocationMarker> _characterMarkerPool;

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
            _locationMarkerPool = new MarkerPool<LocationMarker>(locationMarkerPrefab, locationMarkerParent, preCreatedLocationMarkers);
            _actionMarkerPool = new MarkerPool<ActionInLocationMarker>(actionInLocationMarkerPrefab, actionInLocationMarkerParent, preCreatedActionMarkers);
            _characterMarkerPool = new MarkerPool<CharacterInLocationMarker>(characterInLocationMarkerPrefab, characterInLocationMarkerParent, preCreatedCharacterMarkers);
        }

        private void CreateLocationMarker(LocationMarkerData locationMarkerData, Vector3 markerPosition)
        {
            Debug.Log($"Marker for location: {locationMarkerData.LocationDataEntry.LocationName} created");
            var marker = _locationMarkerPool.GetMarker();
            marker.InitializeMarker(locationMarkerData, markerPosition);

            foreach (var action in marker.LocationMarkerData.ActionMarker)
            {
                UpdateLocationActions(action, marker);
            }
        }

        private void RemoveLocationMarker(int locationIndex)
        {
            var marker = _locationMarkerPool.UsedMarkers.Find(m => m.LocationMarkerData.LocationDataEntry.LocationIndex == locationIndex);
            if(marker == null) return;

            foreach (var actionMarker in marker.GetActionMarkers())
            {
                actionMarker.RemoveAction();
                _actionMarkerPool.ReleaseMarker(actionMarker);
            }

            foreach (var characterMarker in marker.GetCharacterMarkers())
            {
                _characterMarkerPool.ReleaseMarker(characterMarker);
            }
            marker.RemoveMarker();
            Debug.Log($"Location marker removed: {marker.LocationMarkerData.LocationDataEntry.LocationName}");
            _locationMarkerPool.ReleaseMarker(marker);
        }

        private void UpdateLocationActions(BaseAction newAction, LocationMarker marker)
        {
            var actionMarkerParent = marker.GetActionParent();
            var characterMarkerParent = marker.GetCharacterParent();

            if (actionMarkerParent == null || characterMarkerParent == null)
            {
                Debug.LogWarning("Marker parents are null");
                return;
            }

            var actionMarker = _actionMarkerPool.GetMarker();
            var characterMarker = _characterMarkerPool.GetMarker();

            if (actionMarker == null || characterMarker == null)
            {
                Debug.LogError("Failed to get actionMarker or characterMarker from the pool");
                return;
            }

            actionMarker.SetAction(newAction);
            characterMarker.SetCharacter(newAction.ActionInvoker);
            TransferMinorMarker(actionMarker, actionMarkerParent);
            TransferMinorMarker(characterMarker, characterMarkerParent);

            Debug.Log("Action and Character markers assigned to respective parents");
        }

        private static void TransferMinorMarker<T>(T marker, Transform targetParent) where T : MonoBehaviour
        {
            if (marker == null || targetParent == null)
            {
                Debug.LogError("Cannot transfer marker because it or targetParent is null");
                return;
            }

            marker.transform.SetParent(targetParent, false);
            Debug.Log($"Marker transferred to new parent: {targetParent.name}");
        }
        #endregion

        #region MarkerHandling

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

            foreach (var marker in _locationMarkerPool.UsedMarkers)
            {
                float currentScale = marker.transform.localScale.x;
                if (Mathf.Abs(currentScale - targetScale) > 0.01f)
                {
                    marker.transform.localScale = Vector3.Lerp(marker.transform.localScale, Vector3.one * targetScale, Time.deltaTime * scaleChangeSpeed);
                }
            }
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

            foreach (var marker in _locationMarkerPool.UsedMarkers)
            {
                var canvasGroup = marker.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    float currentAlpha = canvasGroup.alpha;
                    if (Mathf.Abs(currentAlpha - targetAlpha) > 0.01f)
                    {
                        canvasGroup.alpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * alphaChangeSpeed);
                    }
                }
            }
        }

        #endregion
    }
}
