using Quantum;
using UnityEngine;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Activates the main content panel as soon as the Quantum frame becomes available.
    /// </summary>
    public class UIUpperLayout : QuantumSceneViewComponent
    {
        public GameObject Content;

        /// <summary>
        /// Shows the content panel when the view is first activated with a valid frame.
        /// </summary>
        public override void OnActivate(Frame frame)
        {
            Content.SetActive(true);
        }
    }
}