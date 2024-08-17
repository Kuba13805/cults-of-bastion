using System.Collections.Generic;
using Characters;
using UnityEngine;

namespace Organizations
{
    [System.Serializable]
    public class Organization
    {
        public int organizationID;
        public string organizationName;
        public string organizationDescription;
        [SerializeReference] public List<Character> organizationMembers = new();
        public OrganizationType organizationType;
    }
}
