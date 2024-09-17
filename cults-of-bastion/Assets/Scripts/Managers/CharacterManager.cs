using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Characters.CharacterBackgrounds;
using Cultures;
using Organizations;
using UnityEngine;

namespace Managers
{
    [RequireComponent(typeof(CharacterBackgroundController), typeof(CharacterModificationController))]
    public class CharacterManager : MonoBehaviour
    {
        private readonly HashSet<int> _characterIDsInUse = new();
        private readonly Queue<int> _characterIDsAvailable = new();
        private GameData _gameData;
        private CharacterGenerator _characterGenerator;

        [SerializeField] private int maxCharacters;
        
        public static event Action OnCharactersLoaded;
        public static event Action OnCharacterManagerInitialized;
        public static event Action OnRequestCharacterGeneratorData;
        public static event Action<Character, int> OnRequestCharacterAssigmentToOrganization;
        public static event Action<Character, List<CharacterModifier>, bool> OnRequestCharacterModificationFromModifiers;

        private void Awake()
        {
            InitializeCharacterIDs();
            SubscribeToEvents();
        }

        private void Start()
        {
            _characterGenerator = GetComponent<CharacterGenerator>();
            StartCoroutine(StartGeneratorInitialization());
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
                      $"added with id {character.characterID}. Character culture: {character.characterCulture} and backgrounds: {character.ChildhoodBackground.BackgroundName} and {character.AdulthoodBackground.BackgroundName}");
        }

        private void RemoveCharacter(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            _gameData.Characters.Remove(_gameData.Characters.Find(character => character.characterID == id));
            ReleaseCharacterID(id);
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
        #region CharacterGeneration
        private IEnumerator StartGeneratorInitialization()
        {
            var tempCultureList = new List<Culture>();
            (List<CharacterBackground>, List<CharacterBackground>) tempBackgroundList = new();
            
            var culturesReceived = false;
            var backgroundsReceived = false;
            
            Action<List<Culture>> onCultureListReceived = cultures =>
            {
                tempCultureList = cultures;
                culturesReceived = true;
            };
            Action<(List<CharacterBackground>, List<CharacterBackground>)> onBackgroundListReceived = backgrounds =>
            {
                tempBackgroundList = backgrounds;
                backgroundsReceived = true;
            };
            CultureController.OnReturnCultureList += onCultureListReceived;
            CharacterBackgroundController.OnReturnBackgrounds += onBackgroundListReceived;
            
            OnRequestCharacterGeneratorData?.Invoke();

            yield return new WaitUntil(() => culturesReceived && backgroundsReceived);
            
            CultureController.OnReturnCultureList -= onCultureListReceived;
            CharacterBackgroundController.OnReturnBackgrounds -= onBackgroundListReceived;
            
            yield return new WaitUntil(() => _characterGenerator.InitializeGenerator(tempCultureList, tempBackgroundList.Item1, tempBackgroundList.Item2));
            
            OnCharacterManagerInitialized?.Invoke();
        }

        private Character GenerateCharacter()
        {
            var character = _characterGenerator.GenerateCharacter();
            ApplyBackgroundModifiers(character);
            AddNewCharacter(character);
            return character;
        }

        private Character GenerateCharacter(Culture culture)
        {
            var character = _characterGenerator.GenerateCharacter(culture.cultureName);
            ApplyBackgroundModifiers(character);
            AddNewCharacter(character);
            return character;
        }

        private Character GenerateCharacter(CharacterConstructor characterConstructor)
        {
            var character = _characterGenerator.GenerateCharacter(characterConstructor);
            ApplyBackgroundModifiers(character);
            AddNewCharacter(character);
            return character;
        }

        private void ApplyBackgroundModifiers(Character character)
        {
            ModifyCharacterWithModifiers(character, character.ChildhoodBackground.BackgroundModifiers, false);
            ModifyCharacterWithModifiers(character, character.AdulthoodBackground.BackgroundModifiers, false);
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
                var character = GenerateCharacter(characterConstructor);

                foreach (var locationData in characterConstructor.ownLocationIds.SelectMany(t => _gameData.Locations.Where(locationData => locationData.locationID == t)))
                {
                    character.characterOwnedLocations.Add(locationData);
                }
                if (characterConstructor.organizationId > 0)
                {
                    yield return StartCoroutine(AssignCharacterToOrganization(character, characterConstructor.organizationId));
                }
            }
            OrganizationManager.OnOrganizationLoadingFinished -= onOrganizationsLoadingFinished;
            yield return null;
        }
        

        private static IEnumerator AssignCharacterToOrganization(Character character, int organizationID)
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
                location.LocationOwner = GenerateCharacter();
                location.LocationOwner.characterOwnedLocations.Add(location);
                Debug.Log($"Character {location.LocationOwner.characterName} {location.LocationOwner.characterSurname} added to {location.locationName}");
            }
            yield return null;
        }
        #endregion

        #region CharacterModifications

        private static void ModifyCharacterWithModifiers(Character character, List<CharacterModifier> characterModifiers, bool isReverse) => 
            OnRequestCharacterModificationFromModifiers?.Invoke(character, characterModifiers, isReverse);

        #endregion
    }
}
