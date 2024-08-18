using System;
using System.Collections.Generic;

namespace Cultures
{
    [Serializable]
    public class CultureConstructor
    {
        public string cultureName;
        public string cultureDescription;
        public List<string> cultureNamesMale;
        public List<string> cultureNamesFemale;
        public List<string> cultureSurnames;
    }
}