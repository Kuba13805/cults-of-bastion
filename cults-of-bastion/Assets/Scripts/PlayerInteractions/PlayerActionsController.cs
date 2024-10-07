using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private LocationData _interactedLocation;

        private readonly Dictionary<string, LocationAction> _locationActionDict = new();
        private readonly List<BaseAction> _indicatorBasedActions = new();
        private readonly List<BaseAction> _repeatableActions= new();

        #region Events

        public static event Action<List<string>> OnPassPossiblePlayerActions;
        public static event Action<List<BaseAction>> OnPassAllPlayerActions;
        public static event Action OnRequestCharacterSelectionForAction;
        public static event Action OnRequestPlayerCharacterForAction;

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
            LocationManager.OnPassLocationDataOnInteraction += StartActionListCoroutine;
            UIController.OnRequestAllPlayerActions += PassAllPlayerActions;
            PlayerInteractionsContentController.OnInvokeActionExecution += InitializeActionCreation;
        }


        private void UnsubscribeFromEvents()
        {
            LocationManager.OnPassLocationDataOnInteraction -= StartActionListCoroutine;
            UIController.OnRequestAllPlayerActions -= PassAllPlayerActions;
            TimeManager.OnHourChanged -= HandleProgressionActions;
            TimeManager.OnHourChanged -= ExecuteRepeatableActions;
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

            _interactedLocation = locationData[0];
            OnPassPossiblePlayerActions?.Invoke(possibleActionList);
        }

        private IEnumerator VerifyAction(BaseAction action, Action<bool> callback, object targetObjects)
        {
            bool allConditionsMet = true;
            foreach (var condition in action.ActionConditions)
            {
                bool conditionMet = false;
                yield return StartCoroutine(VerifyConditionWrapper(condition, targetObjects, result => conditionMet = result));

                if (!conditionMet)
                {
                    allConditionsMet = false;
                    break;
                }
            }
            callback(allConditionsMet);
        }

        private IEnumerator VerifyConditionWrapper(ActionCondition condition, object targetObject, Action<bool> callback)
        {
            bool conditionMet = false;
            _actionConditionVerifier.Verify(new List<ActionCondition> { condition }, result => conditionMet = result, targetObject);
            yield return new WaitUntil(() => conditionMet || conditionMet == false);
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
                    actionProgressIndicator = locationAction.progressIndicator,
                    actionFixedProgression = locationAction.fixedProgression,
                    targetNumber = locationAction.targetNumber,
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
            }
        }

        private static ActionTypes CreateActionTypes(string definedType)
        {
            if (!Enum.TryParse(definedType, out ActionTypes actionType))
            {
                throw new ArgumentException("Invalid action type", nameof(definedType));
            }
            
            switch (actionType)
            {
                case ActionTypes.Personal:
                    return ActionTypes.Personal;
                case ActionTypes.Organization:
                    return ActionTypes.Organization;
                case ActionTypes.Immediate:
                    return ActionTypes.Immediate;
                case ActionTypes.Indicator:
                    return ActionTypes.Indicator;
                case ActionTypes.Repeatable:
                    return ActionTypes.Repeatable;
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
                    if (action.actionTypes.Any(previousType => previousType is ActionTypes.Indicator or ActionTypes.Repeatable))
                    {
                        return false;
                    }
                    break;
                case ActionTypes.Indicator:
                    if (action.actionTypes.Any(previousType => previousType is ActionTypes.Immediate or ActionTypes.Repeatable))
                    {
                        return false;
                    }
                    break;
                case ActionTypes.Repeatable:
                    if (action.actionTypes.Any(previousType => previousType is ActionTypes.Indicator or ActionTypes.Immediate))
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
            return effect;
        }

        #endregion

        #region PlayerActionsExecution

        private void InitializeActionCreation(string actionName)
        {
            CreateInvokedAction(actionName, _interactedLocation);
        }
        private void RegisterIndicatorBasedAction(BaseAction action)
        {
            if(_indicatorBasedActions.Count == 0) TimeManager.OnHourChanged += HandleProgressionActions;
            _indicatorBasedActions.Add(action);
        }

        private void RegisterRepeatableAction(BaseAction action)
        {
            if (_repeatableActions.Count == 0)
                TimeManager.OnHourChanged += ExecuteRepeatableActions;
            _repeatableActions.Add(action);
        }

        private void CancelActionExecuting(BaseAction canceledAction)
        {
            Debug.Log($"Action canceled: {canceledAction.actionName}");
            canceledAction.actionInvoker.CurrentAction = null;
            if (_indicatorBasedActions.Contains(canceledAction))
            {
                _indicatorBasedActions.Remove(canceledAction);
            }

            if (_repeatableActions.Contains(canceledAction))
            {
                _repeatableActions.Remove(canceledAction);
            }
            
            if(_indicatorBasedActions.Count == 0) TimeManager.OnHourChanged -= HandleProgressionActions;
            if (_repeatableActions.Count == 0)
                TimeManager.OnHourChanged -= ExecuteRepeatableActions;
        }

        private void HandleProgressionActions(float obj)
        {
            var tempActionList = new List<BaseAction>();
            foreach (var timeBasedAction in _indicatorBasedActions)
            {
                if (!IncreaseProgression(timeBasedAction)) continue;
                ApplyActionEffects(timeBasedAction);
                timeBasedAction.actionInvoker.CurrentAction = null;
                tempActionList.Add(timeBasedAction);
            }
            foreach (var action in tempActionList)
            {
                CancelActionExecuting(action);
            }
        }


        private void ExecuteRepeatableActions(float f)
        {
            StartCoroutine(VerifyRepeatableActionsConditions());
        }

        private IEnumerator VerifyRepeatableActionsConditions()
        {
            Debug.Log($"Verifying time based non limited actions");
            var actionsToCancel = new List<BaseAction>();
            var actionsToRepeat = new List<BaseAction>();
            foreach (var repeatableAction in _repeatableActions)
            {
                if (repeatableAction == null)
                {
                    Debug.LogWarning($"Skipping null repeatable action");
                    continue;
                }

                if (!IncreaseProgression(repeatableAction)) continue;
                ApplyActionEffects(repeatableAction);
                yield return StartCoroutine(VerifyAction(repeatableAction, result => 
                {
                    _ = result;
                    repeatableAction.isActionPossible = result;
                    Debug.Log($"Action verified: {repeatableAction.actionName} with result: {result}");
                }, repeatableAction.targetObject));

                if (!repeatableAction.isActionPossible)
                {
                    actionsToCancel.Add(repeatableAction);
                    Debug.Log($"Action canceled or no longer possible: {repeatableAction.actionName}");
                }
                else
                {
                    actionsToRepeat.Add(repeatableAction);
                    Debug.Log($"Action renewed: {repeatableAction.actionName}");
                }
            }

            foreach (var action in actionsToCancel)
            {
                CancelActionExecuting(action);
            }

            foreach (var action in actionsToRepeat)
            {
                RepeatAction(action);
            }
        }

        private void RepeatAction(BaseAction repeatableAction)
        {
            repeatableAction.actionCurrentProgression = 0;
            ApplyActionCosts(repeatableAction);
        }
        private bool IncreaseProgression(BaseAction timeBasedAction)
        {
            if (timeBasedAction == null)
            {
                Debug.LogError("timeBasedAction is null!");
                return false;
            }

            if (timeBasedAction.actionInvoker == null)
            {
                Debug.LogError("actionInvoker is null in action: " + timeBasedAction.actionName);
                return false;
            }

            if (timeBasedAction.actionInvoker.CharacterIndicators == null)
            {
                Debug.LogError("CharacterIndicators is null in action: " + timeBasedAction.actionName);
                return false;
            }

            timeBasedAction.actionCalculatedProgression = timeBasedAction.actionFixedProgression +
                                                          timeBasedAction.actionInvoker.CharacterIndicators
                                                              .BaseActionProgressIndicator;

            timeBasedAction.actionCurrentProgression += timeBasedAction.actionCalculatedProgression;

            Debug.Log($"Action progression increased: {timeBasedAction.actionName} with progression: {timeBasedAction.actionCurrentProgression} out of {timeBasedAction.actionProgressIndicator}");
            return !(timeBasedAction.actionProgressIndicator > timeBasedAction.actionCurrentProgression);
        }


        private static IEnumerator HandlePersonalAction(BaseAction newAction, Action<bool> callback)
    {
        var playerCharacterReceived = false;
        var isActionCancelled = false;

        Action<Character> onAssignPlayerCharacter = character =>
        {
            newAction.actionInvoker = character;
            playerCharacterReceived = true;
        };
        Action onActionCancelled = () => isActionCancelled = true;

        CharacterManager.OnPassPlayerCharacter += onAssignPlayerCharacter;
        UIController.OnCancelActionInvoking += onActionCancelled;

        OnRequestPlayerCharacterForAction?.Invoke();

        yield return new WaitUntil(() => playerCharacterReceived || isActionCancelled);

        CharacterManager.OnPassPlayerCharacter -= onAssignPlayerCharacter;
        UIController.OnCancelActionInvoking -= onActionCancelled;

        if (isActionCancelled)
        {
            callback?.Invoke(false);
        }
        else if (playerCharacterReceived && newAction.actionInvoker != null)
        {
            newAction.actionInvoker.CurrentAction = newAction;
            callback?.Invoke(true);
        }
        else
        {
            callback?.Invoke(false);
        }
    }

    private static IEnumerator HandleOrganizationAction(BaseAction newAction, Action<bool> callback)
    {
        var characterReceived = false;
        var isActionCancelled = false;

        Action<Character> onAssignCharacter = character =>
        {
            newAction.actionInvoker = character;
            characterReceived = true;
        };
        Action onActionCancelled = () => isActionCancelled = true;

        UIController.OnPassSelectedCharacterForAction += onAssignCharacter;
        UIController.OnCancelActionInvoking += onActionCancelled;

        OnRequestCharacterSelectionForAction?.Invoke();

        yield return new WaitUntil(() => characterReceived || isActionCancelled);

        UIController.OnPassSelectedCharacterForAction -= onAssignCharacter;
        UIController.OnCancelActionInvoking -= onActionCancelled;

        if (isActionCancelled || newAction.actionInvoker == null)
        {
            callback?.Invoke(false);
        }
        else if (characterReceived && newAction.actionInvoker != null)
        {
            newAction.actionInvoker.CurrentAction = newAction;
            callback?.Invoke(true);
        }
        else
        {
            callback?.Invoke(false);
        }
    }

        /// <summary>
        /// Used to execute location actions
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="locationData"></param>
        private void CreateInvokedAction(string actionName, params LocationData[] locationData)
        {
            var newAction = new LocationAction
            {
                actionName = _locationActionDict[actionName].actionName,
                actionDescription = _locationActionDict[actionName].actionDescription,
                actionProgressIndicator = _locationActionDict[actionName].actionProgressIndicator,
                actionFixedProgression = _locationActionDict[actionName].actionFixedProgression,
                actionTypes = _locationActionDict[actionName].actionTypes,
                ActionConditions = _locationActionDict[actionName].ActionConditions,
                ActionEffects = _locationActionDict[actionName].ActionEffects,
                ActionCosts = _locationActionDict[actionName].ActionCosts,
                targetObject = locationData[0],
                isActionPossible = true,
            };
            StartCoroutine(ExecuteActionTypeLogic(newAction));
        }
        /// <summary>
        /// Used to execute character actions
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="targetCharacters"></param>
        private void CreateInvokedAction(string actionName, params Character[] targetCharacters)
        {
            
        }
        private IEnumerator ExecuteActionTypeLogic(BaseAction newAction)
        {
            // Run the character-based logic as a coroutine and wait for its completion
            yield return StartCoroutine(ExecuteCharacterBasedTypeLogic(newAction));

            // Check if the character-based logic was successful
            if (!newAction.isActionPossible)
            {
                Debug.Log("Action aborted due to character restrictions or action not possible. Action name: " + newAction.actionName);
                _indicatorBasedActions.Remove(newAction);
                _repeatableActions.Remove(newAction);
                newAction = null;
                yield break; // Stop the coroutine here
            }

            Debug.Log($"Character invoker is: {newAction.actionInvoker.characterID}");

            ExecuteTimeBasedTypeLogic(newAction);
            ApplyActionCosts(newAction);
        }

        private IEnumerator ExecuteCharacterBasedTypeLogic(BaseAction newAction)
        {
            foreach (var type in newAction.actionTypes)
            {
                switch (type)
                {
                    case ActionTypes.Personal:
                        // Handle the personal action and wait for it to complete
                        yield return StartCoroutine(HandlePersonalAction(newAction, result => newAction.isActionPossible = result));
                        break;
                    case ActionTypes.Organization:
                        // Handle the organization action and wait for it to complete
                        yield return StartCoroutine(HandleOrganizationAction(newAction, result => newAction.isActionPossible = result));
                        break;
                }

                // If at any point, the action is not possible, exit the coroutine early
                if (!newAction.isActionPossible)
                {
                    yield break;
                }
            }
        }
        private void ExecuteTimeBasedTypeLogic(BaseAction newAction)
        {
            foreach (var type in newAction.actionTypes)
            {
                switch (type)
                {
                    case ActionTypes.Immediate:
                        ApplyActionEffects(newAction);
                        break;
                    case ActionTypes.Indicator:
                        RegisterIndicatorBasedAction(newAction);
                        Debug.Log($"New time based action added: {newAction.actionName}");
                        break;
                    case ActionTypes.Repeatable:
                        RegisterRepeatableAction(newAction);
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
            Debug.Log($"Action applied: {newAction.actionName} with invoker {newAction.actionInvoker.characterName} {newAction.actionInvoker.characterSurname}");
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
        public List<ActionConstructor> LocationActionConstructors = new();
    }
}
