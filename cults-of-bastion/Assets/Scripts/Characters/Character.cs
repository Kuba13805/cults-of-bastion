using System;
using System.Collections.Generic;
using Characters.CharacterBackgrounds;
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
      [NonSerialized] public Culture CharacterCulture;
      public CharacterBackground ChildhoodBackground;
      public CharacterBackground AdulthoodBackground;
      
      public BaseAction currentAction;
      public List<LocationData> characterOwnedLocations = new();
      public Organization characterOrganization;
      
      public CharacterStats CharacterStats = new();
      public CharacterIndicators CharacterIndicators = new(); 
   }
}
public enum CharacterGender
{
   Male,
   Female
}
