using System;
using System.Collections;
using System.Collections.Generic;
using PlayerResources;
using UnityEngine;

namespace PlayerInteractions
{
    public class ActionConditionVerifier : MonoBehaviour
    {
        private List<ActionCondition> _conditions;

        #region Events

        public static event Action<bool> OnActionConditionVerification; 
        public static event Action OnRequestPlayerMoneyValue;
        public static event Action OnRequestPlayerInfluenceValue;

        #endregion

        private void Verify(List<ActionCondition> conditions)
        {
            _conditions = conditions;
            OnActionConditionVerification?.Invoke(StartVerification());
        }


        private bool StartVerification()
        {
            foreach (var condition in _conditions)
            {
                switch (condition.Condition)
                {
                    case ActionConditions.PlayerHasMoneyValue:
                        StartCoroutine(VerifyPlayerHasMoneyValue(condition));
                        if(!condition.ConditionMet) return false;
                        break;
                    case ActionConditions.PlayerHasInfluenceValue:
                        StartCoroutine(VerifyPlayerHasInfluenceValue(condition));
                        if(!condition.ConditionMet) return false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return true;
        }

        #region ActionConditionVerifiers

        private static IEnumerator VerifyPlayerHasMoneyValue(ActionCondition condition)
        {
            var playerMoneyValue = 0f;
            var receivedMoneyValue = false;

            ResourceController.OnPassPlayerMoneyValue += value =>
            {
                receivedMoneyValue = true;
                playerMoneyValue = value;
            };
            
            OnRequestPlayerMoneyValue?.Invoke();
            
            yield return new WaitUntil(() => receivedMoneyValue);

            condition.ConditionMet = playerMoneyValue >= condition.Value;
        }
        private static IEnumerator VerifyPlayerHasInfluenceValue(ActionCondition condition)
        {
            var playerInfluenceValue = 0f;
            var receivedInfluenceValue = false;

            ResourceController.OnPassPlayerInfluenceValue += value =>
            {
                receivedInfluenceValue = true;
                playerInfluenceValue = value;
            };
            
            OnRequestPlayerInfluenceValue?.Invoke();
            
            yield return new WaitUntil(() => receivedInfluenceValue);

            condition.ConditionMet = playerInfluenceValue >= condition.Value;
        }

        #endregion
    }
}