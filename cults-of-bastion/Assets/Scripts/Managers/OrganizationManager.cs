using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters;
using NewGame;
using Organizations;
using UI.Outliner;
using UI.PlayerInteractions;
using UnityEngine;

namespace Managers
{
    public class OrganizationManager : MonoBehaviour
    {
        private List<Organization> _allOrganizations = new();
        private readonly Dictionary<string, OrganizationType> _organizationTypes = new();
        
        private readonly HashSet<int> _organizationIDsInUse = new();
        private Queue<int> _organizationIDsAvailable = new();
        private GameData _gameData;
        private OrganizationTypeData _organizationTypeData;
        
        [SerializeField] private int maxOrganizations;
        
        public static event Action OnOrganizationManagerInitialized;
        public static event Action<Organization> OnOrganizationMemberAdded;
        public static event Action OnOrganizationLoadingFinished;
        public static event Action<List<OrganizationType>> OnPassOrganizationTypes;
        public static event Action<List<Character>> OnPassOrganizationMembers;
        public static event Action<Character> OnMemberAddedToPlayerOrganization;
        public static event Action<int> OnMemberRemovedFromPlayerOrganization;

        private void Awake()
        {
            SubscribeToEvents();
            LoadOrganizationTypes();
            InitializeOrganizationIDs();
        }

        private void Start()
        {
            OnOrganizationManagerInitialized?.Invoke();
        }
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void SubscribeToEvents()
        {
            GameManager.OnGameDataInitialized += StartOrganizationLoading;
            CharacterManager.OnRequestCharacterAssigmentToOrganization += AddCharacterToOrganization;
            NewGameController.OnRequestGameData += PassOrganizationTypes;
            CharacterSelectionForActionController.OnRequestOrganizationMembersForAction += PassOrganizationMembers;
            OutlinerContentController.OnRequestCharacterListForOutliner += PassOrganizationMembers;
        }
        private void UnsubscribeFromEvents()
        {
            GameManager.OnGameDataInitialized -= StartOrganizationLoading;
            CharacterManager.OnRequestCharacterAssigmentToOrganization -= AddCharacterToOrganization;
            NewGameController.OnRequestGameData -= PassOrganizationTypes;
            CharacterSelectionForActionController.OnRequestOrganizationMembersForAction -= PassOrganizationMembers;
            OutlinerContentController.OnRequestCharacterListForOutliner -= PassOrganizationMembers;
        }

        private void InitializeOrganizationIDs()
        {
            for (int i = 1; i <= maxOrganizations; i++)
            {
                _organizationIDsAvailable.Enqueue(i);
            }
        }

        private void StartOrganizationLoading(GameData gameData, bool isNewGameStarting)
        {
            _gameData = gameData;
            if (_gameData.PlayerOrganization == null)
            {
                Debug.Log($"Player organization does not exist.");
            }
            if (_gameData.PlayerOrganization != null && isNewGameStarting)
            {
                _gameData.PlayerOrganization.organizationID = GetNewOrganizationID();
                AddOrganization(_gameData.PlayerOrganization);
                _gameData.PlayerOrganization.organizationMembers.Add(_gameData.PlayerCharacter);
                _gameData.PlayerCharacter.characterOrganization = _gameData.PlayerOrganization;
            }
            StartCoroutine(LoadOrganizations());
        }

        private IEnumerator LoadOrganizations()
        {
            Debug.Log(_gameData.OrganizationConstructors.Count);
            foreach (var organizationConstructor in _gameData.OrganizationConstructors)
            {
                if(_organizationIDsAvailable.Count == 0) continue;
                var organization = new Organization
                {
                    organizationName = organizationConstructor.name,
                    organizationDescription = organizationConstructor.description,
                    organizationID = organizationConstructor.id
                };
                
                if (organization.organizationID == 0)
                {
                    organization.organizationID = GetNewOrganizationID();
                }
                else
                {
                    var tempAvailableIDs = new List<int>(_organizationIDsAvailable);
                    if (tempAvailableIDs.Contains(organization.organizationID))
                    {
                        tempAvailableIDs.Remove(organization.organizationID);
                        _organizationIDsAvailable = new Queue<int>(tempAvailableIDs);
                    }

                    _organizationIDsInUse.Add(organization.organizationID);
                }
                AssignOrganizationType(organization, organizationConstructor);
                AddOrganization(organization);
                Debug.Log($"Organization {organization.organizationName} added with id {organization.organizationID} and type {organization.organizationType.typeName}");
            }
            OnOrganizationLoadingFinished?.Invoke();
            yield return null;
        }
        

        #region OrganizationAddingRemoving

