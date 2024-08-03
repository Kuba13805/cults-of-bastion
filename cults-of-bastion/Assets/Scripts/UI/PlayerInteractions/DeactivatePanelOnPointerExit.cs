using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.PlayerInteractions
{
    public class DeactivatePanelOnPointerExit : MonoBehaviour, IPointerExitHandler
    {
        public void OnPointerExit(PointerEventData eventData)
        {
            gameObject.SetActive(false);
        }
    }
}
