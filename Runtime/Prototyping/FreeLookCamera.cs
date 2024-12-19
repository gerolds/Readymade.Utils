using UnityEngine;
using UnityEngine.InputSystem;

namespace Readymade.Utils.Prototyping
{
    public class FreeLookCamera : MonoBehaviour
    {
        private Vector2 _mouseAbsolute;
        private Vector2 _smoothMouse;

        [SerializeField]
        private Vector2 clampInDegrees = new Vector2(360, 180);

        [SerializeField]
        private Vector2 sensitivity = new(2, 2);

        [SerializeField]
        private Vector2 smoothing = new(3, 3);

        [SerializeField] 
        private bool requireLockedCursor = true;

        private Vector2 _targetDirection;

        private void Start()
        {
            _targetDirection = transform.rotation.eulerAngles;
        }

        private void LateUpdate()
        {
            // maintain orientation while cursor is unlocked
            if (requireLockedCursor && Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            // Allow the script to clamp based on a desired target value.
            Quaternion targetOrientation = Quaternion.Euler(_targetDirection);

            // Get raw mouse input for a cleaner reading on more sensitive mice.
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            // Scale input against the sensitivity setting and multiply that against the smoothing value.
            mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));

            // Interpolate mouse movement over time to apply smoothing delta.
            _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
            _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

            // Find the absolute mouse movement value from point zero.
            _mouseAbsolute += _smoothMouse;

            // Clamp and apply the local x value first, so as not to be affected by world transforms.
            if (clampInDegrees.x < 360f)
                _mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);

            var xRotation = Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right);
            transform.localRotation = xRotation;

            // Then clamp and apply the global y value.
            if (clampInDegrees.y < 360f)
                _mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);

            var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
            transform.localRotation *= yRotation;
            transform.rotation *= targetOrientation;
        }
    }
}