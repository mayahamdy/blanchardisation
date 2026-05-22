using UnityEngine;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Handles fading in and out of a renderer's materials when the object occludes the camera's view of the ball.
    /// </summary>
    [Tooltip("Behaviour that handles fading objects that may get in the way of the ball.")]
    public class FadingObject : MonoBehaviour
    {
        [Tooltip("Which materials will be updated when they need to fade.")]
        public int[] MaterialIndices;

        [Tooltip("Reference to the renderer that will be updated.")]
        public Renderer MeshRenderer;

        /// <summary>
        /// The materials that will be updated
        /// </summary>
        private Material[] _instancedMaterials;

        private bool _fadeOut;

        public bool FadeOut
        {
            get => _fadeOut;

            set
            {
                if (_fadeOut != value)
                {
                    _fadeOut = value;

                    for (int i = 0; i < MaterialIndices.Length; i++)
                    {
                        Color c = _instancedMaterials[MaterialIndices[i]].color;
                        c.a = _fadeOut ? 0f : 1f;
                        _instancedMaterials[MaterialIndices[i]].color = c;
                    }
                }
            }
        }


        /// <summary>
        /// Captures instanced copies of the renderer's materials so that alpha changes do not affect shared material assets.
        /// </summary>
        private void Start()
        {
            _instancedMaterials = MeshRenderer.materials;
        }

        /// <summary>
        /// Cleans up the instanced materials.
        /// </summary>
        private void OnDestroy()
        {
            if (_instancedMaterials == null)
                return;

            for (int i = 0; i < _instancedMaterials.Length; i++)
            {
                if (_instancedMaterials[i] == null)
                    continue;

                Destroy(_instancedMaterials[i]);

                _instancedMaterials[i] = null;
            }

            _instancedMaterials = null;
        }
    }
}