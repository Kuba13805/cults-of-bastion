using System;
using Locations;
using TMPro;
using UnityEngine;

namespace UI.Outliner
{
    public class OutlinerLocationButton : OutlinerButton
    {
        public LocationData locationData;
        [SerializeField] private TextMeshProUGUI locationNameBox;
        
        public static event Action<LocationData> OnLocationButtonClicked;
        public override void InitializeButton(LocationData passedLocationData)
        {
            locationData = passedLocationData;
            locationNameBox.text = locationData.locationName;
        }
        protected override void OnButtonClick() => OnLocationButtonClicked?.Invoke(locationData);
    }
}
