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
        IncreaseStat,
        DecreaseStat,
        GiveTrait,
        RemoveTrait,
    }
}