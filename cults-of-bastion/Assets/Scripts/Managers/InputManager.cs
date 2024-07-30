using UnityEngine;

namespace Managers
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        public PlayerInputControls PlayerInputControls;
        
        private void Awake()
        {
            Instance = this;
            
            CreateNewInput();
        }
        private void OnDestroy()
        {
            PlayerInputControls.Disable();
        }
        private void CreateNewInput()
        {
            PlayerInputControls = new PlayerInputControls();
            PlayerInputControls.Enable();
        }

    }
}