using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Characters.CharacterBackgrounds;
using Cultures;
using GameScenarios;
using Managers;
using Organizations;
using UI.MainMenu;
using UI.MainMenu.NewGameMenu;
using UI.MainMenu.NewGameMenu.CharacterCreation;
using UnityEngine;

namespace NewGame
{
    public class NewGameController : MonoBehaviour
    {
        #region Variables
        
        private Scenario _currentScenario;
        private Character _playerCharacter;
        private List<Character> _playerAgents = new();
        private Organization _createdOrganization;
        
        private List<Scenario> _scenarios = new();
        private List<OrganizationType> _organizationTypes = new();
        private List<Culture> _cultures = new();
        private List<CharacterBackground> _childhoodBackgrounds = new();
        private List<CharacterBackground> _adulthoodBackgrounds = new();

        #endregion

        #region Events

        public static event Action OnRequestGameData;
        public static event Action OnNewGameControllerInitialized;
        public static event Action<List<Scenario>> OnPassGameScenarios;
        public static event Action<List<OrganizationType>> OnPassOrganizationTypes;
        public static event Action<List<Culture>> OnPassCultures;
        public static event Action<List<CharacterBackground>, List<CharacterBackground>> OnPassBackgrounds;
        public static event Action<List<ScenarioModifier>> OnPassScenarioModifiers;
        public static event Action<bool> OnAllowOrganizationCreation;
        public static event Action<string> OnForceOrganizationType;
        public static event Action<Character> OnCharacterCreated; 
        public static event Action OnRequestCharacterGeneration;

        #endregion
        private void OnEnable()
        {
            SubscribeToEvents();
        }

        
        private void Start()
        {
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            StartNewGameButton.OnStartNewGameButtonClicked += InitializeNewGameController;
            GameCreationStagesController.OnCheckIfOrganizationCreationIsAllowed += CheckIfOrganizationCreationIsAllowed;
            GameCreationStagesController.OnCheckIfOrganizationTypeIsForced += CheckIfOrganizationTypeIsForced;
            CharacterPanelController.OnRequestCharacterGeneration += StartCharacterGeneration;
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void UnsubscribeFromEvents()
        {
            StartNewGameButton.OnStartNewGameButtonClicked -= InitializeNewGameController;
            GameCreationStagesController.OnCheckIfOrganizationCreationIsAllowed -= CheckIfOrganizationCreationIsAllowed;
            GameCreationStagesController.OnCheckIfOrganizationTypeIsForced -= CheckIfOrganizationTypeIsForced;
            CharacterPanelController.OnRequestCharacterGeneration -= StartCharacterGeneration;
        }

        private void InitializeNewGameController()
        {
            StartCoroutine(GetGameData());
        }
                

        private IEnumerator GetGameData()
        {
            var areScenariosLoaded = false;
            var areOrganizationTypesLoaded = false;
            var areCulturesLoaded = false;
            var areBackgroundsLoaded = false;

            Action<List<Scenario>> onScenariosLoaded = scenarios =>
            {
                _scenarios = scenarios; 
                areScenariosLoaded = true; 
            };

            Action<List<OrganizationType>> onOrganizationTypesLoaded = organizationTypes =>
            {
                _organizationTypes = organizationTypes;
                areOrganizationTypesLoaded = true;
            };
            
            Action<List<Culture>> onCulturesLoaded = cultures =>
            {
                _cultures = cultures;
                areCulturesLoaded = true;
            };
            Action<(List<CharacterBackground>, List<CharacterBackground>)> onBackgroundsLoaded = backgrounds =>
            {
                _childhoodBackgrounds = backgrounds.Item1;
                _adulthoodBackgrounds = backgrounds.Item2;
                areBackgroundsLoaded = true;
            };
            ScenarioController.OnPassScenarios += onScenariosLoaded;
            OrganizationManager.OnPassOrganizationTypes += onOrganizationTypesLoaded;
            CultureController.OnReturnCultureList += onCulturesLoaded;
            CharacterBackgroundController.OnReturnBackgrounds += onBackgroundsLoaded;
            
            OnRequestGameData?.Invoke();
            
            yield return new WaitUntil(() => areScenariosLoaded && areOrganizationTypesLoaded && 
                                             areCulturesLoaded && areBackgroundsLoaded);
            
            ScenarioController.OnPassScenarios -= onScenariosLoaded;
            OrganizationManager.OnPassOrganizationTypes -= onOrganizationTypesLoaded;
            CultureController.OnReturnCultureList -= onCulturesLoaded;
            CharacterBackgroundController.OnReturnBackgrounds -= onBackgroundsLoaded;
            Debug.Log($"Data loaded. {_scenarios.Count} {_organizationTypes.Count} {_cultures.Count} {_childhoodBackgrounds.Count} {_adulthoodBackgrounds.Count}");
            
            OnNewGameControllerInitialized?.Invoke();
            PassGameData();
        }

        private void PassGameData()
        {
            OnPassGameScenarios?.Invoke(_scenarios);
            OnPassOrganizationTypes?.Invoke(_organizationTypes);
            OnPassCultures?.Invoke(_cultures);
            OnPassBackgrounds?.Invoke(_childhoodBackgrounds, _adulthoodBackgrounds);
        }

        #region NewGameCreationRules

        private void CheckIfOrganizationCreationIsAllowed(Scenario scenario)
        {
            _currentScenario = scenario;
            var allowed = _currentScenario.ScenarioModifiers.Any(scenarioModifier => scenarioModifier.ModiferType == ScenarioModifiers.OrganizationExists && scenarioModifier.BoolValue);
            OnAllowOrganizationCreation?.Invoke(allowed);
        }

        private void CheckIfOrganizationTypeIsForced()
        {
            foreach (var scenarioModifier in _currentScenario.ScenarioModifiers.Where(scenarioModifier => scenarioModifier.ModiferType == ScenarioModifiers.TypeOfOrganization && !string.IsNullOrEmpty(scenarioModifier.StringValue) && !string.IsNullOrWhiteSpace(scenarioModifier.StringValue)))
            {
                OnForceOrganizationType?.Invoke(scenarioModifier.StringValue);
            }
        }

        #endregion

        #region DataPassing
        
        private void StartCharacterGeneration()
        {
            StartCoroutine(HandleGeneratedCharacterData());
        }

        private IEnumerator HandleGeneratedCharacterData()
        {
            var characterCreated = false;
            Action<Character> onCharacterCreated = character =>
            {
                _playerCharacter = character;
                OnCharacterCreated?.Invoke(character);
                characterCreated = true;
            };
            CharacterManager.OnReturnGeneratedCharacter += onCharacterCreated;
            OnRequestCharacterGeneration?.Invoke();
            
            yield return new WaitUntil(() => characterCreated);
            
            CharacterManager.OnReturnGeneratedCharacter -= onCharacterCreated;
        }

        #endregion
    }
}
