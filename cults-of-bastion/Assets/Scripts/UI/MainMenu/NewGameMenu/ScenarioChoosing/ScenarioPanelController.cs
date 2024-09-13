using System.Collections.Generic;
using GameScenarios;
using UnityEngine;

namespace UI.MainMenu.NewGameMenu.ScenarioChoosing
{
    public class ScenarioPanelController : MonoBehaviour
    {
        [SerializeField] private ScenarioButton scenarioPrefab;
        [SerializeField] private Transform scenariosParent;

        public void InitializeScenarioList(List<Scenario> scenarios)
        {
            ClearScenarioList();
            foreach (var scenario in scenarios)
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