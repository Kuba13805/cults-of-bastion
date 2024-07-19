using System;
using System.Collections.Generic;
using System.Linq;
using Locations;
using UnityEngine;

public class CityManager : MonoBehaviour
{
    public List<Location> locations;
    private GameData _gameData;
    
    [SerializeField] private TextAsset _jsonFile;

    private void Start()
    {
        LoadData();
    }

    private void LoadData()
    {
        if (_jsonFile == null) return;

        Dictionary<string, LocationType> locationTypeDict = new Dictionary<string, LocationType>();
        
        _gameData = new GameData
        {
            LocationTypes = JsonUtility.FromJson<GameData>(_jsonFile.text).LocationTypes,
            Locations = JsonUtility.FromJson<GameData>(_jsonFile.text).Locations
        };
        foreach (var locationType in _gameData.LocationTypes)
        {
            locationTypeDict[locationType.TypeName] = locationType;
        }
        
        foreach (var locationData in _gameData.Locations)
        {
            if (locationTypeDict.TryGetValue(locationData.locationTypeName, out var locationType))
            {
                locationData.LocationType = locationType;
                Debug.Log("Match found: " + locationType.TypeName);
            }
        }
        foreach (var location in locations)
        {
            location.locationData = _gameData.Locations.Find(locationData => locationData.locationID == location.locationIndex);
        }
    }
    
    private class GameData
    {
        public List<LocationData> Locations = new();
        public List<LocationType> LocationTypes = new();
    }
}
