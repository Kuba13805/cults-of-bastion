namespace PlayerInteractions
{
    public struct ActionCondition
    {
        public ActionConditions Condition;
        public float Value;
        public bool ConditionMet;
    }

    public enum ActionConditions
    {
        PlayerHasMoneyValue,
        PlayerHasInfluenceValue,
    }
}