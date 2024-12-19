#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes;
#endif
using System;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Readymade.Utils
{
    /// <inheritdoc />
    /// <summary>
    /// A unity event used by <see cref="T:Readymade.Utils.ActivateRandomChildOnAwake" />
    /// </summary>
    [Serializable]
    public class ActivationUnityEvent : UnityEvent<GameObject>
    {
    }

    /// <inheritdoc />
    /// <summary>
    /// Activates a random child of this transform on Awake and disables all others.
    /// </summary>
    public class ActivateRandomChildOnAwake : MonoBehaviour
    {
        private int _selected;

        [Tooltip("Will be invoked immediately after the child GameObject was picked.")]
        [SerializeField]
        private ActivationUnityEvent _onAwake;

        [Tooltip("Will be invoked on Start with the selected child GameObject as argument.")]
        [SerializeField]
        private ActivationUnityEvent _onStart;


        /// <summary>
        /// Unity event.
        /// </summary>
        private void Awake()
        {
            Randomize();
        }

        /// <summary>
        /// Unity event. Fires the configured <see cref="_onStart"/> event.
        /// </summary>
        private void Start()
        {
            if (transform.childCount > _selected)
            {
                _onStart.Invoke(transform.GetChild(_selected).gameObject);
            }
            else
            {
                Debug.LogWarning("Selected child is missing", this);
            }
        }

        /// <summary>
        /// Activates a random child of this transform on Awake and disables all others.
        /// </summary>
        [Button("Activate Random Child")]
        private void Randomize()
        {
            _selected = Random.Range(0, transform.childCount);
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(i == _selected);
            }

            _onAwake.Invoke(transform.GetChild(_selected).gameObject);
        }
    }
}