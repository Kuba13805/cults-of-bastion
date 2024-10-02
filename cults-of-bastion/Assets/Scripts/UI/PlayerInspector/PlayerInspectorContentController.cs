using System;
using System.Collections;
using Characters;
using Locations;
using Organizations;
using UnityEngine;

namespace UI.PlayerInspector
{
    public class PlayerInspectorContentController : MonoBehaviour
    {
        [SerializeField] private GameObject inspectorGameObject;
        [SerializeField] private GameObject locationInspectorContent;
        [SerializeField] private GameObject characterInspectorContent;
        [SerializeField] private GameObject organizationInspectorContent;

        #region Events

        public static event Action OnRequestPlayerCharacter;

        #endregion

        private void Start()
        {
            SubscribeToEvents();
            characterInspectorContent.GetComponent<CharacterInspector>().InitializeStatBoxes();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            UIController.OnLocationSelection += ShowSelectedLocation;
            LocationInspector.OnInvokeLocationOwnerInspector += ShowSelectedCharacter;
            CharacterOwnedLocationButton.OnInvokeLocationInspector += ShowSelectedLocation;
            CharacterInspector.OnInvokeCharacterOrganizationInspector += ShowSelectedOrganization;
            OrganizationMemberButton.OnInvokeCharacterInspector += ShowSelectedCharacter;
            PlayerCharacterButton.OnInspectPlayerCharacter += ShowPlayerCharacter;
        }
        private void UnsubscribeFromEvents()
        {
            UIController.OnLocationSelection -= ShowSelectedLocation;
            LocationInspector.OnInvokeLocationOwnerInspector -= ShowSelectedCharacter;
            CharacterOwnedLocationButton.OnInvokeLocationInspector -= ShowSelectedLocation;
            CharacterInspector.OnInvokeCharacterOrganizationInspector -= ShowSelectedOrganization;
            OrganizationMemberButton.OnInvokeCharacterInspector -= ShowSelectedCharacter;
            PlayerCharacterButton.OnInspectPlayerCharacter -= ShowPlayerCharacter;
        }
        public void ToggleInspector()
        {
            inspectorGameObject.SetActive(!inspectorGameObject.activeSelf);
        }
        private void ShowSelectedLocation(LocationData locationData)
        {
            organizationInspectorContent.SetActive(false);
            characterInspectorContent.SetActive(false);
            locationInspectorContent.SetActive(true);
            locationInspectorContent.GetComponent<LocationInspector>().InitializeInspector(locationData);
            if (!inspectorGameObject.activeSelf)
            {
                ToggleInspector();
            }
        }
        private void ShowSelectedCharacter(Character character)
        {
            locationInspectorContent.SetActive(false);
            organizationInspectorContent.SetActive(false);
            characterInspectorContent.SetActive(true);
            characterInspectorContent.GetComponent<CharacterInspector>().InitializeInspector(character);
            if (!inspectorGameObject.activeSelf)
            {
                ToggleInspector();
            }
        }

        private void ShowSelectedOrganization(Organization organization)
        {
            locationInspectorContent.SetActive(false);
            characterInspectorContent.SetActive(false);
            organizationInspectorContent.SetActive(true);
            organizationInspectorContent.GetComponent<OrganizationInspector>().InitializeInspector(organization);
            if (!inspectorGameObject.activeSelf)
            {
                ToggleInspector();
            }
        }

        private void ShowPlayerCharacter()
        {
            StartCoroutine(RequestPlayerCharacter());
        }

        private IEnumerator RequestPlayerCharacter()
        {
            var isPlayerCharacterLoaded = false;
            Action<Character> onPassPlayerCharacter = playerCharacter =>
            {
                ShowSelectedCharacter(playerCharacter);
                isPlayerCharacterLoaded = true;
            };
            UIController.OnPassPlayerCharacter += onPassPlayerCharacter;
            OnRequestPlayerCharacter?.Invoke();
            
            yield return new WaitUntil(() => isPlayerCharacterLoaded);
            
            UIController.OnPassPlayerCharacter -= onPassPlayerCharacter;
        }
    }
}
