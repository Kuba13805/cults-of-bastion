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

        private static float ModifyResource(float valueToModify, float valueModifier) => valueToModify + valueModifier;

        private void ModifyPlayerMoney(float moneyValue)
        {
            playerMoney.Value = ModifyResource(playerMoney.Value, moneyValue);
            OnPlayerMoneyChanged?.Invoke(moneyValue);
        }

        private void ModifyPlayerInfluence(float influenceValue)
        {
            playerInfluence.Value = ModifyResource(playerInfluence.Value, influenceValue);
            OnPlayerInfluenceChanged?.Invoke(influenceValue);
        }
    }
}