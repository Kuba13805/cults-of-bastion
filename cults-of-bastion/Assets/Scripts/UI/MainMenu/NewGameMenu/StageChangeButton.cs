using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu.NewGameMenu
{
    public class StageChangeButton : MonoBehaviour
    {
        [SerializeField] private bool invokePreviousStage;
        private Button _button;
        public static event Action OnNextStageButtonClicked;
        public static event Action OnPreviousStageButtonClicked;
        
        private void OnEnable()
        {
            _button = GetComponent<Button>();
            _button.interactable = true;
            if (!invokePreviousStage)
            {
                _button.onClick.AddListener(RequestNextStage);
            }
            else
            {
                _button.onClick.AddListener(RequestPreviousStage);   
            }
        }

        private void OnDisable()
        {
            _button.onClick.RemoveAllListeners();
        }

        private void RequestNextStage()
        {
            _button.interactable = false;
            OnNextStageButtonClicked?.Invoke();
        }

        private void RequestPreviousStage()
        {
            _button.interactable = false;
            OnPreviousStageButtonClicked?.Invoke();
        }
    }
}