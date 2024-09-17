using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using Characters.CharacterBackgrounds;
using Cultures;
using GameScenarios;
using Managers;
using Organizations;
using UI.MainMenu.NewGameMenu;
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
            
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void UnsubscribeFromEvents()
        {
            
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
        }
        
    }
}
