using System.Collections.Generic;

namespace App.Prototyping.MissileCommand
{
    /// <summary>
    /// Provides a registry for system components and ticks them at a specified interval. To be used as a component in a system.
    /// </summary>
    /// <typeparam name="TComponent"></typeparam>
    public sealed class SystemComponentRegistry<TComponent> where TComponent : ISystemComponent
    {
        public SystemComponentRegistry(float interval)
        {
            _interval = interval;
        }

        private readonly HashSet<TComponent> _components = new();
        private readonly float _interval;
        private float _nextTick;
        private float _lastTick;
        private float _time;
        private int _tickID;

        public HashSet<TComponent> Components => _components;
        public float Interval => _interval;
        public int Count => _components.Count;
        public int TickID => _tickID;

        public void Tick(float deltaTime)
        {
            _time += deltaTime;
            _tickID++;
            if (_nextTick < _time)
            {
                _nextTick = _time + _interval;
                Tick(_time - _lastTick);
                _lastTick = _time;
            }

            foreach (var component in Components)
            {
                component.Tick(deltaTime);
            }
        }

        public void Register(TComponent component) => Components.Add(component);

        public void UnRegister(TComponent component) => Components.Remove(component);
    }
}