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
    public void RemoveMarker()
    {
        StopCoroutine(UpdateMarkerPosition());
    }

    private IEnumerator UpdateMarkerPosition()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();

            Vector3 targetScreenPosition = _camera.WorldToScreenPoint(_locationPosition);
            
            if (targetScreenPosition.z < 0)
            {
                _rectTransform.gameObject.SetActive(false);
                continue;
            }

            _rectTransform.gameObject.SetActive(true);

            targetScreenPosition.x = Mathf.Clamp(targetScreenPosition.x, 0, Screen.width);
            targetScreenPosition.y = Mathf.Clamp(targetScreenPosition.y, 0, Screen.height);
            
            Vector3 smoothedPosition = Vector3.Lerp(_rectTransform.position, targetScreenPosition, smoothSpeed * Time.deltaTime);

            _rectTransform.position = smoothedPosition;
        }
    }
}