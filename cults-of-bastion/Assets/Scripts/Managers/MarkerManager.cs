using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using Locations;
using PlayerInteractions;
using UI.MapMarkers;
using UnityEngine;

namespace Managers
{
    public class MarkerManager : MonoBehaviour
    {
        #region Variables
        
        private readonly Dictionary<int, LocationMarkerData> _locationMarkers = new();
        private readonly Dictionary<int, bool> _locationVisibilityFlags = new();

        #endregion
        
        public static event Action<LocationMarkerData, Vector3> OnRequestMarkerDisplay;
        public static event Action<int> OnRequestMarkerToBeHidden;
        public static event Action<int> OnRequestLocationPosition;
        public static event Action<int> OnRemoveActionMarker;

        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            PlayerActionsController.OnActionCreated += UpdateMarkers;
            PlayerActionsController.OnActionCancelled += RemoveActionMarkerData;
            LocationVisibilityDetector.OnLocationVisible += InvokeMarkerDisplay;
            LocationVisibilityDetector.OnLocationHidden += RequestMarkerToBeHidden;
        }

        private void UnsubscribeFromEvents()
        {
            PlayerActionsController.OnActionCreated -= UpdateMarkers;
            PlayerActionsController.OnActionCancelled -= RemoveActionMarkerData;
            LocationVisibilityDetector.OnLocationVisible -= InvokeMarkerDisplay;
            LocationVisibilityDetector.OnLocationHidden -= RequestMarkerToBeHidden;
        }
        
        private void RequestMarkerToBeHidden(int locationIndex)
        {
            var markerExists = CheckForMarkerDataExistence(locationIndex);
            if (markerExists.Item1)
            {
                OnRequestMarkerToBeHidden?.Invoke(locationIndex);
                _locationVisibilityFlags[locationIndex] = false;
            }
        }

        private void InvokeMarkerDisplay(int locationIndex, Vector3 locationPositionInWorld)
        {
            if (_locationVisibilityFlags.TryGetValue(locationIndex, out bool isVisible) && isVisible)
            {
                return;
            }

            var markerExists = CheckForMarkerDataExistence(locationIndex);
            if (!markerExists.Item1) return;
            
            OnRequestMarkerDisplay?.Invoke(markerExists.Item2, locationPositionInWorld);
            Debug.Log($"Marker showing for: {_locationMarkers[locationIndex].LocationDataEntry.LocationName} while {_locationMarkers[locationIndex].CharacterMarker.Count} characters are interacting with it");

            _locationVisibilityFlags[locationIndex] = true;
        }
        private static IEnumerator RequestLocationPosition(int locationIndex, Action<Vector3> callback)
        {
            var positionReceived = false;
            Action<Vector3> onPositionReceived = position =>
            {
                callback?.Invoke(position);
                positionReceived = true;
            };

            OnRequestLocationPosition?.Invoke(locationIndex);
            LocationManager.OnReturnLocationPosition += onPositionReceived;

            yield return new WaitUntil(() => positionReceived);
            LocationManager.OnReturnLocationPosition -= onPositionReceived;
        }

        private (bool, LocationMarkerData) CheckForMarkerDataExistence(int locationIndex)
        {
            if (_locationMarkers.TryGetValue(locationIndex, out var markerData))
            {
                return (true, markerData);
            }
            return (false, null);
        }

        private void UpdateMarkers(BaseAction actionMarkerEntry)
        {
            if (actionMarkerEntry == null)
            {
                Debug.LogError("UpdateMarkers: actionMarkerEntry is null");
                return;
            }
            
            if (actionMarkerEntry.targetLocation == null)
            {
                Debug.LogError($"UpdateMarkers: targetLocation is null for action {actionMarkerEntry}");
                return;
            }

            var (exists, markerData) = CheckForMarkerDataExistence(actionMarkerEntry.targetLocation.locationID);

            if (!exists)
            {
                markerData = CreateLocationMarkerData(actionMarkerEntry);
                Debug.Log($"New marker added: {actionMarkerEntry.targetLocation.locationName}");
            }
            else
            {
                if (markerData == null)
                {
                    Debug.LogError($"UpdateMarkers: markerData is null for locationID {actionMarkerEntry.targetLocation.locationID}");
                    return;
                }

                markerData.ActionMarker.Add(actionMarkerEntry);
                markerData.CharacterMarker.Add(actionMarkerEntry.actionInvoker);
                Debug.Log($"Marker updated for: {actionMarkerEntry.targetLocation.locationName}");
            }
            
            if (markerData != null)
            {
                StartCoroutine(RequestLocationPosition(markerData.LocationDataEntry.LocationIndex,
                    position => InvokeMarkerDisplay(markerData.LocationDataEntry.LocationIndex, position)));
                Debug.Log($"Showing marker for: {actionMarkerEntry.targetLocation.locationName}");
            }
            else
            {
                Debug.LogError("UpdateMarkers: markerData is unexpectedly null after creation or update.");
            }
        }


        private LocationMarkerData CreateLocationMarkerData(BaseAction action)
        {
            var newLocationMarkerData = new LocationMarkerData
            {
                LocationDataEntry = CreateLocationDataEntry(action.targetLocation)
            };

            newLocationMarkerData.ActionMarker.Add(action);
            newLocationMarkerData.CharacterMarker.Add(action.actionInvoker);

            _locationMarkers.Add(action.targetLocation.locationID, newLocationMarkerData);
            return newLocationMarkerData;
        }


        private void RemoveActionMarkerData(BaseAction actionToRemove)
        {
            if (!_locationMarkers.TryGetValue(actionToRemove.targetLocation.locationID, out var markerData)) return;
            
            for (int i = markerData.ActionMarker.Count - 1; i >= 0; i--)
            {
                if (!ReferenceEquals(markerData.ActionMarker[i], actionToRemove)) continue;
                    
                markerData.CharacterMarker.Remove(markerData.ActionMarker[i].actionInvoker);
                Debug.Log($"Action marker removed for: {markerData.ActionMarker[i].targetLocation.locationName}");
                markerData.ActionMarker.RemoveAt(i);
                OnRemoveActionMarker?.Invoke(i);
            }

            if (markerData.ActionMarker.Count != 0) return;
                
            _locationMarkers.Remove(actionToRemove.targetLocation.locationID);
            _locationVisibilityFlags[actionToRemove.targetLocation.locationID] = false;
            OnRequestMarkerToBeHidden?.Invoke(actionToRemove.targetLocation.locationID);
            Debug.Log($"Location marker removed: {markerData.LocationDataEntry.LocationName}");
        }

        #region EntriesCreation

        private static LocationDataEntry CreateLocationDataEntry(LocationData location)
        {
            var locationMarkerEntry = new LocationDataEntry
            {
                LocationIndex = location.locationID,
                LocationName = location.locationName
            };
            return locationMarkerEntry;
        }

        #endregion
    }
    public class LocationDataEntry
    {
        public int LocationIndex;
        public string LocationName;
    }
}
