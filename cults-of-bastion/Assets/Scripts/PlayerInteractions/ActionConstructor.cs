using System.Collections.Generic;

namespace PlayerInteractions
{
    [System.Serializable]
    public struct ActionConstructor
    {
        public string name;
        public string description;
        public int duration;
        public List<string> actionTypes;
        public List<string> conditions;
        public List<string> effects;
        public List<string> costs;
    }
}