using System.Collections.Generic;

namespace GameScenarios
{
    public class Scenario
    {
        public string ScenarioName;
        public string ScenarioDescription;
        public int ScenarioStartingCharacterNumber;
        public List<ScenarioModifier> ScenarioModifiers = new();
    }
}