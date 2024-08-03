using System;
using System.Collections.Generic;
using Managers;
using PlayerInteractions;
using PlayerInteractions.LocationActions;
using PlayerResources;
using TMPro;
using UI.PlayerInteractions;
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
        
        public static event Action OnRequestAllPlayerActions;
        public static event Action<List<BaseAction>> OnPassAllPlayerActions;

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
            PlayerInteractionsContentController.OnGetAllPlayerActions += RequestAllPlayerActions;
            PlayerActionsController.OnPassAllPlayerActions += PassAllPlayerActions;
        }
        private void UnsubscribeFromEvents()
        {
            ResourceController.OnPlayerMoneyChanged -= ModifyPlayerMoney;
            ResourceController.OnPlayerInfluenceChanged -= ModifyPlayerInfluence;
            TimeManager.OnDayChanged -= UpdateInGameDate;
            TimeManager.OnHourChanged -= UpdateInGameTime;
            PlayerInteractionsContentController.OnGetAllPlayerActions -= RequestAllPlayerActions;
            PlayerActionsController.OnPassAllPlayerActions -= PassAllPlayerActions;
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

        #region PlayerActions

        private static void RequestAllPlayerActions() => OnRequestAllPlayerActions?.Invoke();
        private static void PassAllPlayerActions(List<BaseAction> baseActions) => OnPassAllPlayerActions?.Invoke(baseActions);

        #endregion
    }
}
