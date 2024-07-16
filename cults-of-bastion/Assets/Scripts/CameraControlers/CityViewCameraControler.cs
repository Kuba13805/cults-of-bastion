using System.Collections;
using Managers;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CityViewCameraControler : MonoBehaviour
{
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private float cameraMovementSpeed;
    [SerializeField] private float maxCameraMovementSpeed;
    [SerializeField] private float minCameraMovementSpeed;
    
    private float _speedChange = 1f;
    
    [SerializeField] private float accelerationTime;
    [SerializeField] private float decelerationTime;
    [SerializeField] private float cameraMovementDuration;
    [SerializeField] private float cameraRotationSpeed;
    [SerializeField] private float cameraZoomSpeed;
    [SerializeField] private float followOffsetMin;
    [SerializeField] private float followOffsetMax;
    [SerializeField] private float zoomValueOnLocationFocus;
    
    private Vector3 _followOffset;
    private CinemachineCameraOffset _cinemachineCameraOffsetComponent;
    
    private PlayerInputControls _playerInputControls;

    private bool _isMoving;
    private bool _isAllowedToRotate;
    
    private float currentSpeed = 0.0f;
    private float targetSpeed = 0.0f;
    private float accelerationRate;
    private float decelerationRate;

    private Coroutine _cameraMovementCoroutine;
    private Coroutine _cameraRotationCoroutine;
    private Coroutine _cameraZoomCoroutine;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }

    private void Start()
    {
        _playerInputControls = InputManager.Instance.playerInputControls;

        _cinemachineCameraOffsetComponent = virtualCamera.GetComponent<CinemachineCameraOffset>();
        _followOffset = _cinemachineCameraOffsetComponent.Offset;
        
        
        accelerationRate = maxCameraMovementSpeed / accelerationTime;
        decelerationRate = maxCameraMovementSpeed / decelerationTime;
        
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        _playerInputControls.CityViewActions.MoveCityCamera.started += ObserveForCameraMovement;

        _playerInputControls.CityViewActions.MoveCityCamera.canceled += ObserveForCameraStop;

        _playerInputControls.CityViewActions.RotateCityCamera.started += ObserveForCameraRotation;

        _playerInputControls.CityViewActions.RotateCityCamera.canceled += ObserveForCameraRotationStop;

        _playerInputControls.CityViewActions.ZoomCityCamera.performed += ObserveForCameraZoom;

        _playerInputControls.CityViewActions.ZoomCityCamera.canceled += ObserveForCameraZoomStop;

        _playerInputControls.CityViewActions.SwitchRotation.started += AllowRotation;

        _playerInputControls.CityViewActions.SwitchRotation.canceled += DoNotAllowRotation;
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void UnsubscribeEvents()
    {
        _playerInputControls.CityViewActions.MoveCityCamera.started -= ObserveForCameraMovement;

        _playerInputControls.CityViewActions.MoveCityCamera.canceled -= ObserveForCameraStop;

        _playerInputControls.CityViewActions.RotateCityCamera.started -= ObserveForCameraRotation;

        _playerInputControls.CityViewActions.RotateCityCamera.canceled -= ObserveForCameraRotationStop;

        _playerInputControls.CityViewActions.ZoomCityCamera.performed -= ObserveForCameraZoom;

        _playerInputControls.CityViewActions.ZoomCityCamera.canceled -= ObserveForCameraZoomStop;

        _playerInputControls.CityViewActions.SwitchRotation.started -= AllowRotation;

        _playerInputControls.CityViewActions.SwitchRotation.canceled -= DoNotAllowRotation;
    }


    #region CameraMovement

    private void ObserveForCameraMovement(InputAction.CallbackContext obj)
    {
        if (_cameraMovementCoroutine != null) StopCoroutine(_cameraMovementCoroutine);
        _cameraMovementCoroutine = StartCoroutine(MoveCamera());
    }
    
    private void ObserveForCameraStop(InputAction.CallbackContext obj)
    {
        if (_cameraMovementCoroutine == null) return;
        
        StopCoroutine(_cameraMovementCoroutine);
        _cameraMovementCoroutine = null;
    }
    private IEnumerator MoveCamera()
    {
        while (true)
        {
            var inputVector = _playerInputControls.CityViewActions.MoveCityCamera.ReadValue<Vector2>();
            bool hasInput = inputVector != Vector2.zero;

            targetSpeed = hasInput ? maxCameraMovementSpeed : 0.0f;

            if (currentSpeed < targetSpeed)
            {
                currentSpeed += accelerationRate * Time.deltaTime;
                if (currentSpeed > targetSpeed)
                {
                    currentSpeed = targetSpeed;
                }
            }
            else if (currentSpeed > targetSpeed)
            {
                currentSpeed -= decelerationRate * Time.deltaTime;
                if (currentSpeed < targetSpeed)
                {
                    currentSpeed = targetSpeed;
                }
            }

            var focusPointTransform = transform;
            var movement = focusPointTransform.forward * inputVector.y + focusPointTransform.right * inputVector.x;

            focusPointTransform.position += movement * currentSpeed * Time.deltaTime;

            yield return null;
        }
    }
    private void FocusOnPoint(Vector3 targetPosition)
    {
        StartCoroutine(MoveCameraToPoint(targetPosition));
        StartCoroutine(ZoomOnFocusedLocation(zoomValueOnLocationFocus));
    }

    private IEnumerator MoveCameraToPoint(Vector3 targetPosition)
    {
        _isMoving = true;
        
        float elapsedTime = 0f;
        Vector3 startingPosition = transform.position;

        while (elapsedTime < cameraMovementDuration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / cameraMovementDuration);
            transform.position = Vector3.Lerp(startingPosition, targetPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;

        _isMoving = false;
    }

    #endregion

    #region CameraRotation

    private void AllowRotation(InputAction.CallbackContext obj)
    {
        _isAllowedToRotate = true;
    }

    private void DoNotAllowRotation(InputAction.CallbackContext obj)
    {
        _isAllowedToRotate = false;
    }
    
    private void ObserveForCameraRotation(InputAction.CallbackContext obj)
    {
        if (!_isAllowedToRotate) return;
        
        if (_cameraRotationCoroutine != null) StopCoroutine(_cameraRotationCoroutine);
        _cameraRotationCoroutine = StartCoroutine(RotateCamera());
    }
    
    private void ObserveForCameraRotationStop(InputAction.CallbackContext obj)
    {
        if (_isAllowedToRotate) return;
        
        if (_cameraRotationCoroutine == null) return;
        
        StopCoroutine(_cameraRotationCoroutine);
        _cameraRotationCoroutine = null;
    }
    


    private IEnumerator RotateCamera()
    {
        while (true)
        {
            var inputRotation = _playerInputControls.CityViewActions.RotateCityCamera.ReadValue<Vector2>().x;
        
            transform.rotation = Quaternion.Euler(0f, inputRotation * cameraRotationSpeed + transform.rotation.eulerAngles.y, 0);

            yield return null;
        }
    }

    #endregion

    #region CameraZoom

    private void ObserveForCameraZoom(InputAction.CallbackContext obj)
    {
        if (_cameraZoomCoroutine != null) StopCoroutine(_cameraZoomCoroutine);
        _cameraZoomCoroutine = StartCoroutine(ZoomCamera());
    }
    
    private void ObserveForCameraZoomStop(InputAction.CallbackContext obj)
    {
        if (Mathf.Approximately(obj.ReadValue<float>(), 0f)) 
            return;
        
        if (_cameraZoomCoroutine == null) return;
        
        StopCoroutine(_cameraZoomCoroutine);
        _cameraZoomCoroutine = null;
    }

    private IEnumerator ZoomCamera()
    {
        while (true)
        {
            Vector3 zoomDir = _followOffset.normalized;
        
            var inputZoom = _playerInputControls.CityViewActions.ZoomCityCamera.ReadValue<Vector2>().y;

            switch (inputZoom)
            {
                case > 0:
                    _followOffset += zoomDir;
                    break;
                case < 0:
                    _followOffset -= zoomDir;
                    break;
            }

            if (_followOffset.magnitude < followOffsetMin)
            {
                _followOffset = zoomDir * followOffsetMin;
            }
            if (_followOffset.magnitude > followOffsetMax)
            {
                _followOffset = zoomDir * followOffsetMax;
            }
        
            float zoomRatio = (_followOffset.magnitude + followOffsetMin) / (followOffsetMax + followOffsetMin);
            cameraMovementSpeed = Mathf.Lerp(maxCameraMovementSpeed, minCameraMovementSpeed, zoomRatio);

            _cinemachineCameraOffsetComponent.Offset =
                Vector3.Lerp(_cinemachineCameraOffsetComponent.Offset, new Vector3(0f, _cinemachineCameraOffsetComponent.Offset.y,_followOffset.z),
                    Time.deltaTime * cameraZoomSpeed);

            yield return null;
        }
    }
    private IEnumerator ZoomOnFocusedLocation(float ZoomValueOnLocationFocus)
    {
        Vector3 zoomDir = _followOffset.normalized;
        float targetZoom = Mathf.Clamp(ZoomValueOnLocationFocus, followOffsetMin, followOffsetMax);
    
        float elapsedTime = 0f;
        float initialZoom = _followOffset.magnitude;

        while (elapsedTime < cameraMovementDuration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / cameraMovementDuration);
            float currentZoom = Mathf.Lerp(initialZoom, targetZoom, t);

            _followOffset = zoomDir * currentZoom;

            float zoomRatio = (currentZoom - followOffsetMin) / (followOffsetMax - followOffsetMin);
            cameraMovementSpeed = Mathf.Lerp(maxCameraMovementSpeed, minCameraMovementSpeed, zoomRatio);

            _cinemachineCameraOffsetComponent.Offset =
                Vector3.Lerp(new Vector3(0f, _cinemachineCameraOffsetComponent.Offset.y, _cinemachineCameraOffsetComponent.Offset.z), new Vector3(0f, _cinemachineCameraOffsetComponent.Offset.y,_followOffset.z),
                    Time.deltaTime * cameraZoomSpeed);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    #endregion
}
