using System;
using GameScenarios;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu.NewGameMenu
{
    public class ScenarioButton : MonoBehaviour
    {
        private Scenario _scenario;
        public static event Action<Scenario> OnScenarioSelected;

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnMouseDown);
        }

        public void InitializeScenario(Scenario scenario)
        {
            _scenario = scenario;
            GetComponentInChildren<TextMeshProUGUI>().text = _scenario.ScenarioName;
        }

        private void OnMouseDown()
        {
            OnScenarioSelected?.Invoke(_scenario);
        }
    }
}