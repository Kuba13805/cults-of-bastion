using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using Characters.CharacterBackgrounds;
using Cultures;
using GameScenarios;
using NewGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu.NewGameMenu.CharacterCreation
{
    public class CharacterPanelController : StagePanelController
    {
        #region Variables

        [SerializeField] private TMP_InputField characterNameInputField;
        [SerializeField] private TMP_InputField characterSurnameInputField;
        [SerializeField] private TMP_InputField characterAgeInputField;
        [SerializeField] private TextMeshProUGUI characterGenderText;
        [SerializeField] private TextMeshProUGUI characterCultureText;
        [SerializeField] private TextMeshProUGUI characterChildhoodBackgroundText;
        [SerializeField] private TextMeshProUGUI characterAdulthoodBackgroundText;

        private Character _generatedCharacter;
        
        private List<ScenarioModifier> _forcedCharacterElements = new();
        private List<Culture> _cultures = new();
        private List<CharacterBackground> _childhoodBackgrounds = new();
        private List<CharacterBackground> _adulthoodBackgrounds = new();

        #endregion

        #region Events

        public static event Action OnRequestCharacterGeneration;

        #endregion

        protected override void Start()
        {
            base.Start();
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            NewGameController.OnPassCultures += LoadCultures;
            NewGameController.OnPassBackgrounds += LoadBackgrounds;
            NewGameController.OnPassScenarioModifiers += LoadForcedCharacterModifiers;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnsubscribeFromEvents();
        }

        private void UnsubscribeFromEvents()
        {
            NewGameController.OnPassCultures -= LoadCultures;
            NewGameController.OnPassBackgrounds -= LoadBackgrounds;
            NewGameController.OnPassScenarioModifiers -= LoadForcedCharacterModifiers;
        }

        protected override void InitializeStage(NewGameStages newGameStages)
        {
            base.InitializeStage(newGameStages);
            Debug.Log($"Character stage initialized!");
            if (newGameStages.Equals(handledStage))
            {
                StartCharacterGeneration();
            }
        }
        #region DataLoading

        private void LoadForcedCharacterModifiers(List<ScenarioModifier> modifiers)
        {
            _forcedCharacterElements = modifiers;
        }

        private void LoadCultures(List<Culture> cultures)
        {
            _cultures = cultures;
        }

        private void LoadBackgrounds(List<CharacterBackground> childhoodBackgrounds, List<CharacterBackground> adulthoodBackgrounds)
        {
            _childhoodBackgrounds = childhoodBackgrounds;
            _adulthoodBackgrounds = adulthoodBackgrounds;
        }

        #endregion

        #region CharacterGeneration

        private void StartCharacterGeneration()
        {
            StartCoroutine(GenerateCharacter());
        }

        private IEnumerator GenerateCharacter()
        {
            var generationReceived = false;
            Action<Character> onCharacterGeneration = character =>
            {
                _generatedCharacter = character;
                EditCharacter();
                generationReceived = true;
            };
            NewGameController.OnCharacterCreated += onCharacterGeneration;
            OnRequestCharacterGeneration?.Invoke();
            
            yield return new WaitUntil(() => generationReceived);
            NewGameController.OnCharacterCreated -= onCharacterGeneration;
        }

        private void EditCharacter()
        {
            characterNameInputField.text = _generatedCharacter.characterName;
            characterSurnameInputField.text = _generatedCharacter.characterSurname;
            characterAgeInputField.text = _generatedCharacter.characterAge.ToString();
            characterGenderText.text = _generatedCharacter.characterGender.ToString();
            characterCultureText.text = _generatedCharacter.characterCulture.cultureName;
            characterChildhoodBackgroundText.text = _generatedCharacter.ChildhoodBackground.BackgroundName;
            characterAdulthoodBackgroundText.text = _generatedCharacter.AdulthoodBackground.BackgroundName;
            Debug.Log($"Character generated: {_generatedCharacter.characterName} {_generatedCharacter.characterSurname} with id {_generatedCharacter.characterID}");
        }

        #endregion
    }
}