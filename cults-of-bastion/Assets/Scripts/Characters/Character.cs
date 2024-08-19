using System.Collections.Generic;
using Characters.CharacterBackground;
using Cultures;
using Locations;
using Organizations;
using PlayerInteractions;

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
      public Culture characterCulture;
      public ChildhoodBackground CharacterChildhoodBackground;
      public AdulthoodBackground CharacterAdulthoodBackground;
      
      public BaseAction CurrentAction;
      public List<LocationData> characterOwnedLocations = new();
      public Organization characterOrganization;
      
      public CharacterStats CharacterStats = new();
   }
}
public enum CharacterGender
{
   Male,
   Female
}
