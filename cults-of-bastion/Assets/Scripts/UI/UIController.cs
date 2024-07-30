using System;
using Managers;
using PlayerResources;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    public class UIController : MonoBehaviour
    {
        #region InGameDateVariables

        [SerializeField] private TextMeshProUGUI inGameDate;
        [SerializeField] private TextMeshProUGUI inGameTime;

        #endregion

        #region PlayerResourcesVariables

        [SerializeField] private TextMeshProUGUI playerMoney;
        [SerializeField] private TextMeshProUGUI playerInfluence;

        #endregion
        
        #region Events

        public static event Action OnPauseGame;
        public static event Action OnResumeGameWithNormalSpeed;
        public static event Action OnResumeGameWithHighSpeed;

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
            ResourceController.OnPlayerMoneyChanged += ModifyPlayerMoney;
            ResourceController.OnPlayerInfluenceChanged += ModifyPlayerInfluence;
            TimeManager.OnDayChanged += UpdateInGameDate;
            TimeManager.OnHourChanged += UpdateInGameTime;
        }
        private void UnsubscribeFromEvents()
        {
            ResourceController.OnPlayerMoneyChanged -= ModifyPlayerMoney;
            ResourceController.OnPlayerInfluenceChanged -= ModifyPlayerInfluence;
            TimeManager.OnDayChanged -= UpdateInGameDate;
            TimeManager.OnHourChanged -= UpdateInGameTime;
        }

        #region GameSpeedChanges

        public void PauseGame()
        {
            OnPauseGame?.Invoke();
        }

        public void ResumeGameWithNormalSpeed()
        {
            OnResumeGameWithNormalSpeed?.Invoke();
        }
        public void ResumeGameWithHighSpeed()
        {
            OnResumeGameWithHighSpeed?.Invoke();
        }

        #endregion

        #region ResourcesModifications

        private void ModifyPlayerMoney(float amount)
        {
            playerMoney.text = $"{amount}$";
        }
        private void ModifyPlayerInfluence(float amount)
        {
            playerInfluence.text = $"{amount}";
        }

        #endregion

        #region InGameDateController

        private void UpdateInGameTime(float time)
        {
            inGameTime.text = $"{time}:00";
        }
        private void UpdateInGameDate(int day, int month, int year)
        {
            inGameDate.text = $"{day}/{month}/{year}";
        }

        #endregion
    }
}
