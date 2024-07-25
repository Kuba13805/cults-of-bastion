using System.Collections.Generic;
using Locations;

namespace Characters
{
   [System.Serializable]
   public class Character
   {
      public int characterID;
      public string characterName;
      public string characterSurname;
      public string characterNickname;
      public int characterAge;
      public CharacterGender characterGender;
      public List<LocationData> characterOwnedLocations = new();
      
      public CharacterStats CharacterStats = new();
   }
}
public enum CharacterGender
{
   Male,
   Female
}
