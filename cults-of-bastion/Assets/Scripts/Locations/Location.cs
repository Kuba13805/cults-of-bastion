using System;
using Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Locations
{
    public class Location : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public LocationData locationData;
        public int locationIndex;

        private bool _pointerOver;

        #region Events

        public static event Action<Vector3> OnFocusOnLocation;

        #endregion

        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            
        }

        private void UnsubscribeFromEvents()
        {
            InputManager.Instance.playerInputControls.CityViewActions.SelectLocation.performed -= SelectLocation;
            InputManager.Instance.playerInputControls.CityViewActions.FocusOnLocation.performed -= FocusOnLocation;
            InputManager.Instance.playerInputControls.CityViewActions.InteractWithLocation.performed -= InteractWithLocation;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _pointerOver = true;
            
            InputManager.Instance.playerInputControls.CityViewActions.SelectLocation.performed += SelectLocation;
            InputManager.Instance.playerInputControls.CityViewActions.FocusOnLocation.performed += FocusOnLocation;
            InputManager.Instance.playerInputControls.CityViewActions.InteractWithLocation.performed += InteractWithLocation;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _pointerOver = false;
            
            InputManager.Instance.playerInputControls.CityViewActions.SelectLocation.performed -= SelectLocation;
            InputManager.Instance.playerInputControls.CityViewActions.FocusOnLocation.performed -= FocusOnLocation;
            InputManager.Instance.playerInputControls.CityViewActions.InteractWithLocation.performed -= InteractWithLocation;
        }

        private void SelectLocation(InputAction.CallbackContext callbackContext)
        {
            Debug.Log("Select Location...");
        }

        private void FocusOnLocation(InputAction.CallbackContext callbackContext)
        {
            OnFocusOnLocation?.Invoke(transform.position);
        }

        private void InteractWithLocation(InputAction.CallbackContext callbackContext)
        {
            Debug.Log("Interact With Location...");
        }
    }
}
