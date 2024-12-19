#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes;
#endif
using System.Text;
using com.convalise.UnityMaterialSymbols;
using DG.Tweening;
using Readymade.Utils.Pooling;
using UnityEngine;
using Vertx.Debugging;
using Random = UnityEngine.Random;

namespace Readymade.Utils.Feedback
{
    /// <summary>
    /// Spawns and animates <see cref="FloatingText"/> instanced. Primarily intended for efficiently displaying numerical values.
    /// </summary>
    public class FloatingTextSpawner : MonoBehaviour
    {
        /// <summary>
        /// Prefab to spawn with this spawner.
        /// </summary>
        [SerializeField]
        [Header("Spawning")]
        [Tooltip("Prefab to spawn with this spawner.")]
        [Required]
        private FloatingText prefab;

        /// <summary>
        /// Reference to the settings for the animation.
        /// </summary>
        [SerializeField]
        [Required]
        [Tooltip("The settings to use for spawning and animating the spawned prefab.")]
        private FloatingTextSettings settings;

#if ODIN_INSPECTOR
        [ShowInInspector]
#else
        [ShowNonSerializedField]
#endif
        private float _debugValue = 10.01f;

        private static StringBuilder s_reusableStringBuilder = new();

        private void Awake()
        {
            Debug.Assert(prefab != null, "ASSERTION FAILED: _prefab != null", this);
            Debug.Assert(settings != null, "ASSERTION FAILED: _settings != null", this);
        }

        /// <summary>
        /// Spawns a debug instance of <see cref="FloatingText"/> with the value <see cref="_debugValue"/>.
        /// </summary>
        [Button]
        public void SpawnDebugText()
        {
            SpawnText(_debugValue);
        }

        /// <summary> Spawns a <see cref="FloatingText"/> at the spawner's location, displaying a specified value.</summary>
        /// <param name="value">The value that will be displayed.</param>
        public void SpawnText(float value)
        {
            SpawnText(value, transform.position);
        }

        /// <summary> Spawns a <see cref="FloatingText"/> at the spawner's location, displaying a specified value.</summary>
        /// <param name="value">The value that will be displayed.</param>
        public void SpawnText(int value)
        {
            SpawnText(value, transform.position, null);
        }

        /// <summary> Spawns a <see cref="FloatingText"/> at the spawner's location, displaying a specified value.</summary>
        /// <param name="value">The value that will be displayed.</param>
        /// <param name="worldPosition">The world position where to spawn the prefab. Offsets from settings will be added.</param>
        public void SpawnText(int value, Vector3 worldPosition)
        {
            SpawnText(value, worldPosition, null);
        }

        /// <summary> Spawns a <see cref="FloatingText"/> at the spawner's location, displaying a specified value.</summary>
        /// <param name="value">The value that will be displayed.</param>
        /// <param name="overrideSettings">An optional settings object to override the default configuration of the spawner.</param>
        public void SpawnText(float value, FloatingTextSettings overrideSettings)
        {
            SpawnText(value, transform.position, overrideSettings);
        }

