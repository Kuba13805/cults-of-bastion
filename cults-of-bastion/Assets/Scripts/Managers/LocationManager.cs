using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Locations;
using UnityEngine;

namespace Managers
{
    public class LocationManager : MonoBehaviour
    {
        public List<Location> locations;
        private GameData _gameData;

        private bool _isCharacterDataLoaded;
        
        public static event Action OnLocationManagerInitialized;
        public static event Action<LocationData> OnPassLocationData; 

        private void Awake()
        {
            SubscribeToEvents();
        }

        private void Start()
        {
            OnLocationManagerInitialized?.Invoke();
        }
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            Location.OnRegisterEmptyLocation += RegisterNewEmptyLocation;
            GameManager.OnGameDataLoaded += StartLocationLoading;
            CharacterManager.OnCharactersLoaded += AllowDataInjection;
            Location.OnInteractWithLocation += PassLocationData;
        }

        private void UnsubscribeFromEvents()
        {
            Location.OnRegisterEmptyLocation -= RegisterNewEmptyLocation;
            GameManager.OnGameDataLoaded -= StartLocationLoading;
            CharacterManager.OnCharactersLoaded -= AllowDataInjection;
            Location.OnInteractWithLocation -= PassLocationData;
        }

        #region LocationDataInjection

        private void RegisterNewEmptyLocation(Location location)
        {
            locations.Add(location);
        }

        private void AllowDataInjection()
        {
            Debug.Log("Allowing data injection.");
            _isCharacterDataLoaded = true;
        }

        private void StartLocationLoading(GameData gameData)
        {
            _gameData = gameData;
            Debug.Log("Start location loading.");
            StartCoroutine(LoadData());
        }

        private IEnumerator LoadData()
        {
            Debug.Log("Loading locations...");
            LoadLocationTypes();
        
            AssignLocationTypeToLocationData();

            Debug.Log($"Waiting for characters to load...");
            yield return new WaitUntil(() => _isCharacterDataLoaded);

            Debug.Log($"Characters loaded. Start injecting");
            InjectLocationDataToWorldLocations();
            
            Debug.Log("Locations loaded.");
            
            yield return null;
        }
    
        private void InjectLocationDataToWorldLocations()
        {
            foreach (var location in locations)
            {
                location.locationData =
                    _gameData.Locations.Find(locationData => locationData.locationID == location.locationIndex);
                Debug.Log($"Location {location.locationData.locationName} injected with {location.locationData.LocationType.typeName} type");
            }
        }

        private void AssignLocationTypeToLocationData()
        {
            foreach (var locationData in _gameData.Locations)
            {
                if (string.IsNullOrEmpty(locationData.locationTypeName))
                {
                    Debug.LogWarning("LocationData locationTypeName is null or empty.");
                    continue;
                }

                if (_gameData.LocationTypeDict.TryGetValue(locationData.locationTypeName, out var locationType))
                {
                    locationData.LocationType = locationType;
                    Debug.Log("Match found: " + locationType.typeName);
                }
                else
                {
                    Debug.LogWarning("No matching LocationType found for " + locationData.locationTypeName);
                }
            }
        }

        private void LoadLocationTypes()
        {
            foreach (var locationType in _gameData.LocationTypes)
            {
                if (string.IsNullOrEmpty(locationType.typeName))
                {
                    Debug.LogWarning("LocationType typeName is null or empty.");
                    continue;
                }

                _gameData.LocationTypeDict.Add(locationType.typeName, locationType);
            }
        }

        #endregion

        #region LocationActionsHandling

        private static void PassLocationData(LocationData locationData) => OnPassLocationData?.Invoke(locationData);

        #endregion
    }
}
