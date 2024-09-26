using System;
using System.Collections;
using NaughtyAttributes;
using UI;
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

        public static event Action OnSceneLoaded;
        public static event Action OnSceneUnloaded;
        public static event Action OnInGameScenesLoaded;

        #endregion

        private void Awake()
        {
            GameManager.OnGameStateChanged += SwitchSceneOnGameStateChange;
            GameManager.OnStartLoading += ShowLoadingScreen;
        }

        private void OnDestroy()
        {
            GameManager.OnGameStateChanged -= SwitchSceneOnGameStateChange;
            GameManager.OnStartLoading -= ShowLoadingScreen;
        }

        private void StartSceneLoading(string sceneName)
        {
            StartCoroutine(LoadScene(sceneName));
        }

        private void StartSceneUnloading(string sceneName)
        {
            StartCoroutine(UnloadScene(sceneName));
        }

        private void ShowLoadingScreen()
        {
            StartCoroutine(WaitForLoading());
        }
        private IEnumerator WaitForLoading()
        {
            StartSceneLoading(loadingScene);
            var loadingInProcess = true;
            Action onLoadingFinished = () => loadingInProcess = false;
            
            LoadingScreenController.OnLoadingFinished += onLoadingFinished;
            yield return new WaitUntil(() => !loadingInProcess);
            LoadingScreenController.OnLoadingFinished -= onLoadingFinished;
            StartSceneUnloading(loadingScene);
        }

        private void SwitchSceneOnGameStateChange(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    StartSceneLoading(mainMenuScene);
                    break;
                case GameState.LoadingGame:
                    StartSceneLoading("TestCityMapScene");
                    StartSceneLoading("PlayerUIScene");
                    StartSceneUnloading(mainMenuScene);
                    OnInGameScenesLoaded?.Invoke();
                    break;
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