        /// <summary>
        /// <summary> Spawns a <see cref="FloatingText"/> at given world position location, displaying a specified value.</summary>
        /// </summary>
        /// <param name="value">The value that will be displayed.</param>
        /// <param name="worldPosition">The world position where to spawn the prefab. Offsets from settings will be added.</param>
        /// <param name="overrideSettings">An optional settings object to override the default configuration of the spawner.</param>
        /// <param name="icon">An optional icon to display.</param>
        public void SpawnText(
            float value,
            Vector3 worldPosition,
            FloatingTextSettings overrideSettings = default,
            MaterialSymbolData icon = default
        )
        {
            if (!this)
            {
                // don't spawn if the object is destroyed
                Debug.LogWarning(
                    $"[{nameof(FloatingTextSpawner)}] Ignored a request to spawn text on a destroyed object.");
                return;
            }

            FloatingTextSettings set = overrideSettings != default
                ? overrideSettings
                : settings;

            // we do not return the instance so we can kill its tweens later and the user does not have to keep track of it or
            // interfere with the lifetime.
            if (prefab.TryGetInstance(
                    worldPosition + set.SpawnOffset + Random.insideUnitSphere * settings.RandomizePosition,
                    Quaternion.identity,
                    null,
                    out FloatingText instance
                )
            )
            {
                DOTween.Kill(instance); // just in case

                Debug.Assert(set, "ASSERTION FAILED: overrideSettings != null", this);

                Color color = value > 0
                    ? set.PositiveColor
                    : set.NegativeColor;

                if (set.ShowValue && instance.ValueText)
                {
                    if (set.HumanReadable)
                    {
                        float readableValue = ToReadable(value, out string readableFormat);
                        instance.ValueText.SetText(readableFormat, readableValue);
                    }
                    else
                    {
                        instance.ValueText.SetText(set.ValueFormat, value);
                    }
                }
                else
                {
                    instance.ValueText.SetText(string.Empty);
                }

                if (instance.ValueText)
                {
                    instance.ValueText.color = color;
                }

                if (instance.Icon)
                {
                    instance.Icon.symbol = (icon.code == default(char) ? set.Icon : icon);
                }

                instance.LookAtCameraComponent.Scale = set.Scale;

                if (set.ShowValue && set.AppendSuffix && instance.SuffixText)
                {
                    instance.SuffixText.text = settings.Suffix;
                    instance.SuffixText.color = color;
                }

                instance.SuffixText.enabled = set.ShowValue && set.AppendSuffix;
                Vector3 animationOffset = set.AnimationOffset;
                Vector3 targetPosition = instance.AnimationTarget.position + animationOffset;
                float fadeDuration = set.LifeTime;
                Sequence sequence = DOTween.Sequence()
                        .AppendInterval(set.Delay)
                        .Append(instance.Group.DOFade(0, fadeDuration))
                        .Join(instance.AnimationTarget.DOMove(targetPosition, set.LifeTime))
                    ;
                if (set.AnimateScale)
                {
                    sequence.Join(DOTween.To(
                            () => set.Scale,
                            scale => instance.LookAtCameraComponent.Scale = scale,
                            set.ScaleAtEnd,
                            set.LifeTime
                        )
                    );
                }

                sequence.AppendCallback(() => ReleaseInstance(instance.GetComponent<PooledInstance>()));
                sequence.SetTarget(instance.AnimationTarget);
                sequence.Restart();
            }
        }

        /// <summary>
        /// Turns a value into a human-readable format.
        /// </summary>
        /// <param name="value">The value to make readable.</param>
        /// <param name="format">The TMP_Text format string to display the readable value.</param>
        /// <returns>The value for display in <paramref name="format"/>.</returns>
        private static float ToReadable(float value, out string format)
        {
            s_reusableStringBuilder.Clear();

            float absoluteValue = Mathf.Abs(value);
            char magnitude = absoluteValue switch
            {
                >= 1_000_000_000 => 'B',
                >= 1_000_000 => 'M',
                >= 1_000 => 'K',
                _ => '\0'
            };

            float vSignificant = absoluteValue switch
            {
                >= 1_000_000_000 => absoluteValue / 1_000_000_000,
                >= 1_000_000 => absoluteValue / 1_000_000,
                >= 1_000 => absoluteValue / 1_000,
                _ => absoluteValue
            };

            string numberFormat = "{0:0}";
            if (Mathf.Abs(Mathf.Round(vSignificant) - vSignificant) > vSignificant / 10f)
            {
                numberFormat = vSignificant switch
                {
                    < 0.001f => "{0:4}",
                    < 0.01f => "{0:3}",
                    < 0.1f => "{0:3}", // we never want to display just one decimal because it looks confusing (for some reason -> UX testing)
                    < 1f => "{0:1}", // we never want to display just one decimal because it looks confusing (for some reason -> UX testing)
                    < 10f => "{0:0}", // we never want to display just one decimal because it looks confusing (for some reason -> UX testing)
                    _ => "{0:0}"
                };
            }

            if (value < 0)
            {
                s_reusableStringBuilder.Append('-');
            }

            s_reusableStringBuilder.Append(numberFormat);
            s_reusableStringBuilder.Append(magnitude);
            //s_sb.Append ( string.Format ( format, vSignificant ) );
            format = s_reusableStringBuilder.ToString();
            return vSignificant;
        }

        /// <summary>
        /// Releases the instance and kills its tweens.
        /// </summary>
        /// <param name="instance">The instance to be released back to the pool.</param>
        private void ReleaseInstance(PooledInstance instance)
        {
            DOTween.Kill(instance);
            instance.Release();
        }

        /// <summary> Unity event.</summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0x22, 0x88, 0xff, 0.5f);
            Gizmos.DrawWireSphere(transform.position + settings?.SpawnOffset ?? default, 0.3f);
            D.raw(new Shape.Text(transform.position + settings?.SpawnOffset ?? default, "FloatingTextSpawner"),
                Color.white,
                Color.black);
        }
    }
}