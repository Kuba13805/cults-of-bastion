using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Characters;
using Organizations;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.PlayerInspector
{
    public class CharacterInspector : MonoBehaviour
    {
        private Character _characterData;
        [SerializeField] private TextMeshProUGUI characterNameBox;
        [SerializeField] private GameObject statBoxPrefab;
        [SerializeField] private Transform statBoxParent;
        [SerializeField] private GameObject organizationButton;
        [SerializeField] private TextMeshProUGUI characterAgeBox;
        [SerializeField] private TextMeshProUGUI characterCultureBox;

        [SerializeField] private GameObject locationBoxPrefab;
        [SerializeField] private Transform locationBoxParent;
        
        private readonly GameObject[] _statBoxList = new GameObject[8];

        public static event Action<Organization> OnInvokeCharacterOrganizationInspector; 

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
            ClearOwnedLocations();
            
            DisplayCharacterName();
            DisplayCharacterAge();
            DisplayCharacterCulture();
            DisplayCharacterStats();
            
            DisplayOwnedLocations();
            CheckIfCharacterHasOrganization();
        }

        public void InvokeCharacterOrganizationInspector()
        {
            OnInvokeCharacterOrganizationInspector?.Invoke(_characterData.characterOrganization);
        }

        private void CheckIfCharacterHasOrganization()
        {
            if(_characterData.characterOrganization == null)
            {
                organizationButton.SetActive(false);
            }
            else
            {
                organizationButton.SetActive(true);
            }
        }

        private void DisplayCharacterName()
        {
            characterNameBox.text = !string.IsNullOrEmpty(_characterData.characterSurname) ? $"{_characterData.characterName} \"{_characterData.characterNickname}\" {_characterData.characterSurname}" : $"{_characterData.characterName} {_characterData.characterSurname}";
        }

        private void DisplayCharacterAge()
        {
            characterAgeBox.text = _characterData.characterAge.ToString();
        }

        private void DisplayCharacterCulture()
        {
            characterCultureBox.text = _characterData.characterCulture.cultureName;
        }

        private void DisplayCharacterStats()
        {
            var characterStats = _characterData.CharacterStats;
            
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

        private void DisplayOwnedLocations()
        {
            foreach (var location in _characterData.characterOwnedLocations)
            {
                var locationButton = Instantiate(locationBoxPrefab, locationBoxParent);
                locationButton.GetComponent<CharacterOwnedLocationButton>().InitializeInspector(location);
            }
        }

        private void ClearOwnedLocations()
        {
            for (int i = 0; i < locationBoxParent.GetComponentsInChildren<CharacterOwnedLocationButton>().Length; i++)
            {
                Destroy(locationBoxParent.GetComponentsInChildren<CharacterOwnedLocationButton>()[i].gameObject);
            }
        }
    }
}