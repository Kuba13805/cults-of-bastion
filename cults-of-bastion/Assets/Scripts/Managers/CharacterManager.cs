using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Cultures;
using Organizations;
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
        public static event Action OnCharacterManagerInitialized;
        public static event Action<Character, int> OnRequestCharacterAssigmentToOrganization;
        public static event Action<string> OnRequestCultureAssignment;
        public static event Action OnRequestRandomCultureAssignment;

        private void Awake()
        {
            InitializeCharacterIDs();
            SubscribeToEvents();
        }

        private void Start()
        {
            OnCharacterManagerInitialized?.Invoke();
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

        #region AddRemoveCharacters

        private void AddNewCharacter(Character character)
        {
            character.characterID = GetNewCharacterID();
            if(character.characterID <= 0) return;
            _gameData.Characters.Add(character);
            Debug.Log($"Character {character.characterName} {character.characterSurname} vel {character.characterNickname} at {character.characterAge} " +
                      $"added with id {character.characterID}. {character.CharacterStats.Strength.Name}: {character.CharacterStats.Strength.Value} - {character.CharacterStats.Strength.Desc}");
        }

        private void RemoveCharacter(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            _gameData.Characters.Remove(_gameData.Characters.Find(character => character.characterID == id));
            ReleaseCharacterID(id);
        }

        #endregion

        #region CharacterGeneration

        private IEnumerator GenerateCharacter(Action<Character> callback, params string[] cultureName)
        {
            var tempCulture = new Culture();
            
            Action<Culture> onCultureAssigned = culture =>
            {
                tempCulture = culture;
            };

            if (cultureName.Length == 0)
            {
                CultureController.OnReturnRequestedCulture += onCultureAssigned;
                OnRequestRandomCultureAssignment?.Invoke();
                yield return new WaitUntil(() => tempCulture != null);
                CultureController.OnReturnRequestedCulture -= onCultureAssigned;
            }
            else
            {
                CultureController.OnReturnRequestedCulture += onCultureAssigned;
                OnRequestCultureAssignment?.Invoke(cultureName[0]);
                yield return new WaitUntil(() => tempCulture != null);
                CultureController.OnReturnRequestedCulture -= onCultureAssigned;
            }

            var newGenerator = new CharacterGenerator(tempCulture);
            var newCharacter = newGenerator.GenerateCharacter();
            AddNewCharacter(newCharacter);
            callback(newCharacter);
        }

        #endregion

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
            Debug.Log($"Characters are being loaded.");
            _gameData = gameData;
            StartCoroutine(LoadAndProcessCharacters());
        }

        private IEnumerator LoadAndProcessCharacters()
        {
            yield return StartCoroutine(LoadCharacters());
            yield return StartCoroutine(InjectCharactersToLocationData());
            yield return StartCoroutine(GenerateOwnersForEmptyLocations());
            Debug.Log($"Characters loading finished.");
            OnCharactersLoaded?.Invoke();
        }

        private IEnumerator LoadCharacters()
        {
            var organizationsLoadingFinished = false;
            
            Action onOrganizationsLoadingFinished = () => organizationsLoadingFinished = true;
            OrganizationManager.OnOrganizationLoadingFinished += onOrganizationsLoadingFinished;
            

            yield return new WaitUntil(() => organizationsLoadingFinished);
            foreach (var characterConstructor in _gameData.CharacterConstructors)
            {
                if(_characterIDsAvailable.Count == 0) continue;
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

                if (characterConstructor.organizationId > 0)
                {
                    yield return StartCoroutine(AssignCharacterToOrganization(character, characterConstructor.organizationId));
                }

                if (string.IsNullOrEmpty(characterConstructor.culture))
                {
                    yield return StartCoroutine(AssignCharacterCulture(character, "culture_british"));
                }
                else
                {
                    yield return StartCoroutine(AssignCharacterCulture(character, characterConstructor.culture));
                }
                _gameData.Characters.Add(character);
                Debug.Log($"Loaded {character.characterGender} character: {character.characterName}, id: {character.characterID} " +
                          $"with {character.characterOwnedLocations.Count} owned locations. {character.CharacterStats.Strength.Desc}: {character.CharacterStats.Strength.Value}");
            }
            OrganizationManager.OnOrganizationLoadingFinished -= onOrganizationsLoadingFinished;
            yield return null;
        }

        private IEnumerator AssignCharacterToOrganization(Character character, int organizationID)
        {
            var isCharacterAssigned = false;
            Organization assignedOrganisation = null; 
            
            Action<Organization> onMemberAdded = organization =>
            {
                isCharacterAssigned = true;
                assignedOrganisation = organization;
            };
            OrganizationManager.OnOrganizationMemberAdded += onMemberAdded;
                
            OnRequestCharacterAssigmentToOrganization?.Invoke(character, organizationID);
            yield return new WaitUntil(() => isCharacterAssigned);
            character.characterOrganization = assignedOrganisation;
            Debug.Log($"Character organization {character.characterOrganization.organizationName} assigned to character {character.characterName} {character.characterSurname}");
            OrganizationManager.OnOrganizationMemberAdded -= onMemberAdded;
        }

        private IEnumerator AssignCharacterCulture(Character character, string cultureName)
        {
            Action<Culture> onCultureAssigned = culture =>
            {
                character.characterCulture = culture;
            };
            CultureController.OnReturnRequestedCulture += onCultureAssigned;
            OnRequestCultureAssignment?.Invoke(cultureName);
            
            yield return new WaitUntil(() => character.characterCulture != null);
            Debug.Log($"Character culture {character.characterCulture.cultureName} assigned to character {character.characterName} {character.characterSurname}");
            CultureController.OnReturnRequestedCulture -= onCultureAssigned;
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
                yield return GenerateCharacter(result => location.LocationOwner = result);
                location.LocationOwner.characterOwnedLocations.Add(location);
                Debug.Log($"Character {location.LocationOwner.characterName} {location.LocationOwner.characterSurname} added to {location.locationName}");
            }
            yield return null;
        }
        #endregion
    }
}
