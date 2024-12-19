using System;
using System.Collections.Generic;
using System.Linq;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Readymade.Utils
{
    public class Identity : MonoBehaviour
    {
        [Tooltip("A type object that can be used to identify this object.")]
        [SerializeField]
        #if ODIN_INSPECTOR
        [ListDrawerSettings(ShowPaging = false, ShowFoldout = false)]
#else
        [ReorderableList]
#endif
        public Object[] identity;

        private HashSet<Object> _identity;

        private void Awake()
        {
            _identity = identity.Where(it => it).ToHashSet();
        }

        /// This collection must not be modified.
        public ISet<Object> ID => _identity;

        public bool Contains(Object other)
        {
            return _identity.Contains(other);
        }

        public bool Overlaps(ISet<Object> others)
        {
            return _identity.Overlaps(others);
        }
    }
}