using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.GenericConfirmation
{
    public class ConfirmationPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI messageText;
        
        public static event Action<bool> OnConfirmation;
        
        private void Start()
        {
            ConfirmationRequester.OnRequestConfirmation += ShowPanel;
        }

        private void OnDestroy()
        {
            ConfirmationRequester.OnRequestConfirmation -= ShowPanel;
        }

        private void ShowPanel()
        {
            
        }

        private void Confirm()
        {
            OnConfirmation?.Invoke(true);
            panel.SetActive(false);
            
        }

        private void Cancel()
        {
            OnConfirmation?.Invoke(false);
            panel.SetActive(false);
        }
    }
}
