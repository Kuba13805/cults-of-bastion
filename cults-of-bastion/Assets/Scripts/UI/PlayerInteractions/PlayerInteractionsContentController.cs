using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayerInteractions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PlayerInteractions
{
    public class PlayerInteractionsContentController : MonoBehaviour
    {
        [SerializeField] private Button actionButtonPrefab;
        [SerializeField] private Transform actionButtonParent;
        private List<BaseAction> _actionList = new();
        private readonly Dictionary<string, GameObject> _actionButtonList = new();
        
        [SerializeField] private float xOffset;
        [SerializeField] private float yOffset;
    
        public static event Action OnGetAllPlayerActions;
        public static event Action<string> OnInvokeActionExecution; 

        private void Start()
        {
            StartCoroutine(GetAllPlayerActions());
            PlayerActionsController.OnPassPossiblePlayerActions += ActivateLocationActionButtons;
            PlayerInteractionButton.OnActionInvoked += InvokeActionExecution;
        }

        private void OnDestroy()
        {
            PlayerActionsController.OnPassPossiblePlayerActions -= ActivateLocationActionButtons;
            PlayerInteractionButton.OnActionInvoked -= InvokeActionExecution;
        }
        private void InvokeActionExecution(string actionName)
        {
            ToggleActionPanel();
            OnInvokeActionExecution?.Invoke(actionName);
        }
    
        private void InstantiateActionButtons()
        {
            foreach (var action in _actionList)
            {
                InstantiateActionButton(action);
            }
        }
        private void InstantiateActionButton(BaseAction action)
        {
            var newButton = Instantiate(actionButtonPrefab, actionButtonParent);
            newButton.GetComponent<PlayerInteractionButton>().InitializeInteractionButton(action.actionName);
        
            _actionButtonList.Add(action.actionName, newButton.gameObject);
        }

        private void ActivateLocationActionButtons(List<string> listOfPossibleActions)
        {
            DeactivateActionButtons();
            foreach (var action in from action in _actionButtonList from actionName 
                         in listOfPossibleActions.Where(actionName => action.Key == actionName) select action)
            {
                action.Value.SetActive(true);
            }
            
            ToggleActionPanel();
        }
        private void DeactivateActionButtons()
        {
            foreach (var button in actionButtonParent.GetComponentsInChildren<Button>())
            {
                button.gameObject.SetActive(false);
            }
        }

        private void ToggleActionPanel()
        {
            var mousePosition = Input.mousePosition;
            
            mousePosition.x += xOffset;
            mousePosition.y += yOffset;
            
            actionButtonParent.position = mousePosition;
            
            actionButtonParent.gameObject.SetActive(actionButtonParent.gameObject.activeSelf == false);
        }
    
        private IEnumerator GetAllPlayerActions()
        {
            yield return null;
            var actionsLoaded = false;

            UIController.OnPassAllPlayerActions += OnGetAllActions;
        
            OnGetAllPlayerActions?.Invoke();
        
            yield return new WaitUntil(() => actionsLoaded);
            UIController.OnPassAllPlayerActions -= OnGetAllActions;
            InstantiateActionButtons();
        
            DeactivateActionButtons();
        
            yield break;

            void OnGetAllActions(List<BaseAction> baseActions)
            {
                _actionList = baseActions;
                actionsLoaded = true;
            }
        }
    }
}
