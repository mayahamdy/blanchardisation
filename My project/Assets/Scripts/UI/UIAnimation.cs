using UnityEngine;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Wraps Unity's Animation component with auto-play and loop support, and exposes a one-shot play method for external callers.
    /// </summary>
    [RequireComponent(typeof(Animation))]
    public class UIAnimation : MonoBehaviour
    {
        [SerializeField] private bool _auto;
        [SerializeField] private bool _loop;
        [SerializeField] private Animation _animation;

        /// <summary>
        /// Configures the wrap mode based on the loop setting and plays the animation automatically if auto-play is enabled.
        /// </summary>
        private void OnEnable()
        {
            _animation.wrapMode = _loop ? WrapMode.Loop : WrapMode.Once;

            if (_auto)
            {
                _animation.Play();
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Auto-populates the Animation reference in the editor if it has not been assigned manually.
        /// </summary>
        private void OnValidate()
        {
            if (_animation == null)
            {
                _animation = GetComponent<Animation>();
            }
        }
#endif

        /// <summary>
        /// Forces the animation to play exactly once, overriding any loop setting that may have been active.
        /// </summary>
        public void PlayOnce()
        {
            _animation.wrapMode = WrapMode.Once;
            _animation.Play();
        }
    }
}