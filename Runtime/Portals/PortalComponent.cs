using System;
using Cysharp.Threading.Tasks;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes;
#endif
using Readymade.Utils.Patterns;
using UnityEngine;
using UnityEngine.Events;
using Vertx.Debugging;
using Debug = UnityEngine.Debug;

namespace Readymade.Utils.Portals
{
    /// <summary>
    /// Part of the portal system. Can be placed in the world and allows local interactions to trigger transitions
    /// between scenes or places when activated. Must be paired with a <see cref="PortalExit"/> to define the
    /// destination and a triggering component to activate the transition.
    /// </summary>
    public class PortalComponent : MonoBehaviour
    {
        public enum Mode
        {
            Local,
            Scene
        }

        [BoxGroup("Behaviour")]
        [SerializeField]
        private Mode portalMode;

        [BoxGroup("Exit")]
        [ShowIf(nameof(portalMode), Mode.Local)]
        [SerializeField]
        private PortalExit localExit;

        [BoxGroup("Exit")]
        [ShowIf(nameof(portalMode), Mode.Scene)]
        [SerializeField]
        private TransitConfig config;

        [BoxGroup("FX")] [SerializeField] private AudioClip transitStartClip;

        [BoxGroup("Events")] [SerializeField] private UnityEvent onEnter;

        public event Action<PortalUser> Entered;

        private PortalSystem _system;

        public TransitConfig Config => config;

        public Mode PortalMode => portalMode;

        public AudioClip EnterClip => transitStartClip;

        private void Start()
        {
            if (!_system)
            {
                _system = Services.Get<PortalSystem>();
            }

            _system.Register(this);
        }

        private void OnDestroy()
        {
            if (_system)
            {
                _system.UnRegister(this);
            }
        }

        /// <summary>
        /// Called by <see cref="PortalSystem"/>.
        /// </summary>
        /// <param name="user"></param>
        internal virtual void OnEnter(PortalUser user)
        {
            onEnter?.Invoke();
            Entered?.Invoke(user);
        }

        public async UniTask<bool> TryEnterAsync(PortalUser user)
        {
            return await _system.TryEnterAsync(this, user);
        }

        public async UniTask<bool> TryEnterAsync(GameObject userObject)
        {
            if (userObject.TryGetComponent<PortalUser>(out var user))
            {
                return await TryEnterAsync(user);
            }

            Debug.LogWarning(
                $"[{nameof(PortalComponent)}] {userObject.name} does not have a {nameof(PortalUser)} component.",
                userObject);

            return false;
        }

        public void Enter(UnityEngine.Object userObject)
        {
            Component component = userObject as Component;
            if (component && component.TryGetComponent<PortalUser>(out var user))
            {
                TryEnterAsync(user).Forget();
            }
        }

        public void Enter(GameObject userObject)
        {
            TryEnterAsync(userObject).Forget();
        }

        public void Enter(PortalUser user)
        {
            TryEnterAsync(user).Forget();
        }

        private void OnDrawGizmos()
        {
            if (portalMode == Mode.Local && localExit)
            {
                D.raw(new Shape.Arrow(new Shape.Line(transform.position, localExit.ExitPose.position)));
            }
        }
    }
}