using System;
using System.Collections.Generic;
using Characters;

namespace PlayerInteractions
{
    [Serializable]
    public abstract class BaseAction : IPlayerAction
    {
        public string actionName;
        public string actionDescription;
        public int actionDuration;
        public bool isActionPossible;

        public List<ActionTypes> actionTypes = new();
        public List<ActionCondition> ActionConditions = new();
        public List<ActionEffect> ActionEffects = new();
        public List<ActionEffect> ActionCosts = new();

        public bool isDuringAction;
        public bool isStopped;

        public Character actionInvoker;
        
        //action icon
        
        public virtual void Execute()
        {
            if (!isDuringAction)
            {
                isDuringAction = true;
            }
        }

        public void Cancel()
        {
            if (isDuringAction)
            {
                isDuringAction = false;
            }
        }

        public void StopExecuting()
        {
            if (!isDuringAction) return;
            
            isDuringAction = false;
            isStopped = true;
        }
    }

    public enum ActionTypes
    {
        Personal, //for only main player character
        Organization, //for any character in the player organization
        Immediate, //action is immediately executed
        TimeBased, //action is executed after certain amount of time
        TimeBasedNonLimited, //action is executed until the player cancels it
        Illegal, //action is considered illegal and provides character with crime points
    }
}
