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
        public static event Action<GameData> OnGameDataInitialized;
        public static event Action OnStartDataLoading;
        public static event Action OnAllowCharacterManagerInitialization;
        public static event Action OnStartLoading;
        public static event Action OnFinishLoading;

        #endregion

        private void Start()
        {
            SubscribeToEvents();
            OnStartLoading?.Invoke();
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
            OnFinishLoading?.Invoke();
            Debug.Log($"Game initialization finished");
        }
        
        private enum ControllerType
        {
            Culture,
            Background,
            Scenario,
            Organization,
            Location
        }

        private static IEnumerator InitializeGameData()
        {
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
            OnStartDataLoading?.Invoke();

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
            OnStartLoading?.Invoke();
            ChangeGameState(GameState.LoadingGame);
            InitializeSavedGameData();
        }
        private void InitializeSavedGameData()
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
            StartCoroutine(WaitForSavedGameDataLoading());
        }

        private IEnumerator WaitForSavedGameDataLoading()
        {
            var organizationDataLoaded = false;
            var locationDataLoaded = false;
            var characterDataLoaded = false;
            Action onOrganizationDataLoaded = () => organizationDataLoaded = true;
            Action onLocationDataLoaded = () => locationDataLoaded = true;
            Action onCharacterDataLoaded = () => characterDataLoaded = true;
            
            OrganizationManager.OnOrganizationLoadingFinished += onOrganizationDataLoaded;
            LocationManager.OnLocationLoadingFinished += onLocationDataLoaded;
            CharacterManager.OnCharactersLoaded += onCharacterDataLoaded;
            OnGameDataInitialized?.Invoke(_gameData);
            
            yield return new WaitUntil(() => organizationDataLoaded && locationDataLoaded && characterDataLoaded);
            
            OrganizationManager.OnOrganizationLoadingFinished -= onOrganizationDataLoaded;
            LocationManager.OnLocationLoadingFinished -= onLocationDataLoaded;
            CharacterManager.OnCharactersLoaded -= onCharacterDataLoaded;
            yield return new WaitForSeconds(5f);
            OnFinishLoading?.Invoke();
        }
    }
    
    public enum GameState
    {
        GameInitialization,
        MainMenu,
        InGame,
        LoadingGame,
        Saving,
        GameOver,
        Paused,
        QuitingToMenu
    }
}