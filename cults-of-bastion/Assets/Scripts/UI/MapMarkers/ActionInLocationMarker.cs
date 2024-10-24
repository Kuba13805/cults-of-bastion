using System;
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
        private Coroutine _updateProgressCoroutine;

        private void OnEnable()
        {
            TimeManager.OnHourChanged += UpdateActionProgress;
        }

        private void OnDisable()
        {
            TimeManager.OnHourChanged -= UpdateActionProgress;
            if (_updateProgressCoroutine != null)
            {
                StopCoroutine(_updateProgressCoroutine);
            }
        }

        public void SetAction(BaseAction action)
        {
            _action = action;
            progressBar.fillAmount = _action.GetProgression();
        }

        public void RemoveAction()
        {
            if (_updateProgressCoroutine != null)
            {
                StopCoroutine(_updateProgressCoroutine);
            }
            _action = null;
        }

        private void UpdateActionProgress(float f)
        {
            if (_action == null)
            {
                Debug.LogWarning("No action set.");
                return;
            }

            if (_updateProgressCoroutine != null)
            {
                StopCoroutine(_updateProgressCoroutine);
            }

            _updateProgressCoroutine = StartCoroutine(SmoothUpdateProgressBar());
        }

        private IEnumerator SmoothUpdateProgressBar()
        {
            while (_action != null)
            {
                float targetProgress = _action.GetProgression();
                float currentProgress = progressBar.fillAmount;

                while (Math.Abs(progressBar.fillAmount - targetProgress) > 0.01f)
                {
                    progressBar.fillAmount = Mathf.Lerp(currentProgress, targetProgress, 0.1f);
                    currentProgress = progressBar.fillAmount;

                    yield return null;

                    targetProgress = _action.GetProgression();
                }

                if (Math.Abs(progressBar.fillAmount - 1) < 0.05f)
                {
                    progressBar.fillAmount = 0;
                }

                yield return null;
            }
        }
    }
}
