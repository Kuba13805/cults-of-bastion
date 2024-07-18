using System;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        private GameState _currentGameState;
        
        public static event Action<GameState> OnGameStateChanged;

        private void Start()
        {
            ChangeGameState(GameState.Menu);
        }

        private void ChangeGameState(GameState state)
        {
            _currentGameState = state;
            OnGameStateChanged?.Invoke(state);
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