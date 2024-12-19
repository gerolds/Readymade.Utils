using System;
using UnityEngine;
using UnityEngine.Serialization;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes;
#endif

namespace Readymade.Utils.UI
{
    /// <summary>
    /// Aligns the transform of a GameObject such that it faces the camera plane (Billboard) and compensates any foreshortening
    /// due to perspective.
    /// </summary>
    [ExecuteAlways]
    public class LookAtCameraMinMax : MonoBehaviour
    {
        [BoxGroup("Transform")]
        [Tooltip("Whether to adjust the transform's scale based on camera distance to compensate for distance.")]
        [SerializeField]
        public bool AdjustScale = true;

        [BoxGroup("Transform")]
        [Tooltip(
            "Whether to use min/max mode instead of scale mode. Min/max mode interpolates the scale between min and max " +
            "values based on camera distance relative to max distance. Scale mode applies a constant factor to camera distance " +
            "to calculate the scale.")]
        [SerializeField]
        public bool UseMinMaxMode = false;

        [BoxGroup("Transform")]
        [SerializeField]
        [Min(0)]
        [Tooltip("Control the scale of the transform relative to distance from camera.")]
        [FormerlySerializedAs("scale")]
        [ShowIf(nameof(AdjustScale))]
        [DisableIf(nameof(UseMinMaxMode))]
        public float Scale = .1f;

        [BoxGroup("Transform")]
        [SerializeField]
        [Min(0)]
        [Tooltip("The maximum scale allowed for this transform.")]
        [ShowIf(nameof(AdjustScale))]
        [EnableIf(nameof(UseMinMaxMode))]
        public float MaxScale = 2f;

        [BoxGroup("Transform")]
        [SerializeField]
        [Min(0)]
        [Tooltip("The minimum scale allowed for this transform.")]
        [ShowIf(nameof(AdjustScale))]
        [EnableIf(nameof(UseMinMaxMode))]
        public float MinScale = 0.25f;

        [BoxGroup("Transform")]
        [SerializeField]
        [Min(0)]
        [Tooltip("Distance at which the behaviour hides the canvas. Default is 50.")]
        private float _maxDistance = 50f;

        [BoxGroup("Transform")]
        [SerializeField]
        [Min(0)]
        [Tooltip(
            "Distance beyond max distance over which the canvas will be faded out, given the Canvas has a CanvasGroup component. Default is 10.")]
        private float _fadeDistance = 10f;

        [BoxGroup("RectTransform")]
        [SerializeField]
        [Min(0)]
        [Tooltip("The pivot of the Canvas' RectTransform.")]
        private Vector2 _pivot = new Vector2(0.5f, 0.5f);

        [BoxGroup("RectTransform")]
        [SerializeField]
        [Min(0)]
        [Tooltip("The size of the Canvas' RectTransform.")]
        private Vector2 _size = new Vector2(0.5f, 0.5f);

        [BoxGroup("Debug")]
        [SerializeField]
        [Tooltip(
            "Whether to execute updates in edit-mode (helpful for evaluating settings). This will dirty the scene and should be off when settings are found.")]
        private bool _executeInEditMode;

        private RectTransform _rectTransform;
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private Camera _camera;
        private Transform _camTrs;

        /// <summary>
        /// Event function.
        /// </summary>
        private void Reset()
        {
            ConformCanvas();
        }

        /// <summary>
        /// Event function.
        /// </summary>
        private void OnEnable()
        {
            ConformCanvas();
            Debug.Assert(!_canvas || _canvas.renderMode == RenderMode.WorldSpace,
                "ASSERTION FAILED: _canvas.renderMode == RenderMode.WorldSpace", this);
            Debug.Assert(Camera.main, "ASSERTION FAILED: Camera.main != null", this);
            _camera = Camera.main;
        }

        /// <summary>
        /// Event function.
        /// </summary>
        private void OnValidate()
        {
            if (_executeInEditMode)
            {
                ConformCanvas();
            }
        }

        /// <summary>
        /// Event function.
        /// </summary>
        private void LateUpdate()
        {
            if (!Application.isPlaying && !_executeInEditMode)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                _camera = UnityEditor.SceneView.lastActiveSceneView.camera;
            }
#endif
            if (!_camera)
            {
                return;
            }

            _camTrs = _camera.transform;
            Vector3 camPos = _camTrs.position;
            Plane camPlane = new(inPoint: camPos, inNormal: _camTrs.forward);
            Vector3 selfPos = transform.position;
            float camPlaneDistance = camPlane.GetDistanceToPoint(selfPos);
            float camDistance = Vector3.Distance(selfPos, camPos);

            // if we have a canvas group we fade the canvas.
            if (_canvasGroup)
            {
                if (_fadeDistance > 0 && camDistance > _maxDistance)
                {
                    _canvasGroup.alpha = 1f - Mathf.Clamp01((camDistance - _maxDistance) / _fadeDistance);
                }
                else
                {
                    _canvasGroup.alpha = 1f;
                }
            }

            // hide beyond max distance
            if (_canvas)
            {
                // if we have a canvas we disable it. We keep the gameobject active so we don't have to refresh the canvas when enabling it again
                if (camDistance > _maxDistance + _fadeDistance)
                {
                    _canvas.enabled = false;
                    return;
                }
                else
                {
                    _canvas.enabled = true;
                }
            }
            else
            {
                if (camDistance > _maxDistance + _fadeDistance)
                {
                    gameObject.SetActive(false);
                    return;
                }
                else
                {
                    gameObject.SetActive(true);
                }
            }

            // don't update when beyond max distance
            if (camDistance > _maxDistance + _fadeDistance || camPlaneDistance < _camera.nearClipPlane)
            {
                return;
            }

            Vector3 closestOnCamPlane = camPlane.ClosestPointOnPlane(selfPos);
            transform.SetPositionAndRotation(selfPos, Quaternion.LookRotation(selfPos - closestOnCamPlane, Vector3.up));
            if (UseMinMaxMode)
            {
                transform.localScale = AdjustScale
                    ? Vector3.one * Mathf.Lerp(MinScale, MaxScale, Mathf.Clamp01(camPlaneDistance / _maxDistance))
                    : Vector3.one;
            }
            else
            {
                transform.localScale = AdjustScale
                    ? Vector3.one * (Scale * camPlaneDistance)
                    : Vector3.one;
            }
        }


        /// <summary>
        /// Resets this Gamobject's transform to the identity transform.
        /// </summary>
        [Button]
        public void ResetTransform()
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            ConformCanvas();
        }

        /// <summary>
        /// Conforms the components on this GameObject to the requirements of this script.
        /// </summary>
        private void ConformCanvas()
        {
            if (!_canvas)
            {
                _canvas = GetComponent<Canvas>();
            }

            if (!_rectTransform)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            if (!_canvasGroup)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }


            if (_canvas)
            {
                _canvas.renderMode = RenderMode.WorldSpace;
            }

            if (_rectTransform)
            {
                _rectTransform.pivot = _pivot;
                _rectTransform.anchorMin = Vector2.zero;
                _rectTransform.anchorMax = Vector2.zero;
                _rectTransform.sizeDelta = _size;
            }

            transform.hideFlags = HideFlags.NotEditable;
        }
    }
}