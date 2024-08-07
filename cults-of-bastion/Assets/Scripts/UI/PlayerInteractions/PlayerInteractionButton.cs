using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PlayerInteractions
{
    public class PlayerInteractionButton : MonoBehaviour
    {
        private string _actionName;
        public static event Action<string> OnActionInvoked;
        
        public void InitializeInteractionButton(string actionName)
        {
            _actionName = actionName;
            GetComponentInChildren<TextMeshProUGUI>().text = actionName;
        }
        public void InvokeAction()
        {
            OnActionInvoked?.Invoke(_actionName);
        }
    }
}