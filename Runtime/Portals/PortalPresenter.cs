#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes;
#endif
using UnityEngine;

namespace Readymade.Utils.Portals
{
    [RequireComponent(typeof(PortalComponent))]
    public class PortalPresenter : MonoBehaviour
    {
        private PortalComponent _component;
        [SerializeField] [Required] private PortalDisplay display;

        private void Start()
        {
            _component = GetComponent<PortalComponent>();
            if (display.icon)
            {
                display.icon.sprite = _component?.Config?.ExitIdentity?.IconSprite;
            }

            if (display.label)
            {
                display.label.text = _component?.Config?.transitInfo;
            }
        }
    }
}