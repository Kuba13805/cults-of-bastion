using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Locations;
using Managers;
using PlayerInteractions.LocationActions;
using PlayerResources;
using UI;
using UI.PlayerInteractions;
using UnityEngine;

namespace PlayerInteractions
{
    [RequireComponent(typeof(ActionConditionVerifier))]
    public class PlayerActionsController : MonoBehaviour
    {
        private ActionsData _actionsData;
        private ActionConditionVerifier _actionConditionVerifier;

        private readonly Dictionary<string, LocationAction> _locationActionDict = new();
        private List<BaseAction> _currentTimeBasedActionList = new();
        private List<BaseAction> _currentTimeBasedNonLimitedList= new();

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
            PlayerInteractionsContentController.OnInvokeActionExecution += InitializeActionCreation;
        }


        private void UnsubscribeFromEvents()
        {
            LocationManager.OnPassLocationData -= StartActionListCoroutine;
            UIController.OnRequestAllPlayerActions -= PassAllPlayerActions;
            TimeManager.OnHourChanged -= DecreaseActionsDuration;
            TimeManager.OnWeekCycle -= ExecuteTimeBasedNonLimitedActions;
            PlayerInteractionsContentController.OnInvokeActionExecution -= InitializeActionCreation;
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
                foreach (var newType in locationAction.actionTypes.Select(CreateActionTypes))
                {
                    if(!VerifyActionTypeConditions(newType, newAction)) continue;
                    newAction.actionTypes.Add(newType);
                }

                foreach (var newCondition in locationAction.conditions.Select(CreateActionCondition))
                {
                    newAction.ActionConditions.Add(newCondition);
                }
                foreach (var newEffect in locationAction.effects.Select(CreateActionEffect))
                {
                    newAction.ActionEffects.Add(newEffect);
                }
                foreach (var newCost in locationAction.costs.Select(CreateActionEffect))
                {
                    newAction.ActionCosts.Add(newCost);
                }

                _locationActionDict.Add(newAction.actionName, newAction);
                Debug.Log($"New action added: {newAction.actionName} to dictionary.");
            }
        }

        private static ActionTypes CreateActionTypes(string definedType)
        {
            if (!Enum.TryParse(definedType, out ActionTypes actionType))
            {
                throw new ArgumentException("Invalid action type", nameof(definedType));
            }

            Debug.Log($"New action type added: {actionType}");
            switch (actionType)
            {
                case ActionTypes.Personal:
                    return ActionTypes.Personal;
                case ActionTypes.Organization:
                    return ActionTypes.Organization;
                case ActionTypes.Immediate:
                    return ActionTypes.Immediate;
                case ActionTypes.TimeBased:
                    return ActionTypes.TimeBased;
                case ActionTypes.TimeBasedNonLimited:
                    return ActionTypes.TimeBasedNonLimited;
                case ActionTypes.Illegal:
                    return ActionTypes.Illegal;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }

        private bool VerifyActionTypeConditions(ActionTypes type, BaseAction action)
        {
            switch (type)
            {
                case ActionTypes.Personal:
                    if (action.actionTypes.Any(previousType => previousType == ActionTypes.Organization))
                    {
                        return false;
                    }
                    break;
                case ActionTypes.Organization:
                    if (action.actionTypes.Any(previousType => previousType == ActionTypes.Personal))
                    {
                        return false;
                    }
                    break;
                case ActionTypes.Immediate:
                    if (action.actionTypes.Any(previousType => previousType is ActionTypes.TimeBased or ActionTypes.TimeBasedNonLimited))
                    {
                        return false;
                    }
                    break;
                case ActionTypes.TimeBased:
                    if (action.actionTypes.Any(previousType => previousType is ActionTypes.Immediate or ActionTypes.TimeBasedNonLimited))
                    {
                        return false;
                    }
                    break;
                case ActionTypes.TimeBasedNonLimited:
                    if (action.actionTypes.Any(previousType => previousType is ActionTypes.TimeBased or ActionTypes.Immediate))
                    {
                        return false;
                    }
                    break;
            }
            return true;
        }

        private static ActionCondition CreateActionCondition(string definedCondition)
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

        private static ActionEffect CreateActionEffect(string definedEffect)
        {
            var parsedEffectDefinition = definedEffect.Split(" ");
            
            var effect = new ActionEffect();
            
            if (!Enum.TryParse(parsedEffectDefinition[0], out ActionEffects actionEffect))
            {
                throw new ArgumentException("Invalid action effect", nameof(parsedEffectDefinition));
            }

            switch (actionEffect)
            {
                case ActionEffects.AddMoney:
                    effect.Effect = ActionEffects.AddMoney;
                    effect.Value = int.Parse(parsedEffectDefinition[2]);
                    break;
                case ActionEffects.RemoveMoney:
                    effect.Effect = ActionEffects.RemoveMoney;
                    effect.Value = int.Parse(parsedEffectDefinition[2]);
                    break;
                case ActionEffects.AddInfluence:
                    effect.Effect = ActionEffects.AddInfluence;
                    effect.Value = int.Parse(parsedEffectDefinition[2]);
                    break;
                case ActionEffects.RemoveInfluence:
                    effect.Effect = ActionEffects.RemoveInfluence;
                    effect.Value = int.Parse(parsedEffectDefinition[2]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionEffect), actionEffect, null);
            }
            Debug.Log($"New effect added to action: {effect.Effect} with value: {effect.Value}");
            return effect;
        }

        #endregion

        #region PlayerActionsExecution

        private void InitializeActionCreation(string actionName)
        {
            var tempChar = new Character();
            CreateInvokedAction(tempChar, actionName, new LocationData());
        }
        private void RegisterTimeBasedAction(BaseAction action)
        {
            if(_currentTimeBasedActionList.Count == 0) TimeManager.OnHourChanged += DecreaseActionsDuration;
            _currentTimeBasedActionList.Add(action);
        }

        private void RegisterTimeBasedNonLimitedAction(BaseAction action)
        {
            if (_currentTimeBasedNonLimitedList.Count == 0)
                TimeManager.OnWeekCycle += ExecuteTimeBasedNonLimitedActions;
            _currentTimeBasedNonLimitedList.Add(action);
        }

        private void CancelActionExecuting(BaseAction canceledAction)
        {
            Debug.Log($"Action canceled: {canceledAction.actionName}");
            canceledAction.actionInvoker.CurrentAction = null;
            if (_currentTimeBasedActionList.Contains(canceledAction))
            {
                _currentTimeBasedActionList.Remove(canceledAction);
            }

            if (_currentTimeBasedNonLimitedList.Contains(canceledAction))
            {
                _currentTimeBasedNonLimitedList.Remove(canceledAction);
            }
            
            if(_currentTimeBasedActionList.Count == 0) TimeManager.OnHourChanged -= DecreaseActionsDuration;
            if (_currentTimeBasedNonLimitedList.Count == 0)
                TimeManager.OnWeekCycle -= ExecuteTimeBasedNonLimitedActions;
        }

        private void DecreaseActionsDuration(float obj)
        {
            var tempActionList = new List<BaseAction>();
            foreach (var timeBasedAction in _currentTimeBasedActionList)
            {
                timeBasedAction.actionDuration -= 1;
                if (timeBasedAction.actionDuration > 0) continue;
                
                ApplyActionEffects(timeBasedAction);
                timeBasedAction.actionInvoker.CurrentAction = null;
                Debug.Log($"Action completed: {timeBasedAction.actionName}.");
                tempActionList.Add(timeBasedAction);
            }
            foreach (var action in tempActionList)
            {
                CancelActionExecuting(action);
            }
        }
        private void ExecuteTimeBasedNonLimitedActions()
        {
            StartCoroutine(VerifyTimeBasedNonLimitedActionsConditions());
        }

        private IEnumerator VerifyTimeBasedNonLimitedActionsConditions()
        {
            var tempActionList = new List<BaseAction>();
            foreach (var nonLimitedAction in _currentTimeBasedNonLimitedList)
            {
                ApplyActionEffects(nonLimitedAction);
                yield return StartCoroutine(VerifyAction(nonLimitedAction, result => 
                {
                    _ = result;
                    nonLimitedAction.isActionPossible = result;
                }, new LocationData()));

                if (!nonLimitedAction.isActionPossible)
                {
                    tempActionList.Add(nonLimitedAction);
                }
            }

            foreach (var action in tempActionList)
            {
                CancelActionExecuting(action);
            }
        }

        /// <summary>
        /// Used to execute location actions
        /// </summary>
        /// <param name="actionInvoker"></param>
        /// <param name="actionName"></param>
        /// <param name="locationData"></param>
        private void CreateInvokedAction(Character actionInvoker, string actionName, params LocationData[] locationData)
        {
            var newAction = new LocationAction
            {
                actionName = _locationActionDict[actionName].actionName,
                actionDuration = _locationActionDict[actionName].actionDuration,
                actionDescription = _locationActionDict[actionName].actionDescription,
                actionTypes = _locationActionDict[actionName].actionTypes,
                ActionConditions = _locationActionDict[actionName].ActionConditions,
                ActionEffects = _locationActionDict[actionName].ActionEffects,
                ActionCosts = _locationActionDict[actionName].ActionCosts,
                targetLocation = locationData[0],
                actionInvoker = actionInvoker,
                isActionPossible = true
            };
            actionInvoker.CurrentAction = newAction;
            ApplyActionCosts(newAction);
            VerifyNewActionTypes(newAction);
        }
        /// <summary>
        /// Used to execute character actions
        /// </summary>
        /// <param name="actionInvoker"></param>
        /// <param name="actionName"></param>
        /// <param name="targetCharacters"></param>
        private void CreateInvokedAction(Character actionInvoker, string actionName, params Character[] targetCharacters)
        {
            
        }
        private void VerifyNewActionTypes(BaseAction newAction)
        {
            foreach (var type in newAction.actionTypes)
            {
                switch (type)
                {
                    case ActionTypes.Immediate:
                        ApplyActionEffects(newAction);
                        break;
                    case ActionTypes.TimeBased:
                        RegisterTimeBasedAction(newAction);
                        Debug.Log($"New time based action added: {newAction.actionName}");
                        break;
                    case ActionTypes.TimeBasedNonLimited:
                        RegisterTimeBasedNonLimitedAction(newAction);
                        Debug.Log($"New time based non limited action added: {newAction.actionName}");
                        break;
                }
            }
        }

        private void ApplyActionCosts(BaseAction newAction)
        {
            foreach (var cost in newAction.ActionCosts)
            {
                ApplyEffect(cost);
                Debug.Log($"Action cost applied: {cost.Effect} with value {cost.Value}");
            }
        }
        private void ApplyActionEffects(BaseAction newAction)
        {
            foreach (var effect in newAction.ActionEffects)
            {
                ApplyEffect(effect);
                Debug.Log($"Action effect applied: {effect.Effect} with value {effect.Value}");
            }
        }

        private void ApplyEffect(ActionEffect effect)
        {
            switch (effect.Effect)
            {
                case ActionEffects.AddInfluence:
                    ResourceChanger.ModifyInfluence(effect.Value);
                    break;
                case ActionEffects.RemoveInfluence:
                    ResourceChanger.ModifyInfluence(-effect.Value);
                    break;
                case ActionEffects.AddMoney:
                    ResourceChanger.ModifyMoney(effect.Value);
                    break;
                case ActionEffects.RemoveMoney:
                    ResourceChanger.ModifyMoney(-effect.Value);
                    break;
                case ActionEffects.IncreaseCharacterStat:
                    break;
                case ActionEffects.DecreaseCharacterStat:
                    break;
                case ActionEffects.ChanceToIncreaseCharacterStat:
                    break;
                case ActionEffects.ChanceToDecreaseCharacterStat:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }

    public class ActionsData
    {
        public List<LocationActionConstructor> LocationActionConstructors = new();
    }
}
