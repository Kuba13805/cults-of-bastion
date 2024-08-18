using System;
using System.Collections.Generic;

namespace Cultures
{
    [Serializable]
    public class Culture
    {
        public string cultureName;
        public string cultureDescription;
        public List<NamingEntry> CultureNamesMale;
        public List<NamingEntry> CultureNamesFemale;
        public List<NamingEntry> CultureSurnames;
    }
}