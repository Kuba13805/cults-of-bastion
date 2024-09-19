using System;
using System.Collections.Generic;
using GameScenarios;
using NewGame;
using UnityEngine;

namespace UI.MainMenu.NewGameMenu.ScenarioChoosing
{
    public class ScenarioPanelController : StagePanelController
    {
        [SerializeField] private ScenarioButton scenarioPrefab;
        [SerializeField] private Transform scenariosParent;
        
        private List<Scenario> _scenarios;
        private Scenario _selectedScenario;
        
        public static event Action<Scenario> OnSelectedScenario;

        protected override void Start()
        {
            base.Start();
            NewGameController.OnPassGameScenarios += InitializeScenarioList;
            ScenarioButton.OnScenarioSelected += UpdateSelectedScenario;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            NewGameController.OnPassGameScenarios -= InitializeScenarioList;
            ScenarioButton.OnScenarioSelected -= UpdateSelectedScenario;
        }

        private void UpdateSelectedScenario(Scenario scenario)
        {
            _selectedScenario = scenario;
            OnSelectedScenario?.Invoke(_selectedScenario);
        }

        private void InitializeScenarioList(List<Scenario> scenarios)
        {
            ClearScenarioList();
            _scenarios = scenarios;
            InstantiateScenarioList();
            UpdateSelectedScenario(_scenarios[0]);
        }

        private void InstantiateScenarioList()
        {
            foreach (var scenario in _scenarios)
            {
                CreateScenarioButton(scenario);
            }
        }
        private void CreateScenarioButton(Scenario scenario)
        {
            var scenarioButton = Instantiate(scenarioPrefab, scenariosParent);
            scenarioButton.InitializeScenario(scenario);
        }
        private void ClearScenarioList()
        {
            foreach (var child in scenariosParent.GetComponentsInChildren<ScenarioButton>())
            {
                Destroy(child.gameObject);
            }
        }
    }
}