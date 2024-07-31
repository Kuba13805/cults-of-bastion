using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayerInteractions;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractionsPanel : MonoBehaviour
{
    [SerializeField] private Button actionButtonPrefab;
    [SerializeField] private Transform actionButtonParent;
    private List<BaseAction> _actionList = new();
    private Dictionary<string, GameObject> _actionButtonList = new();
    
    public static event Action OnGetAllPlayerActions;

    private void Start()
    {
        StartCoroutine(GetAllPlayerActions());
        PlayerActionsController.OnPassPossiblePlayerActions += ActivateLocationActionButtons;
    }

    private void OnDestroy()
    {
        PlayerActionsController.OnPassPossiblePlayerActions -= ActivateLocationActionButtons;
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
        newButton.GetComponentInChildren<TextMeshProUGUI>().text = action.actionName;
        
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
    }
    private void DeactivateActionButtons()
    {
        foreach (var button in actionButtonParent.GetComponentsInChildren<Button>())
        {
            button.gameObject.SetActive(false);
        }
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
