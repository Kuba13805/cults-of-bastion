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

        

        #endregion

        private void OnEnable()
        {
            SubscribeToEvents();
            
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        
        private void SubscribeToEvents()
        {
            
        }

        private void UnsubscribeFromEvents()
        {
            
        }
    }
}