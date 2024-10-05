using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using Locations;
using Managers;
using PlayerInteractions;
using PlayerResources;
using TMPro;
using UI.PlayerInspector;
using UI.PlayerInteractions;
using UnityEngine;

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
        public static event Action<LocationData> OnLocationSelection;
        public static event Action<Character> OnPassPlayerCharacter;
        public static event Action OnRequestPlayerCharacter;
        public static event Action OnRequestCharacterSelectionForAction;
        public static event Action<Character> OnPassSelectedCharacterForAction;
        public static event Action OnCancelActionInvoking; 

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
            LocationManager.OnPassLocationDataOnSelection += PassLocationDataOnSelection;
            PlayerCharacterButton.OnInspectPlayerCharacter += RequestPlayerCharacter;
            PlayerActionsController.OnRequestCharacterSelectionForAction += RequestCharacterSelection;
            CharacterSelectionForActionController.OnPassSelectedCharacterForAction += PassSelectedCharacter;
            CharacterSelectionForActionController.OnCancelActionInvoking += CancelActionInvoking;
        }
        private void UnsubscribeFromEvents()
        {
            ResourceController.OnPlayerMoneyChanged -= ModifyPlayerMoney;
            ResourceController.OnPlayerInfluenceChanged -= ModifyPlayerInfluence;
            TimeManager.OnDayChanged -= UpdateInGameDate;
            TimeManager.OnHourChanged -= UpdateInGameTime;
            PlayerInteractionsContentController.OnGetAllPlayerActions -= RequestAllPlayerActions;
            PlayerActionsController.OnPassAllPlayerActions -= PassAllPlayerActions;
            LocationManager.OnPassLocationDataOnSelection -= PassLocationDataOnSelection;
            PlayerCharacterButton.OnInspectPlayerCharacter -= RequestPlayerCharacter;
            PlayerActionsController.OnRequestCharacterSelectionForAction -= RequestCharacterSelection;
            CharacterSelectionForActionController.OnPassSelectedCharacterForAction -= PassSelectedCharacter;
            CharacterSelectionForActionController.OnCancelActionInvoking -= CancelActionInvoking;
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

        #region InspectorDataPassing
        private static void PassLocationDataOnSelection(LocationData locationData) => OnLocationSelection?.Invoke(locationData);
        private void RequestPlayerCharacter() => StartCoroutine(WaitForPlayerCharacter());

        private static IEnumerator WaitForPlayerCharacter()
        {
            var isPlayerCharacterLoaded = false;
            Action<Character> onPassPlayerCharacter = playerCharacter =>
            {
                OnPassPlayerCharacter?.Invoke(playerCharacter);
                isPlayerCharacterLoaded = true;
            };
            CharacterManager.OnPassPlayerCharacter += onPassPlayerCharacter;
            OnRequestPlayerCharacter?.Invoke();
            yield return new WaitUntil(() => isPlayerCharacterLoaded);
            
            CharacterManager.OnPassPlayerCharacter -= onPassPlayerCharacter;
        }

        #endregion

        #region CharacterDataPassing

        private static void RequestCharacterSelection() => OnRequestCharacterSelectionForAction?.Invoke();
        private static void PassSelectedCharacter(Character selectedCharacter) => OnPassSelectedCharacterForAction?.Invoke(selectedCharacter);
        private static void CancelActionInvoking() => OnCancelActionInvoking?.Invoke();

        #endregion
    }
}
