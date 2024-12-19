using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Readymade.Utils.Pooling
{
    /// <summary>
    /// A automatic (auto-expanding) pool for GameObjects. Backs the ad-hoc usage of <see cref="PoolableObject{T}"/>.
    /// </summary>
    /// <remarks>
    /// Intended use case is to either allocate and pre-warm pools for specific prefabs or let them be created
    /// on-demand when instances are requested via
    /// <see cref="PoolableObject{T}"/>.<see cref="PoolableObject{T}.TryGetInstance(out T)"/> or
    /// <see cref="TryGetInstance(GameObject, out PooledInstance)"/> and overloads.
    /// </remarks>
    public sealed class GameObjectPool
    {
        //
        // TYPE API
        //

        private const int PoolCountWarningThreshold = 50;
        private const int InstanceCountWarningThreshold = 100;

        /// <summary>
        /// Tracks all pools.
        /// </summary>
        private static Dictionary<GameObject, GameObjectPool> s_allPools = new();

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void EnterPlaymodeHandler()
        {
            // in case the domain is not reloaded we have to clear tracked references manually.
            s_allPools.Clear();
        }
#endif

        /// <summary>
        /// Attempts to get the pool for a given prefab.
        /// </summary>
        /// <param name="prefab">The prefab for which to find a pool.</param>
        /// <param name="pool">The pool, if any.</param>
        /// <returns>Whether a pool was found.</returns>
        /// <remarks>For creating editor-configurable pools, use a <see cref="PoolableObject{T}"/> on the prefab to reference the pool and request instances.</remarks>
        public static bool TryGetPool([NotNull] GameObject prefab, [AllowNull] out GameObjectPool pool) =>
            s_allPools.TryGetValue(prefab, out pool);

        /// <summary>
        /// Creates a dynamically expanding pool for a given prefab. This method is idempotent: Subsequent calls for the same
        /// prefab will return the same instance.
        /// </summary>
        /// <param name="prefab">The prefab to pool.</param>
        /// <param name="initialCapacity">The initial capacity of the pool.</param>
        /// <param name="preWarm">Whether to allocate instances immediately.</param>
        /// <returns>The pool that was created.</returns>
        /// <remarks>For creating editor-configurable pools, use a <see cref="PoolableObject{T}"/> on the prefab to reference the pool and request instances.</remarks>
        public static GameObjectPool GetPool([NotNull] GameObject prefab, int initialCapacity = 4, bool preWarm = false)
        {
            if (s_allPools.TryGetValue(prefab, out GameObjectPool pool))
            {
                return pool;
            }

            if (s_allPools.Count > PoolCountWarningThreshold)
            {
                int total = s_allPools.Values.Sum(p => p.Count);
                Debug.LogWarning(
                    $"[{nameof(GameObjectPool)}] there are over {PoolCountWarningThreshold} pools for {total} instances. Was this intended?");
            }

            pool = new GameObjectPool
            {
                Prefab = prefab,
                _capacity = initialCapacity,
                _isExpanding = true
            };
            CreateContainer(pool);
            s_allPools[prefab] = pool;
            if (preWarm)
            {
                pool.PreWarm(pool._capacity);
            }

            return pool;
        }

        /// <summary>
        /// Creates a new fixed size pool for a given prefab. This method is idempotent: Subsequent calls for the same prefab
        /// will return the same instance.
        /// </summary>
        /// <param name="prefab">The prefab to pool.</param>
        /// <param name="capacity">The fixed capacity of the pool.</param>
        /// <param name="preWarm">Whether to immediately allocate instances in the pool.</param>
        /// <returns>The pool that was created.</returns>
        /// <remarks>For creating editor-configurable pools, use a <see cref="PoolableObject{T}"/> on the prefab to reference the pool and request instances.</remarks>
        public static GameObjectPool GetFixedPool([NotNull] GameObject prefab, int capacity, bool preWarm = false)
        {
            GameObjectPool pool = GetPool(prefab, capacity, false);
            pool._isExpanding = false;
            if (preWarm)
            {
                pool.PreWarm();
            }

            return pool;
        }

        /// <summary>
        /// Creates a container <see cref="GameObject"/> in the scene for a given pool.
        /// </summary>
        /// <param name="pool">The pool to create a container for.</param>
        private static void CreateContainer(GameObjectPool pool)
        {
            GameObject containerGo = new($"POOL {pool.Prefab.name}");
            containerGo.SetActive(true);
            pool._container = containerGo.transform;
        }

        /// <summary>
        /// Attempts to get a pooled instance of a given prefab object with a given world-transform. A pool will automatically be created for this prefab if none exists.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate and pool.</param>
        /// <param name="position">The position of the instance before activation.</param>
        /// <param name="rotation">The rotation of the instance before activation.</param>
        /// <param name="parent">The parent of the instance before activation.</param>
        /// <param name="instance">The pooled instance.</param>
        /// <param name="activate">Whether to activate the object immediately.</param>
        /// <returns>Whether a pooled instance was found.</returns>
        /// <remarks>For creating editor-configurable pools, use a <see cref="PoolableObject{T}"/> on the prefab to reference the pool and request instances.</remarks>
        public static bool TryGetInstance(
            [NotNull] GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            [AllowNull] Transform parent,
            [AllowNull] out PooledInstance instance,
            bool activate = true
        )
        {
            GameObjectPool pool = GetPool(prefab, 4, true);
            return pool.TryGet(position, rotation, parent, out instance, activate);
        }

        /// <summary>
        /// Attempts to get a pooled instance of a given prefab object. A pool will automatically be created for this prefab if none exists.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate and pool.</param>
        /// <param name="instance">The pooled instance.</param>
        /// <returns>Whether a pooled instance was found.</returns>
        /// <remarks>For creating editor-configurable pools, use a <see cref="PoolableObject{T}"/> on the prefab to reference the pool and request instances.</remarks>
        public static bool TryGetInstance([NotNull] GameObject prefab, [AllowNull] out PooledInstance instance)
        {
            GameObjectPool pool = GetPool(prefab, 4, true);
            return pool.TryGet(out instance);
        }

        /// <summary>
        /// Attempts to get a pooled instance of a given prefab component. A pool will automatically be created for this prefab if none exists.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate and pool.</param>
        /// <param name="component">The pooled instance.</param>
        /// <typeparam name="T">Component on the prefab to return.</typeparam>
        /// <returns>Whether a pooled instance was found.</returns>
        /// <remarks>For creating editor-configurable pools, use a <see cref="PoolableObject{T}"/> on the prefab to reference the pool and request instances.</remarks>
        public static bool TryGetInstance<T>([NotNull] T prefab, [AllowNull] out T component) where T : MonoBehaviour
        {
            GameObjectPool pool = GetPool(prefab.gameObject, 4, true);
            if (pool.TryGet(out PooledInstance pooledInstance) && pooledInstance.TryGetComponent(out component))
            {
                return true;
            }
            else
            {
                component = default;
                return false;
            }
        }

        /// <summary>
        /// Attempts to get a pooled instance of a given prefab component a given world-transform. A pool will automatically be created for this prefab if none exists.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate and pool.</param>
        /// <param name="position">The position of the instance before activation.</param>
        /// <param name="rotation">The rotation of the instance before activation.</param>
        /// <param name="parent">The parent of the instance before activation.</param>
        /// <param name="component">The pooled instance.</param>
        /// <param name="activate">Whether to activate the object immediately.</param>
        /// <typeparam name="T">Component on the prefab to return.</typeparam>
        /// <returns>Whether a pooled instance was found.</returns>
        /// <remarks>For creating editor-configurable pools, use a <see cref="PoolableObject{T}"/> on the prefab to reference the pool and request instances.</remarks>
        public static bool TryGetInstance<T>(
            T prefab,
            Vector3 position,
            Quaternion rotation,
            [AllowNull] Transform parent,
            [AllowNull] out T component,
            bool activate = true
        ) where T : MonoBehaviour
        {
            GameObjectPool pool = GetPool(prefab.gameObject, 4, true);
            if (pool.TryGet(
                    position,
                    rotation,
                    parent,
                    out PooledInstance pooledInstance,
                    activate
                ) && pooledInstance.TryGetComponent(out component))
            {
                return true;
            }
            else
            {
                component = default;
                return false;
            }
        }

        //
        // OBJECT API
        //

        /// <summary>
        /// The prefab that is pooled by this instance.
        /// </summary>
        private GameObject _prefab;

        /// <summary>
        /// Maximum number of instances that will be created in this pool.
        /// </summary>
        private int _capacity = 64;

        /// <summary>
        /// Number of instances that will be initially created in this pool.
        /// </summary>
        private const int _initialCount = 4;

        /// <summary>
        /// Inactive instances.
        /// </summary>
        private Queue<PooledInstance> _inactive = new();

        /// <summary>
        /// Active instances.
        /// </summary>
        private HashSet<PooledInstance> _active = new();

        /// <summary>
        /// Whether this pool can expand.
        /// </summary>
        private bool _isExpanding;

        private Transform _container;

        /// <summary>
        /// The prefab that is pooled by this instance.
        /// </summary>
        public GameObject Prefab
        {
            get => _prefab;
            internal set => _prefab = value;
        }

        /// <summary>
        /// Number of active instances that have been obtained from this pool.
        /// </summary>
        public int ActiveCount => _active.Count;

        /// <summary>
        /// Total number of instances this pool supports.
        /// </summary>
        public int Capacity => _capacity;

        /// <summary>
        /// Number of instances that can still be obtained from the pool.
        /// </summary>
        public int AvailableCount => Capacity - ActiveCount;

        /// <summary>
        /// Number of currently inactive instances.
        /// </summary>
        public int InactiveCount => _inactive.Count;

        /// <summary>
        /// Number of instances currently managed by the pool (inactive + active).
        /// </summary>
        public int Count => _inactive.Count + _active.Count;

        /// <summary>
        /// Whether this pool can expand.
        /// </summary>
        public bool IsExpanding => _isExpanding;

        /// <summary>
        /// The parent of all inactive instances.
        /// </summary>
        public Transform Container => _container;

        /// <summary>
        /// Creates instances in the pool based on <see cref="Capacity"/>. Allows the user to decide when the creation of
        /// instances should take place. 
        /// </summary>
        public void PreWarm() => PreWarm(_capacity);

        /// <summary>
        /// Creates a number of instances in the pool.
        /// </summary>
        /// <param name="count">Number of instances to create.</param>
        private void PreWarm(int count)
        {
            Debug.Assert(
                count + _active.Count + _inactive.Count <= _capacity,
                "ASSERTION FAILED: count + ActiveCount + InactiveCount <= MaxCount"
            );

            bool originalPrefabActiveState = _prefab.gameObject.activeSelf;
            _prefab.gameObject.SetActive(false);

            for (int i = 0; i < count; i++)
            {
                GameObject instance = Object.Instantiate(_prefab, Vector3.zero, Quaternion.identity, _container);
                if (!instance.TryGetComponent(out PooledInstance pooledObject))
                {
                    pooledObject = instance.AddComponent<PooledInstance>();
                    pooledObject.SetOwner(this);
                }
                else
                {
                    pooledObject.SetOwner(this);
                }

                _inactive.Enqueue(pooledObject);
            }

            _prefab.gameObject.SetActive(originalPrefabActiveState);
        }

        /// <summary>
        /// Attempts to get a disabled instance from this pool. The instance will be placed at the world origin.
        /// </summary>
        /// <param name="instance">The instance, if any. Null otherwise.</param>
        /// <returns>Whether an instance was found in the pool. False if the pool was at maximum capacity.</returns>
        public bool TryGet([AllowNull] out PooledInstance instance) =>
            TryGet(Vector3.zero, Quaternion.identity, null, out instance, false);

        /// <summary>
        /// Attempts to get an instance from this pool and places it in the world.
        /// </summary>
        /// <param name="parent">The parent under which to place the instance.</param>
        /// <param name="position">The position in world space for the instance.</param>
        /// <param name="rotation">The rotation in world space for the instance.</param>
        /// <param name="instance">The instance, if any. Null otherwise.</param>
        /// <param name="activate">Whether to activate the instance.</param>
        /// <returns>Whether an instance was found in the pool. False if the pool was at maximum capacity.</returns>
        public bool TryGet(
            Vector3 position,
            Quaternion rotation,
            [AllowNull] Transform parent,
            [AllowNull] out PooledInstance instance,
            bool activate = true
        )
        {
            EnsurePoolExists();

            // remove invalid entries (destroyed instances)
            if (_inactive.TryPeek(out PooledInstance entry) && !entry)
            {
                TrimInvalidEntries();
            }

            // expand _capacity on demand (if not fixed)
            if (_isExpanding && _inactive.Count == 0)
            {
                ExpandTo(_capacity * 2);
            }

            // create pooled instances on demand up to _capacity
            if (_inactive.Count == 0 && _active.Count < _capacity)
            {
                PreWarm(Mathf.Min(Mathf.Max(1, _active.Count * 2), _capacity - _active.Count));
            }

            if (_inactive.Count == 0)
            {
                instance = default;
                return false;
            }

            instance = _inactive.Dequeue();
            instance.transform.SetParent(parent);
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.transform.localScale = Vector3.one;
            instance.gameObject.SetActive(activate);
            if (!_active.Add(instance))
            {
                Debug.LogWarning(
                    $"[{nameof(GameObjectPool)}] object {instance.name} appears to be already active. This is " +
                    $"unexpected and should not happen. Was the object's active state changed outside the pool?");
            }

            return true;
        }

        /// <summary>
        /// Removes any invalid instances from the front of the queue.
        /// </summary>
        private void TrimInvalidEntries()
        {
            for (int i = _inactive.Count - 1; i >= 0; i--)
            {
                PooledInstance it = _inactive.Peek();
                if (it)
                {
                    break;
                }

                _inactive.Dequeue();
            }

            // we assume that if there is an invalid inactive item there are likely also invalid active ones.
            _active.RemoveWhere(it => !it);
        }

        /// <summary>
        /// Ensure that a pool exists and the initial count of instances is allocated.
        /// </summary>
        private void EnsurePoolExists()
        {
            Debug.Assert(_prefab != null, "ASSERTION FAILED: _prefab != null");
            Debug.Assert(_inactive != null, "ASSERTION FAILED: _inactive != null");
            Debug.Assert(_active != null, "ASSERTION FAILED: _active != null");
            if (Count < _initialCount)
            {
                PreWarm(Mathf.Max(_initialCount - Count, 0));
            }
        }

        /// <summary>
        /// Release a given object into the pool.
        /// </summary>
        /// <param name="pooledInstance">The object to release.</param>
        public void Release([NotNull] PooledInstance pooledInstance)
        {
            _active.Remove(pooledInstance);
            if (pooledInstance)
            {
                _inactive.Enqueue(pooledInstance);
                pooledInstance.transform.SetParent(_container);
                pooledInstance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                pooledInstance.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Expand the pool to a given minimum size.
        /// </summary>
        /// <param name="instanceCount">The minimum number of instances this pool should support.</param>
        public void ExpandTo(int instanceCount)
        {
            if (!_isExpanding)
            {
                throw new InvalidOperationException("Cannot expand a fixed pool.");
            }

            if (instanceCount > _capacity)
            {
                Debug.Log(
                    $"[{nameof(GameObjectPool)}] Expanding pool for prefab {_prefab.name} from {_capacity} to {instanceCount}");
                if (instanceCount > InstanceCountWarningThreshold)
                {
                    Debug.LogWarning(
                        $"[{nameof(GameObjectPool)}] There are now {instanceCount} instances pooled for prefab {_prefab.name}, this is a lot. Was this intended?");
                }
            }

            _capacity = Mathf.Max(instanceCount, _capacity);
        }
    }
}