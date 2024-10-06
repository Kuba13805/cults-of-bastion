using System;
using Locations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Outliner
{
    public class OutlinerLocationButton : OutlinerButton
    {
        public LocationData locationData;
        [SerializeField] private TextMeshProUGUI locationNameBox;
        
        public static event Action<LocationData> OnLocationButtonClicked;

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnButtonClick);
        }

        private void OnDisable()
        {
            GetComponent<Button>().onClick.RemoveListener(OnButtonClick);
        }

        public override void InitializeButton(LocationData passedLocationData)
        {
            locationData = passedLocationData;
            locationNameBox.text = locationData.locationName;
        }
        protected override void OnButtonClick() => OnLocationButtonClicked?.Invoke(locationData);
    }
}
