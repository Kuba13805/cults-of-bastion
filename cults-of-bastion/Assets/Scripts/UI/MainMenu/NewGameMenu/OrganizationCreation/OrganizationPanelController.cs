using System;
using System.Collections;
using System.Collections.Generic;
using Organizations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu.NewGameMenu.OrganizationCreation
{
    public class OrganizationPanelController : MonoBehaviour
    {
        #region Variables

        [SerializeField] private TMP_InputField organizationNameInputField;
        [SerializeField] private TextMeshProUGUI organizationTypeName;
        [SerializeField] private Button nextStageButton;
        
        private List<OrganizationType> _organizationTypes = new();
        private int _currentOrganizationTypeIndex;

        #endregion

        #region Events

        public static event Action<bool> OnOrganizationTypeChangeBlocked;
        public static event Action OnRequestForcedOrganizationType;

        #endregion

        private void OnEnable()
        {
            SubscribeToEvents();
            LockNextStageButton();
            StartCheckingOrganizationType();
        }

        private void Start()
        {
            //StartCheckingOrganizationType();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        
        private void SubscribeToEvents()
        {
            OrganizationTypeChangeButton.OnNextOrganizationTypeButtonClicked += GetNextOrganizationType;
            OrganizationTypeChangeButton.OnPreviousOrganizationTypeButtonClicked += GetPreviousOrganizationType;
            organizationNameInputField.onValueChanged.AddListener(CheckStageRequirements);
        }

        private void UnsubscribeFromEvents()
        {
            OrganizationTypeChangeButton.OnNextOrganizationTypeButtonClicked -= GetNextOrganizationType;
            OrganizationTypeChangeButton.OnPreviousOrganizationTypeButtonClicked -= GetPreviousOrganizationType;
            organizationNameInputField.onValueChanged.RemoveListener(CheckStageRequirements);
        }

        #region OrganizationTypesSelection

        public void InitializeOrganizationTypeList(List<OrganizationType> organizationTypes)
        {
            Debug.Log(organizationTypes.Count);
            _organizationTypes = organizationTypes;
            organizationTypeName.text = _organizationTypes[_currentOrganizationTypeIndex].typeName;
        }

        private void GetNextOrganizationType()
        {
            _currentOrganizationTypeIndex++;
            Debug.Log(_currentOrganizationTypeIndex);
            if (_currentOrganizationTypeIndex >= _organizationTypes.Count) _currentOrganizationTypeIndex = 0;
            organizationTypeName.text = _organizationTypes[_currentOrganizationTypeIndex].typeName;
        }
        private void GetPreviousOrganizationType()
        {
            _currentOrganizationTypeIndex--;
            Debug.Log(_currentOrganizationTypeIndex);
            if (_currentOrganizationTypeIndex < 0) _currentOrganizationTypeIndex = _organizationTypes.Count - 1;
            organizationTypeName.text = _organizationTypes[_currentOrganizationTypeIndex].typeName;
        }

        private void StartCheckingOrganizationType()
        {
            StartCoroutine(CheckIfOrganizationTypeIsForced());
        }

        private IEnumerator CheckIfOrganizationTypeIsForced()
        {
            var buttonsInitialized = false;
            
            Action<string> onTypeButtonsInitialization = receivedTypeName =>
            {
                buttonsInitialized = true;
                if (!string.IsNullOrEmpty(receivedTypeName) && !string.IsNullOrWhiteSpace(receivedTypeName))
                {
                    var typeIndex = _organizationTypes.FindIndex(type => type.typeName == receivedTypeName);
                    _currentOrganizationTypeIndex = typeIndex;
                    organizationTypeName.text = _organizationTypes[_currentOrganizationTypeIndex].typeName;
                    OnOrganizationTypeChangeBlocked?.Invoke(true);
                }
                else
                {
                    OnOrganizationTypeChangeBlocked?.Invoke(false);
                }
            }; 
            
            NewGamePanelController.OnPassForcedOrganizationType += onTypeButtonsInitialization;
            OnRequestForcedOrganizationType?.Invoke();
            
            yield return new WaitUntil(() => buttonsInitialized);
            
            NewGamePanelController.OnPassForcedOrganizationType -= onTypeButtonsInitialization;
        }

        #endregion
        private void LockNextStageButton()
        {
            nextStageButton.interactable = false;
        }
        private void UnlockNextStageButton()
        {
            nextStageButton.interactable = true;
        }
        private void CheckStageRequirements(string arg0)
        {
            if (!string.IsNullOrEmpty(organizationNameInputField.text) && !string.IsNullOrWhiteSpace(organizationNameInputField.text))
            {
                UnlockNextStageButton();
            }
            else
            {
                LockNextStageButton();
            }
        }
    }
}