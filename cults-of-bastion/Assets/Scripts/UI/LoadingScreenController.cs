using System;
using Managers;
using UnityEngine;

namespace UI
{
    public class LoadingScreenController : MonoBehaviour
    {
        [SerializeField] private GameObject loadingScreen;
        
        public static event Action OnLoadingFinished;
        private void Start()
        {
            loadingScreen.SetActive(true);
            GameManager.OnFinishLoading += HideLoadingScreen;
        }

        private void OnDestroy()
        {
            GameManager.OnFinishLoading -= HideLoadingScreen;
        }

        private void HideLoadingScreen()
        {
            loadingScreen.SetActive(false);
            OnLoadingFinished?.Invoke();
        }
    }
}