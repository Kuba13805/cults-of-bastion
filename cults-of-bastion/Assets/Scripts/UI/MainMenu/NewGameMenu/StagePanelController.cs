using System;
using UnityEngine;

namespace UI.MainMenu.NewGameMenu
{
    public abstract class StagePanelController : MonoBehaviour
    {
        [SerializeField] private NewGameStages handledStage;
        protected virtual void Start()
        {
            NewGamePanelController.OnStageChanged += InitializeStage;
        }

        protected virtual void OnDestroy()
        {
            NewGamePanelController.OnStageChanged -= InitializeStage;
        }

        private void InitializeStage(NewGameStages newGameStages)
        {
            if(newGameStages != handledStage) return;
            DisplayPanelContent();
        }

        protected virtual void DisplayPanelContent() => throw new NotImplementedException();
    }
}