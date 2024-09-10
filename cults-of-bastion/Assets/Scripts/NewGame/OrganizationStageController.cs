using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameScenarios;
using Organizations;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace NewGame
{
    public class OrganizationStageController : StageController
    {
        #region Variables
        
        private bool _allowedToProceed;

        #endregion

        #region Events

        public static event Action<ScenarioModifiers> OnCheckForOrganizationModifier;
        public static event Action<string> OnLockOrganizationType;
        public static event Action OnResetLockOrganizationType;

        #endregion
        private void Start()
        {
            StartCoroutine(VerifyScenarioModifiers());
        }

        private IEnumerator VerifyScenarioModifiers()
        {
            yield return CheckIfOrganizationExists();
            if(_allowedToProceed) yield return CheckIfOrganizationTypeIsForced();
        }

        private IEnumerator CheckIfOrganizationExists()
        {
            var receivedMessage = false;
            Action<bool> organizationExists = value =>
            {
                _allowedToProceed = value;
                receivedMessage = true;
            };
            
            NewGameController.OnCheckForOrganizationBoolModifier += organizationExists;
            
            OnCheckForOrganizationModifier?.Invoke(ScenarioModifiers.OrganizationExists);

            yield return new WaitUntil(() => receivedMessage);
            
            NewGameController.OnCheckForOrganizationBoolModifier -= organizationExists;

            if (!_allowedToProceed) NextStage();
        }

        private static IEnumerator CheckIfOrganizationTypeIsForced()
        {
            string organizationType = null;
            var receivedMessage = false;
            Action<string> organizationTypeIsForced = value =>
            {
                organizationType = value;
                receivedMessage = true;
            };
            NewGameController.OnCheckForOrganizationStringModifier += organizationTypeIsForced;
            OnCheckForOrganizationModifier?.Invoke(ScenarioModifiers.TypeOfOrganization);
            
            yield return new WaitUntil(() => receivedMessage);
            
            NewGameController.OnCheckForOrganizationStringModifier -= organizationTypeIsForced;

            if (!string.IsNullOrEmpty(organizationType))
            {
                OnLockOrganizationType?.Invoke(organizationType);
            }
            else
            {
                OnResetLockOrganizationType?.Invoke();
            }
        }

    }
}