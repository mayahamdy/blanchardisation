using Quantum;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using Touch = UnityEngine.InputSystem.Controls.TouchControl;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Singleton that reads mouse and touch input each frame and broadcasts typed input events to other components.
    /// </summary>
    public delegate void InputEvent(Vector2 viewportPos);

    public delegate void MultipleTouchEvent(ReadOnlyArray<Touch> touches);

    public delegate void AlertEvent();

    public sealed class InputManager : QuantumSceneViewComponent
    {
        public static InputManager Instance { get; private set; }
        public static event InputEvent OnInputDown;
        public static event InputEvent OnInputDrag;
        public static event InputEvent OnInputUp;
        public static event InputEvent OnMouseScrollChanged;
        public static event MultipleTouchEvent OnMultipleTouchEvent;

        private Camera _mainCamera;

        /// <summary>
        /// Enforces the singleton pattern, sets the target frame rate, and caches the main camera.
        /// </summary>
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

            Application.targetFrameRate = 60;
            _mainCamera = Camera.main;
        }

        /// <summary>
        /// Each frame, reads input and delegates to RaiseInputEvents for the local player.
        /// </summary>
        public override void OnUpdateView()
        {
            Frame frame = Game.Frames.Verified;
            TurnContainer turnContainer = frame.GetSingleton<TurnContainer>();
            if (turnContainer.CurrentTurn.Type != ETurnType.Play)
                return;

            RaiseInputEvents();
        }

        /// <summary>
        /// Reads the current mouse and touch state and fires the appropriate typed input events for down, drag, up, and scroll.
        /// </summary>
        private void RaiseInputEvents()
        {
            if (Touchscreen.current != null && Touchscreen.current.touches.Count >= 2 && OnMultipleTouchEvent != null)
            {
                OnMultipleTouchEvent(Touchscreen.current.touches);
            }
            else
            {
                if (_mainCamera == null)
                    return;

                if (Mouse.current == null)
                    return;

                if (Mouse.current.leftButton.wasPressedThisFrame && OnInputDown != null)
                {
                    OnInputDown(_mainCamera.ScreenToViewportPoint(Mouse.current.position.ReadValue()));
                }
                else if (Mouse.current.leftButton.isPressed && OnInputDrag != null)
                {
                    OnInputDrag(_mainCamera.ScreenToViewportPoint(Mouse.current.position.ReadValue()));
                }

                if (Mouse.current.leftButton.wasReleasedThisFrame && OnInputUp != null)
                {
                    OnInputUp(_mainCamera.ScreenToViewportPoint(Mouse.current.position.ReadValue()));
                }

                if (Math.Abs(Mouse.current.scroll.y.ReadValue()) > 0.01 && OnMouseScrollChanged != null)
                {
                    OnMouseScrollChanged(_mainCamera.ScreenToViewportPoint(Mouse.current.scroll.ReadValue()));
                }
            }
        }
    }
}