#if ODIN_INSPECTOR
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Readymade.Utils.Odin
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class OdinHelper
    {
        static OdinHelper()
        {
            _layers = null;
        }

        private static ValueDropdownList<int> _layers;

        public const string LayerDropdownProperty = nameof(OdinHelper) + "." + nameof(Layers);

        public static ValueDropdownList<int> Layers
        {
            get
            {
                if (_layers == null)
                {
                    _layers = new ValueDropdownList<int>();
                    _layers.AddRange(Enumerable.Range(0, 32)
                        .Select(it => new ValueDropdownItem<int>(LayerMask.LayerToName(it), it)));
                }

                return _layers;
            }
        }
    }
}
#endif