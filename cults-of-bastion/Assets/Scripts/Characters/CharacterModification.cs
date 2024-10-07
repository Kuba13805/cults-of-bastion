namespace Characters
{
    public class CharacterModification
    {
        public CharacterModifications ModificationType;
        public int Value;
        public string StringValue;
    }

    public enum CharacterModifications
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