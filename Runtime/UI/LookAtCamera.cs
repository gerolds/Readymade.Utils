using UnityEngine;
using UnityEngine.Serialization;

namespace Readymade.Utils.UI {
    /// <summary>
    /// Aligns the transform of a GameObject such that it faces the camera plane (Billboard) and compensates any foreshortening
    /// due to perspective.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class LookAtCamera : MonoBehaviour
    {
        [SerializeField]
        [Min(0)]
        [Tooltip("Control the scale of the transform relative to distance from camera.")]
        private float scale = .1f;

        [FormerlySerializedAs("perspective")]
        [SerializeField]
        [Min(0)]
        [Tooltip("Control the amount of foreshortening still applied to the scale.")]
        private float perspectiveMix = .8f;

        [SerializeField]
        [Min(0)]
        [Tooltip("Distance at which the behaviour hides the canvas.")]
        private float maxDistance = 50f;

        private Transform _selfTrs;
        private Canvas _canvas;
        private Camera _camera;
        private Transform _camTrs;

        public float Scale
        {
            get => scale;
            set => scale = value;
        }

        private void OnEnable()
        {
            _canvas = GetComponent<Canvas>();
            Debug.Assert(_canvas, "_canvas != null");
            Debug.Assert(_canvas.renderMode == RenderMode.WorldSpace, "_canvas.renderMode == RenderMode.WorldSpace");
            Debug.Assert(Camera.main, "Camera.main");
            _selfTrs = transform;
            _camera = Camera.main;
            _camTrs = _camera.transform;
        }

        private void Update()
        {
            if (!_camera)
            {
                return;
            }

            Vector3 camPos = _camTrs.position;
            Plane camPlane = new(inPoint: camPos, inNormal: _camTrs.forward);
            Vector3 selfPos = _selfTrs.position;
            Vector3 delta = camPos - selfPos;
            float camDistanceSquared = delta.sqrMagnitude;
            float maxSquared = maxDistance * maxDistance;

            // hide beyond max distance
            if (_canvas)
            {
                if (camDistanceSquared > maxSquared)
                {
                    _canvas.enabled = false;
                    return;
                }
                else
                {
                    _canvas.enabled = true;
                }
            }

            // don't update when beyond max distance
            if (camDistanceSquared > maxSquared)
            {
                return;
            }

            float camDistance = Mathf.Sqrt(camDistanceSquared);
            float camPlaneDistance = camPlane.GetDistanceToPoint(selfPos);
            Vector3 closestOnCamPlane = camPlane.ClosestPointOnPlane(selfPos);
            _selfTrs.SetPositionAndRotation(selfPos, Quaternion.LookRotation(selfPos - closestOnCamPlane, Vector3.up));
            transform.localScale =
                Vector3.one * (scale * Mathf.Lerp(camPlaneDistance, 1f / camDistance, perspectiveMix));
        }
    }
}