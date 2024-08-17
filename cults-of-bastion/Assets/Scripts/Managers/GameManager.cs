using System;
using System.Collections;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        private GameState _currentGameState;
        
        public static event Action<GameState> OnGameStateChanged;
        public static event Action<GameData> OnGameDataLoaded; 

        private GameData _gameData;
        private bool _locationManagerReady;
        private bool _characterManagerReady;
        private bool _organizationManagerReady;

        private void Awake()
        {
            StartCoroutine(PrepareForInitialization());
        }

        private void Start()
        {
            ChangeGameState(GameState.InGameCityMap);
        }

        private void ChangeGameState(GameState state)
        {
            _currentGameState = state;
            OnGameStateChanged?.Invoke(state);
        }

        private IEnumerator PrepareForInitialization()
        {
            
            LocationManager.OnLocationManagerInitialized += () => _locationManagerReady = true;
            CharacterManager.OnCharacterManagerInitialized += () => _characterManagerReady = true;
            OrganizationManager.OnOrganizationManagerInitialized += () => _organizationManagerReady = true;
            
            Debug.Log("Waiting for managers to load.");
            yield return new WaitUntil(() => _locationManagerReady && _characterManagerReady && _organizationManagerReady);
            Debug.Log("Managers loaded. Start initializing game.");
            InitializeGame();
            
        }
        private void InitializeGame()
        {
            var cityConfig = Resources.Load<TextAsset>("DataToLoad/testLocationData");
            if(cityConfig == null) return;

            var parsedCityConfigData = JsonUtility.FromJson<GameData>(cityConfig.text);

            if (parsedCityConfigData == null)
            {
                throw new Exception("Failed to parse city config data.");
            }

            _gameData = new GameData
            {
                LocationTypes = parsedCityConfigData.LocationTypes,
                Locations = parsedCityConfigData.Locations,
                CharacterConstructors = parsedCityConfigData.CharacterConstructors,
                OrganizationConstructors = parsedCityConfigData.OrganizationConstructors,
                PlayerCharacter = parsedCityConfigData.PlayerCharacter,
                PlayerOrganization = parsedCityConfigData.PlayerOrganization
                
            };
            OnGameDataLoaded?.Invoke(_gameData);
        }
    }
    
    public enum GameState
    {
        Menu,
        InGameCityMap,
        Loading,
        Saving,
        GameOver,
        Paused
    }
}