using System;
using System.Collections;
using System.Collections.Generic;
using GameScenarios;
using UI.MainMenu.NewGameMenu;
using UnityEngine;

namespace NewGame
{
    public class ScenarioStageController : StageController
    {
        #region Variables

        private List<Scenario> _scenarios = new();
        
        [SerializeField] private GameObject scenarioPrefab;
        [SerializeField] private Transform scenarioParent;

        #endregion

        #region Events

        public static event Action OnRequestScenarios;

        #endregion
        protected override void ToggleStage(NewGameStages newStage)
        {
            base.ToggleStage(newStage);
            if(gameObject.activeSelf) StartScenarioCreation();
            if(!gameObject.activeSelf) ClearScenarios();
        }
        private void StartScenarioCreation()
        {
            StartCoroutine(CreateScenarios());
        }
        private IEnumerator CreateScenarios()
        {
            Action<List<Scenario>> loadScenarios = scenarios =>
            {
                _scenarios = scenarios;
            };
            ScenarioController.OnPassScenarios += loadScenarios;
            OnRequestScenarios?.Invoke();
            
            yield return new WaitUntil(() => _scenarios.Count > 0);
            ScenarioController.OnPassScenarios -= loadScenarios;
            
            foreach (var scenario in _scenarios)
            {
                CreateScenario(scenario);
            }
            yield return null;
        }

        private void CreateScenario(Scenario scenario)
        {
            var scenarioObject = Instantiate(scenarioPrefab, scenarioParent);
            scenarioObject.GetComponent<ScenarioButton>().InitializeScenario(scenario);
        }

        private void ClearScenarios()
        {
            foreach (var scenarioButton in scenarioParent.GetComponentsInChildren<ScenarioButton>())
            {
                Destroy(scenarioButton.gameObject);
            }
        }
    }
}