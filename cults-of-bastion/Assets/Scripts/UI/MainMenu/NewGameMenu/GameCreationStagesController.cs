using System;
using System.Collections;
using GameScenarios;
using NewGame;
using UI.MainMenu.NewGameMenu.ScenarioChoosing;
using UnityEngine;

namespace UI.MainMenu.NewGameMenu
{
    public class GameCreationStagesController : MonoBehaviour
    {
        #region Variables

        private NewGameStages _currentStage;
        private bool _organizationCreationIsAllowed;

        #endregion

        #region Events

        public static event Action<NewGameStages> OnStageChanged;
        public static event Action<Scenario> OnCheckIfOrganizationCreationIsAllowed;
        public static event Action<string> OnForceOrganizationType;
        public static event Action OnReleaseOrganizationType;
        public static event Action OnCheckIfOrganizationTypeIsForced;

        #endregion

        private void Start()
        {
            _currentStage = NewGameStages.ChooseGameScenario;
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            NewGameController.OnNewGameControllerInitialized += UpdateCurrentStage;
            StageChangeButton.OnNextStageButtonClicked += GetNextStage;
            StageChangeButton.OnPreviousStageButtonClicked += GetPreviousStage;
            ScenarioPanelController.OnSelectedScenario += CheckIfOrganizationCreationIsAllowed;
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void UnsubscribeFromEvents()
        {
            NewGameController.OnNewGameControllerInitialized -= UpdateCurrentStage;
            StageChangeButton.OnNextStageButtonClicked -= GetNextStage;
            StageChangeButton.OnPreviousStageButtonClicked -= GetPreviousStage;
            ScenarioPanelController.OnSelectedScenario -= CheckIfOrganizationCreationIsAllowed;
        }

        #region StageChange

        private void GetNextStage()
        {
            _currentStage++;
            if (!_organizationCreationIsAllowed && _currentStage == NewGameStages.CreateOrganization)
            {
                Debug.Log($"Organization creation is not allowed.");
                GetNextStage();
            }
            UpdateCurrentStage();
        }

        private void GetPreviousStage()
        {
            _currentStage--;
            if (!_organizationCreationIsAllowed && _currentStage == NewGameStages.CreateOrganization)
            {
                Debug.Log($"Organization creation is not allowed.");
                GetPreviousStage();
            }
            UpdateCurrentStage();
        }
        private void UpdateCurrentStage()
        {
            OnStageChanged?.Invoke(_currentStage);
        }

        #endregion

        #region StageRules

        private void CheckIfOrganizationCreationIsAllowed(Scenario scenario)
        {
            StartCoroutine(RequestOrganizationCreationChecking(scenario));

            if (_organizationCreationIsAllowed)
            {
                CheckIfOrganizationTypeIsForced();
            }
        }

        private IEnumerator RequestOrganizationCreationChecking(Scenario scenario)
        {
            var checkingInfoReceived = false;
            Action<bool> onOrganizationCreationCheck = allowed =>
            {
                _organizationCreationIsAllowed = allowed;
                checkingInfoReceived = true;
            };
            NewGameController.OnAllowOrganizationCreation += onOrganizationCreationCheck;
            OnCheckIfOrganizationCreationIsAllowed?.Invoke(scenario);
            
            yield return new WaitUntil(() => checkingInfoReceived);
            
            NewGameController.OnAllowOrganizationCreation -= onOrganizationCreationCheck;
        }

        private void CheckIfOrganizationTypeIsForced()
        {
            StartCoroutine(RequestOrganizationTypeChecking());
        }
        private IEnumerator RequestOrganizationTypeChecking()
        {
            var organizationTypeName = "";
            var checkingInfoReceived = false;
            Action<string> onOrganizationTypeCheck = organizationType =>
            {
                organizationTypeName = organizationType;
                checkingInfoReceived = true;
            };
            NewGameController.OnForceOrganizationType += onOrganizationTypeCheck;
            OnCheckIfOrganizationTypeIsForced?.Invoke();
            
            yield return new WaitUntil(() => checkingInfoReceived);
            if(!string.IsNullOrEmpty(organizationTypeName) && !string.IsNullOrWhiteSpace(organizationTypeName))
            {
                OnForceOrganizationType?.Invoke(organizationTypeName);
            }
            else
            {
                OnReleaseOrganizationType?.Invoke();
            }
            
            NewGameController.OnForceOrganizationType -= onOrganizationTypeCheck;
        }

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