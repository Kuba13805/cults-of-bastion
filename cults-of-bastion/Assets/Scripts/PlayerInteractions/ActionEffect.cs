namespace PlayerInteractions
{
    public struct ActionEffect
    {
        public ActionEffects Effect;
        public float Value;
        public string StringValue;
    }

    public enum ActionEffects
    {
        AddInfluence,
        RemoveInfluence,
        AddMoney,
        RemoveMoney,
        IncreaseCharacterStat,
        DecreaseCharacterStat,
        ChanceToIncreaseCharacterStat,
        ChanceToDecreaseCharacterStat,
    }
}