using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu.NewGameMenu.CharacterCreation
{
    public class RollCharacterButton : MonoBehaviour
    {
        private Button _rollButton;

        public static event Action OnRollCharacter;

        private void Start()
        {
            _rollButton = GetComponent<Button>();
            _rollButton.onClick.AddListener(RollCharacter);
            RollCharacter();
        }
        private void OnDisable()
        {
            _rollButton.onClick.RemoveListener(RollCharacter);
        }
        private static void RollCharacter() => OnRollCharacter?.Invoke();
    }
}