using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters;
using GameScenarios;
using Managers;
using Organizations;
using UI.MainMenu;
using UI.MainMenu.NewGameMenu;
using UnityEngine;

namespace NewGame
{
    public class NewGameController : MonoBehaviour
    {
        private NewGameStages _currentStage;
        private Scenario _currentScenario;
        private Character _createdCharacter;
        private Organization _createdOrganization;

        #region Events

        public static event Action<NewGameStages> OnStageChange;
        public static event Action OnRequestGameScenarios;
        public static event Action<List<Scenario>> OnPassScenarios;
        public static event Action<List<OrganizationType>> OnPassOrganizationTypes;
        public static event Action OnRequestOrganizationTypes;
        
        #endregion
        private void Start()
        {
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            StartNewGameButton.OnStartNewGameButtonClicked += InitializeNewGameStages;
            NewGamePanelController.OnInvokeNextStage += ChangeStageToNextStage;
            NewGamePanelController.OnInvokePreviousStage += ChangeStageToPreviousStage;
            NewGamePanelController.OnRequestGameScenarios += RequestGameScenarios;
            NewGamePanelController.OnSelectedScenario += SetSelectedScenario;
            NewGamePanelController.OnRequestOrganizationTypes += RequestOrganizationTypes;
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void UnsubscribeFromEvents()
        {
            StartNewGameButton.OnStartNewGameButtonClicked -= InitializeNewGameStages;
            NewGamePanelController.OnInvokeNextStage -= ChangeStageToNextStage;
            NewGamePanelController.OnInvokePreviousStage -= ChangeStageToPreviousStage;
            NewGamePanelController.OnRequestGameScenarios -= RequestGameScenarios;
            NewGamePanelController.OnSelectedScenario -= SetSelectedScenario;
            NewGamePanelController.OnRequestOrganizationTypes -= RequestOrganizationTypes;
        }

        #region NewGameStagesControll

        private void InitializeNewGameStages()
        {
            _currentStage = NewGameStages.ChooseGameScenario;
            UpdateCurrentStage();
        }
        private void ChangeStageToNextStage()
        {
            GetNextStage();
            if (_currentStage == NewGameStages.CreateOrganization)
            {
                var isOrganizationCreationAllowed = OrganizationStageCheck();
                if (!isOrganizationCreationAllowed)
                {
                    GetNextStage();
                }
            }
            UpdateCurrentStage();
        }

        private void ChangeStageToPreviousStage()
        {
            GetPreviousStage();
            if (_currentStage == NewGameStages.CreateOrganization)
            {
                var isOrganizationCreationAllowed = OrganizationStageCheck();
                if (!isOrganizationCreationAllowed)
                {
                    GetPreviousStage();
                }
            }
            UpdateCurrentStage();
        }

        private bool OrganizationStageCheck()
        {
            var isOrganizationCreationAllowed = false;
            foreach (var scenarioModifier in _currentScenario.ScenarioModifiers.Where(scenarioModifier => scenarioModifier.ModiferType == ScenarioModifiers.OrganizationExists && scenarioModifier.BoolValue))
            {
                isOrganizationCreationAllowed = true;
            }
            return isOrganizationCreationAllowed;
        }

        private void GetNextStage()
        {
            _currentStage++;
            _currentStage = _currentStage > NewGameStages.ReadyToStart ? NewGameStages.ReadyToStart : _currentStage;
        }
        private void GetPreviousStage()
        {
            _currentStage--;
            _currentStage = _currentStage < NewGameStages.ChooseGameScenario ? NewGameStages.ChooseGameScenario : _currentStage;
        }

        private void UpdateCurrentStage()
        {
            Debug.Log(_currentStage);
            OnStageChange?.Invoke(_currentStage);
        }

        #endregion

        #region GameScenariosControll

        private void RequestGameScenarios()
        {
            StartCoroutine(PassScenarios());
        }

        private static IEnumerator PassScenarios()
        {
            var receivedScenarios = false;
            var scenarioList = new List<Scenario>();
            
            Action<List<Scenario>> onReceivedScenarios = scenarios =>
            {
                scenarioList = scenarios;
                receivedScenarios = true;
            };
            ScenarioController.OnPassScenarios += onReceivedScenarios;
            OnRequestGameScenarios?.Invoke();
            
            yield return new WaitUntil(() => receivedScenarios);
            
            ScenarioController.OnPassScenarios -= onReceivedScenarios;
            OnPassScenarios?.Invoke(scenarioList);
        }
        private void RequestOrganizationTypes()
        {
            StartCoroutine(PassOrganizationTypes());
        }

        private static IEnumerator PassOrganizationTypes()
        {
            var receivedOrganizationTypes = false;
            var organizationTypes = new List<OrganizationType>();
            
            Action<List<OrganizationType>> onReceivedOrganizationTypes = organizationTypesList =>
            {
                organizationTypes = organizationTypesList;
                receivedOrganizationTypes = true;
            };
            OrganizationManager.OnPassOrganizationTypes += onReceivedOrganizationTypes;
            OnRequestOrganizationTypes?.Invoke();
            
            yield return new WaitUntil(() => receivedOrganizationTypes);
            
            OrganizationManager.OnPassOrganizationTypes -= onReceivedOrganizationTypes;
            OnPassOrganizationTypes?.Invoke(organizationTypes);
        }
        
        private void SetSelectedScenario(Scenario scenario) => _currentScenario = scenario;

        #endregion
    }

    public enum NewGameStages
    {
        ChooseGameScenario,
        //ChooseMap,
        CreateOrganization,
        CreateCharacter,
        ReadyToStart
    }
}
