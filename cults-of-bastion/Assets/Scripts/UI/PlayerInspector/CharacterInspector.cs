using System;
using System.Linq;
using System.Reflection;
using Characters;
using TMPro;
using UnityEngine;

namespace UI.PlayerInspector
{
    public class CharacterInspector : MonoBehaviour
    {
        private Character _characterData;
        [SerializeField] private TextMeshProUGUI characterNameBox;
        [SerializeField] private GameObject statBoxPrefab;
        [SerializeField] private Transform statBoxParent;
        
        private readonly GameObject[] _statBoxList = new GameObject[8];

        public void InitializeStatBoxes()
        {
            for (int i = 0; i < 8; i++)
            {
                var statBoxInstance = Instantiate(statBoxPrefab, statBoxParent);
                _statBoxList[i] = statBoxInstance;
            }
        }

        public void InitializeInspector(Character character)
        {
            _characterData = character;
            
            DisplayCharacterName();
            DisplayCharacterStats();
        }

        private void DisplayCharacterName()
        {
            characterNameBox.text = !string.IsNullOrEmpty(_characterData.characterSurname) ? $"{_characterData.characterName} \"{_characterData.characterNickname}\" {_characterData.characterSurname}" : $"{_characterData.characterName} {_characterData.characterSurname}";
        }

        private void DisplayCharacterStats()
        {
            var characterStats = _characterData.CharacterStats;

            // Pobieranie pól zamiast właściwości
            var statsFields = characterStats.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in statsFields)
            {
                if (!typeof(BaseStat).IsAssignableFrom(field.FieldType)) continue;

                var stat = field.GetValue(characterStats) as BaseStat;
                var index = Array.IndexOf(statsFields, field);

                AssignStat(stat, index);
            }
        }

        private void AssignStat(BaseStat stat, int index)
        {
            _statBoxList[index].GetComponentsInChildren<TextMeshProUGUI>()[0].text = stat.Value.ToString();
            _statBoxList[index].GetComponentsInChildren<TextMeshProUGUI>()[1].text = stat.Name;
        }
    }
}