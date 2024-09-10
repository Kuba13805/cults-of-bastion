using System;
using UnityEngine;

namespace NewGame
{
    public abstract class StageController : MonoBehaviour
    {
        #region Variables

        [SerializeField] private NewGameStages handledStage;
        [SerializeField] private GameObject stageContent;

        #endregion

        #region Events

        public static event Action OnNextStage;
        public static event Action OnPreviousStage;

        #endregion

        private void Start()
        {
            NewGameController.OnStageChanged += ToggleStage;
        }

        private void OnDestroy()
        {
            NewGameController.OnStageChanged -= ToggleStage;
        }

        protected virtual void ToggleStage(NewGameStages newStage)
        {
            stageContent.SetActive(newStage == handledStage);
        }

        protected static void NextStage() => OnNextStage?.Invoke();
        protected static void PreviousStage() => OnPreviousStage?.Invoke();
    }
}