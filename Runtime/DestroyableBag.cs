using System.Collections.Generic;
using UnityEngine;

namespace Readymade.Utils
{
    /// <summary>
    /// A generic bag that can hold any type of <see cref="UnityEngine.Object"/> and allows them
    /// to be destroyed together.
    /// </summary>
    /// <example>
    /// Useful for easily collecting instances for this explicit purpose while instancing them and
    /// then destroying them each time they are recreated.
    /// <code>
    /// private DestroyableBag _destroyOnRebuild = new ():
    /// 
    /// private void Rebuild () {
    ///     _destroyOnRebuild.DestroyAll();
    ///     
    ///     foreach ( var widget in widgets ) {
    ///         var instance = Instantiate ( widgetPrefab );
    ///         _destroyOnRebuild.Add ( instance );
    ///     }
    /// 
    ///     var marker = new GameObject ( "Special marker" );
    ///     _destroyOnRebuild.Add ( marker );
    /// }
    /// </code>
    /// </example>
    public class DestroyableBag
    {
        // backing storage that keeps items unique.
        private readonly ISet<Object> _backingStorage = new HashSet<Object>();

        /// <summary>
        /// Add an item to be bag.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(Object item) => _backingStorage.Add(item);

        /// <summary>
        /// Whether the bag contains a given item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool Contains(Object item) => _backingStorage.Contains(item);


        /// <summary>The number of items in the bag.</summary>
        public int Count => _backingStorage.Count;

        /// <summary>
        /// Destroys all items in the bag and clears it.
        /// </summary>
        public void DestroyAll()
        {
            foreach (Object item in _backingStorage)
            {
                // check if the item is still alive (could have been destroyed by some other process)
                if (item)
                {
                    Object.Destroy(item);
                }
            }
        }
    }
}