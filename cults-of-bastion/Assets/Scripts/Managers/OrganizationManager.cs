using System.Collections;
using System.Collections.Generic;
using Organizations;
using UnityEngine;

namespace Managers
{
    public class OrganizationManager : MonoBehaviour
    {
        public Organization playerOrganization;
        public List<Organization> allOrganizations = new();
        public List<OrganizationType> organizationTypes = new();
        
        private readonly HashSet<int> _organizationIDsInUse = new();
        private readonly Queue<int> _organizationIDsAvailable = new();
        private GameData _gameData;
        
        [SerializeField] private int maxOrganizations;
        
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
            GameManager.OnGameDataLoaded += StartOrganizationLoading;
        }
        private void UnsubscribeFromEvents()
        {
            GameManager.OnGameDataLoaded -= StartOrganizationLoading;
        }

        private void InitializeOrganizationIDs()
        {
            for (int i = 0; i < maxOrganizations; i++)
            {
                _organizationIDsAvailable.Enqueue(i);
            }
        }

        private void StartOrganizationLoading(GameData gameData)
        {

        }

        private IEnumerator LoadOrganizations()
        {
            foreach (var organizationConstructor in _gameData.OrganizationConstructors)
            {
                var organization = new Organization
                {
                    organizationName = organizationConstructor.name,
                    organizationID = GetNewOrganizationID(),
                };
                if(organization.organizationID <= 0) continue;
                AddOrganization(organization);
                Debug.Log($"Organization {organization.organizationName} added with id {organization.organizationID}");
            }
            yield return null;
        }

        private void AddOrganization(Organization organization)
        {
            allOrganizations.Add(organization);
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
            allOrganizations.Remove(organization);
        }
        private void ReleaseOrganizationID(int id)
        {
            if (!_organizationIDsInUse.Contains(id)) return;
            
            _organizationIDsInUse.Remove(id);
            _organizationIDsAvailable.Enqueue(id);
        }
        private IEnumerator LoadOrganizationTypes()
        {
            yield return null;
        }
    }
}