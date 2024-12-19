using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Common.Misc.Source.Common.Runtime
{
    public static class PrefabNameRevert
    {
        [MenuItem("Tools/Prefab/Revert Name")]
        static void RevertPrefabNames()
        {
            RevertAllNames(Selection.GetFiltered(typeof(GameObject), SelectionMode.Editable));
        }
        
        public static void RemoveNameModification(UnityEngine.Object aObj)
        {
            var mods = new List<PropertyModification>(PrefabUtility.GetPropertyModifications(aObj));
            for (int i = 0; i < mods.Count; i++)
            {
                if (mods[i].propertyPath == "m_Name")
                    mods.RemoveAt(i);
            }

            PrefabUtility.SetPropertyModifications(aObj, mods.ToArray());
        }

        public static void RevertAllNames(Object[] aObjects)
        {
            var items = new List<Object>();
            foreach (var item in aObjects)
            {
                var prefab = PrefabUtility.GetPrefabParent(item);
                if (prefab != null)
                {
                    Undo.RecordObject(item, "Revert perfab name");
                    item.name = prefab.name;
                    items.Add(item);
                }
            }

            Undo.FlushUndoRecordObjects();
            foreach (var item in items)
            {
                RemoveNameModification(item);
            }
        }
    }
}