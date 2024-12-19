using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Sirenix.Utilities;
using UnityEngine;

namespace Readymade.Utils.Patterns
{
    public static class Services
    {
        private static readonly Dictionary<Type, object> s_services = new();
        private static readonly Dictionary<Type, (object factory, Mode mode)> s_factories = new();
        private static bool s_isLocked;

        public static bool IsIsLocked => s_isLocked;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnSceneLoad()
        {
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymode()
        {
            s_isLocked = false;
            s_factories.Clear();
            s_services.Clear();
        }
#endif

        public static void Lock()
        {
            s_isLocked = true;
        }

        public static T Get<T>()
        {
            T instance = default;
            if (!s_services.TryGetValue(typeof(T), out object service))
            {
                if (s_factories.TryGetValue(typeof(T), out (object factory, Mode mode) builder))
                {
                    instance = ((Func<T>)builder.factory).Invoke();
                    switch (builder.mode)
                    {
                        case Mode.PerCall:
                            break;
                        case Mode.Single:
                        // we should never end up here but if we do, this is what we want to we pretend this is
                        // a lazy factory.
                        case Mode.SingleLazy:
                            s_services[typeof(T)] = instance;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    throw new InvalidOperationException($"No service of type {typeof(T).Name} found.");
                }
            }
            else
            {
                instance = (T)service;
            }

            return instance;
        }
        
        public static bool TryGet<T>( out T instance)
        {
            instance = default;
            if (!s_services.TryGetValue(typeof(T), out object service))
            {
                if (s_factories.TryGetValue(typeof(T), out (object factory, Mode mode) builder))
                {
                    instance = ((Func<T>)builder.factory).Invoke();
                    switch (builder.mode)
                    {
                        case Mode.PerCall:
                            break;
                        case Mode.Single:
                        // we should never end up here but if we do, this is what we want to we pretend this is
                        // a lazy factory.
                        case Mode.SingleLazy:
                            s_services[typeof(T)] = instance;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                instance = (T)service;
            }

            return true;
        }

        public static void Register<T>(T instance, GameObject bindTo = default)
        {
            EnsureUnlocked();
            EnsureNoDuplicate(typeof(T));
            s_services[typeof(T)] = instance;
            if (bindTo)
            {
                BindToAsync<T>(bindTo).Forget();
            }
            else if (instance is Component component)
            {
                BindToAsync<T>(component.gameObject).Forget();
            }

            Debug.Log($"[{nameof(Services)}] Registered {typeof(T).GetNiceName()}.");
        }

        private static async UniTaskVoid BindToAsync<T>(GameObject bindTo)
        {
            await bindTo.OnDestroyAsync();
            s_services.Remove(typeof(T));
            Debug.Log($"[{nameof(Services)}] Unregistered {typeof(T).GetNiceName()}.");
        }

        public static void Register<T>(Func<T> factory, Mode mode = Mode.Single, GameObject bindTo = default)
        {
            EnsureUnlocked();
            EnsureNoDuplicate(typeof(T));

            s_factories[typeof(T)] = (factory, mode);
            if (bindTo != null)
            {
                Bind(bindTo).Forget();
            }

            if (mode == Mode.Single)
            {
                s_services[typeof(T)] = factory.Invoke();
            }

            return;

            async UniTaskVoid Bind(GameObject lifecycleSource)
            {
                await lifecycleSource.OnDestroyAsync();
                UnRegister<T>();
            }
        }

        private static void EnsureUnlocked()
        {
            if (s_isLocked)
                throw new InvalidOperationException(
                    $"The service locator was locked. Further changes to its configuration are not allowed.");
        }

        private static void EnsureNoDuplicate(Type type)
        {
            if (s_services.ContainsKey(type) || s_factories.ContainsKey(type))
            {
                throw new InvalidOperationException($"A service of type {type.Name} is already registered.");
            }
        }

        public enum Mode
        {
            Single,
            SingleLazy,
            PerCall
        }

        public static void UnRegister<T>()
        {
            s_services.Remove(typeof(T));
        }
    }
}