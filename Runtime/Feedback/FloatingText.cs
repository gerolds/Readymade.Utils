using com.convalise.UnityMaterialSymbols;
using Readymade.Utils.Pooling;
using Readymade.Utils.UI;
using TMPro;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes;
#endif

namespace Readymade.Utils.Feedback
{
    /// <summary>
    /// Component that can be used to acquire pooled instances of type PooledFloatingText. A pool will be automatically created for these instances with the settings given in the prefab.
    /// </summary>
    public class FloatingText : PoolableObject<FloatingText>
    {
        [SerializeField]
        [Required]
        [Tooltip("The text that displays a suffix to the value.")]
        private TMP_Text suffixText;

        [SerializeField]
        [Required]
        [Tooltip("The image that displays the associated icon.")]
        private MaterialSymbol icon;

        [SerializeField]
        [Tooltip("The " + nameof(LookAtCamera) + " component on this object.")]
        private LookAtCameraMinMax lookAtCameraComponent;

        [SerializeField]
        [Tooltip("The transform that will be animated.")]
        private Transform animationTarget;

        [SerializeField]
        [Tooltip("The canvas group to animate visibility and interactivity of the prefab.")]
        private CanvasGroup canvasGroup;

        [Tooltip("The text that displays the value.")]
        [Required]
        [SerializeField]
        private TMP_Text valueText;

        /// <summary>
        /// The text field of the component.
        /// </summary>
        public TMP_Text ValueText => valueText;

        public TMP_Text SuffixText => suffixText;

        public MaterialSymbol Icon => icon;

        public LookAtCameraMinMax LookAtCameraComponent => lookAtCameraComponent;

        public Transform AnimationTarget => animationTarget;

        public CanvasGroup Group => canvasGroup;
    }
}