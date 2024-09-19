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
        private readonly Dictionary<string, LocationType> _locationTypeDict = new();
        private LocationTypeData _locationTypeData;

        private bool _isCharacterDataLoaded;
        
        public static event Action OnLocationManagerInitialized;
        public static event Action<LocationData> OnPassLocationDataOnInteraction;
        public static event Action<LocationData> OnPassLocationDataOnSelection; 

        private void Awake()
        {
            SubscribeToEvents();
        }

        private void Start()
        {
            LoadLocationTypes();
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
            Location.OnInteractWithLocation += PassLocationDataOnInteraction;
            Location.OnSelectLocation += PassLocationDataOnSelection;
        }

        private void UnsubscribeFromEvents()
        {
            Location.OnRegisterEmptyLocation -= RegisterNewEmptyLocation;
            GameManager.OnGameDataLoaded -= StartLocationLoading;
            CharacterManager.OnCharactersLoaded -= AllowDataInjection;
            Location.OnInteractWithLocation -= PassLocationDataOnInteraction;
            Location.OnSelectLocation -= PassLocationDataOnSelection;
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

                if (_locationTypeDict.TryGetValue(locationData.locationTypeName, out var locationType))
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
            var locationTypeConfig = Resources.Load<TextAsset>("DataToLoad/locationTypes");
            if (locationTypeConfig == null) return;

            var parsedLocationTypeConfigData = JsonUtility.FromJson<LocationTypeData>(locationTypeConfig.text);
            if (parsedLocationTypeConfigData == null)
            {
                throw new Exception("Failed to parse location types config data.");
            }

            _locationTypeData = new LocationTypeData
            {
                LocationTypeConstructors = parsedLocationTypeConfigData.LocationTypeConstructors
            };

            InitializeLocationTypes();
        }

        private void InitializeLocationTypes()
        {
            Debug.Log($"Constructors count: {_locationTypeData.LocationTypeConstructors.Count}");
            foreach (var locationType in _locationTypeData.LocationTypeConstructors.Select(constructor => new LocationType
                     {
                         typeName = constructor.typeName,
                         typeDescription = constructor.typeDescription
                     }))
            {
                _locationTypeDict.Add(locationType.typeName, locationType);
                Debug.Log($"New location type added: {locationType.typeName}");
            }
        }

        #endregion

        #region LocationActionsHandling

        private static void PassLocationDataOnInteraction(LocationData locationData) => OnPassLocationDataOnInteraction?.Invoke(locationData);
        private static void PassLocationDataOnSelection(LocationData locationData) => OnPassLocationDataOnSelection?.Invoke(locationData);

        #endregion
    }

    public class LocationTypeData
    {
        public List<LocationType> LocationTypeConstructors = new();
    }
}
