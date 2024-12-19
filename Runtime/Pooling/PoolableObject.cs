using NaughtyAttributes;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEngine;

namespace Readymade.Utils.Pooling
{
    /// <summary>
    /// Component that can be used to acquire pooled instances of it's GameObject. A pool will be automatically created for these instances with the settings given in the prefab.
    /// </summary>
    [AddComponentMenu(nameof(Readymade) + "/Pooling/" + nameof(PoolableObject))]
    public class PoolableObject : MonoBehaviour
    {
        [Tooltip(
            "Whether the pool for this object is limited to a fixed maximum capacity. Attempts to resize it will throw exceptions. Default is false.")]
        [SerializeField]
        private bool isFixed = false;

        [Tooltip("Whether to prewarm the pool immediately or expand it on demand. Default is true.")]
        [SerializeField]
        private bool preWarm = true;

        [Tooltip("The count of pooled objects that this prefab will produce. Default is 4.")]
        [SerializeField]
        [Min(4)]
        private int capacity = 4;

        /// <summary>
        /// Attempts to get an instance of this object from a pool. The instance will be placed at the world origin and be disabled.
        /// </summary>
        public bool TryGetInstance([AllowNull] out PooledInstance result) =>
            TryGetInstance(Vector3.zero, Quaternion.identity, null, out result, false);

        /// <summary>
        /// Attempts to get an instance of this object from a pool.
        /// </summary>
        /// <param name="parent">The parent for this instance. Can be null.</param>
        /// <param name="position">The world position to assign to the instance.</param>
        /// <param name="rotation">The world rotation to assign to the instance.</param>
        /// <param name="instance">The instance, if any. Null otherwise.</param>
        /// <param name="activate">Whether to activate the instance.</param>
        /// <returns>Whether an instance was available.</returns>
        public bool TryGetInstance(
            Vector3 position,
            Quaternion rotation,
            [AllowNull] Transform parent,
            [AllowNull] out PooledInstance instance,
            bool activate = true
        )
        {
            if (!GameObjectPool.TryGetPool(gameObject, out GameObjectPool pool))
            {
                if (isFixed)
                {
                    pool = GameObjectPool.GetFixedPool(gameObject, capacity, preWarm);
                }
                else
                {
                    pool = GameObjectPool.GetPool(gameObject, capacity, preWarm);
                }
            }

            // It is technically possible that the same prefab will be requested with different capacities, to cover this case we
            // expand the pool to the largest value of those. Fixed pools will throw an exceptions that that case. Typically
            // instances of PoolableObject should be unique per prefab.
            pool.ExpandTo(capacity);

            instance = default;
            return pool.TryGet(position, rotation, parent, out instance, activate);
        }

        /// <summary>
        /// Attempts to get the pool for a given prefab.
        /// </summary>
        /// <param name="pool">The pool, if any.</param>
        /// <returns>Whether a pool was found.</returns>
        /// <seealso cref="GetPool"/>
        public bool TryGetPool(out GameObjectPool pool) => GameObjectPool.TryGetPool(gameObject, out pool);

        /// <summary>
        /// Get a pool for a given prefab. If none exists, one is created.
        /// </summary>
        /// <returns>The pool.</returns>
        /// <seealso cref="TryGetPool"/>
        public GameObjectPool GetPool()
        {
            if (!GameObjectPool.TryGetPool(gameObject, out GameObjectPool pool))
            {
                if (isFixed)
                {
                    pool = GameObjectPool.GetFixedPool(gameObject, capacity, preWarm);
                }
                else
                {
                    pool = GameObjectPool.GetPool(gameObject, capacity, preWarm);
                }
            }

            return pool;
        }
    }

    /// <summary>
    /// Component that can be used to acquire pooled instances of it's GameObject. A pool will be automatically created for it.
    /// </summary>
    public abstract class PoolableObject<T> : PoolableObject
    {
        public bool TryGetInstance(out T result) =>
            TryGetInstance(Vector3.zero, Quaternion.identity, null, out result);

        public bool TryGetInstance(
            Vector3 position,
            Quaternion rotation,
            [AllowNull] Transform parent,
            [AllowNull] out T result,
            bool activate = true
        ) => TryGetInstance(position, rotation, parent, out result, out _, activate);

        public bool TryGetInstance(
            Vector3 position,
            Quaternion rotation,
            [AllowNull] Transform parent,
            [AllowNull] out T result,
            [AllowNull] out PooledInstance pooledObject,
            bool activate = true
        )
        {
            bool success = TryGetInstance(position, rotation, parent, out pooledObject, activate);
            if (success)
            {
                if (pooledObject.TryGetComponent<T>(out T component))
                {
                    result = component;
                    return true;
                }
                else
                {
                    pooledObject.Release();
                    result = default;
                    return false;
                }
            }
            else
            {
                result = default;
                return false;
            }
        }
    }
}