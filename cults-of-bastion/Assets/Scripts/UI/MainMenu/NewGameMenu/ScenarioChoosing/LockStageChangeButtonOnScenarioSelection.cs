using System;
using GameScenarios;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu.NewGameMenu.ScenarioChoosing
{
    public class LockStageChangeButtonOnScenarioSelection : MonoBehaviour
    {
        private Button _stageChangeButton;

        private void Start()
        {
            _stageChangeButton = GetComponent<Button>();
            LockStageChangeButton();
            
            ScenarioButton.OnScenarioSelected += UnlockStageChangeButton;
        }

        private void OnDisable()
        {
            ScenarioButton.OnScenarioSelected -= UnlockStageChangeButton;
        }

        private void UnlockStageChangeButton(Scenario scenario)
        {
            _stageChangeButton.interactable = true;
        }
        private void LockStageChangeButton()
        {
            _stageChangeButton.interactable = false;
        }
    }
}