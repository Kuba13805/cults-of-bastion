using System;
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
            progressBar.fillAmount = _action.GetProgression();
        }

        public void RemoveAction()
        {
            StopAllCoroutines();
            _action = null;
        }

        private void UpdateActionProgress(float f)
        {
            if (_action == null)
            {
                Debug.LogWarning("No action set.");
                return;
            }
            progressBar.fillAmount = _action.GetProgression();
            if (Math.Abs(progressBar.fillAmount - 1) < 0.05f)
            {
                progressBar.fillAmount = 0;
            }
        }
    }
}