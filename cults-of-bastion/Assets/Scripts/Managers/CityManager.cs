using System.Collections.Generic;
using System.Linq;
using Characters;
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
        
        var parsedData = JsonUtility.FromJson<GameData>(_jsonFile.text);
        if (parsedData == null)
        {
            Debug.LogError("Failed to parse JSON data.");
            return;
        }

        _gameData = new GameData
        {
            LocationTypes = parsedData.LocationTypes,
            Locations = parsedData.Locations,
            CharacterConstructors = parsedData.CharacterConstructors
        };
        
        LoadCharacters();

        LoadLocationTypes();
        
        InjectCharactersToLocationData();
        
        AssignLocationTypeToLocationData();

        InjectLocationDataToWorldLocations();
    }

    private void LoadCharacters()
    {
        foreach (var characterConstructor in _gameData.CharacterConstructors)
        {
            var character = new Character
            {
                characterName = characterConstructor.name,
                characterGender = characterConstructor.gender == "Male" ? CharacterGender.Male : CharacterGender.Female
            };

            foreach (var locationData in characterConstructor.ownLocationIds.SelectMany(t => _gameData.Locations.Where(locationData => locationData.locationID == t)))
            {
                character.characterOwnedLocations.Add(locationData);
            }
            
            _gameData.Characters.Add(character);
            Debug.Log($"Loaded {character.characterGender} character: {character.characterName} with {character.characterOwnedLocations.Count} owned locations.");
        }
    }

    private void InjectCharactersToLocationData()
    {
        foreach (var character in _gameData.Characters)
        {
            foreach (var locationData in _gameData.Locations.Where(locationData => character.characterOwnedLocations.Contains(locationData)))
            {
                locationData.LocationOwner = character;
            }
        }
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

    private class GameData
    {
        public List<LocationData> Locations = new();
        public List<LocationType> LocationTypes = new();
        public List<Character> Characters = new();
        public List<CharacterConstructor> CharacterConstructors = new();
        
        public readonly Dictionary<string, LocationType> LocationTypeDict = new();
    }
}
