using UnityEngine;

namespace Managers
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        public PlayerInputControls playerInputControls;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
            
            CreateNewInput();
        }
        private void CreateNewInput()
        {
            playerInputControls = new PlayerInputControls();
            playerInputControls.Enable();
        }
    }
}