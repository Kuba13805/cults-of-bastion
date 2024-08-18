using System;
using Characters;
using Organizations;
using TMPro;
using UnityEngine;

namespace UI.PlayerInspector
{
    public class OrganizationInspector : MonoBehaviour
    {
        private Organization _organization;
        [SerializeField] private TextMeshProUGUI organizationNameBox;
        [SerializeField] private TextMeshProUGUI organizationTypeBox;
        
        [SerializeField] private GameObject memberBoxPrefab;
        [SerializeField] private Transform memberBoxParent;

        public void InitializeInspector(Organization organization)
        {
            _organization = organization;
            organizationNameBox.text = organization.organizationName;
            organizationTypeBox.text = organization.organizationType.typeName;
            DisplayOrganizationMembers();
        }

        private void DisplayOrganizationMembers()
        {
            ClearOrganizationMembers();
            foreach (var character in _organization.organizationMembers)
            {
                DisplayOrganizationMember(character);
            }
        }
        private void DisplayOrganizationMember(Character character)
        {
           var memberBox = Instantiate(memberBoxPrefab, memberBoxParent);
           memberBox.GetComponent<OrganizationMemberButton>().InitializeMemberButton(character);
        }
        private void ClearOrganizationMembers()
        {
            for (int i = 0; i < memberBoxParent.childCount; i++)
            {
                Destroy(memberBoxParent.GetComponentsInChildren<OrganizationMemberButton>()[i].gameObject);
            }
        }
    }
}