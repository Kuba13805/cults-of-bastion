using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class SceneManager : MonoBehaviour
    {
        #region Variables
        
        [SerializeField][Scene] private string loadingScene;
        [SerializeField][Scene] private string mainMenuScene;
        private string _currentSceneName;

        #endregion
        #region Events

        public static event Action OnStartSceneLoading;
        public static event Action OnStartSceneUnloading;
        public static event Action OnSceneLoaded;
        public static event Action OnSceneUnloaded;

        #endregion

        private void Awake()
        {
            GameManager.OnGameStateChanged += SwitchSceneOnGameStateChange;
        }

        private void OnDestroy()
        {
            GameManager.OnGameStateChanged -= SwitchSceneOnGameStateChange;
        }

        private void StartSceneLoading(string sceneName)
        {
            OnStartSceneLoading?.Invoke();
            StartCoroutine(LoadScene(sceneName));
        }

        private void StartSceneUnloading(string sceneName)
        {
            OnStartSceneUnloading?.Invoke();
            StartCoroutine(UnloadScene(sceneName));
        }

        private void SwitchSceneOnGameStateChange(GameState state)
        {
            switch (state)
            {
                case GameState.GameInitialization:
                    StartSceneLoading(loadingScene);
                    break;
                case GameState.MainMenu:
                    StartSceneLoading(mainMenuScene);
                    StartSceneUnloading(loadingScene);
                    break;
                case GameState.NewGameMenu:
                    break;
                case GameState.InGameCityMap:
                    StartSceneLoading("TestCityMapScene");
                    StartSceneLoading("PlayerUIScene");
                    StartSceneUnloading(mainMenuScene);
                    break;
                case GameState.Loading:
                    break;
                case GameState.Saving:
                    break;
                case GameState.GameOver:
                    break;
                case GameState.Paused:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private IEnumerator LoadScene(string sceneName)
        {
            var asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!asyncOperation.isDone)
            {
                yield return null;
            }

            _currentSceneName = sceneName;
            OnSceneLoaded?.Invoke();
        }
        private static IEnumerator UnloadScene(string sceneName)
        {
            var asyncOperation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
            while (!asyncOperation.isDone)
            {
                yield return null;
            }
            OnSceneUnloaded?.Invoke();
        }
    }
}
