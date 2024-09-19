using System;
using System.Collections;
using Characters;
using Cultures;
using NewGame;
using Organizations;
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
        private bool _cultureControllerReady;

        private void Start()
        {
            ChangeGameState(GameState.InGameCityMap);
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            NewGameController.OnStartNewGame += StartNewGame;
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void UnsubscribeFromEvents()
        {
            NewGameController.OnStartNewGame -= StartNewGame;
        }

        private void StartNewGame(Character playerCharacter, Organization playerOrganization)
        {
            _gameData = new GameData
            {
                PlayerOrganization = playerOrganization,
                PlayerCharacter = playerCharacter
            };
            PrepareForInitialization();
        }

        private void ChangeGameState(GameState state)
        {
            _currentGameState = state;
            OnGameStateChanged?.Invoke(state);
        }

        private void PrepareForInitialization()
        {
            Debug.Log("Start initializing game.");
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

            _gameData.Locations = parsedCityConfigData.Locations;
            _gameData.OrganizationConstructors = parsedCityConfigData.OrganizationConstructors;
            _gameData.CharacterConstructors = parsedCityConfigData.CharacterConstructors;
            _gameData.Organizations = parsedCityConfigData.Organizations;
            _gameData.Characters = parsedCityConfigData.Characters;
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