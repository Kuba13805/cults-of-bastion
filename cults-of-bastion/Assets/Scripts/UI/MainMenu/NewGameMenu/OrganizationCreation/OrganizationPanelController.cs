using System;
using System.Collections.Generic;
using Organizations;
using TMPro;
using UnityEngine;

namespace UI.MainMenu.NewGameMenu.OrganizationCreation
{
    public class OrganizationPanelController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField organizationNameInputField;
        [SerializeField] private TextMeshProUGUI organizationTypeName;
        
        private List<OrganizationType> _organizationTypes = new();
        private int _currentOrganizationTypeIndex;

        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void SubscribeToEvents()
        {
            OrganizationTypeChangeButton.OnNextOrganizationTypeButtonClicked += GetNextOrganizationType;
            OrganizationTypeChangeButton.OnPreviousOrganizationTypeButtonClicked += GetPreviousOrganizationType;
        }

        private void UnsubscribeFromEvents()
        {
            OrganizationTypeChangeButton.OnNextOrganizationTypeButtonClicked -= GetNextOrganizationType;
            OrganizationTypeChangeButton.OnPreviousOrganizationTypeButtonClicked -= GetPreviousOrganizationType;
        }

        public void InitializeOrganizationList(List<OrganizationType> organizationTypes)
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
    }
}