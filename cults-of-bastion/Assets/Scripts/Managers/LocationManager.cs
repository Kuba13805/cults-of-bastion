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

        private void OnEnable()
        {
            Location.OnRegisterEmptyLocation += RegisterNewEmptyLocation;
        }

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
            GameManager.OnGameDataLoaded += StartLocationLoading;
            CharacterManager.OnCharactersLoaded += AllowDataInjection;
        }

        private void UnsubscribeFromEvents()
        {
            Location.OnRegisterEmptyLocation -= RegisterNewEmptyLocation;
            GameManager.OnGameDataLoaded -= StartLocationLoading;
            CharacterManager.OnCharactersLoaded -= AllowDataInjection;
        }

        private void RegisterNewEmptyLocation(Location location)
        {
            locations.Add(location);
        }

        private void AllowDataInjection()
        {
            _isCharacterDataLoaded = true;
        }

        private void StartLocationLoading(GameData gameData)
        {
            _gameData = gameData;
            StartCoroutine(LoadData());
        }

        private IEnumerator LoadData()
        {
            LoadLocationTypes();
        
            AssignLocationTypeToLocationData();

            yield return new WaitUntil(() => _isCharacterDataLoaded);

            InjectLocationDataToWorldLocations();
            
            Debug.Log("Locations loaded.");
        }
    
        private void InjectLocationDataToWorldLocations()
        {
            foreach (var location in locations)
            {
                location.locationData =
                    _gameData.Locations.Find(locationData => locationData.locationID == location.locationIndex);
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
    }
}
