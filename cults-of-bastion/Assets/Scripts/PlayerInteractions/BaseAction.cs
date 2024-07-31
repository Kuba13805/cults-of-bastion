using System;
using System.Collections.Generic;

namespace PlayerInteractions
{
    [Serializable]
    public abstract class BaseAction : IPlayerAction
    {
        public string actionName;
        public string actionDescription;
        public int actionDuration;
        
        public List<ActionCondition> ActionConditions = new ();

        private bool _isActionPossible;
        private bool _isDuringAction;
        private Action<bool> _onActionConditionVerification;
        
        #region MyRegion

        public static event Action<List<ActionCondition>> OnActionConditionsVerification; 

        #endregion
        
        //action icon
        //action list of effects
        //action list of conditions
        //action list of costs
        
        public virtual void Execute()
        {
            if (!_isDuringAction)
            {
                _isDuringAction = true;
            }
        }

        public void Cancel()
        {
            if (_isDuringAction)
            {
                _isDuringAction = true;
            }
        }

        public void CheckIfPossible()
        {
            _onActionConditionVerification = verificationResult => _isActionPossible = verificationResult;
            
            ActionConditionVerifier.OnActionConditionVerification += _onActionConditionVerification;
            
            OnActionConditionsVerification?.Invoke(ActionConditions);
            
            ActionConditionVerifier.OnActionConditionVerification -= _onActionConditionVerification;
        }
    }
}
