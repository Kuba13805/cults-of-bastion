using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu.NewGameMenu.OrganizationCreation
{
    public class OrganizationTypeChangeButton : MonoBehaviour
    {
        [SerializeField] private bool invokePreviousOrganizationType;
        
        public static event Action OnNextOrganizationTypeButtonClicked;
        public static event Action OnPreviousOrganizationTypeButtonClicked;

        private void OnEnable()
        {
            if (invokePreviousOrganizationType)
            {
                GetComponent<Button>().onClick.AddListener(RequestPreviousOrganizationType);
            }
            else
            {
                GetComponent<Button>().onClick.AddListener(RequestNextOrganizationType);
            }
            
        }
        private void OnDisable()
        {
            GetComponent<Button>().onClick.RemoveAllListeners(); 
            
        }

        private void BlockOrganizationTypeChange(bool b)
        {
            GetComponent<Button>().interactable = !b;
            Debug.Log($"Organization type change blocked: {b}");
        }
        private static void RequestNextOrganizationType() => OnNextOrganizationTypeButtonClicked?.Invoke();
        private static void RequestPreviousOrganizationType() => OnPreviousOrganizationTypeButtonClicked?.Invoke();
    }
}