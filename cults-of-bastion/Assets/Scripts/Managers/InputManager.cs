using UnityEngine;

namespace Managers
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        public PlayerInputControls PlayerInputControls;
        
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
            PlayerInputControls = new PlayerInputControls();
            PlayerInputControls.Enable();
        }
    }
}