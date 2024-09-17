using System;
using System.Collections.Generic;
using Characters;
using Characters.CharacterBackgrounds;
using Cultures;
using GameScenarios;
using TMPro;
using UnityEngine;

namespace UI.MainMenu.NewGameMenu.CharacterCreation
{
    public class CharacterPanelController : MonoBehaviour
    {
        #region Variables

        [SerializeField] private TMP_InputField characterNameInputField;
        [SerializeField] private TMP_InputField characterSurnameInputField;
        [SerializeField] private TMP_InputField characterAgeInputField;
        [SerializeField] private TextMeshProUGUI characterGenderText;

        private List<ScenarioModifier> _forcedCharacterElements = new();
        private List<Culture> _cultures = new();
        private List<CharacterBackground> _childhoodBackgrounds = new();
        private List<CharacterBackground> _adulthoodBackgrounds = new();

        #endregion

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void UnsubscribeFromEvents()
        {
            
        }
    }
}