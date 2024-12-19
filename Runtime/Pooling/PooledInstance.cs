using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

namespace Readymade.Utils.Pooling
{
    /// <summary>
    /// Component placed on pooled game object by <see cref="GameObjectPool"/> to keep track of overrides to the objects
    /// active state that would affect the pools internal state.
    /// </summary>
    [AddComponentMenu(nameof(Readymade) + "/Pooling/" + nameof(PooledInstance))]
    public sealed class PooledInstance : MonoBehaviour, IDisposable
    {
        private GameObjectPool _pool;

        /// <summary>
        /// Signals the instance that it should return to the pool on its own terms.
        /// </summary>
        public void Forget(float delay)
        {
            UniTask.Void(async ct =>
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: ct).SuppressCancellationThrow();
                if (!ct.IsCancellationRequested)
                {
                    Release();
                }
            }, this.GetCancellationTokenOnDestroy());
        }

        /// <summary>
        /// Signals the instance that it should return to the pool when the given owner is destroyed.
        /// </summary>
        /// <param name="owner"></param>
        public void Forget(GameObject owner)
        {
            if (!transform.IsChildOf(owner.transform) && !owner.transform.IsChildOf(transform))
            {
                owner.OnDestroyAsync().ContinueWith(Release).Forget();
            }
            else
            {
                Debug.LogWarning("Cannot forget an object that is a parent or child of the owner.", this);
            }
        }

        /// <summary>
        /// The pool this instance belongs to. Defined only after <see cref="SetOwner"/> was called. 
        /// </summary>
        public GameObjectPool Pool
        {
            get
            {
                if (_pool == null)
                {
                    throw new InvalidOperationException(
                        $"Cannot access {nameof(Pool)} before the object is claimed by a pool.");
                }

                return _pool;
            }
        }

        /// <summary>
        /// Sets the pool that owns this instance.
        /// </summary>
        /// <param name="pool">The pool that owns this instance.</param>
        /// <exception cref="InvalidOperationException">When called again.</exception>
        /// <remarks>This is called before Awake() during object instantiation.</remarks>
        public void SetOwner(GameObjectPool pool)
        {
            if (_pool != null)
            {
                throw new InvalidOperationException("The owner of a pooled object cannot be changed.");
            }

            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        }

        /// <summary>
        /// Release this <see cref="GameObject"/> back into its pool.
        /// </summary>
        /// <remarks>
        /// If this is called from OnDisable, Unity will complain.
        /// </remarks>
        public void Release()
        {
            if (this)
            {
                Debug.Assert(_pool != null, "Polled instance has no owner. Was it created by a pool?", this);
                _pool.Release(this);
            }
        }

        /// <summary>Redirection of <see cref="IDisposable"/> to trigger a <see cref="Release"/>.</summary>
        /// <remarks>
        /// If this is called from OnDisable, Unity will complain.
        /// </remarks>
        public void Dispose() => Release();
    }
}