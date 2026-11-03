using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace BallGame.Input
{
    public sealed class InputReader : MonoBehaviour
    {
        private readonly List<RaycastResult> _uiRaycastResults = new();

        private GameInput _input;

        private PointerEventData _pointerEventData;

        public event Action PressReleased;

        public event Action PressStarted;

        public bool IsPressed { get; private set; }

        public void SetEnabled(bool isEnabled)
        {
            if (isEnabled)
            {
                _input.Gameplay.Enable();
                return;
            }

            Release();
            _input.Gameplay.Disable();
        }

        private void Awake()
        {
            _input = new GameInput();
        }

        private void OnEnable()
        {
            _input.Gameplay.Charge.started += OnChargeStarted;
            _input.Gameplay.Charge.canceled += OnChargeCanceled;
            _input.Gameplay.Enable();
        }

        private void OnDisable()
        {
            Release();
            _input.Gameplay.Charge.started -= OnChargeStarted;
            _input.Gameplay.Charge.canceled -= OnChargeCanceled;
            _input.Gameplay.Disable();
        }

        private void OnDestroy()
        {
            _input.Dispose();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                Release();
            }
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                Release();
            }
        }

        private void OnChargeStarted(InputAction.CallbackContext context)
        {
            if (IsPressed || IsPointerOverUi())
            {
                return;
            }

            IsPressed = true;
            PressStarted?.Invoke();
        }

        private void OnChargeCanceled(InputAction.CallbackContext context)
        {
            Release();
        }

        private void Release()
        {
            if (!IsPressed)
            {
                return;
            }

            IsPressed = false;
            PressReleased?.Invoke();
        }

        // EventSystem.IsPointerOverGameObject is unreliable inside Input System callbacks, so raycast the UI directly.
        private bool IsPointerOverUi()
        {
            var eventSystem = EventSystem.current;
            var pointer = Pointer.current;

            if (eventSystem == null || pointer == null)
            {
                return false;
            }

            _pointerEventData ??= new PointerEventData(eventSystem);
            _pointerEventData.position = pointer.position.ReadValue();

            _uiRaycastResults.Clear();
            eventSystem.RaycastAll(_pointerEventData, _uiRaycastResults);

            return _uiRaycastResults.Count > 0;
        }
    }
}
