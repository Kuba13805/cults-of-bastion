using System;
using System.Collections;
using System.Collections.Generic;
using GameScenarios;
using NewGame;
using Organizations;
using UI.MainMenu.NewGameMenu.OrganizationCreation;
using UI.MainMenu.NewGameMenu.ScenarioChoosing;
using UnityEngine;

namespace UI.MainMenu.NewGameMenu
{
    public class NewGamePanelController : MonoBehaviour
    {
        #region Variables 

        [SerializeField] private ScenarioPanelController scenariosPanel;
        [SerializeField] private OrganizationPanelController organizationPanel;
        [SerializeField] private GameObject characterPanel;
        [SerializeField] private GameObject readyToStartPanel;

        private Scenario _selectedScenario;

        #endregion

        #region Events

        public static event Action OnInvokeNextStage;
        public static event Action OnInvokePreviousStage;
        public static event Action OnRequestGameScenarios;
        public static event Action<Scenario> OnSelectedScenario;
        public static event Action OnRequestOrganizationTypes;

        #endregion

        private void Start()
        {
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            NewGameController.OnStageChange += ChangeStage;
            StageChangeButton.OnNextStageButtonClicked += InvokeNextStage;
            StageChangeButton.OnPreviousStageButtonClicked += InvokePreviousStage;
            ScenarioButton.OnScenarioSelected += SelectScenario;
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void UnsubscribeFromEvents()
        {
            NewGameController.OnStageChange -= ChangeStage;
            StageChangeButton.OnNextStageButtonClicked -= InvokeNextStage;
            StageChangeButton.OnPreviousStageButtonClicked -= InvokePreviousStage;
            ScenarioButton.OnScenarioSelected -= SelectScenario;
        }

        private void SelectScenario(Scenario selectedScenario)
        {
            _selectedScenario = selectedScenario;
            OnSelectedScenario?.Invoke(_selectedScenario);
        }

        private static void InvokeNextStage() => OnInvokeNextStage?.Invoke();
        private static void InvokePreviousStage() => OnInvokePreviousStage?.Invoke();

        #region StageChange

        private void ChangeStage(NewGameStages stage)
        {
            switch (stage)
            {
                case NewGameStages.ChooseGameScenario:
                    InitializeScenarioSelection();
                    break;
                case NewGameStages.CreateOrganization:
                    InitializeOrganizationCreation();
                    break;
                case NewGameStages.CreateCharacter:
                    InitializeCharacterCreation();
                    break;
                case NewGameStages.ReadyToStart:
                    InitializeReadyToStart();
                    break;
            }
            #endregion
        }

        private void InitializeScenarioSelection()
        {
            StartCoroutine(GetGameScenarios());
            StartCoroutine(GetOrganizationTypes());
            scenariosPanel.gameObject.SetActive(true);
            organizationPanel.gameObject.SetActive(false);
            characterPanel.SetActive(false);
        }
        private IEnumerator GetGameScenarios()
        {
            var receivedScenarios = false;
            Action<List<Scenario>> onReceivedGameScenarios = scenarioList =>
            {
                receivedScenarios = true;
                scenariosPanel.InitializeScenarioList(scenarioList);
            };
            
            NewGameController.OnPassScenarios += onReceivedGameScenarios;
            OnRequestGameScenarios?.Invoke();
            
            yield return new WaitUntil(() => receivedScenarios);
            
            NewGameController.OnPassScenarios -= onReceivedGameScenarios;
        }

        private IEnumerator GetOrganizationTypes()
        {
            var receivedOrganizationTypes = false;
            Action<List<OrganizationType>> onReceivedOrganizationTypes = organizationTypes =>
            {
                receivedOrganizationTypes = true;
                organizationPanel.InitializeOrganizationList(organizationTypes);
            };
            
            NewGameController.OnPassOrganizationTypes += onReceivedOrganizationTypes;
            OnRequestOrganizationTypes?.Invoke();
            
            yield return new WaitUntil(() => receivedOrganizationTypes);
            
            NewGameController.OnPassOrganizationTypes -= onReceivedOrganizationTypes;
        }

        private void InitializeOrganizationCreation()
        {
            scenariosPanel.gameObject.SetActive(false);
            organizationPanel.gameObject.SetActive(true);
            characterPanel.SetActive(false);
        }

        private void InitializeCharacterCreation()
        {
            scenariosPanel.gameObject.SetActive(false);
            organizationPanel.gameObject.SetActive(false);
            readyToStartPanel.SetActive(false);
            characterPanel.SetActive(true);
        }

        private void InitializeReadyToStart()
        {
            characterPanel.SetActive(false);
            readyToStartPanel.SetActive(true);
        }
    }
}