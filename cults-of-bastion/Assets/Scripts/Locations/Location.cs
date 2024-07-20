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

        public static event Action<LocationData> OnSelectLocation;
        public static event Action<Vector3> OnFocusOnLocation;
        public static event Action<LocationData> OnInteractWithLocation;

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
            InputManager.Instance.PlayerInputControls.CityViewActions.SelectLocation.performed -= SelectLocation;
            InputManager.Instance.PlayerInputControls.CityViewActions.FocusOnLocation.performed -= FocusOnLocation;
            InputManager.Instance.PlayerInputControls.CityViewActions.InteractWithLocation.performed -= InteractWithLocation;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _pointerOver = true;
            
            InputManager.Instance.PlayerInputControls.CityViewActions.SelectLocation.performed += SelectLocation;
            InputManager.Instance.PlayerInputControls.CityViewActions.FocusOnLocation.performed += FocusOnLocation;
            InputManager.Instance.PlayerInputControls.CityViewActions.InteractWithLocation.performed += InteractWithLocation;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _pointerOver = false;
            
            InputManager.Instance.PlayerInputControls.CityViewActions.SelectLocation.performed -= SelectLocation;
            InputManager.Instance.PlayerInputControls.CityViewActions.FocusOnLocation.performed -= FocusOnLocation;
            InputManager.Instance.PlayerInputControls.CityViewActions.InteractWithLocation.performed -= InteractWithLocation;
        }

        private void SelectLocation(InputAction.CallbackContext callbackContext)
        {
            if(!_pointerOver) return;
            OnSelectLocation?.Invoke(locationData);
            Debug.Log($"Location selected: {locationData.locationName} of type {locationData.LocationType.typeName} with owner {locationData.LocationOwner.characterName} - {locationData.LocationType.typeDescription}");
        }

        private void FocusOnLocation(InputAction.CallbackContext callbackContext)
        {
            if(!_pointerOver) return;
            OnFocusOnLocation?.Invoke(transform.position);
        }

        private void InteractWithLocation(InputAction.CallbackContext callbackContext)
        {
            if(!_pointerOver) return;
            OnInteractWithLocation?.Invoke(locationData);
        }
    }
}
