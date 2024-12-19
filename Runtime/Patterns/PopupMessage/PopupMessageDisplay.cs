#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes;
#endif
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Readymade.Utils.Patterns.PopupMessage {
    public class PopupMessageDisplay : MonoBehaviour {
        [Required]
        public TMP_Text message;

        [Required]
        public Button confirmation;

        [Required]
        public CanvasGroup group;

        [FormerlySerializedAs ( "color" )]
        [Required]
        public Image swatch;
    }
}