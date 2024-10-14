using System.Collections;
using UnityEngine;
using UI.MapMarkers;
using Unity.Cinemachine;

public class LocationMarker : MonoBehaviour
{
    public LocationMarkerData LocationMarkerData;
    private Vector3 _locationPosition;
    private Camera _camera;
    private RectTransform _rectTransform;
    [SerializeField] private float smoothSpeed = 100f;

    public void InitializeMarker(LocationMarkerData newLocationMarkerData, Vector3 locationPosition)
    {
        LocationMarkerData = newLocationMarkerData;
        _locationPosition = locationPosition;
        
        _camera = Camera.main;

        _rectTransform = GetComponent<RectTransform>();
        gameObject.SetActive(true);

        StartCoroutine(UpdateMarkerPosition());
    }

    private IEnumerator UpdateMarkerPosition()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            
            // Convert world position to screen position
            Vector3 targetScreenPosition = _camera.WorldToScreenPoint(_locationPosition);

            // If the location is behind the camera, hide the marker
            if (targetScreenPosition.z < 0)
            {
                _rectTransform.gameObject.SetActive(false);
                continue;
            }
            else
            {
                _rectTransform.gameObject.SetActive(true);
            }

            // Set the position directly without padding
            // The marker will now be allowed to go to the very edges of the screen
            targetScreenPosition.x = Mathf.Clamp(targetScreenPosition.x, 0, Screen.width);
            targetScreenPosition.y = Mathf.Clamp(targetScreenPosition.y, 0, Screen.height);

            // Smoothly move the UI marker towards the target position
            Vector3 smoothedPosition = Vector3.Lerp(_rectTransform.position, targetScreenPosition, smoothSpeed * Time.deltaTime);

            _rectTransform.position = smoothedPosition;
        }
    }
}