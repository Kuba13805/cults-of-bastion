using System.Collections.Generic;
using PlayerInteractions;

namespace Characters.CharacterBackgrounds
{
    public class CharacterBackground
    {
        public string BackgroundName;
        public string BackgroundDescription;
        public BackgroundType BackgroundType;
        public List<CharacterModifier> BackgroundModifiers;
    }
}