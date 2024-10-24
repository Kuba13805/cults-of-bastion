using System;
using Characters;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MapMarkers
{
    public class CharacterInLocationMarker : MonoBehaviour
    {
        [NonSerialized] private Character _character;
        public static event Action<Character> OnInvokeSelectedCharacter;
        public void SetCharacter(Character character)
        {
            _character = character;
        }

        private void OnEnable()
        {
            GetComponent<Button>().onClick.AddListener(InvokeSelectedCharacter);
        }

        private void OnDisable()
        {
            GetComponent<Button>().onClick.RemoveListener(InvokeSelectedCharacter);
        }

        private void InvokeSelectedCharacter()
        {
            OnInvokeSelectedCharacter?.Invoke(_character);
        }
    }
}
