using System;
using System.Collections;
using System.Linq;
using GameScenarios;
using UnityEngine;

namespace NewGame
{
    public class NewGameController : MonoBehaviour
    {
        private NewGameStages _currentStage;
        private Scenario _currentScenario;

        #region Events

        public static event Action<NewGameStages> OnStageChanged;
        public static event Action<bool> OnCheckForOrganizationBoolModifier;
        public static event Action<string> OnCheckForOrganizationStringModifier; 

        #endregion
        private void Start()
        {
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            StageController.OnNextStage += NextStage;
            StageController.OnPreviousStage += PreviousStage;
            OrganizationStageController.OnCheckForOrganizationModifier += CheckForOrganizationModifier;
            StartNewGameButton.OnStartNewGameButtonClicked += InitializeGameCreation;
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void UnsubscribeFromEvents()
        {
            StageController.OnNextStage -= NextStage;
            StageController.OnPreviousStage -= PreviousStage;
            OrganizationStageController.OnCheckForOrganizationModifier -= CheckForOrganizationModifier;
            StartNewGameButton.OnStartNewGameButtonClicked -= InitializeGameCreation;
        }

        private void SetCurrentScenario(Scenario scenario)
        {
            _currentScenario = scenario;
        }


        #region ManageNewGameStages

        private void InitializeGameCreation()
        {
            Debug.Log("Initializing New Game");
            _currentStage = NewGameStages.ChooseGameScenario;
            OnStageChanged?.Invoke(_currentStage);
        }

        private void NextStage()
        {
            _currentStage++;
            OnStageChanged?.Invoke(_currentStage);
        }

        private void PreviousStage()
        {
            _currentStage--;
            OnStageChanged?.Invoke(_currentStage);
        }

        #endregion

        #region ScenarioModifiersHandling
        
        private void CheckForOrganizationModifier(ScenarioModifiers modifier)
        {
            switch (modifier)
            {
                case ScenarioModifiers.OrganizationExists:
                {
                    if (_currentScenario.ScenarioModifiers.Any(scenarioModifier => scenarioModifier.ModiferType == ScenarioModifiers.OrganizationExists && scenarioModifier.BoolValue))
                    {
                        OnCheckForOrganizationBoolModifier?.Invoke(true);
                    }
                    OnCheckForOrganizationBoolModifier?.Invoke(false);

                    break;
                }
                case ScenarioModifiers.TypeOfOrganization:
                {
                    foreach (var scenarioModifier in _currentScenario.ScenarioModifiers.Where(scenarioModifier => scenarioModifier.ModiferType == ScenarioModifiers.TypeOfOrganization))
                    {
                        OnCheckForOrganizationStringModifier?.Invoke(scenarioModifier.StringValue);
                        break;
                    }
                    OnCheckForOrganizationStringModifier?.Invoke(string.Empty);
                    
                    break;
                }
            }
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
