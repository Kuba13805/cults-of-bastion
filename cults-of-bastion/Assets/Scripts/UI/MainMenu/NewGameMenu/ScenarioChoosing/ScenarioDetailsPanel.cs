using GameScenarios;
using TMPro;
using UnityEngine;

namespace UI.MainMenu.NewGameMenu.ScenarioChoosing
{
    public class ScenarioDetailsPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scenarioName;
        [SerializeField] private TextMeshProUGUI scenarioDescription;

        private void OnEnable()
        {
            ScenarioPanelController.OnSelectedScenario += InitializeScenarioDetails;
        }

        private void OnDestroy()
        {
            ScenarioPanelController.OnSelectedScenario -= InitializeScenarioDetails;
        }

        private void InitializeScenarioDetails(Scenario scenario)
        {
            Debug.Log("Scenario selected: " + scenario.ScenarioName);
            scenarioName.text = scenario.ScenarioName;
            scenarioDescription.text = scenario.ScenarioDescription;

            foreach (var scenarioModifier in scenario.ScenarioModifiers)
            {
                scenarioDescription.text += $"\n{scenarioModifier.ModiferType} : {scenarioModifier.Value} : {scenarioModifier.StringValue} : {scenarioModifier.BoolValue}";
            }
        }
    }
}