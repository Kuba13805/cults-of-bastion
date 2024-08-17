using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Organizations;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class OrganizationManager : MonoBehaviour
    {
        [SerializeField] private Organization playerOrganization;
        private List<Organization> _allOrganizations = new();
        private readonly Dictionary<string, OrganizationType> _organizationTypes = new();
        
        private readonly HashSet<int> _organizationIDsInUse = new();
        private Queue<int> _organizationIDsAvailable = new();
        private GameData _gameData;
        private OrganizationTypeData _organizationTypeData;
        
        [SerializeField] private int maxOrganizations;
        
        public static event Action OnOrganizationManagerInitialized;

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
            GameManager.OnGameDataLoaded += StartOrganizationLoading;
        }
        private void UnsubscribeFromEvents()
        {
            GameManager.OnGameDataLoaded -= StartOrganizationLoading;
        }

        private void InitializeOrganizationIDs()
        {
            for (int i = 1; i <= maxOrganizations; i++)
            {
                _organizationIDsAvailable.Enqueue(i);
            }
        }

        private void StartOrganizationLoading(GameData gameData)
        {
            _gameData = gameData;
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
            _allOrganizations.Remove(organization);
        }
        private void ReleaseOrganizationID(int id)
        {
            if (!_organizationIDsInUse.Contains(id)) return;
            
            _organizationIDsInUse.Remove(id);
            _organizationIDsAvailable.Enqueue(id);
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
                Debug.Log($"New organization type added: {organizationType.typeName}");
            }
        }

        #endregion
    }

    public class OrganizationTypeData
    {
        public List<OrganizationType> OrganizationTypeConstructors = new();
    }
}