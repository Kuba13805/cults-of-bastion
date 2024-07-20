using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Characters
{
    [System.Serializable]
    public struct CharacterConstructor
    {
        public string name;
        public string surname;
        public string nickname;
        public int age;
        public string gender;
        public List<int> ownLocationIds;
    }
}