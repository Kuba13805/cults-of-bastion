using System.Collections.Generic;
using Characters;

namespace Organizations
{
    [System.Serializable]
    public class Organization
    {
        public int organizationID;
        public string organizationName;
        public string organizationDescription;
        public List<Character> organizationMembers = new();
        public OrganizationType organizationType;
    }
}
