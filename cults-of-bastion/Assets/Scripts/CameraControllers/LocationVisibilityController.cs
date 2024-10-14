using System.Collections;
using System.Collections.Generic;
using Locations;
using Unity.Cinemachine;
using UnityEngine;

namespace CameraControllers
{
    public class LocationVisibilityController : MonoBehaviour
    {
        private CinemachineCamera _virtualCamera;
        private Camera _activeCamera;
        [SerializeField] private List<LocationVisibilityDetector> _locations = new();

        [SerializeField] private int batchSize = 10;
        private int _currentBatchStartIndex = 0;

        private void Awake()
        {
            _virtualCamera = GetComponent<CinemachineCamera>();
        }

        private void OnEnable()
        {
            LocationVisibilityDetector.OnRegisterLocation += RegisterLocation;
            LocationVisibilityDetector.OnUnregisterLocation += UnregisterLocation;
        }

        private void OnDisable()
        {
            LocationVisibilityDetector.OnRegisterLocation -= RegisterLocation;
            LocationVisibilityDetector.OnUnregisterLocation -= UnregisterLocation;
        }

        private void Start()
        {
            if (_virtualCamera != null)
            {
                Debug.Log($"Virtual camera found: {_virtualCamera}");
                
                _activeCamera = Camera.main;

                if (_activeCamera != null)
                {
                    Debug.Log($"Active camera found: {_activeCamera}");
                }
                else
                {
                    Debug.LogError("Main Camera not found. Ensure that the main camera is tagged as 'MainCamera'.");
                }
            }
            else
            {
                Debug.LogError("CinemachineVirtualCamera not found.");
            }

            StartCoroutine(CheckVisibilityInBatches());
        }

        private void RegisterLocation(LocationVisibilityDetector location)
        {
            _locations.Add(location);
        }

        private void UnregisterLocation(LocationVisibilityDetector location)
        {
            _locations.Remove(location);
        }

        private IEnumerator CheckVisibilityInBatches()
        {
            var wait = new WaitForSeconds(0.1f);
            while (true)
            {
                yield return wait;
                CheckBatchVisibility();
            }
        }

        private void CheckBatchVisibility()
        {
            if (_activeCamera == null)
            {
                Debug.Log($"Camera is null");
                return;
            }
            
            int endIndex = Mathf.Min(_currentBatchStartIndex + batchSize, _locations.Count);

            for (int i = _currentBatchStartIndex; i < endIndex; i++)
            {
                var location = _locations[i];
                Vector3 viewPos = _activeCamera.WorldToViewportPoint(location.transform.position);
                bool isVisible = (viewPos.x is >= 0 and <= 1 && viewPos.y is >= 0 and <= 1 && viewPos.z > 0);

                if (isVisible)
                {
                    location.InvokeLocationVisible();
                }
                else
                {
                   location.InvokeLocationHidden();
                }
            }
            
            _currentBatchStartIndex += batchSize;
            
            if (_currentBatchStartIndex >= _locations.Count)
            {
                _currentBatchStartIndex = 0;
            }
        }
    }
}