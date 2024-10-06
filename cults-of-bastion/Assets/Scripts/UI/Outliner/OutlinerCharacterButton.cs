using System;
using Characters;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Outliner
{
    public class OutlinerCharacterButton : OutlinerButton
    {
        public Character character;
        [SerializeField] private TextMeshProUGUI characterNameBox;
        
        public static event Action<Character> OnCharacterButtonClicked;

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnButtonClick);
        }

        private void OnDisable()
        {
            GetComponent<Button>().onClick.RemoveListener(OnButtonClick);
        }

        public override void InitializeButton(Character passedCharacterData)
        {
            character = passedCharacterData;
            characterNameBox.text = character.characterName + " " + character.characterSurname;
        }
        protected override void OnButtonClick() => OnCharacterButtonClicked?.Invoke(character);
    }
}
