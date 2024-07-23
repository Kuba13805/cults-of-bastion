using System;
using Mono.Cecil;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        private GameState _currentGameState;
        
        public static event Action<GameState> OnGameStateChanged;
        public static event Action<GameData> OnGameDataLoaded; 

        private GameData _gameData;

        private void Start()
        {
            ChangeGameState(GameState.InGameCityMap);
            InitializeGame();
        }

        private void ChangeGameState(GameState state)
        {
            _currentGameState = state;
            OnGameStateChanged?.Invoke(state);
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
                CharacterConstructors = parsedCityConfigData.CharacterConstructors
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