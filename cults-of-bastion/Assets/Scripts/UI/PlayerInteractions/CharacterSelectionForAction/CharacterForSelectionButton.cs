using System;
using Characters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PlayerInteractions.CharacterSelectionForAction
{
    public class CharacterForSelectionButton : MonoBehaviour
    {
        private int _characterID;
        
        public static event Action<int> OnCharacterSelected;

        private void OnEnable()
        {
            MakeInteractable(_characterID);
            GetComponent<Button>().onClick.AddListener(SelectCharacter);
            OnCharacterSelected += MakeInteractable;
        }

        private void OnDisable()
        {
            GetComponent<Button>().onClick.RemoveListener(SelectCharacter);
            OnCharacterSelected -= MakeInteractable;
        }

        public void Initialize(int characterId, string characterName)
        {
            _characterID = characterId;
            GetComponentInChildren<TextMeshProUGUI>().text = characterName;
        }

        public void HideButton()
        {
            if (_characterID == 0) gameObject.SetActive(false);
        }

        public void Clear()
        {
            _characterID = 0;
            GetComponentInChildren<TextMeshProUGUI>().text = string.Empty;
        }

        private void SelectCharacter()
        {
            OnCharacterSelected?.Invoke(_characterID);
            GetComponent<Button>().interactable = false;
        }
        private void MakeInteractable(int i)
        {
            GetComponent<Button>().interactable = true;
        }
    }
}