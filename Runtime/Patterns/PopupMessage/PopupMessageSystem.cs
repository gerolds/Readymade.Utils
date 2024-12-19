using System;
using Cysharp.Threading.Tasks;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Readymade.Utils.Patterns.PopupMessage
{
    /// <summary>
    /// Spawns temporary messages into a stack of other messages and kills them with a delay.
    /// </summary>
    public class PopupMessageSystem : MonoBehaviour
    {
        public enum MessageType
        {
            Default,
            Information,
            Warning,
            Error
        }

        [Tooltip("A prefab for messages.")]
        [SerializeField]
        [Required]
        private PopupMessageDisplay prefab;

        [Tooltip("The container to spawn messages into.")]
        [SerializeField]
        [Required]
        private RectTransform container;

        [Tooltip("The default duration that messages will stick. A value of 0 makes messages sticky.")]
        [SerializeField]
        [MinValue(0)]
        private float standardTimeout = 8f;

        [Tooltip(
            "The color for information messages. Will be multiplied with the color of the swatch in the popup message Prefab.")]
        [SerializeField]
        private Color information = new Color32(0x00, 0xFF, 0xCC, 0xFF);

        [Tooltip(
            "The color for warning messages. Will be multiplied with the color of the swatch in the popup message Prefab.")]
        [SerializeField]
        private Color warning = new Color32(0xFF, 0xCC, 0x00, 0xFF);

        [Tooltip("The color for error messages. Will be multiplied with the color of the swatch in the popup message Prefab.")]
        [SerializeField]
        private Color error = new Color32(0xFF, 0x33, 0x00, 0xFF);

        [Tooltip("The maximum number of messages before culling occurs.")]
        [SerializeField]
        [MinValue(0)]
        [MaxValue(100)]
        private int maxMessageCount = 20;

        [SerializeField]
        private UnityEvent onMessagePosted;

        /// <summary>
        /// Post a basic message with just a string, using default settings.
        /// </summary>
        /// <param name="message">The message to post.</param>
        public void ShowMessage(string message)
        {
            ShowMessage(message, standardTimeout);
        }

        /// <summary>
        /// Post a basic message with just a string, that is sticky, otherwise using default settings.
        /// </summary>
        /// <param name="message">The message to post.</param>
        public void ShowStickyMessage(string message)
        {
            ShowMessage(message, 0);
        }

        /// <summary>
        /// Post a sticky message with a callback.
        /// </summary>
        /// <param name="message">The message to post.</param>
        /// <param name="onConfirmed">The callback to invoke when the message is confirmed.</param>
        public void ShowMessage(string message, Action onConfirmed)
        {
            ShowMessage(message, 0, MessageType.Default, onConfirmed);
        }

        /// <summary>
        /// Post a sticky message if a specific type and with a callback.
        /// </summary>
        /// <param name="message">The message to post.</param>
        /// <param name="type">The type of this message.</param>
        /// <param name="onConfirmed">The callback to invoke when the message is confirmed.</param>
        public void ShowMessage(string message, MessageType type, Action onConfirmed)
        {
            ShowMessage(message, 0, type, onConfirmed);
        }

        private void ShowMessage(
            string message,
            float timeout,
            MessageType type = MessageType.Default,
            Action onConfirmed = null
        )
        {
            bool requireConfirmation = timeout <= 0;

            EnsureMaxSize();
            PopupMessageDisplay instance = Instantiate(prefab, container.transform);
            instance.confirmation.onClick.AddListener(() => ConfirmationHandler(instance, onConfirmed));
            instance.message.text = message;
            Color baseColor = instance.swatch.color;
            instance.swatch.color = type switch
            {
                MessageType.Default     => instance.swatch.color,
                MessageType.Information => MultiplyColors(information, baseColor),
                MessageType.Warning     => MultiplyColors(warning, baseColor),
                MessageType.Error       => MultiplyColors(error, baseColor),
                _                       => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };


            // we rebuild the layout here immediately to fix any content size fitters.
            LayoutRebuilder.ForceRebuildLayoutImmediate(instance.GetComponent<RectTransform>());

            if (!requireConfirmation)
            {
                if (instance.group)
                {
                    FadeAsync(instance.group, timeout).Forget();
                }

                Destroy(instance.gameObject, timeout);
            }

            onMessagePosted.Invoke();
        }

        private async UniTaskVoid FadeAsync(CanvasGroup instanceGroup, float duration)
        {
            float started = Time.time;
            while (instanceGroup && (started + duration) < Time.time && instanceGroup.alpha > 0)
            {
                instanceGroup.alpha = Mathf.Lerp(1f, 0, (Time.time - started) / duration);
                await UniTask.NextFrame();
            }
        }

        private void EnsureMaxSize()
        {
            maxMessageCount = Mathf.Min(100, Mathf.Max(0, maxMessageCount));
            if (container.transform.childCount > maxMessageCount)
            {
                while (transform.childCount > maxMessageCount)
                {
                    Destroy(transform.GetChild(0));
                }
            }
        }

        private static Color MultiplyColors(Color a, Color b) => new(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);

        private void ConfirmationHandler(PopupMessageDisplay instance, Action onConfirmed)
        {
            onConfirmed?.Invoke();
            Destroy(instance.gameObject);
        }

        /// <summary>
        /// Clear all current messages.
        /// </summary>
        public void ClearAll()
        {
            if (this && container)
            {
                for (int i = container.childCount - 1; i >= 0; i--)
                {
                    Destroy(container.GetChild(i).gameObject);
                }
            }
        }

        private void OnEnable()
        {
            ClearAll();
        }

        private void OnDisable()
        {
            ClearAll();
        }
    }
}