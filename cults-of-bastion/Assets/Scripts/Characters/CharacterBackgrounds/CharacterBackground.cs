using System.Collections.Generic;
using Cultures;
using PlayerInteractions;

namespace Characters.CharacterBackgrounds
{
    public class CharacterBackground
    {
        public string BackgroundName;
        public string BackgroundDescription;
        public BackgroundType BackgroundType;
        public List<CharacterModification> BackgroundModifiers;
        public List<string> AllowedCulturesForBackground;
        public List<string> DisallowedCulturesForBackground;
    }
}