using System.Collections.Generic;
using Sirenix.OdinInspector;
using Readymade.Utils.UI;
using Unity.Mathematics;
using UnityEngine;

namespace Readymade.Utils.UI
{
    public class LivePreviewEnvironment : MonoBehaviour
    {
        [Sirenix.OdinInspector.BoxGroup("Environment")]
        [Tooltip("The camera used to render the object.")]
        [SerializeField]
        [Sirenix.OdinInspector.Required]
        private new Camera camera;

        [Sirenix.OdinInspector.BoxGroup("Environment")]
        [Tooltip("The layer on which to render the objects.")]
        [SerializeField]
        private int layer;

        [Sirenix.OdinInspector.BoxGroup("Environment")]
        [Tooltip(
            "The root of the environment. Will be used to make the environment invisible to the main camera and physics.")]
        [SerializeField]
        [Sirenix.OdinInspector.Required]
        private Transform environmentRoot;

        [Sirenix.OdinInspector.BoxGroup("Environment")]
        [Tooltip("The parent under which the preview object will be instantiated.")]
        [SerializeField]
        [Sirenix.OdinInspector.Required]
        private Transform objectLocator;

        [Sirenix.OdinInspector.BoxGroup("Motion")]
        [Tooltip("Whether to constrain the orbit angle.")]
        [SerializeField]
        private bool constrainOrbit;

        [Sirenix.OdinInspector.BoxGroup("Motion")]
        [Tooltip("The y-axis rotational constraints when enabled.")]
        [Sirenix.OdinInspector.ShowIf(nameof(constrainOrbit))]
        [Sirenix.OdinInspector.MinMaxSlider(-180f, 180f)]
        [SerializeField]
        private Vector2 orbitRange = new(-180f, 180f);

        [Sirenix.OdinInspector.BoxGroup("Motion")]
        [Tooltip("The tilt angle constraint.")]
        [Sirenix.OdinInspector.MinMaxSlider(-80f, 80f)]
        [SerializeField]
        private Vector2 tiltRange = new Vector2(-40f, 40f);

        [Sirenix.OdinInspector.BoxGroup("Motion")]
        [Tooltip("The initial tilt angle.")]
        [SerializeField]
        [Range(-80f, 80f)]
        private float initialTilt = -55f;

        [Sirenix.OdinInspector.BoxGroup("Motion")]
        [Tooltip("The initial orbit angle.")]
        [SerializeField]
        [Range(-180f, 180f)]
        private float initialOrbit = -30f;

        [Sirenix.OdinInspector.BoxGroup("Motion")]
        [Tooltip("The responsiveness of the camera to rotational inputs.")]
        [SerializeField]
        [Range(1, 10f)]
        private float cameraSharpness = 5f;

        [Sirenix.OdinInspector.BoxGroup("Framing")]
        [Tooltip(
            "The distance factor is multiplied with the size of the object to determine the distance of the camera from the object.")]
        [SerializeField]
        [Range(1f, 10f)]
        private float distanceFactor = 3f;

        private float _orbit;
        private float _tilt;
        private static List<Renderer> s_renderers;
        private static readonly Vector3[] s_boundingBoxPoints = new Vector3[8];
        private static readonly Vector3[] s_localBoundsMinMax = new Vector3[2];


        public Camera Camera => camera;

        public Transform ObjectLocator => objectLocator;

        public Transform EnvironmentRoot => environmentRoot;

        public void OrbitBy(float angle)
            => SetOrbitSmooth(_orbit + angle);

        private void SetOrbit(float angle)
        {
            var next = constrainOrbit switch
            {
                true => Mathf.Clamp(angle, orbitRange.x, orbitRange.y),
                false => angle
            };
            _orbit = next;
        }

        private void SetOrbitSmooth(float angle)
        {
            float next = constrainOrbit switch
            {
                true => Mathf.Clamp(angle, orbitRange.x, orbitRange.y),
                false => angle
            };
            _orbit = Mathf.Lerp(_orbit, next, 1f - Mathf.Exp(-cameraSharpness * Time.deltaTime));
        }

        public void TiltBy(float angle)
            => SetTiltSmooth(_tilt + angle);

        private void SetTilt(float angle)
        {
            float next = Mathf.Clamp(angle, -80f, 80f);
            _tilt = next;
        }

        private void SetTiltSmooth(float angle)
        {
            float next = Mathf.Clamp(angle, -80f, 80f);
            _tilt = Mathf.Lerp(_tilt, next, 1f - Mathf.Exp(-cameraSharpness * Time.deltaTime));
        }

        public void Setup(GameObject prefab, Color color, RenderTexture output)
        {
            SetOrbit(initialOrbit);
            SetTilt(initialTilt);
            SetupObject(prefab);
            SetupCamera(color, output);
        }

        private void SetupObject(GameObject prefab)
        {
            for (int i = objectLocator.childCount - 1; i >= 0; i--)
            {
                Destroy(objectLocator.GetChild(i).gameObject);
            }

            GameObject instance = Instantiate(prefab, Vector3.zero, quaternion.identity, objectLocator);
            instance.transform.localScale = Vector3.one;
            SetLayerRecursively(instance.transform, layer);
        }

        private static void SetLayerRecursively(Transform trs, int layer)
        {
            trs.gameObject.layer = layer;
            for (int i = 0; i < trs.childCount; i++)
            {
                SetLayerRecursively(trs.GetChild(i), layer);
            }
        }

        private void SetupCamera(Color color, RenderTexture output)
        {
            camera.cullingMask = 1 << layer;
            camera.orthographic = false;
            camera.backgroundColor = color;
            camera.nearClipPlane = 0.1f;
            camera.targetTexture = output;
        }

        private void Update()
        {
            var cameraRotation = Quaternion.Euler(_tilt, _orbit, 0);
            PreviewGenerator.CalculateBounds(objectLocator, true, cameraRotation, out var bounds);
            camera.transform.position = bounds.center;
            camera.transform.rotation = cameraRotation;
            camera.transform.position -= camera.transform.forward * (bounds.extents.magnitude * distanceFactor);
            //PreviewGenerator.CalculateCameraPosition(camera, bounds, padding);
            camera.farClipPlane = (camera.transform.position - bounds.center).magnitude + bounds.size.magnitude;
        }

        public void Disable()
        {
            if (environmentRoot)
            {
                environmentRoot.gameObject.SetActive(false);
            }
        }

        public void Enable()
        {
            environmentRoot.gameObject.SetActive(true);
        }
    }
}