using System;
using UnityEngine;

namespace UI.MainMenu.NewGameMenu
{
    public class NewGamePanelController : MonoBehaviour
    {
        #region Variables

        private NewGameStages _currentStage;

        #endregion

        #region Events

        public static event Action<NewGameStages> OnStageChanged;
        

        #endregion

        private void Start()
        {
            _currentStage = NewGameStages.ChooseGameScenario;
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void UnsubscribeFromEvents()
        {
            
        }

        #region StageChange

        private void GetNextStage()
        {
            _currentStage++;
            UpdateCurrentStage();
        }

        private void GetPreviousStage()
        {
            _currentStage--;
            UpdateCurrentStage();
        }
        private void UpdateCurrentStage() => OnStageChanged?.Invoke(_currentStage);

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