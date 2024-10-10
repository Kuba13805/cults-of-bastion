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

        private List<LocationMarkerData> _locationMarkers = new();


        #endregion

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
        }

        private void UnsubscribeFromEvents()
        {
            PlayerActionsController.OnActionCreated -= UpdateMarkers;
            PlayerActionsController.OnActionCancelled += RemoveActionMarkerData;
        }


        private (bool, LocationMarkerData) CheckForMarkerDataExistence(int locationIndex)
        {
            var markerDataExistence = _locationMarkers.Any(markerData => markerData.LocationDataEntry.LocationIndex == locationIndex);
            if (!markerDataExistence)
            {
                return (false, null);
            }
            var markerData = _locationMarkers.Find(markerData => markerData.LocationDataEntry.LocationIndex == locationIndex);
            return (true, markerData);
        }

        private void UpdateMarkers(BaseAction actionMarkerEntry)
        {
            var markerDataExistence = CheckForMarkerDataExistence(actionMarkerEntry.targetLocation.locationID);
            if (!markerDataExistence.Item1)
            {
                CreateLocationMarker(actionMarkerEntry);
                Debug.Log($"New marker added: {actionMarkerEntry.targetLocation.locationName}");
            }
            else
            {
                markerDataExistence.Item2.ActionMarker.Add(actionMarkerEntry);
                markerDataExistence.Item2.CharacterMarker.Add(actionMarkerEntry.actionInvoker);
                Debug.Log($"Marker updated for: {actionMarkerEntry.targetLocation.locationName}");
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
            _locationMarkers.Add(newLocationMarkerData);
        }

        private void RemoveActionMarkerData(BaseAction actionToRemove)
        {
            foreach (var markerData in _locationMarkers)
            {
                for (int i = markerData.ActionMarker.Count - 1; i >= 0; i--)
                {
                    if (ReferenceEquals(markerData.ActionMarker[i], actionToRemove))
                    {
                        markerData.CharacterMarker.Remove(markerData.ActionMarker[i].actionInvoker);
                        Debug.Log($"Marker removed for: {markerData.ActionMarker[i].targetLocation.locationName}");
                        markerData.ActionMarker.RemoveAt(i);
                    }
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
