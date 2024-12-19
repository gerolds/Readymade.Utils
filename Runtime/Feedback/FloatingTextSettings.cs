using com.convalise.UnityMaterialSymbols;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes;
#endif

namespace Readymade.Utils.Feedback
{
    /// <summary>
    /// Configuration for instantiating and animating <see cref="FloatingText"/> through a <see cref="FloatingTextSpawner"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "New " + nameof(FloatingTextSettings),
        menuName = nameof(Readymade) + "/" + nameof(Feedback) + "/" + nameof(FloatingTextSettings))]
    public class FloatingTextSettings : ScriptableObject
    {
        [BoxGroup("Appearance")]
        [SerializeField]
        private MaterialSymbolData iconData;

        [BoxGroup("Appearance")]
        [SerializeField]
        private float randomizePosition;

        [BoxGroup("Value")]
        [SerializeField]
        [Tooltip("Whether to display the value. It can make sense to ommit the value and only display an icon.")]
        private bool showValue = true;

        [BoxGroup("Value")]
        [SerializeField]
        [Tooltip("Whether to append a suffix to the value display. Default is false.")]
        [ShowIf(nameof(ShowValue))]
        private bool appendSuffix;

        [BoxGroup("Value")]
        [SerializeField]
        [EnableIf(nameof(AppendSuffix))]
        [Tooltip("The suffix to append to the value display.")]
        [ShowIf(nameof(ShowValue))]
        private string suffix = string.Empty;

        [BoxGroup("Animation")]
        [SerializeField]
        private float delay;

        [BoxGroup("Animation")]
        [SerializeField]
        [Tooltip("Local offset, relative to the spawn position received in SpawnText() where the floating prefab" +
            " is spawned. It is assumed that the position given in the SpawnText method is the position of the " +
            "object and does not presuppose anything about the the FX to be played.")]
        private Vector3 animationOffset = new(0.0f, 1.5f, 0.0f);

        [BoxGroup("Animation")]
        [SerializeField]
        [Min(0)]
        [Tooltip("The time in seconds the text is visible. Default is 2.")]
        private float lifeTime = 2f;

        [BoxGroup("Animation")]
        [SerializeField]
        [Tooltip("The scale of the canvas at the end. Default is 0.1.")]
        [EnableIf("AnimateScale")]
        private float scaleAtEnd = 0.1f;

        [BoxGroup("Animation")]
        [SerializeField]
        [Tooltip("Whether scale should be animated. Default is false.")]
        private bool animateScale;

        [BoxGroup("Animation")]
        [SerializeField]
        [Tooltip("The scale of the canvas when spawning. Default is 0.3.")]
        private float scale = 0.3f;

        [BoxGroup("Animation")]
        [SerializeField]
        [Tooltip("Offsets the spawn position.")]
        private Vector3 spawnOffset = new Vector3(0.0f, 1.5f, 0.0f);

        [BoxGroup("Appearance")]
        [SerializeField]
        [Tooltip("The color to use for negative values.")]
        private Color negativeColor = Color.red;

        [BoxGroup("Appearance")]
        [SerializeField]
        [Tooltip("The color to be used for positive values.")]
        private Color positiveColor = Color.white;

        [BoxGroup("Value")]
        [SerializeField]
        [Tooltip(
            "The format to use for displaying numeric values. Default is '{0}'. For fractional values and unity use something like '${0.00}'.")]
        [ShowIf(nameof(ShowValue))]
        [DisableIf(nameof(HumanReadable))]
        private string valueFormat = "{0}";

        [BoxGroup("Value")]
        [SerializeField]
        [Tooltip("Whether the always display the sign of numbers. Default is false.")]
        [ShowIf(nameof(ShowValue))]
        [DisableIf(nameof(HumanReadable))]
        private bool showSign;

        [BoxGroup("Value")]
        [Tooltip(
            "Whether to format large values into a friendlier, more readable format. Enabling this will have a performance impact.")]
        [SerializeField]
        [ShowIf(nameof(ShowValue))]
        private bool humanReadable;

        public bool HumanReadable => humanReadable;

        /// <summary>
        /// Whether to always show the sign of numbers.
        /// </summary>
        public bool ShowSign => showSign;

        /// <summary>
        /// The format to use for displaying numeric values. This is a <see cref="TextMesh"/> format string, not a C# format string.
        /// </summary>
        public string ValueFormat => valueFormat;

        /// <summary>
        /// The color to be used for positive values.
        /// </summary>
        public Color PositiveColor => positiveColor;

        /// <summary>
        /// The color used for negative values.
        /// </summary>
        public Color NegativeColor => negativeColor;

        /// <summary>
        /// Offsets the spawn position.
        /// </summary>
        public Vector3 SpawnOffset => spawnOffset;

        /// <summary>
        /// The scale of the canvas when spawning the text.
        /// </summary>
        public float Scale => scale;

        /// <summary>
        /// Whether scale be animated.
        /// </summary>
        public bool AnimateScale => animateScale;

        /// <summary>
        /// The scale of the canvas at the end of the lifeTime.
        /// </summary>
        public float ScaleAtEnd => scaleAtEnd;

        /// <summary>
        /// Controlling the time it takes to fade out the text and the pooledInstance gets returned to the pool.
        /// </summary>
        public float LifeTime => lifeTime;

        /// <summary>
        /// Offset to the spawnPosition for animating the text.
        /// </summary>
        public Vector3 AnimationOffset => animationOffset;

        public float Delay => delay;

        public float RandomizePosition => randomizePosition;

        /// <summary>
        /// Whether to always show the sign of numbers.
        /// </summary>
        public bool ShowValue => showValue;

        /// <summary>
        /// Should the UnitText be enabled when spawning.
        /// </summary>
        public bool AppendSuffix => appendSuffix;

        /// <summary>
        /// A suffix to append to the value display.
        /// </summary>
        public string Suffix => suffix;

        public MaterialSymbolData Icon => iconData;
    }
}