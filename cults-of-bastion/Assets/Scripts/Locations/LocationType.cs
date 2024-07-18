using UnityEngine;

namespace Locations
{
   public struct LocationType
   {
      public string TypeName;
      public string TypeDescription;
      //public Texture2D Icon;
   
      //locationInteractions
      
      public LocationType(string typeName, string typeDescription)
      {
         TypeName = typeName;
         TypeDescription = typeDescription;
      }
   }
}
