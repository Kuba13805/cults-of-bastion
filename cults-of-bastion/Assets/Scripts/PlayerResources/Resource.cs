using System;
using UnityEngine;

namespace PlayerResources
{
    public abstract class Resource
    {
        [HideInInspector] public string ResourceName;
        [HideInInspector] public string ResourceDesc;
        public float Value;
    }
    [Serializable]
    public class ResourceMoney : Resource
    {
        public new string ResourceName => "resource_money_name";
        public new string ResourceDesc => "resource_money_desc";
    }
    [Serializable]
    public class ResourceInfluence : Resource
    {
        public new string ResourceName => "resource_influence_name";
        public new string ResourceDesc => "resource_influence_desc";
    }
}
