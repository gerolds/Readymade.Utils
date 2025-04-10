using System.Diagnostics;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes;
#endif
using Readymade.Utils.Patterns;
using Readymade.Utils;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

namespace Readymade.Utils.Portals
{
    public class PortalUser : MonoBehaviour
    {
        [BoxGroup("Transform")]
        [Tooltip("The root of the actor to be teleported. This GameObject will be deactivated and moved to the" +
            " portal exit.")]
        [SerializeField]
        [Required]
        private Transform userRootTransform;

        [BoxGroup("Events")] [SerializeField] private UnityEvent onEnter;
        [BoxGroup("Events")] [SerializeField] private PoseUnityEvent onExit;

        private PortalSystem _system;

        public Transform UserRoot => userRootTransform;

        private void Start()
        {
            _system = Services.Get<PortalSystem>();
        }

        public void OnEnter(PortalComponent portal)
        {
            var sw = Stopwatch.StartNew();
            onEnter?.Invoke();
            
            // disabling the user object while in transit keeps it from triggering colliders on scene load before
            // we had a chance to move it.
            //userRootTransform.gameObject.SetActive(false);
            Debug.Log(
                $"[{nameof(PortalUser)}] User '{name}' at portal '{portal.name}' completed after {sw.ElapsedMilliseconds}ms.",
                this);
        }

        public void OnExit(PortalExit exit)
        {
            if (userRootTransform)
            {
                userRootTransform.SetPositionAndRotation(exit.ExitPose.position, exit.ExitPose.rotation);
                userRootTransform.gameObject.SetActive(true);
                Debug.Log(
                    $"[{nameof(PortalUser)}] Moved user '{name}' to '{exit.Identity.name}' at {exit.ExitPose.position}.",
                    this);
            }
            else
            {
                Debug.Log(
                    $"[{nameof(PortalUser)}] User '{name}' has no {nameof(userRootTransform)} assigned and " +
                    $"will therefore not be moved to {exit.ExitPose.position}.",
                    this);
            }

            onExit?.Invoke(exit.ExitPose);
        }
    }
}