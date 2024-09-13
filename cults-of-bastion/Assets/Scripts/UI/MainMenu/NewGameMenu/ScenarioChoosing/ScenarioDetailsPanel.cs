using GameScenarios;
using TMPro;
using UnityEngine;

namespace UI.MainMenu.NewGameMenu.ScenarioChoosing
{
    public class ScenarioDetailsPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scenarioName;
        [SerializeField] private TextMeshProUGUI _scenarioDescription;

        private void Start()
        {
            ScenarioButton.OnScenarioSelected += InitializeScenarioDetails;
        }

        private void OnDestroy()
        {
            ScenarioButton.OnScenarioSelected -= InitializeScenarioDetails;
        }

        private void InitializeScenarioDetails(Scenario scenario)
        {
            Debug.Log("Scenario selected: " + scenario.ScenarioName);
            _scenarioName.text = scenario.ScenarioName;
            _scenarioDescription.text = scenario.ScenarioDescription;

            foreach (var scenarioModifier in scenario.ScenarioModifiers)
            {
                _scenarioDescription.text += $"\n{scenarioModifier.ModiferType} : {scenarioModifier.Value} : {scenarioModifier.StringValue} : {scenarioModifier.BoolValue}";
            }
        }
    }
}