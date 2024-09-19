using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NewGame;
using Organizations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu.NewGameMenu.OrganizationCreation
{
    public class OrganizationPanelController : StagePanelController
    {
        #region Variables

        [SerializeField] private TMP_InputField organizationNameInputField;
        [SerializeField] private TextMeshProUGUI organizationTypeName;
        [SerializeField] private Button nextStageButton;
        
        private readonly Dictionary<string, OrganizationType> _organizationTypeDictionary = new();
        private int _currentOrganizationTypeIndex;

        #endregion

        #region Events

        public static event Action<bool> OnLockTypeButtons;

        #endregion

        protected override void Start()
        {
            base.Start();
            UnsubscribeFromEvents();
            SubscribeToEvents();
            CheckNameInputField(organizationNameInputField.text);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            NewGameController.OnPassOrganizationTypes += LoadOrganizationTypes;
            OrganizationTypeChangeButton.OnNextOrganizationTypeButtonClicked += GetNextType;
            OrganizationTypeChangeButton.OnPreviousOrganizationTypeButtonClicked += GetPreviousType;
            organizationNameInputField.onValueChanged.AddListener(CheckNameInputField);
            GameCreationStagesController.OnForceOrganizationType += ForceOrganizationType;
            GameCreationStagesController.OnReleaseOrganizationType += ReleaseOrganizationType;
        }

        private void UnsubscribeFromEvents()
        {
            NewGameController.OnPassOrganizationTypes -= LoadOrganizationTypes;
            OrganizationTypeChangeButton.OnNextOrganizationTypeButtonClicked -= GetNextType;
            OrganizationTypeChangeButton.OnPreviousOrganizationTypeButtonClicked -= GetPreviousType;
            organizationNameInputField.onValueChanged.RemoveAllListeners();
            GameCreationStagesController.OnForceOrganizationType -= ForceOrganizationType;
            GameCreationStagesController.OnReleaseOrganizationType -= ReleaseOrganizationType;
        }

        private void LoadOrganizationTypes(List<OrganizationType> types)
        {
            _organizationTypeDictionary.Clear();
            foreach (var type in types.Where(type => !_organizationTypeDictionary.ContainsKey(type.typeName)))
            {
                _organizationTypeDictionary.Add(type.typeName, type);
            }
            UpdateSelectedType(_organizationTypeDictionary.Values.ToList()[0].typeName);
        }

        private void UpdateSelectedType(string typeName)
        {
            organizationTypeName.text = _organizationTypeDictionary.GetValueOrDefault(typeName).typeName;
        }

        private void GetNextType()
        {
            _currentOrganizationTypeIndex++;
            if (_currentOrganizationTypeIndex >= _organizationTypeDictionary.Count)
            {
                _currentOrganizationTypeIndex = 0;
            }
            UpdateSelectedType(_organizationTypeDictionary.Values.ToList()[_currentOrganizationTypeIndex].typeName);
        }
        private void GetPreviousType()
        {
            _currentOrganizationTypeIndex--;
            if (_currentOrganizationTypeIndex < 0)
            {
                _currentOrganizationTypeIndex = _organizationTypeDictionary.Count - 1;
            }
            UpdateSelectedType(_organizationTypeDictionary.Values.ToList()[_currentOrganizationTypeIndex].typeName);
        }

        private void ForceOrganizationType(string typeName)
        {
            if (!_organizationTypeDictionary.TryGetValue(typeName, out var organizationType)) return;
            StartCoroutine(WaitForContentActivation(organizationType));
        }

        private IEnumerator WaitForContentActivation(OrganizationType organizationType)
        {
            yield return new WaitUntil(() => panelContent.activeSelf);
            UpdateSelectedType(organizationType.typeName);
            OnLockTypeButtons?.Invoke(true);
        }

        private static void ReleaseOrganizationType() => OnLockTypeButtons?.Invoke(false);

        private void CheckNameInputField(string organizationName)
        {
            nextStageButton.interactable = !string.IsNullOrEmpty(organizationName) && !string.IsNullOrWhiteSpace(organizationName);
        }
    }
}