        private void AddOrganization(Organization organization)
        {
            _allOrganizations.Add(organization);
        }

        private int GetNewOrganizationID()
        {
            if(_organizationIDsAvailable.Count <= 0) return -1;

            var id = _organizationIDsAvailable.Dequeue();
            _organizationIDsInUse.Add(id);
            return id;
        }
        private void RemoveOrganization(Organization organization)
        {
            ReleaseOrganizationID(organization.organizationID);
            foreach (var character in organization.organizationMembers)
            {
                character.characterOrganization = null;
            }
            _allOrganizations.Remove(organization);
        }
        private void ReleaseOrganizationID(int id)
        {
            if (!_organizationIDsInUse.Contains(id)) return;
            
            _organizationIDsInUse.Remove(id);
            _organizationIDsAvailable.Enqueue(id);
        }

        #endregion

        #region OrganizationMembersHandling

        private void AddCharacterToOrganization(Character character, int organizationID)
        {
            var tempOrganization = _allOrganizations.Find(organization => organization.organizationID == organizationID);
            if (tempOrganization == null) return;
            tempOrganization.organizationMembers.Add(character);
            Debug.Log($"Organization {tempOrganization.organizationName} added character {character.characterName} {character.characterSurname} with id {character.characterID}");
            OnOrganizationMemberAdded?.Invoke(tempOrganization);
            if(_gameData.PlayerOrganization == null) return;
            if (organizationID == _gameData.PlayerOrganization.organizationID)
            {
                OnMemberAddedToPlayerOrganization?.Invoke(character);
            }
        }

        private void RemoveCharacterFromOrganization(Character character)
        {
            var tempOrganization = _allOrganizations.Find(organization => organization.organizationID == character.characterOrganization.organizationID);
            if (tempOrganization == null) return;
            tempOrganization.organizationMembers.Remove(character);
            Debug.Log($"Organization {tempOrganization.organizationName} removed character {character.characterName} {character.characterSurname} with id {character.characterID}");
            if (tempOrganization.organizationID == _gameData.PlayerOrganization.organizationID)
            {
                OnMemberRemovedFromPlayerOrganization?.Invoke(character.characterID);
            }
        }

        #endregion

        #region OrganizationTypeHandling

        private void AssignOrganizationType(Organization organization, OrganizationConstructor organizationConstructor)
        {
            if (string.IsNullOrEmpty(organizationConstructor.typeName))
            {
                Debug.LogWarning("No organization type assigned to organization " + organization.organizationName);
            }

            if (_organizationTypes.TryGetValue(organizationConstructor.typeName, out var organizationType))
            {
                organization.organizationType = organizationType;
            }
            else
            {
                Debug.LogWarning("No matching organization type found for " + organizationConstructor.typeName);
            }
        }
        private void LoadOrganizationTypes()
        {
            var organizationTypeConfig = Resources.Load<TextAsset>("DataToLoad/organizationTypes");
            if(organizationTypeConfig == null) return;

            var parsedOrganizationTypeConfigData =
                JsonUtility.FromJson<OrganizationTypeData>(organizationTypeConfig.text);
            if (parsedOrganizationTypeConfigData == null)
            {
                throw new Exception("Failed to parse organization types config data.");
            }
            _organizationTypeData = new OrganizationTypeData
            {
                OrganizationTypeConstructors = parsedOrganizationTypeConfigData.OrganizationTypeConstructors
            };
            InitializeOrganizationTypes();
        }
        private void InitializeOrganizationTypes()
        {
            foreach (var organizationType in _organizationTypeData.OrganizationTypeConstructors.Select(organizationTypeConstructor => new OrganizationType
                     {
                         typeName = organizationTypeConstructor.typeName,
                         typeDescription = organizationTypeConstructor.typeDescription
                     }))
            {
                _organizationTypes.Add(organizationType.typeName, organizationType);
            }
        }

        #endregion

        #region DataPassing

        private void PassOrganizationTypes()
        {
            var organizationTypes = _organizationTypes.Values.ToList();
            OnPassOrganizationTypes?.Invoke(organizationTypes);
        }

        private void PassOrganizationMembers()
        {
            if (_gameData.PlayerOrganization != null)
            {
                OnPassOrganizationMembers?.Invoke(_gameData.PlayerOrganization.organizationMembers);
            }
            else
            {
                var tempCharacterList = new List<Character>();
                tempCharacterList.Add(_gameData.PlayerCharacter);
                OnPassOrganizationMembers?.Invoke(tempCharacterList);
            }
        }

        #endregion
    }

    public class OrganizationTypeData
    {
        public List<OrganizationType> OrganizationTypeConstructors = new();
    }
}