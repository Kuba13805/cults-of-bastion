using System;
using System.Collections.Generic;
using System.Linq;
using CameraControllers;
using Characters;
using Locations;
using PlayerInteractions;
using UI.MapMarkers;
using UnityEngine;

namespace Managers
{
    public class MarkerController : MonoBehaviour
    {
        #region Variables
        
        private Dictionary<int, LocationMarkerData> _locationMarkers = new();

        #endregion
        
        public static event Action<int> OnRequestMarkerDisplay;
        public static event Action<int> OnRequestMarkerToBeHidden;
        public static event Action<LocationMarkerData> OnLocationMarkerCreated;
        public static event Action<int> OnLocationMarkerRemoved;
        public static event Action<int> OnLocationMarkerUpdated;

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
            LocationVisibilityDetector.OnLocationVisible += RequestMarkerDisplay;
            LocationVisibilityDetector.OnLocationHidden += RequestMarkerToBeHidden;
        }

        private void UnsubscribeFromEvents()
        {
            PlayerActionsController.OnActionCreated -= UpdateMarkers;
            PlayerActionsController.OnActionCancelled -= RemoveActionMarkerData;
            LocationVisibilityDetector.OnLocationVisible -= RequestMarkerDisplay;
            LocationVisibilityDetector.OnLocationHidden -= RequestMarkerToBeHidden;
        }

        private void RequestMarkerDisplay(int locationIndex)
        {
            var markerExists = CheckForMarkerDataExistence(locationIndex);
            if (markerExists.Item1)
            {
                OnRequestMarkerDisplay?.Invoke(locationIndex);
                Debug.Log($"Marker showing for: {_locationMarkers[locationIndex].LocationDataEntry.LocationName} while {_locationMarkers[locationIndex].CharacterMarker.Count} characters are interacting with it");
            }
        }

        private void RequestMarkerToBeHidden(int locationIndex)
        {
            var markerExists = CheckForMarkerDataExistence(locationIndex);
            if (markerExists.Item1)
            {
                OnRequestMarkerToBeHidden?.Invoke(locationIndex);
            }
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
            var (exists, markerData) = CheckForMarkerDataExistence(actionMarkerEntry.targetLocation.locationID);
            
            if (!exists)
            {
                CreateLocationMarker(actionMarkerEntry);
                Debug.Log($"New marker added: {actionMarkerEntry.targetLocation.locationName}");
            }
            else
            {
                markerData.ActionMarker.Add(actionMarkerEntry);
                markerData.CharacterMarker.Add(actionMarkerEntry.actionInvoker);
                Debug.Log($"Marker updated for: {actionMarkerEntry.targetLocation.locationName}");
                OnLocationMarkerUpdated?.Invoke(actionMarkerEntry.targetLocation.locationID);
            }
        }

        private void CreateLocationMarker(BaseAction action)
        {
            var newLocationMarkerData = new LocationMarkerData
            {
                LocationDataEntry = CreateLocationDataEntry(action.targetLocation)
            };
            newLocationMarkerData.ActionMarker.Add(action);
            newLocationMarkerData.CharacterMarker.Add(action.actionInvoker);
            _locationMarkers.Add(action.targetLocation.locationID, newLocationMarkerData);
            OnLocationMarkerCreated?.Invoke(newLocationMarkerData);
        }

        private void RemoveActionMarkerData(BaseAction actionToRemove)
        {
            if (_locationMarkers.TryGetValue(actionToRemove.targetLocation.locationID, out var markerData))
            {
                for (int i = markerData.ActionMarker.Count - 1; i >= 0; i--)
                {
                    if (ReferenceEquals(markerData.ActionMarker[i], actionToRemove))
                    {
                        markerData.CharacterMarker.Remove(markerData.ActionMarker[i].actionInvoker);
                        Debug.Log($"Action marker removed for: {markerData.ActionMarker[i].targetLocation.locationName}");
                        markerData.ActionMarker.RemoveAt(i);
                    }
                }
                
                if (markerData.ActionMarker.Count == 0)
                {
                    _locationMarkers.Remove(actionToRemove.targetLocation.locationID);
                    OnLocationMarkerRemoved?.Invoke(actionToRemove.targetLocation.locationID);
                    Debug.Log($"Location marker removed: {markerData.LocationDataEntry.LocationName}");
                }
            }
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
