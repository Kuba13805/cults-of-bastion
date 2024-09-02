namespace GameScenarios
{
    public class ScenarioModifier
    {
        public ScenarioModifiers ModiferType;
        public int Value;
        public string StringValue;
        public bool BoolValue;
        public int NumberOfCharactersAffected;
    }

    public enum ScenarioModifiers
    {
        OrganizationExists,
        TypeOfOrganization,
        ChanceForCharacterBackground,
        StartingOwnedBuilding,
        ChanceForCharacterCulture,
        ChanceForCharacterTrait,
        StartingResources,
        StartingQuestline,
        StartingEvent,
    }
}