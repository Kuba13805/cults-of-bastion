using System;
using UnityEngine;

namespace Managers
{
    public class UIController : MonoBehaviour
    {
        public static event Action OnPauseGame;
        public static event Action OnResumeGameWithNormalSpeed;
        public static event Action OnResumeGameWithHighSpeed;
        public void PauseGame()
        {
            OnPauseGame?.Invoke();
        }

        public void ResumeGameWithNormalSpeed()
        {
            OnResumeGameWithNormalSpeed?.Invoke();
        }
        public void ResumeGameWithHighSpeed()
        {
            OnResumeGameWithHighSpeed?.Invoke();
        }
    }
}
