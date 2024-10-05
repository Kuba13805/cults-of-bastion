using Characters;
using Locations;
using UnityEngine;

namespace UI.Outliner
{
    public abstract class OutlinerButton : MonoBehaviour
    {
        public virtual void InitializeButton(LocationData locationData)
        {
            
        }

        public virtual void InitializeButton(Character character)
        {
            
        }
        protected virtual void OnButtonClick()
        {
            
        }
    }
}