#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes;
#endif
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Readymade.Utils.Portals
{
    public class TransitScreenDisplay : MonoBehaviour
    {
        [FormerlySerializedAs("canvasGroup")] [SerializeField] [Required] public CanvasGroup fadeGroup;
        [SerializeField] [Required] public TMP_Text info;
    }
}