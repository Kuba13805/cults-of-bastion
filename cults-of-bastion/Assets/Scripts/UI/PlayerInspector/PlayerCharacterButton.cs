using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PlayerInspector
{
    public class PlayerCharacterButton : MonoBehaviour
    {
        public static event Action OnInspectPlayerCharacter;

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            GetComponent<Button>().onClick.RemoveListener(OnClick);
        }
        private static void OnClick() => OnInspectPlayerCharacter?.Invoke();
    }
}
