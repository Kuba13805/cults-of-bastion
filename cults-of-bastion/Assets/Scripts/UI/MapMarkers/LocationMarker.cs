using System.Collections;
using System.Collections.Generic;
using UI.MapMarkers;
using UnityEngine;
using UnityEngine.Serialization;

public class LocationMarker : MonoBehaviour
{
    [SerializeField] private GameObject characterInLocationMarkerParent;
    [SerializeField] private GameObject actionInLocationMarkerParent;
    
    [SerializeField] private float smoothSpeed = 100f;
    
    public LocationMarkerData LocationMarkerData;
    
    private Vector3 _locationPosition;
    private Camera _camera;
    private RectTransform _rectTransform;
    
    private List<GameObject> _characterMarkers = new();
    private List<GameObject> _actionMarkers = new();
    
    private List<GameObject> _activeCharacterMarkers = new();
    private List<GameObject> _activeActionMarkers = new();

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
    public Transform GetActionParent()
    {
        return actionInLocationMarkerParent.transform;
    }
    public Transform GetCharacterParent()
    {
        return characterInLocationMarkerParent.transform;
    }
}