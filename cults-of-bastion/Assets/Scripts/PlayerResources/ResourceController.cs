using System;
using UnityEngine;

namespace PlayerResources
{
    public class ResourceController : MonoBehaviour
    {
        public ResourceMoney playerMoney;
        public ResourceInfluence playerInfluence;

        public static event Action<float> OnPlayerMoneyChanged;
        public static event Action<float> OnPlayerInfluenceChanged;

        private void Start()
        {
            ResourceChanger.OnMoneyModification += ModifyPlayerMoney;
            ResourceChanger.OnInfluenceModification += ModifyPlayerInfluence;
        }

        private void OnDestroy()
        {
            ResourceChanger.OnMoneyModification -= ModifyPlayerMoney;
            ResourceChanger.OnInfluenceModification -= ModifyPlayerInfluence;
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
    }
}