using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes;
#endif
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Readymade.Utils
{
    /// <summary>
    /// Fires events when a trigger collision starts and ends. Aggregates multiple trigger events into one event pair.
    /// </summary>
    /// <remarks>Trigger colliders that this object is inside of when enabled will not yield an entry event.</remarks>
    [RequireComponent(typeof(Collider))]
    public class TriggerBox : MonoBehaviour
    {
        [BoxGroup("Filtering")]
        [Tooltip("Only colliders with an " + nameof(Identity) + " component, in their parent hierarchy, that match " +
            "up with the references in the allow/reject list will be considered for collision events. Typically these " +
            "should be SOs that are used as type objects. When enabled, all contacts without an " + nameof(Identity) +
            " will be ignored.")]
        [SerializeField]
        private bool filterContacts;

        [BoxGroup("Filtering")]
        [ShowIf(nameof(filterContacts))]
#if !ODIN_INSPECTOR
        [ReorderableList]
#endif
        [SerializeField]
        [Tooltip("Identity objects that are allowed to trigger this " + nameof(TriggerBox) + ".")]
        private Object[] allow;

        [BoxGroup("Filtering")]
        [ShowIf(nameof(filterContacts))]
#if ODIN_INSPECTOR
        [ListDrawerSettings(ShowPaging = false, ShowFoldout = false)]
#else
        [ReorderableList]
#endif
        [SerializeField]
        [Tooltip("Identity objects that are ignored by this " + nameof(TriggerBox) + ".")]
        private Object[] reject;

        [BoxGroup("Events")]
        [Tooltip("Whether to fire event when this component is loaded.")]
        [SerializeField]
        private bool initialCheck = true;

        [BoxGroup("Events")]
        [Tooltip("Events to fire when any collider starts colliding with this object's collider.")]
        [SerializeField]
        private ObjectUnityEvent onAny;

        [BoxGroup("Events")]
        [Tooltip(
            "Events to fire when the last collider is no longer colliding with this object's. I.e. when there aren't any colliding colliders anymore.")]
        [SerializeField]
        private ObjectUnityEvent onNone;

        [BoxGroup("Events")]
        [Tooltip("Invoked when a contact exits the trigger.")]
        [SerializeField]
        private ObjectUnityEvent onContactExit;

        [BoxGroup("Events")]
        [Tooltip("Invoked when a contact enters the trigger.")]
        [SerializeField]
        private ObjectUnityEvent onContactEnter;

        [BoxGroup("Debugging")]
        [Tooltip("Whether to log debug messages.")]
        [SerializeField]
        private bool debug;

        [BoxGroup("Debugging")]
        [ShowInInspector]
        private HashSet<Collider> _contacts = new();

        /// <summary>
        /// Whether at least one collider is currently colliding with this object's collider.
        /// </summary>
        public bool HasContact => _contacts.Any();

        /// <summary>
        /// Called when the first contact enters the trigger.
        /// </summary>
        public event Action<TriggerBox, Object> Any;

        /// <summary>
        /// Called when the last contact leaves the trigger.
        /// </summary>
        public event Action<TriggerBox, Object> None;

        private Func<Collider, bool> _predicate;

        private bool _anyInvoked;
        private HashSet<Object> _reject;
        private HashSet<Object> _allow;
        private Object _lastTriggeringObject;
        private Collider _collider;
        private static List<Collider> s_toRemove = new();

        public ISet<Collider> Contacts => _contacts;

        private void Awake()
        {
            _allow = allow.Where(it => it != null).ToHashSet();
            _reject = reject.Where(it => it != null).ToHashSet();
            _collider = GetComponent<Collider>();
        }

        private void Start()
        {
            if (initialCheck)
            {
                HandleSetTriggers();
            }
        }

        /// <summary>
        /// Sets the predicate that determines whether a collider should be considered for collision events.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        public void SetValidationDelegate(Func<Collider, bool> predicate)
        {
            _predicate = predicate;

            int removedCount = 0;
            _contacts.ToList().ForEach(it =>
            {
                if (!predicate(it))
                {
                    if (_contacts.Remove(it))
                    {
                        removedCount++;
                        _lastTriggeringObject = it.gameObject;
                    }
                }
            });

            if (removedCount > 0)
            {
                HandleSetTriggers();
            }
        }


        /// <summary> Fire events based on current collision state. Fire <see cref="onNone"/> if no collider is present and <see cref="onAny"/> if any collider is present. </summary>
        private void HandleSetTriggers()
        {
            if (_contacts.Count == 0)
            {
                if (debug)
                {
                    Debug.Log("Invoking OnNone event", this);
                }

                onNone.Invoke(_lastTriggeringObject);
                None?.Invoke(this, _lastTriggeringObject);
                _anyInvoked = false;
            }
            else
            {
                if (!_anyInvoked)
                {
                    if (debug)
                    {
                        Debug.Log("Invoking OnAny event", this);
                    }

                    onAny.Invoke(_lastTriggeringObject);
                    Any?.Invoke(this, _lastTriggeringObject);
                    _anyInvoked = true;
                }
            }
        }

        private void OnDisable()
        {
            foreach (var contact in _contacts)
            {
                OnTriggerExit(contact);
            }

            _contacts.Clear();
        }

        private void FixedUpdate()
        {
            // detect colliders that have moved out of the trigger or deactivated.
            if (_contacts.Count > 0)
            {
                s_toRemove.Clear();
                foreach (var other in _contacts)
                {
                    if (!other)
                    {
                        s_toRemove.Add(other);
                    }
                    else if (!other.enabled)
                    {
                        s_toRemove.Add(other);
                    }
                    else if (!other.gameObject.activeInHierarchy)
                    {
                        s_toRemove.Add(other);
                    }
                    else if (!other.bounds.Intersects(_collider.bounds))
                    {
                        s_toRemove.Add(other);
                    }
                }

                foreach (var other in s_toRemove)
                {
                    OnTriggerExit(other);
                }
            }
        }

        /// <summary> Unity event. </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (filterContacts)
            {
                Identity otherIdentity = other.GetComponentInParent<Identity>();
                if (otherIdentity == null)
                {
                    return;
                }

                // reject all that don't have an identity or aren't in the allow list.
                // reject all unidentified objects.
                if (allow.Length > 0)
                {
                    if (!otherIdentity.Overlaps(_allow))
                    {
                        return;
                    }
                }

                // reject all that don't have an identity and aren't on the reject list.
                if (reject.Length > 0)
                {
                    if (otherIdentity.Overlaps(_reject))
                    {
                        return;
                    }
                }
            }

            if (_predicate != null && !_predicate(other))
            {
                return;
            }

            if (_contacts.Add(other))
            {
                if (debug)
                {
                    Debug.Log(
                        $"{other.name} has entered {name}. There are currently {_contacts.Count} active collisions in total",
                        this);
                }
            }
            else
            {
                if (debug)
                {
                    Debug.LogWarning(
                        $"{other.name} has entered {name} again, this suggests an exit event was not " +
                        $"detected. This is likely due to the collider being disabled or moved out of the trigger " +
                        $"in a way that prevents exit trigger events.",
                        this);
                }
            }

            _lastTriggeringObject = other.gameObject;
            onContactEnter.Invoke(other.gameObject);
            HandleSetTriggers();
        }

        /// <summary> Unity event. </summary>
        private void OnTriggerExit(Collider other)
        {
            if (_contacts.Remove(other))
            {
                if (debug)
                {
                    if (other)
                    {
                        Debug.Log(
                            $"{other.name} has exited {name}. There are currently {_contacts.Count} active collisions in total",
                            this);
                    }
                    else
                    {
                        Debug.Log(
                            $"{other} has exited {name}. There are currently {_contacts.Count} active collisions in total",
                            this);
                    }
                }

                _lastTriggeringObject = other;
                onContactExit.Invoke(other);
                HandleSetTriggers();
            }
        }
    }
}