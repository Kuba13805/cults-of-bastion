using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Locations;
using Managers;
using PlayerInteractions.LocationActions;
using UI;
using UnityEngine;

namespace PlayerInteractions
{
    [RequireComponent(typeof(ActionConditionVerifier))]
    public class PlayerActionsController : MonoBehaviour
    {
        private ActionsData _actionsData;
        private ActionConditionVerifier _actionConditionVerifier;

        private readonly Dictionary<string, LocationAction> _locationActionDict = new();

        #region Events

        public static event Action<List<string>> OnPassPossiblePlayerActions;
        public static event Action<List<BaseAction>> OnPassAllPlayerActions; 

        #endregion
        private void Awake()
        {
            SubscribeToEvents();
            LoadActions();
        }

        private void Start()
        {
            _actionConditionVerifier = GetComponent<ActionConditionVerifier>();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            LocationManager.OnPassLocationData += StartActionListCoroutine;
            UIController.OnRequestAllPlayerActions += PassAllPlayerActions;
        }


        private void UnsubscribeFromEvents()
        {
            LocationManager.OnPassLocationData -= StartActionListCoroutine;
            UIController.OnRequestAllPlayerActions -= PassAllPlayerActions;
        }

        #region PlayerActionsHandling
        private void PassAllPlayerActions() => OnPassAllPlayerActions?.Invoke(new List<BaseAction>(_locationActionDict.Values.ToList()));

        private void StartActionListCoroutine(LocationData locationData) => StartCoroutine(CreateLocationActionsList(locationData));

        private IEnumerator CreateLocationActionsList(params LocationData[] locationData)
        {
            var possibleActionList = new List<string>();
            var tempActionList = _locationActionDict.Values.ToList();
            foreach (var locationAction in tempActionList)
            {
                yield return StartCoroutine(VerifyAction(locationAction, result => 
                {
                    _ = result;
                    locationAction.isActionPossible = result;
                    Debug.Log($"Action {locationAction.actionName} is possible: {result}");
                }, locationData[0]));

                if (locationAction.isActionPossible)
                {
                    possibleActionList.Add(locationAction.actionName);
                }
            }
            OnPassPossiblePlayerActions?.Invoke(possibleActionList);
        }

        private IEnumerator VerifyAction(BaseAction locationAction, Action<bool> callback, params LocationData[] locationData)
        {
            bool allConditionsMet = true;
            foreach (var condition in locationAction.ActionConditions)
            {
                bool conditionMet = false;
                yield return StartCoroutine(VerifyConditionWrapper(condition, locationData[0], result => conditionMet = result));

                if (!conditionMet)
                {
                    allConditionsMet = false;
                    break;
                }
            }
            callback(allConditionsMet);
        }

        private IEnumerator VerifyConditionWrapper(ActionCondition condition, LocationData locationData, Action<bool> callback)
        {
            bool conditionMet = false;
            _actionConditionVerifier.Verify(new List<ActionCondition> { condition }, result => conditionMet = result, locationData);
            yield return new WaitUntil(() => conditionMet == true || conditionMet == false);
            callback(conditionMet);
        }

        #endregion

        #region PlayerActionsCreation

        private void LoadActions()
        {
            var actionsConfig = Resources.Load<TextAsset>("DataToLoad/testActions");
            if (actionsConfig == null) return;
            
            var parsedActionsConfigData = JsonUtility.FromJson<ActionsData>(actionsConfig.text);
            if (parsedActionsConfigData == null)
            {
                throw new Exception("Failed to parse actions config data.");
            }
            
            _actionsData = new ActionsData
            {
                LocationActionConstructors = parsedActionsConfigData.LocationActionConstructors
            };

            InitializeLocationActions();
        }

        private void InitializeLocationActions()
        {
            foreach (var locationAction in _actionsData.LocationActionConstructors)
            {
                var newAction = new LocationAction
                {
                    actionName = locationAction.name,
                    actionDuration = locationAction.duration,
                    actionDescription = locationAction.description,
                };

                foreach (var newCondition in locationAction.conditions.Select(CreateActionCondition))
                {
                    newAction.ActionConditions.Add(newCondition);
                }

                _locationActionDict.Add(newAction.actionName, newAction);
                Debug.Log($"New action added: {newAction.actionName} to dictionary.");
            }
        }

        private ActionCondition CreateActionCondition(string definedCondition)
        {
            var parsedConditionDefinition = definedCondition.Split(" ");

            var condition = new ActionCondition();

            if (!Enum.TryParse(parsedConditionDefinition[0], out ActionConditions actionCondition))
            {
                throw new ArgumentException("Invalid action condition", nameof(definedCondition));
            }

            switch (actionCondition)
            {
                case ActionConditions.PlayerHasMoneyValue:
                    condition.Condition = ActionConditions.PlayerHasMoneyValue;
                    condition.Value = int.Parse(parsedConditionDefinition[2]);
                    break;
                case ActionConditions.PlayerHasInfluenceValue:
                    condition.Condition = ActionConditions.PlayerHasInfluenceValue;
                    condition.Value = int.Parse(parsedConditionDefinition[2]);
                    break;
                case ActionConditions.TargetLocationType:
                    condition.Condition = ActionConditions.TargetLocationType;
                    condition.StringValue = parsedConditionDefinition[2];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionCondition), actionCondition, null);
            }
            Debug.Log($"New condition added to action: {condition.Condition} with value: {condition.Value} - {condition.StringValue}");
            return condition;
        }

        #endregion
    }

    public class ActionsData
    {
        public List<LocationAction> LocationActions = new();
        public List<LocationActionConstructor> LocationActionConstructors = new();
    }
}
