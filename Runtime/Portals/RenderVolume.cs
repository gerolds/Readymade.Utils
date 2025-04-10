using NaughtyAttributes;
using Readymade.Utils.Patterns;
using UnityEngine;
using Vertx.Debugging;

namespace Readymade.Utils.Portals
{
    [RequireComponent(typeof(Collider))]
    [ExecuteAlways]
    public class RenderVolume : MonoBehaviour
    {
        [Required] [SerializeField] private RenderVolumeProfile profile;

        private RenderVolumeSystem _system;
        private Collider _collider;
        public RenderVolumeProfile Profile => profile;

        public Collider Collider => _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            Debug.Assert(_collider, "No collider found on RenderVolume. Please add one.", this);
        }

        private void Start()
        {
            if (Application.isPlaying)
            {
                _system = Services.Get<RenderVolumeSystem>();
                Debug.Assert(_collider, "No RenderVolumeSystem found in ServiceLocator. Please add one.", this);
                _system.Register(this);
            }
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                _system = FindAnyObjectByType<RenderVolumeSystem>();
                Debug.Assert(_system, "No RenderVolumeSystem found in the scene. Please add one.", this);
                _system?.Register(this);
            }
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                if (_system)
                {
                    _system.UnRegister(this);
                }
            }
        }

        private void OnDestroy()
        {
            if (Application.isPlaying)
            {
                if (_system)
                {
                    _system.UnRegister(this);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out RenderVolumeUser user))
            {
                _system?.OnEnter(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out RenderVolumeUser user))
            {
                _system?.OnExit(this);
            }
        }

        private void OnDrawGizmosSelected()
        {
            D.raw(GetComponent<Collider>(), new Color(1f, .5f, 0, .5f));
        }
    }
}