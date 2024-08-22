using System.Collections.Generic;

namespace Characters.CharacterBackgrounds
{
    [System.Serializable]
    public class CharacterBackgroundConstructor
    {
        public string backgroundName;
        public string backgroundDescription;
        public string backgroundTypeName;
        public List<string> backgroundEffects;
        public List<string> allowedCulturesForBackground;
        public List<string> disallowedCulturesForBackground;
    }
}