using App.Interactable;
using com.convalise.UnityMaterialSymbols;
using Readymade.Utils.Patterns;
using UnityEngine;

namespace App.Core.POI
{
    public class PointOfInterest : MonoBehaviour, ISystemComponent<SystemBase<PointOfInterest>>
    {
        private SystemBase<PointOfInterest> _system;
        [SerializeField] private MaterialSymbolData symbol;
        [SerializeField] private float maxRange = 1000f;
        [SerializeField] private Color color = Color.white;
        [SerializeField] private bool startVisible = true;

        private void Start()
        {
            _system = Services.Get<PointOfInterestSystem>();
            _system.Register(this);
            IsVisible = startVisible;
        }

        private void OnDestroy()
        {
            _system.UnRegister(this);
        }

        SystemBase<PointOfInterest> ISystemComponent<SystemBase<PointOfInterest>>.System
        {
            get => _system;
            set => _system = value;
        }

        public MaterialSymbolData Symbol => symbol;

        public float MaxRange => maxRange;

        public Color Color => color;

        public bool IsVisible { get; set; }
    }
}