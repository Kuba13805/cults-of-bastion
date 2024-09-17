using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Characters
{
    [System.Serializable]
    public struct CharacterConstructor
    {
        public int characterID;
        public string characterName;
        public string characterSurname;
        public string characterNickname;
        public int characterAge;
        public string characterGender;
        public string characterCulture;
        public string childhoodBackground;
        public string adulthoodBackground;
        
        //do zrobienia: current action;
        public List<int> ownLocationIds;
        public int organizationId;
    }
}