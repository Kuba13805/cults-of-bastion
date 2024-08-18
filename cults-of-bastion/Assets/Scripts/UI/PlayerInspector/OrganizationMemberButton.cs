using System;
using Characters;
using UnityEngine;

namespace UI.PlayerInspector
{
    public class OrganizationMemberButton : MonoBehaviour
    {
        private Character _character;

        public static event Action<Character> OnInvokeCharacterInspector; 

        public void InitializeMemberButton(Character character)
        {
            _character = character;
        }
        public void InvokeCharacterInspector()
        {
            OnInvokeCharacterInspector?.Invoke(_character);
        }
    }
}