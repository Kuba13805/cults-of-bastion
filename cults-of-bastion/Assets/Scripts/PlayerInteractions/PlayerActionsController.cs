using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using PlayerInteractions.LocationActions;
using UnityEngine;

namespace PlayerInteractions
{
    public class PlayerActionsController : MonoBehaviour
    {
        private ActionsData _actionsData;

        private Dictionary<string, LocationAction> _locationActionDict = new();
        private void Awake()
        {
            SubscribeToEvents();
            LoadActions();
        }

        private void SubscribeToEvents()
        {
            
        }

        private void UnsubscribeFromEvents()
        {
            
        }
        
        private void LoadActions()
        {
            var actionsConfig = Resources.Load<TextAsset>("DataToLoad/testActions");
            if(actionsConfig == null) return;
            
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

                foreach (var newCondition in locationAction.conditions.Select(definedCondition => CreateActionCondition(definedCondition)))
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionCondition), actionCondition, null);
            }
            Debug.Log($"New condition added to action: {condition.Condition} with value: {condition.Value}");
            return condition;
        }
    }

    public class ActionsData
    {
        public List<LocationAction> LocationActions = new();
        public List<LocationActionConstructor> LocationActionConstructors = new();
    }
}