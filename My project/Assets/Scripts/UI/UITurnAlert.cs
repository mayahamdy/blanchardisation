using System.Collections;
using UnityEngine;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Disabled for single-player mode. Previously displayed an alert when a player attempted to interact during another player's turn.
    /// </summary>
    public class UITurnAlert : MonoBehaviour
    {
        public GameObject TurnAlert;

        /// <summary>
        /// Hides the alert panel on startup. Event subscription removed for single-player mode.
        /// </summary>
        private void Start()
        {
            // Disabled for single-player: InputManager.OnNotYourTurnAlert += ShowAlert;
            TurnAlert.SetActive(false);
        }

        /// <summary>
        /// Starts the coroutine that briefly shows then hides the turn alert.
        /// </summary>
        private void ShowAlert()
        {
            StartCoroutine(ShowAlertCoroutine());
        }

        /// <summary>
        /// Shows the alert panel, waits one second, then hides it again.
        /// </summary>
        private IEnumerator ShowAlertCoroutine()
        {
            TurnAlert.SetActive(true);
            yield return new WaitForSeconds(1);
            TurnAlert.SetActive(false);
        }

        /// <summary>
        /// Cleanup removed for single-player mode (no event subscription).
        /// </summary>
        private void OnDestroy()
        {
            // Disabled for single-player: InputManager.OnNotYourTurnAlert -= ShowAlert;
        }
    }
}