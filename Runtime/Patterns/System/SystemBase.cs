using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Readymade.Utils.Patterns.System
{
    public abstract class SystemBase<TAspect> : MonoBehaviour
        where TAspect : Object, ISystemComponent<SystemBase<TAspect>>
    {
        [SerializeField] private bool findOnStart;

        public event Action<SystemBase<TAspect>, ISystemComponent<SystemBase<TAspect>>, RegistrationEvent>
            CompositionChanged;

        private readonly ISet<TAspect> _components = new HashSet<TAspect>();

        public void Register(TAspect component)
        {
            if (_components.Add(component))
            {
                component.name = $"{component.name}__{_components.Count}";
                OnRegistered(component);
                CompositionChanged?.Invoke(this, component, RegistrationEvent.Added);
            }
        }

        protected abstract void OnRegistered(TAspect component);

        public void UnRegister(TAspect component)
        {
            if (_components.Remove(component))
            {
                OnUnregistered(component);
                CompositionChanged?.Invoke(this, component, RegistrationEvent.Removed);
            }
        }

        protected abstract void OnUnregistered(TAspect component);

        protected IEnumerator<TAspect> GetEnumerator() => _components.GetEnumerator();
        protected ISet<TAspect> Components => _components;
        protected int Count => _components.Count;

        public bool FindOnStart => findOnStart;

        protected void Clear() => _components.Clear();

        private void Start()
        {
            if (findOnStart)
            {
                FindAll();
            }
        }

        protected void FindAll()
        {
            TAspect[] components =
                FindObjectsByType<TAspect>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (TAspect component in components)
            {
                _components.Add(component);
                component.System = this;
            }
        }
    }

    public enum RegistrationEvent
    {
        Added,
        Removed
    }
}