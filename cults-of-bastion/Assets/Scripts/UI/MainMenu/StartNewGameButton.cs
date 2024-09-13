using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu
{
    public class StartNewGameButton : MonoBehaviour
    {
        public static event Action OnStartNewGameButtonClicked;
        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(() => OnStartNewGameButtonClicked?.Invoke());
        }

        private void OnDestroy()
        {
            GetComponent<Button>().onClick.RemoveListener(() => OnStartNewGameButtonClicked?.Invoke());
        }
    }
}
