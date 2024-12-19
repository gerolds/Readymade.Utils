using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Readymade.Utils.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class LivePreviewRender : UIBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [Required] [SerializeField] private RenderTexture output;
        [Required] [SerializeField] private Graphic graphic;
        [Required] [SerializeField] private LivePreviewEnvironment environment;
        [SerializeField] private Color color = Color.gray;
        [SerializeField] private float tiltSpeed = -3f;
        [SerializeField] private float orbitSpeed = 3f;

        public void DisableEnvironment()
        {
            if (!environment)
            {
                return;
            }

            environment.Disable();
            if (graphic)
            {
                graphic.enabled = false;
            }
        }

        public void EnableEnvironment()
        {
            if (!environment)
            {
                return;
            }

            environment.Enable();
            if (graphic)
            {
                graphic.enabled = true;
            }
        }

        public void Set(GameObject prefab)
        {
            if (prefab && environment)
            {
                environment.Setup(prefab, color, output);
                EnableEnvironment();
            }
            else
            {
                DisableEnvironment();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
        }

        public void OnDrag(PointerEventData eventData)
        {
            environment.TiltBy(eventData.delta.y * tiltSpeed);
            environment.OrbitBy(eventData.delta.x * orbitSpeed);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            DisableEnvironment();
        }
    }
}