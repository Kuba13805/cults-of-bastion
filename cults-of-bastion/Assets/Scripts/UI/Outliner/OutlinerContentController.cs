using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using Managers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Outliner
{
    public class OutlinerContentController : MonoBehaviour
    {
        #region Variables

        [SerializeField] private GameObject characterButtonPrefab;
        [SerializeField] private Transform characterButtonParent;
        [SerializeField] private GameObject locationButtonPrefab;
        [SerializeField] private Transform locationButtonParent;
        [SerializeField] private Button toggleOutlinerButton;
        [SerializeField] private GameObject outlinerGameObject;
        
        private List<OutlinerCharacterButton> _characterButtonList = new();
        private List<OutlinerLocationButton> _locationButtonList = new();

        #endregion

        #region Events

        public static event Action OnRequestCharacterListForOutliner;

        #endregion
        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        private void SubscribeToEvents()
        {
            OrganizationManager.OnMemberRemovedFromPlayerOrganization += RemoveCharacterButton;
            OrganizationManager.OnMemberAddedToPlayerOrganization += AddNewCharacterToOutliner;
            toggleOutlinerButton.onClick.AddListener(ToggleOutliner);
        }

        private void UnsubscribeFromEvents()
        {
            OrganizationManager.OnMemberRemovedFromPlayerOrganization -= RemoveCharacterButton;
            OrganizationManager.OnMemberAddedToPlayerOrganization -= AddNewCharacterToOutliner;
            toggleOutlinerButton.onClick.RemoveListener(ToggleOutliner);
        }

        private void ToggleOutliner()
        {
            if (!outlinerGameObject.activeSelf)
            {
                StartCoroutine(RequestCharacterList());
            }
            outlinerGameObject.SetActive(!outlinerGameObject.activeSelf);
        }

        private IEnumerator RequestCharacterList()
        {
            var characterListReceived = false;
            Action<List<Character>> onReceiveCharacterList = characters =>
            {
                InstantiateCharacterButtons(characters);
                characterListReceived = true;
            };
            
            OrganizationManager.OnPassOrganizationMembers += onReceiveCharacterList;
            OnRequestCharacterListForOutliner?.Invoke();
            
            yield return new WaitUntil(() => characterListReceived);
            
            OrganizationManager.OnPassOrganizationMembers -= onReceiveCharacterList;
        }

        private void InstantiateCharacterButtons(List<Character> characterList)
        {
            if (characterList == null) return;
            
            var existingCharacterIDs = new HashSet<int>();
            foreach (var button in _characterButtonList)
            {
                existingCharacterIDs.Add(button.character.characterID);
            }
            
            foreach (var character in characterList)
            {
                if (!existingCharacterIDs.Contains(character.characterID))
                {
                    InstantiateCharacterButton(character);
                    existingCharacterIDs.Add(character.characterID);
                }
            }
        }


        private void InstantiateCharacterButton(Character character)
        {
            if (character == null) return;
            var characterButton = CreateButton(characterButtonPrefab, characterButtonParent);
            _characterButtonList.Add(characterButton.GetComponent<OutlinerCharacterButton>());
            characterButton.GetComponent<OutlinerCharacterButton>().InitializeButton(character);
            InstantiateLocationButtons(character);
        }

        private void AddNewCharacterToOutliner(Character character)
        {
            InstantiateCharacterButton(character);
        }

        private void RemoveCharacterButton(int characterID)
        {
            var characterButton = _characterButtonList.Find(x => x.character.characterID == characterID);
            if (characterButton == null) return;
            _characterButtonList.Remove(characterButton);
            foreach (var location in characterButton.character.characterOwnedLocations)
            {
                RemoveLocationButton(location.locationID);
            }
            Destroy(characterButton.gameObject);
        }
        private void InstantiateLocationButtons(Character locationOwner)
        {
            foreach (var location in locationOwner.characterOwnedLocations)
            {
                var locationButton = CreateButton(locationButtonPrefab, locationButtonParent);
                _locationButtonList.Add(locationButton.GetComponent<OutlinerLocationButton>());
                if (locationButton != null) locationButton.GetComponent<OutlinerLocationButton>().InitializeButton(location);
            }
        }
        private void RemoveLocationButton(int locationID)
        {
            var locationButton = _locationButtonList.Find(x => x.locationData.locationID == locationID);
            if (locationButton != null) _locationButtonList.Remove(locationButton);
            Destroy(locationButton.gameObject);
        }
        private static GameObject CreateButton(GameObject buttonPrefab, Transform buttonParent)
        {
            var button = Instantiate(buttonPrefab, buttonParent);
            return button;
        }
    }
}
