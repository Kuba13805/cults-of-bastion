using System.Collections.Generic;

namespace GameScenarios
{
    [System.Serializable]
    public class ScenarioConstructor
    {
        public string scenarioName;
        public string scenarioDescription;
        public int scenarioID;
        public int scenarioStartingCharacterNumber;
        public List<string> scenarioModifiers = new();
    }
}