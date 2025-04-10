using System;
using com.convalise.UnityMaterialSymbols;
using UnityEngine;

namespace App.Core.POI
{
    [RequireComponent(typeof(RectTransform))]
    public class BlipDisplay : MonoBehaviour
    {
        [SerializeField] private MaterialSymbol symbol;

        private void Reset()
        {
            if (!symbol)
            {
                symbol = GetComponentInChildren<MaterialSymbol>();
            }
        }

        private void OnValidate()
        {
            if (!symbol)
            {
                symbol = GetComponentInChildren<MaterialSymbol>();
            }
        }

        public RectTransform Pivot => (RectTransform)transform;
        public MaterialSymbol Symbol => symbol;
    }
}