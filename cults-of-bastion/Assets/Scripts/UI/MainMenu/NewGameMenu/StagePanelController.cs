using UnityEngine;

namespace UI.MainMenu.NewGameMenu
{
    public abstract class StagePanelController : MonoBehaviour
    {
        [SerializeField] protected NewGameStages handledStage;
        [SerializeField] protected GameObject panelContent;
        protected virtual void Start()
        {
            GameCreationStagesController.OnStageChanged += InitializeStage;
        }

        protected virtual void OnDestroy()
        {
            GameCreationStagesController.OnStageChanged -= InitializeStage;
        }

        protected virtual void InitializeStage(NewGameStages newGameStages)
        {
            panelContent.SetActive(handledStage.Equals(newGameStages));
        }
    }
}