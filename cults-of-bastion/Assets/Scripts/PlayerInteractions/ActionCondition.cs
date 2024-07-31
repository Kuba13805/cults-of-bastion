namespace PlayerInteractions
{
    public struct ActionCondition
    {
        public ActionConditions Condition;
        public float Value;
        public string StringValue;
        public bool ConditionMet;
    }

    public enum ActionConditions
    {
        PlayerHasMoneyValue,
        PlayerHasInfluenceValue,
        TargetLocationType,
    }
}