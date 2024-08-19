using System.Collections.Generic;
using PlayerInteractions;

namespace Characters.CharacterBackground
{
    public abstract class BaseBackground
    {
        public string BackgroundName;
        public string BackgroundDescription;
        public BackgroundType BackgroundType;
        public List<ActionEffect> BackgroundEffects = new List<ActionEffect>();
    }
}