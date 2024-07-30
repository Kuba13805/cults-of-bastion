using System;

namespace PlayerResources
{
    public class ResourceChanger
    {
        public static event Action<float> OnMoneyModification;
        public static event Action<float> OnInfluenceModification;
        
        public static void ModifyMoney(float amount)
        {
            OnMoneyModification?.Invoke(amount);
        }
        
        public static void ModifyInfluence(float amount)
        {
            OnInfluenceModification?.Invoke(amount);
        }
    }
}