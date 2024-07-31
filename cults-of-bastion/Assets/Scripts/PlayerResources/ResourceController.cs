using System;
using PlayerInteractions;
using UnityEngine;

namespace PlayerResources
{
    public class ResourceController : MonoBehaviour
    {
        public ResourceMoney playerMoney;
        public ResourceInfluence playerInfluence;

        #region MyRegion

        public static event Action<float> OnPlayerMoneyChanged;
        public static event Action<float> OnPlayerInfluenceChanged;
        public static event Action<float> OnPassPlayerMoneyValue;
        public static event Action<float> OnPassPlayerInfluenceValue;

        #endregion

        private void Start()
        {
            ResourceChanger.OnMoneyModification += ModifyPlayerMoney;
            ResourceChanger.OnInfluenceModification += ModifyPlayerInfluence;
            ActionConditionVerifier.OnRequestPlayerMoneyValue += PassPlayerMoneyValue;
            ActionConditionVerifier.OnRequestPlayerInfluenceValue += PassPlayerInfluenceValue;
            
            OnPassPlayerInfluenceValue?.Invoke(playerInfluence.Value);
            OnPassPlayerMoneyValue?.Invoke(playerMoney.Value);
        }

        private void OnDestroy()
        {
            ResourceChanger.OnMoneyModification -= ModifyPlayerMoney;
            ResourceChanger.OnInfluenceModification -= ModifyPlayerInfluence;
            ActionConditionVerifier.OnRequestPlayerMoneyValue -= PassPlayerMoneyValue;
            ActionConditionVerifier.OnRequestPlayerInfluenceValue -= PassPlayerInfluenceValue;
        }



        private static float ModifyResource(float valueToModify, float valueModifier)
        {
            var newValue = valueToModify + valueModifier;
            return newValue;
        }

        private void ModifyPlayerMoney(float moneyValue)
        {
            var newValue = ModifyResource(playerMoney.Value, moneyValue);
            playerMoney.Value = newValue;
            OnPlayerMoneyChanged?.Invoke(newValue);
        }

        private void ModifyPlayerInfluence(float influenceValue)
        {
            var newValue = ModifyResource(playerInfluence.Value, influenceValue);
            playerInfluence.Value = newValue;
            OnPlayerInfluenceChanged?.Invoke(newValue);
        }

        #region ConditionsHandling

        private void PassPlayerMoneyValue()
        {
            OnPassPlayerMoneyValue?.Invoke(playerMoney.Value);
        }
        private void PassPlayerInfluenceValue()
        {
            OnPassPlayerInfluenceValue?.Invoke(playerInfluence.Value);
        }

        #endregion
    }
}