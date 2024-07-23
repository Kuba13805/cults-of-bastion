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
        private HashSet<int> _characterIDsInUse = new();
        private Queue<int> _characterIDsAvailable = new();
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

        private void AddNewCharacter()
        {
            
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
            OnCharactersLoaded?.Invoke();
        }

        private IEnumerator LoadCharacters()
        {
            foreach (var characterConstructor in _gameData.CharacterConstructors)
            {
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

        #endregion
    }
}
