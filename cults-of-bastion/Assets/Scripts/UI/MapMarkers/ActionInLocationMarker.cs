using System.Collections;
using Managers;
using PlayerInteractions;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MapMarkers
{
    public class ActionInLocationMarker : MonoBehaviour
    {
        [SerializeField] private Image progressBar;
        [SerializeField] private Image actionIconBox;

        private BaseAction _action;
        private float _currentProgress;
        private const float LerpDuration = 1f;

        private void OnEnable()
        {
            TimeManager.OnHourChanged += UpdateActionProgress;
        }

        private void OnDisable()
        {
            TimeManager.OnHourChanged -= UpdateActionProgress;
        }

        public void SetAction(BaseAction action)
        {
            _action = action;
            _currentProgress = _action.GetProgression();
            progressBar.fillAmount = _currentProgress;
        }
        public void RemoveAction()
        {
            StopAllCoroutines();
        }
        
        private void UpdateActionProgress(float f)
        {
            if (_action == null)
            {
                Debug.LogWarning("No action set.");
                return;
            }

            var newProgress = _action.GetProgression();
            StartCoroutine(SmoothProgressTransition(_currentProgress, newProgress));
        }
        
        private IEnumerator SmoothProgressTransition(float startValue, float endValue)
        {
            float elapsedTime = 0f;
            while (elapsedTime < LerpDuration)
            {
                elapsedTime += Time.deltaTime;
                _currentProgress = Mathf.Lerp(startValue, endValue, elapsedTime / LerpDuration);
                progressBar.fillAmount = _currentProgress;
                yield return null;
            }
            
            _currentProgress = endValue;
            progressBar.fillAmount = _currentProgress;
        }
    }

}
