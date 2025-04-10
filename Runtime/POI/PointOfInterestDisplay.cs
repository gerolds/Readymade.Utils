using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace App.Core.POI
{
    public class PointOfInterestDisplay : MonoBehaviour
    {
        [Required] [SerializeField] private RectTransform container;
        [Required] [SerializeField] private RectTransform boundaryIndicator;
        [Required] [SerializeField] private TMP_Text scaleIndicator;

        public RectTransform Container => container;
        public RectTransform BoundaryIndicator => boundaryIndicator;
        public TMP_Text ScaleIndicator => scaleIndicator;
    }
}