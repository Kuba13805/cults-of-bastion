using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu.NewGameMenu
{
    public class StageChangeButton : MonoBehaviour
    {
        [SerializeField] private bool invokePreviousStage;
        public static event Action OnNextStageButtonClicked;
        public static event Action OnPreviousStageButtonClicked;
        private void OnEnable()
        {
            if (!invokePreviousStage)
            {
                GetComponent<Button>().onClick.AddListener(RequestNextStage);
            }
            else
            {
                GetComponent<Button>().onClick.AddListener(RequestPreviousStage);   
            }
        }

        private void OnDisable()
        {
            GetComponent<Button>().onClick.RemoveAllListeners();
        }

        private void RequestNextStage()
        {
            OnNextStageButtonClicked?.Invoke();
        }

        private void RequestPreviousStage()
        {
            OnPreviousStageButtonClicked?.Invoke();
        }
    }
}