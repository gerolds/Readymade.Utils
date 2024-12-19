using UnityEngine;
using UnityEngine.Events;

namespace Readymade.Utils.Prototyping {
    public class UnityEventComponent : MonoBehaviour {
        [SerializeField]
        private UnityEvent onStart;

        [SerializeField]
        private UnityEvent onDestroy;

        [SerializeField]
        private UnityEvent onEnable;

        [SerializeField]
        private UnityEvent onDisable;

        private void Start () {
            onStart.Invoke ();
        }

        private void OnDestroy () {
            onDestroy.Invoke ();
        }

        private void OnEnable () {
            onEnable.Invoke ();
        }

        private void OnDisable () {
            onDisable.Invoke ();
        }
    }
}