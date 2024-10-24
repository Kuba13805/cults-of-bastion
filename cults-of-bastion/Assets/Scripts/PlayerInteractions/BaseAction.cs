using System;
using System.Collections.Generic;
using Characters;
using Locations;
using UnityEngine;

namespace PlayerInteractions
{
    [Serializable]
    public class BaseAction : IPlayerAction
    {
        public string actionName;
        public string actionDescription;
        public int actionDuration;
        public bool isActionPossible;
        public int targetNumber;
        public float actionProgressIndicator;
        public float actionCurrentProgression;
        public float actionCalculatedProgression;
        public float actionFixedProgression;

        [NonSerialized] public List<ActionTypes> ActionTypes = new();
        [NonSerialized] public List<ActionCondition> ActionConditions = new();
        [NonSerialized] public List<ActionEffect> ActionEffects = new();
        [NonSerialized] public List<ActionEffect> ActionCosts = new();

        public bool isDuringAction;
        public bool isStopped;

        [NonSerialized] public Character ActionInvoker;
        [NonSerialized] public LocationData TargetLocation;
        [NonSerialized] public Character TargetCharacter;
        
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
        public float GetProgression()
        {
            return actionProgressIndicator <= 0 ? 0f : Mathf.Clamp(actionCurrentProgression / actionProgressIndicator, 0f, 1f);
        }
    }

    public enum ActionTypes
    {
        Personal, //for only main player character
        Organization, //for any character in the player organization
        Immediate, //action is immediately executed
        Indicator, //action is executed after certain amount of time
        Repeatable, //action is executed until the player cancels it
        Illegal, //action is considered illegal and provides character with crime points
    }
}
