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
using UI.MainMenu.NewGameMenu.OrganizationCreation;
using UI.MainMenu.NewGameMenu.ScenarioChoosing;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NewGame
{
    public class NewGameController : MonoBehaviour
    {
        #region Variables
        
        private Scenario _currentScenario;
        private Character _playerCharacter;
        private List<Character> _playerAgents = new();
        private Organization _playerOrganization;
        
        private List<Scenario> _scenarios = new();
        private List<OrganizationType> _organizationTypes = new();
        private List<Culture> _cultures = new();
        private List<CharacterBackground> _childhoodBackgrounds = new();
        private List<CharacterBackground> _adulthoodBackgrounds = new();

        private bool _newGameCreated;

        #endregion

        #region Events

        public static event Action OnRequestGameData;
        public static event Action OnNewGameControllerInitialized;
        public static event Action<List<Scenario>> OnPassGameScenarios;
        public static event Action<List<OrganizationType>> OnPassOrganizationTypes;
        public static event Action<List<Culture>> OnPassCultures;
        public static event Action<List<CharacterBackground>, List<CharacterBackground>> OnPassBackgrounds;
        public static event Action<List<ScenarioModifier>> OnPassScenarioCharacterModifiers;
        public static event Action<bool> OnAllowOrganizationCreation;
        public static event Action<string> OnForceOrganizationType;
        public static event Action<Character> OnCharacterCreated; 
        public static event Action<List<ScenarioModifier>> OnRequestCharacterGeneration;
        public static event Action<Character, Organization> OnStartNewGame;

        #endregion
        
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
            ScenarioPanelController.OnSelectedScenario += CheckIfScenarioHasCharacterModifiers;
            CharacterPanelController.OnCharachterCreated += UpdatePlayerCharacter;
            OrganizationPanelController.OnOrganizationCreated += UpdatePlayerOrganization;
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
            ScenarioPanelController.OnSelectedScenario -= CheckIfScenarioHasCharacterModifiers;
            CharacterPanelController.OnCharachterCreated -= UpdatePlayerCharacter;
            OrganizationPanelController.OnOrganizationCreated -= UpdatePlayerOrganization;
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

        private static void CheckIfScenarioHasCharacterModifiers(Scenario scenario)
        {
            var tempModifierList = scenario.ScenarioModifiers.Where(scenarioModifier => scenarioModifier.ModiferType is ScenarioModifiers.ChanceForCharacterBackground or 
                ScenarioModifiers.ChanceForCharacterCulture or ScenarioModifiers.ChanceForCharacterTrait).ToList();
            OnPassScenarioCharacterModifiers?.Invoke(tempModifierList);
        }

        #endregion

        #region DataPassing
        
        private void StartCharacterGeneration(List<ScenarioModifier> scenarioModifiers)
        {
            StartCoroutine(HandleGeneratedCharacterData(scenarioModifiers));
        }

        private IEnumerator HandleGeneratedCharacterData(List<ScenarioModifier> scenarioModifiers)
        {
            var characterCreated = false;
            Action<Character> onCharacterCreated = character =>
            {
                _playerCharacter = character;
                OnCharacterCreated?.Invoke(character);
                characterCreated = true;
            };
            CharacterManager.OnReturnGeneratedCharacter += onCharacterCreated;
            OnRequestCharacterGeneration?.Invoke(scenarioModifiers);
            
            yield return new WaitUntil(() => characterCreated);
            
            CharacterManager.OnReturnGeneratedCharacter -= onCharacterCreated;
        }
        private void UpdatePlayerCharacter(Character createdCharacter)
        {
            _playerCharacter = createdCharacter;
            CreateNewGameData();
        }

        private void UpdatePlayerOrganization(string organizationName, OrganizationType organizationType)
        {
            _playerOrganization = new Organization
            {
                organizationName = organizationName,
                organizationType = organizationType
            };
        }

        private void CreateNewGameData()
        {
            if(_newGameCreated) return;
            OnStartNewGame?.Invoke(_playerCharacter, _playerOrganization);
            Debug.Log("New Game Started");
            _newGameCreated = true;
        }

        #endregion
    }
}
