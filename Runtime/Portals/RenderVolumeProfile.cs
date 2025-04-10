using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Readymade.Utils.Portals
{
    [CreateAssetMenu(menuName = nameof(App) + "/" + nameof(Core) + "/" + nameof(RenderVolumeProfile),
        fileName = "New " + nameof(RenderVolumeProfile),
        order = 0)]
    public class RenderVolumeProfile : ScriptableObject
    {
        [BoxGroup("Sky")] [SerializeField] private Material skybox;

        [BoxGroup("Environment Lighting")]
        [SerializeField]
        [Range(0, 1)]
        private float ambientIntensity = 1f;

        [BoxGroup("Environment Lighting")]
        [SerializeField]
        private AmbientMode ambientMode = AmbientMode.Skybox;

        [BoxGroup("Environment Lighting")]
        [SerializeField]
        [ShowIf(nameof(ambientMode), AmbientMode.Flat)]
        private Color ambientLight = new(0.9f, 0.875f, 0.8f);

        [BoxGroup("Fog")] [SerializeField] private bool fog;

        [BoxGroup("Fog")]
        [ShowIf(nameof(fog))]
        [SerializeField]
        [Min(0)]
        private float fogEndDistance = 50;

        [BoxGroup("Fog")]
        [ShowIf(nameof(fog))]
        [SerializeField]
        [Min(0)]
        private float fogStartDistance = 500;

        [BoxGroup("Fog")]
        [ShowIf(nameof(fog))]
        [SerializeField]
        private FogMode fogMode = FogMode.Linear;

        [BoxGroup("Fog")]
        [ShowIf(nameof(fog))]
        [SerializeField]
        private Color fogColor = Color.white;

        [BoxGroup("Fog")]
        [ShowIf(nameof(fog))]
        [SerializeField]
        private float fogDensity = 0.01f;

        [BoxGroup("Environment Reflections")]
        [SerializeField]
        private DefaultReflectionMode defaultReflectionMode = DefaultReflectionMode.Skybox;

        [BoxGroup("Environment Reflections")]
        [SerializeField]
        [Range(0, 1)]
        private float reflectionIntensity = 1;

        [SerializeField] private Color subtractiveShadowColor;

        [BoxGroup("Environment Lighting")]
        [ColorUsage(false, true)]
        [SerializeField]
        [ShowIf(nameof(ambientMode), AmbientMode.Trilight)]
        private Color ambientSkyColor = new(0.9f, 0.875f, 0.8f);

        [BoxGroup("Environment Lighting")]
        [ColorUsage(false, true)]
        [SerializeField]
        [ShowIf(nameof(ambientMode), AmbientMode.Trilight)]
        private Color ambientEquatorColor = new(0.3f, 0.3f, 0.3f);

        [BoxGroup("Environment Lighting")]
        [ColorUsage(false, true)]
        [SerializeField]
        [ShowIf(nameof(ambientMode), AmbientMode.Trilight)]
        private Color ambientGroundColor = new(0.08f, 0.09f, 0.1f);

        [Range(0, 1)]
        [BoxGroup("Halo")]
        [SerializeField]
        private float haloStrength = 0.5f;

        [Min(0)]
        [BoxGroup("Flares")]
        [SerializeField]
        private float flareFadeSpeed = 3;

        [Range(0, 1)]
        [BoxGroup("Flares")]
        [SerializeField]
        private float flareStrength = 1f;

        [SerializeField] private float priority = 0;

        private DropdownList<int> _resolutions = new()
        {
            { "16", 16 },
            { "32", 32 },
            { "64", 64 },
            { "128", 128 },
            { "256", 256 },
            { "512", 512 },
            { "1024", 1024 },
            { "2048", 2048 },
        };

        public float Priority => priority;
        public bool Fog => fog;
        public float FogEndDistance => fogEndDistance;
        public float FogStartDistance => fogStartDistance;
        public FogMode FogMode => fogMode;
        public Color FogColor => fogColor;
        public float FogDensity => fogDensity;
        public AmbientMode AmbientMode => ambientMode;
        public Color AmbientLight => ambientLight;
        public Color AmbientSkyColor => ambientSkyColor;
        public Color AmbientEquatorColor => ambientEquatorColor;
        public Color AmbientGroundColor => ambientGroundColor;
        public float AmbientIntensity => ambientIntensity;
        public DefaultReflectionMode DefaultReflectionMode => defaultReflectionMode;
        public float ReflectionIntensity => reflectionIntensity;
        public Color SubtractiveShadowColor => subtractiveShadowColor;
        public float HaloStrength => haloStrength;
        public float FlareFadeSpeed => flareFadeSpeed;
        public float FlareStrength => flareStrength;
        public Material Skybox => skybox;
    }
}