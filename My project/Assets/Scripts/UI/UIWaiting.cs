using Quantum;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UI = UnityEngine.UI;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Displays the start panel when the game is in the waiting state, with a button to start the game.
    /// </summary>
    public class UIWaiting : QuantumSceneViewComponent
    {
        public GameObject Panel;
        public UI.Button StartButton;

        /// <summary>
        /// Sets up the start button and hides the panel on startup.
        /// </summary>
        private void Awake()
        {
            if (StartButton != null)
            {
                StartButton.onClick.AddListener(OnStartButtonClicked);
            }
            Panel.SetActive(false);
        }

        /// <summary>
        /// Shows the start panel when the game is in the waiting state, and checks for space key input.
        /// </summary>
        public override void OnUpdateView()
        {
            if (Game.Frames.Verified.TryGetSingleton(out TurnContainer turnContainer) == false)
                return;

            Panel.SetActive(turnContainer.CurrentTurn.Type == ETurnType.Waiting);
            
            // Allow space key to start the game
            if (turnContainer.CurrentTurn.Type == ETurnType.Waiting && 
                Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                OnStartButtonClicked();
            }
        }

        /// <summary>
        /// Sends a start game command when the start button is clicked.
        /// </summary>
        private void OnStartButtonClicked()
        {
            Debug.Log("Sending Start Game Command.");
            CommandDispatcher.Instance.SendStartCommand();
        }

        /// <summary>
        /// Unsubscribes from the button click event when destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (StartButton != null)
            {
                StartButton.onClick.RemoveListener(OnStartButtonClicked);
            }
        }
    }
}