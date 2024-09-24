using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Characters.CharacterBackgrounds;
using Cultures;
using GameScenarios;
using NewGame;
using Organizations;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        #region Variables

        private GameState _currentGameState = GameState.GameInitialization;

        private GameData _gameData;

        #endregion

        #region Events

        public static event Action<GameState> OnGameStateChanged;
        public static event Action<GameData> OnGameDataLoaded;
        public static event Action OnStartDataLoading;
        public static event Action OnAllowCharacterManagerInitialization;

        #endregion

        private void Start()
        {
            SubscribeToEvents();
            ChangeGameState(_currentGameState);
            StartCoroutine(InitializeGame());
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

        private IEnumerator InitializeGame()
        {
            Debug.Log($"Starting base game data initialization");
            yield return StartCoroutine(InitializeGameData());
            yield return new WaitForSeconds(5f);
            Debug.Log($"Base game data initialization finished. Changing game state to main menu");
            ChangeGameState(GameState.MainMenu);
            Debug.Log($"Game initialization finished");
        }
        
        private enum ControllerType
        {
            Culture,
            Background,
            Scenario,
            Organization,
            Character,
            Location
        }

        private IEnumerator InitializeGameData()
        {
            OnStartDataLoading?.Invoke();

            var loadingStates = new Dictionary<ControllerType, bool>
            {
                { ControllerType.Culture, false },
                { ControllerType.Background, false },
                { ControllerType.Scenario, false },
                { ControllerType.Organization, false },
                { ControllerType.Location, false }
            };
            
            Action onCultureLoaded = () => loadingStates[ControllerType.Culture] = true;
            Action onBackgroundLoaded = () => loadingStates[ControllerType.Background] = true;
            Action onScenarioLoaded = () => loadingStates[ControllerType.Scenario] = true;
            Action onOrganizationLoaded = () => loadingStates[ControllerType.Organization] = true;
            Action onLocationLoaded = () => loadingStates[ControllerType.Location] = true;

            CultureController.OnCultureControllerInitialized += onCultureLoaded;
            CharacterBackgroundController.OnCharacterBackgroundControllerInitialized += onBackgroundLoaded;
            ScenarioController.OnScenarioControllerInitialized += onScenarioLoaded;
            OrganizationManager.OnOrganizationManagerInitialized += onOrganizationLoaded;
            LocationManager.OnLocationManagerInitialized += onLocationLoaded;

            yield return new WaitUntil(() => loadingStates.Values.All(loaded => loaded));

            CultureController.OnCultureControllerInitialized -= onCultureLoaded;
            CharacterBackgroundController.OnCharacterBackgroundControllerInitialized -= onBackgroundLoaded;
            ScenarioController.OnScenarioControllerInitialized -= onScenarioLoaded;
            OrganizationManager.OnOrganizationManagerInitialized -= onOrganizationLoaded;
            LocationManager.OnLocationManagerInitialized -= onLocationLoaded;

            OnAllowCharacterManagerInitialization?.Invoke();

            var characterManagerLoaded = false;
            Action onCharacterManagerLoaded = () => characterManagerLoaded = true;

            CharacterManager.OnCharacterManagerInitialized += onCharacterManagerLoaded;

            yield return new WaitUntil(() => characterManagerLoaded);

            CharacterManager.OnCharacterManagerInitialized -= onCharacterManagerLoaded;
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
            ChangeGameState(GameState.InGameCityMap);
            LoadSavedGameData();
        }
        private void LoadSavedGameData()
        {
            var cityConfig = Resources.Load<TextAsset>("DataToLoad/gameData");
            if(cityConfig == null) return;

            var parsedCityConfigData = JsonUtility.FromJson<GameData>(cityConfig.text);

            if (parsedCityConfigData == null)
            {
                throw new Exception("Failed to parse city config data.");
            }

            _gameData.LocationData = parsedCityConfigData.LocationData;
            _gameData.OrganizationConstructors = parsedCityConfigData.OrganizationConstructors;
            _gameData.CharacterConstructors = parsedCityConfigData.CharacterConstructors;
            OnGameDataLoaded?.Invoke(_gameData);
        }
    }
    
    public enum GameState
    {
        GameInitialization,
        MainMenu,
        NewGameMenu,
        InGameCityMap,
        Loading,
        Saving,
        GameOver,
        Paused
    }
}