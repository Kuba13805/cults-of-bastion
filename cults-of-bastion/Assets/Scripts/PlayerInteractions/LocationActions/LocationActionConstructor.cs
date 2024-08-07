using System.Collections.Generic;

namespace PlayerInteractions.LocationActions
{
    [System.Serializable]
    public struct LocationActionConstructor
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