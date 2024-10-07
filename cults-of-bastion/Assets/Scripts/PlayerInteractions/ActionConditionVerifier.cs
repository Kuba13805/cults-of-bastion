using System;
using System.Collections;
using System.Collections.Generic;
using Locations;
using PlayerResources;
using UnityEngine;

namespace PlayerInteractions
{
    public class ActionConditionVerifier : MonoBehaviour
    {
        private List<ActionCondition> _conditions = new();

        #region Events

        public static event Action OnRequestPlayerMoneyValue;
        public static event Action OnRequestPlayerInfluenceValue;

        #endregion

        public void Verify(IEnumerable<ActionCondition> conditions, Action<bool> callback, object targetObjects)
        {
            _conditions = new List<ActionCondition>(conditions);
            StartCoroutine(StartVerificationCoroutine(callback, targetObjects));
        }

        private IEnumerator StartVerificationCoroutine(Action<bool> callback, object targetObjects)
        {
            int falseFlags = 0;

            for (int i = 0; i < _conditions.Count; i++)
            {
                bool result = false;
                yield return StartCoroutine(VerifyCondition(_conditions[i], isConditionMet => result = isConditionMet, targetObjects));
                var condition = _conditions[i];
                condition.ConditionMet = result;
                _conditions[i] = condition;
                if (!condition.ConditionMet)
                {
                    falseFlags++;
                }
            }

            bool allConditionsMet = falseFlags == 0;
            callback?.Invoke(allConditionsMet);
        }

        private IEnumerator VerifyCondition(ActionCondition condition, Action<bool> resultCallback, object targetObjects)
        {
            switch (condition.Condition)
            {
                case ActionConditions.PlayerHasMoneyValue:
                    yield return StartCoroutine(VerifyPlayerHasMoneyValue(condition, resultCallback));
                    break;
                case ActionConditions.PlayerHasInfluenceValue:
                    yield return StartCoroutine(VerifyPlayerHasInfluenceValue(condition, resultCallback));
                    break;
                case ActionConditions.TargetLocationType:
                    LocationData targetLocation;
                    try
                    {
                        targetLocation = targetObjects as LocationData;
                    }
                    catch (InvalidCastException e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                    yield return StartCoroutine(VerifyLocationType(targetLocation, condition, resultCallback));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region ActionConditionVerifiers

        private IEnumerator VerifyPlayerHasMoneyValue(ActionCondition condition, Action<bool> resultCallback)
        {
            var playerMoneyValue = 0f;
            var receivedMoneyValue = false;

            void OnMoneyValueReceived(float value)
            {
                receivedMoneyValue = true;
                playerMoneyValue = value;
                ResourceController.OnPassPlayerMoneyValue -= OnMoneyValueReceived;
            }

            ResourceController.OnPassPlayerMoneyValue += OnMoneyValueReceived;

            OnRequestPlayerMoneyValue?.Invoke();
            yield return new WaitUntil(() => receivedMoneyValue);

            bool conditionMet = playerMoneyValue >= condition.Value;

            resultCallback(conditionMet);
        }

        private IEnumerator VerifyPlayerHasInfluenceValue(ActionCondition condition, Action<bool> resultCallback)
        {
            var playerInfluenceValue = 0f;
            var receivedInfluenceValue = false;

            void OnInfluenceValueReceived(float value)
            {
                receivedInfluenceValue = true;
                playerInfluenceValue = value;
                ResourceController.OnPassPlayerInfluenceValue -= OnInfluenceValueReceived;
            }

            ResourceController.OnPassPlayerInfluenceValue += OnInfluenceValueReceived;

            OnRequestPlayerInfluenceValue?.Invoke();
            yield return new WaitUntil(() => receivedInfluenceValue);

            bool conditionMet = playerInfluenceValue >= condition.Value;

            resultCallback(conditionMet);
        }

        private IEnumerator VerifyLocationType(LocationData locationData, ActionCondition condition, Action<bool> resultCallback)
        {
            bool conditionMet = condition.StringValue.Equals(locationData.LocationType.typeName);

            resultCallback(conditionMet);
            yield return null;
        }

        #endregion
    }
}
