using System.Collections.Generic;
using UnityEngine;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Raycasts between the camera and the current target to detect occluding objects and fades them out.
    /// </summary>
    [RequireComponent(typeof(Camera), typeof(CameraController))]
    [Tooltip(
        "Behaviour that test if there are any obstacles between the camera and current target and fades them out if so.")]
    public class FadingController : MonoBehaviour
    {
        [Tooltip("The layer to query for the raycast.")]
        public LayerMask Layer;

        /// <summary>
        /// The list of objects that are currently hit by the collider.
        /// </summary>
        private List<FadingObject> _fadingObjectList = new List<FadingObject>();

        /// <summary>
        /// Cached reference to the camera.
        /// </summary>
        private Camera _camera;

        /// <summary>
        /// Cached reference to the Camera Controller.
        /// </summary>
        private CameraController _cameraController;

        [Tooltip("A slight margin between the target and camera to prevent false positives")]
        public float Margin = 0.1f;

        RaycastHit[] _raycastHitResults = new RaycastHit[4];

        /// <summary>
        /// Caches the Camera and CameraController components from the same GameObject.
        /// </summary>
        private void Start()
        {
            _camera = GetComponent<Camera>();
            _cameraController = GetComponent<CameraController>();
        }

        /// <summary>
        /// Casts a ray from the camera towards the current target each physics step and fades any occluding FadingObjects.
        /// </summary>
        private void FixedUpdate()
        {
            if (_cameraController.TargetPosition.HasValue == false)
                return;

            Vector3 cameraToTarget = _cameraController.TargetPosition.Value - _camera.transform.position;
            Ray ray = new Ray(_cameraController.transform.position - _cameraController.transform.forward,
                cameraToTarget.normalized);

            float dist = cameraToTarget.magnitude - Margin;
            int hits = Physics.RaycastNonAlloc(ray, _raycastHitResults, dist, Layer);

            if (hits == 0)
            {
                UpdateFadeObject(null);
            }
            else
            {
                for (int i = 0; i < hits; i++)
                {
                    UpdateFadeObject(_raycastHitResults[i].collider.GetComponent<FadingObject>());
                }
            }
        }

        /// <summary>
        /// Adds a newly detected occluder to the fade list, or clears all faded objects when the ray hits nothing.
        /// </summary>
        private void UpdateFadeObject(FadingObject fadingObject)
        {
            if (_fadingObjectList.Contains(fadingObject))
                return;

            if (fadingObject == null)
            {
                for (int i = _fadingObjectList.Count - 1; i >= 0; i--)
                {
                    _fadingObjectList[i].FadeOut = false;
                    _fadingObjectList.RemoveAt(i);
                }

                return;
            }

            fadingObject.FadeOut = true;
            _fadingObjectList.Add(fadingObject);
        }
    }
}