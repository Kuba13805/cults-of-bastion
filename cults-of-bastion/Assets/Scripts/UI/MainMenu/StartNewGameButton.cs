using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.MainMenu
{
    public class StartNewGameButton : MonoBehaviour
    {
        public static event Action OnStartNewGameButtonClicked;

        private void OnEnable()
        {
            GetComponent<Button>().onClick.AddListener(StartNewGameCreation);
        }

        private void OnDestroy()
        {
            GetComponent<Button>().onClick.RemoveListener(StartNewGameCreation);
        }

        private void StartNewGameCreation()
        {
            GetComponent<Button>().interactable = false;
            OnStartNewGameButtonClicked?.Invoke();
        }
    }
}
