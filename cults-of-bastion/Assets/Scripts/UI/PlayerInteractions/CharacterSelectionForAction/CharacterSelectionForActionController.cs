using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using Managers;
using NUnit.Framework;
using Organizations;
using PlayerInteractions;
using UI.PlayerInteractions.CharacterSelectionForAction;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace UI.PlayerInteractions
{
    public class CharacterSelectionForActionController : MonoBehaviour
    {
        #region Variables

        [SerializeField] private GameObject characterButtonPrefab;
        [SerializeField] private Transform characterButtonParent;
        [SerializeField] private GameObject selectionScreen;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button confirmButton;

        [SerializeField] private int maxButtons;
        private Character _selectedCharacter;
        private readonly List<CharacterForSelectionButton> _characterButtonList = new();
        private List<Character> _characterList = new();

        #endregion

        #region Events

        public static event Action<Character> OnPassSelectedCharacterForAction;
        public static event Action OnCancelActionInvoking;

        public static event Action OnRequestOrganizationMembersForAction;
        

        #endregion

        private void Start()
        {
            SubscribeToEvents();
            InstantiateCharacterButtons();
            StartCoroutine(RequestCharacterList());
            confirmButton.interactable = false;
        }
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        private void SubscribeToEvents()
        {
            CharacterForSelectionButton.OnCharacterSelected += AssignSelectedCharacter;
            cancelButton.onClick.AddListener(CancelCharacterSelection);
            confirmButton.onClick.AddListener(ConfirmCharacterSelection);
            UIController.OnRequestCharacterSelectionForAction += DisplaySelectionScreen;
        }
        private void UnsubscribeFromEvents()
        {
            CharacterForSelectionButton.OnCharacterSelected -= AssignSelectedCharacter;
            cancelButton.onClick.RemoveListener(CancelCharacterSelection);
            confirmButton.onClick.RemoveListener(ConfirmCharacterSelection);
            UIController.OnRequestCharacterSelectionForAction -= DisplaySelectionScreen;
        }
        private void DisplaySelectionScreen()
        {
            InitializeSelectionScreen();
            DisplayInitializedCharacterButtons();
            selectionScreen.SetActive(true);
        }

        private void DisplayInitializedCharacterButtons()
        {
            foreach (var button in _characterButtonList)
            {
                button.HideButton();
            }
        }
        private void HideSelectionScreen()
        {
            selectionScreen.SetActive(false);
            ClearSelectionScreen();
        }
        private void AssignSelectedCharacter(int characterId)
        {
            var selectedCharacter = _characterList.Find(character => character.characterID == characterId);
            _selectedCharacter = selectedCharacter;
            confirmButton.interactable = true;
        }
        private void ConfirmCharacterSelection()
        {
            OnPassSelectedCharacterForAction?.Invoke(_selectedCharacter);
            HideSelectionScreen();
        }
        private void CancelCharacterSelection()
        {
            OnCancelActionInvoking?.Invoke();
            HideSelectionScreen();
        }

        private IEnumerator RequestCharacterList()
        {
            Action<List<Character>> onAssignCharacterList = characters =>
            {
                _characterList = characters;
            };
            
            OrganizationManager.OnPassOrganizationMembers += onAssignCharacterList;
            OnRequestOrganizationMembersForAction?.Invoke();
            
            yield return new WaitUntil(() => _characterList.Count > 0);
            
            OrganizationManager.OnPassOrganizationMembers -= onAssignCharacterList;
            
            InitializeSelectionScreen();
        }

        private void InitializeSelectionScreen()
        {
            for (int i = 0; i < _characterList.Count; i++)
            {
                _characterButtonList[i].Initialize(_characterList[i].characterID, $"{_characterList[i].characterName} {_characterList[i].characterSurname}");
            }
        }
        private void ClearSelectionScreen()
        {
            for (int i = 0; i < _characterList.Count; i++)
            {
                _characterButtonList[i].Clear();
            }
        }

        private void InstantiateCharacterButtons()
        {
            for (int i = 0; i < maxButtons; i++)
            {
                CreateCharacterButton();
            }
        }
        private void CreateCharacterButton()
        {
            var characterButton = Instantiate(characterButtonPrefab, characterButtonParent);
            _characterButtonList.Add(characterButton.GetComponent<CharacterForSelectionButton>());
        }
    }
}
