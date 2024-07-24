using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters;
using UnityEngine;

namespace Managers
{
    public class CharacterManager : MonoBehaviour
    {
        private readonly HashSet<int> _characterIDsInUse = new();
        private readonly Queue<int> _characterIDsAvailable = new();
        private GameData _gameData;

        [SerializeField] private int maxCharacters;
        
        public static event Action OnCharactersLoaded;

        private void Awake()
        {
            InitializeCharacterIDs();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            GameManager.OnGameDataLoaded += StartCharacterLoading;
        }

        private void UnsubscribeFromEvents()
        {
            GameManager.OnGameDataLoaded -= StartCharacterLoading;
        }
        private void InitializeCharacterIDs()
        {
            for (int i = 1; i <= maxCharacters; i++)
            {
                _characterIDsAvailable.Enqueue(i);
            }
        }

        private void AddNewCharacter(Character character)
        {
            character.characterID = GetNewCharacterID();
            _gameData.Characters.Add(character);
            Debug.Log($"Character {character.characterName} {character.characterSurname} vel {character.characterNickname} at {character.characterAge} added with id {character.characterID}");
        }

        private void RemoveCharacter(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            _gameData.Characters.Remove(_gameData.Characters.Find(character => character.characterID == id));
            ReleaseCharacterID(id);
        }

        #region HandleCharacterIDs

        private int GetNewCharacterID()
        {
            if (_characterIDsAvailable.Count <= 0) return -1;
            
            var id = _characterIDsAvailable.Dequeue();
            _characterIDsInUse.Add(id);
            return id;
        }

        private void ReleaseCharacterID(int id)
        {
            if (!_characterIDsInUse.Contains(id)) return;
            
            _characterIDsInUse.Remove(id);
            _characterIDsAvailable.Enqueue(id);
        }

        #endregion

        #region CharLoadingFromFile
        private void StartCharacterLoading(GameData gameData)
        {
            _gameData = gameData;
            StartCoroutine(LoadCharacters());
            StartCoroutine(InjectCharactersToLocationData());
            StartCoroutine(GenerateOwnersForEmptyLocations());
            OnCharactersLoaded?.Invoke();
        }

        private IEnumerator LoadCharacters()
        {
            foreach (var characterConstructor in _gameData.CharacterConstructors)
            {
                if(characterConstructor.ownLocationIds.Count == 0) continue;
                var character = new Character
                {
                    characterName = characterConstructor.name,
                    characterSurname = characterConstructor.surname,
                    characterNickname = characterConstructor.nickname,
                    characterGender = characterConstructor.gender == "Male" 
                        ? CharacterGender.Male : CharacterGender.Female,
                    characterAge = characterConstructor.age,
                    characterID = GetNewCharacterID(),
                };

                foreach (var locationData in characterConstructor.ownLocationIds.SelectMany(t => _gameData.Locations.Where(locationData => locationData.locationID == t)))
                {
                    character.characterOwnedLocations.Add(locationData);
                }
            
                _gameData.Characters.Add(character);
                Debug.Log($"Loaded {character.characterGender} character: {character.characterName}, id: {character.characterID} with {character.characterOwnedLocations.Count} owned locations.");
            }

            yield return null;
        }
        
        private IEnumerator InjectCharactersToLocationData()
        {
            foreach (var character in _gameData.Characters)
            {
                foreach (var locationData in _gameData.Locations.Where(locationData => character.characterOwnedLocations.Contains(locationData)))
                {
                    locationData.LocationOwner = character;
                }
            }
            yield return null;
        }

        private IEnumerator GenerateOwnersForEmptyLocations()
        {
            foreach (var location in _gameData.Locations.Where(location => location.LocationOwner == null))
            {
                location.LocationOwner = GenerateCharacterForLocation();
                Debug.Log($"Character {location.LocationOwner.characterName} {location.LocationOwner.characterSurname} added to {location.locationName}");
            }
            yield return null;
        }
        private Character GenerateCharacterForLocation()
        {
            var newCharacterGenerator = new CharacterGenerator();
            var character = newCharacterGenerator.GenerateCharacter();
            AddNewCharacter(character);
            return character;
        }
        #endregion
    }
}
