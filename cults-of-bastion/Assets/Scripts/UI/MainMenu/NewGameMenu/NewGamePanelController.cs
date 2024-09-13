using System;
using System.Collections;
using System.Collections.Generic;
using GameScenarios;
using NewGame;
using UI.MainMenu.NewGameMenu.ScenarioChoosing;
using UnityEngine;

namespace UI.MainMenu.NewGameMenu
{
    public class NewGamePanelController : MonoBehaviour
    {
        #region Variables 

        [SerializeField] private ScenarioPanelController scenariosPanel;
        [SerializeField] private GameObject organizationPanel;
        [SerializeField] private GameObject characterPanel;
        [SerializeField] private GameObject readyToStartPanel;

        private Scenario _selectedScenario;

        #endregion

        #region Events

        public static event Action OnInvokeNextStage;
        public static event Action OnInvokePreviousStage;
        public static event Action OnRequestGameScenarios;
        public static event Action<Scenario> OnSelectedScenario;

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
            scenariosPanel.gameObject.SetActive(true);
            organizationPanel.SetActive(false);
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

        private void InitializeOrganizationCreation()
        {
            scenariosPanel.gameObject.SetActive(false);
            organizationPanel.SetActive(true);
            characterPanel.SetActive(false);
        }

        private void InitializeCharacterCreation()
        {
            scenariosPanel.gameObject.SetActive(false);
            organizationPanel.SetActive(false);
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