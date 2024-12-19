using com.convalise.UnityMaterialSymbols;
using Readymade.Utils.Patterns;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes;
#endif

namespace Readymade.Utils.Feedback
{
    /// <summary>
    /// Spawns and animates <see cref="FloatingText"/> instance. This is a flyweight variant of <see cref="FloatingTextSpawner"/>
    /// that uses the global spawner it receives from the service locator. 
    /// </summary>
    /// <remarks>This intended only as a convenience target for UnityEvents.</remarks>
    public class FlyweightFloatingTextSpawner : MonoBehaviour
    {
        /// <summary>
        /// Reference to the settings for the animation.
        /// </summary>
        [SerializeField]
        [Required]
        [Tooltip("The settings to use for spawning and animating the spawned prefab.")]
        private FloatingTextSettings settings;

        private FloatingTextSpawner _spawner;

        /// <summary>
        /// event function.
        /// </summary>
        private void Start()
        {
            Services.TryGet(out _spawner);
            Debug.Assert(settings, "ASSERTION FAILED: settings != null", this);
            Debug.Assert(_spawner, "ASSERTION FAILED: _spawner != null", this);
        }

        /// <inheritdoc cref="FloatingTextSpawner.SpawnText(Sprite)"/>
        public void SpawnText(MaterialSymbolData icon)
        {
            SpawnText(0, transform.position, settings, icon);
        }

        /// <inheritdoc cref="FloatingTextSpawner.SpawnText(float)"/>
        public void SpawnText(float value)
        {
            SpawnText(value, transform.position, settings, default);
        }

        /// <inheritdoc cref="FloatingTextSpawner.SpawnText(int)"/>
        public void SpawnText(int value)
        {
            SpawnText(value, transform.position, settings, default);
        }

        /// <inheritdoc cref="FloatingTextSpawner.SpawnText(int, Vector3)"/>
        public void SpawnText(int value, Vector3 worldPosition)
        {
            SpawnText(value, worldPosition, settings, default);
        }

        /// <inheritdoc cref="FloatingTextSpawner.SpawnText(float, FloatingTextSettings)"/>
        public void SpawnText(float value, FloatingTextSettings overrideSettings)
        {
            SpawnText(value, transform.position, overrideSettings, default);
        }

        /// <inheritdoc cref="SpawnText(com.convalise.UnityMaterialSymbols.MaterialSymbolData)"/>
        public void SpawnText(
            float value,
            Vector3 worldPosition,
            FloatingTextSettings overrideSettings,
            MaterialSymbolData icon
        )
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (!_spawner)
            {
                Debug.LogWarning(
                    $"[{nameof(FlyweightFloatingTextSpawner)}] A call to {nameof(SpawnText)} was ignored because it occured before the component was initialized.",
                    this);
            }
            else
            {
                _spawner.SpawnText(value, worldPosition, overrideSettings, icon);
            }
        }
    }
}