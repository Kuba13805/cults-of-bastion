namespace Characters
{
    public class CharacterModifier
    {
        public CharacterModifiers ModifierType;
        public int Value;
        public string StringValue;
    }

    public enum CharacterModifiers
    {
        ModifyStat,
        GiveTrait,
        RemoveTrait,
        ChangeName,
        ChangeSurname,
        AddNickname,
        RemoveNickname,
    }
}