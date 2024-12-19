/* Copyright 2023 Gerold Schneider
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the “Software”), to
 * deal in the Software without restriction, including without limitation the
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */

using System;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Readymade.Utils
{
    /// <summary>
    /// Unifies various input-/control-sources for a <see cref="Canvas"/> enabled state and makes it observable. This is
    /// useful to easily hook up modal UI in the UnityEditor while also integrating it with a dedicated, higher level state
    /// manager that then doesn't have to reference a multitude of signal sources.
    /// </summary>
    public class CanvasToggle : MonoBehaviour
    {
        [BoxGroup("References")]
        [InfoBox(
            "Use this component to observe and control UI via Canvas/CanvasGroup toggles. Useful to unify input to and " +
            "from UI with various input methods and to hook up UI visibility as input source to other system, particularly" +
            "UI state managers. Facilitates a graceful transition from prototyping to fully specified UX.")]
        [Tooltip("The canvas that will be controlled by this component.")]
        [SerializeField]
        [Required]
        private Canvas targetCanvas;

        [BoxGroup("References")]
        [SerializeField]
        [Tooltip(
            "The canvas group to use to control interactable and blocking state. This should be on the same object as " +
            "or a child of the Canvas.")]
        private CanvasGroup group;

        [BoxGroup("Behaviour")]
        [Tooltip("Whether to enable the canvas on Start().")]
        [SerializeField]
        private bool startEnabled;

        [BoxGroup("Behaviour")]
        [Tooltip("The selectable component (if any) to become selected when the canvas is enabled.")]
        [SerializeField]
        private Selectable firstSelected;

        [BoxGroup("Behaviour")]
        [Tooltip(
            "Whether to restore a previous selection of a child when the canvas is enabled again.")]
        [SerializeField]
        private bool restoreLastSelected;

        [BoxGroup("Signal sources")]
        [Tooltip(
            "When enabled the component will not handle the enabled state of this canvas locally. This is useful when state of multiple canvases has to " +
            "coordinated in a modal way.")]
        [SerializeField]
        private bool handleSignalsExternally = false;

        [FormerlySerializedAs("useAsSignalSource")]
        [Tooltip(
            "When enabled, will collect input and redirect it to either control the toggle state internally or " +
            "expose it as signals for external handling. This is the primary use case for this component, it unifies " +
            "the solution to the most common issue with UI panels into one component, namely that it has multiple very " +
            "different triggers for a state change: pointer events, keyboard shortcuts, scripted calls, initial state " +
            "and state dependent on a parent, sibling or dependent system, together with changed-actions that need " +
            "to be invoked locally but should not be exposed to other systems.")]
        [BoxGroup("Signal sources")]
        [SerializeField]
        [DisableIf(nameof(handleSignalsExternally))]
        private bool isSignalSource = true;

        [BoxGroup("Signal sources")]
        [SerializeField]
        [ShowIf(nameof(isSignalSource))]
        private Button openButton;

        [BoxGroup("Signal sources")]
        [SerializeField]
        [ShowIf(nameof(isSignalSource))]
        private Button closeButton;

        [BoxGroup("Signal sources")]
        [SerializeField]
        [ShowIf(nameof(isSignalSource))]
        private Toggle toggle;

        [BoxGroup("Signal sources")]
        [SerializeField]
        [ShowIf(nameof(isSignalSource))]
        private InputActionReference toggleActionReference;

        [BoxGroup("Signal sources")]
        [SerializeField]
        [ShowIf(nameof(isSignalSource))]
        private InputActionReference closeActionReference;

        [BoxGroup("Signal sources")]
        [SerializeField]
        [ShowIf(nameof(isSignalSource))]
        private InputAction toggleAction;

        [BoxGroup("Signal sources")]
        [SerializeField]
        [ShowIf(nameof(isSignalSource))]
        private InputAction closeAction;

        [BoxGroup("Signal sources")]
        [Tooltip(
            "Whether to enable any assigned input action references. Useful if their enabled state is not yet " +
            "controlled anywhere else. The configured local action will be enabled regardless of this toggle.")]
        [SerializeField]
        [DisableIf(nameof(handleSignalsExternally))]
        [ShowIf(nameof(isSignalSource))]
        private bool enableActions;

        [BoxGroup("Audio")] [SerializeField] private AudioClip openSfx;

        [BoxGroup("Audio")] [SerializeField] private AudioClip loopSfx;

        [BoxGroup("Audio")] [SerializeField] private AudioClip closeSfx;

        [BoxGroup("Audio")] [SerializeField] private AudioSource audioSource;

        [BoxGroup("Events")]
        [SerializeField]
        [Tooltip("Called whenever the internal enabled state of the canvas changes.")]
        public CanvasEnabledUnityEvent onChanged;

        private Selectable _lastSelected;
        private bool _triggeredThisFrame;

        /// <summary>
        /// Called whenever the internal enabled state of the canvas changes.
        /// </summary>
        /// <remarks>
        /// Calls are unaffected by the enabled state of this component and the active state of the
        /// GameObject.
        /// </remarks>
        public event Action<bool> Changed;

        /// <summary>
        /// Called whenever a signal is received and <see cref="handleSignalsExternally"/> is enabled. Will be silent
        /// when signals are handled locally. The subscriber to this event is expected to call <see cref="SetEnabled"/>.
        /// </summary>
        public event Action<bool> SignalReceived;

        /// <summary>
        /// Internal flag to determine whether the component is handling signals internally.
        /// </summary>
        public bool IsHandlingSignals => !handleSignalsExternally && isSignalSource;

        /// <summary>
        /// Whether this component is configured for external signal handling. When true, the component will not handle
        /// signals internally and a subscriber to <see cref="SignalReceived"/> is expected to handle them. Calls to
        /// <see cref="SetEnabled"/> are expected to be handled manually.
        /// </summary>
        public bool NeedsExternalSignalHandling => handleSignalsExternally && isSignalSource;

        /// <summary>
        /// Whether the internal state (enabled state of the canvas) is true.
        /// </summary>
        /// <remarks>
        /// This value is unaffected by the enabled state of this component and the active state of
        /// the GameObject.
        /// </remarks>
        public bool IsEnabled => targetCanvas.enabled;

        private void OnValidate()
        {
            if (handleSignalsExternally)
            {
                isSignalSource = true;
            }
        }

        [Button("Discover components")]
        private void Reset()
        {
            if (!targetCanvas)
            {
                targetCanvas = GetComponent<Canvas>();
            }

            if (!group)
            {
                group = GetComponent<CanvasGroup>();
            }

            if (!audioSource)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        private void Update()
        {
            _triggeredThisFrame = false;
        }

        private void OnEnable()
        {
            // subscribe to signals

            if (IsHandlingSignals)
            {
                SubscribeToSignalSources();
            }

            // set initial state

            if (toggle)
            {
                toggle.SetIsOnWithoutNotify(startEnabled);
            }

            if (targetCanvas)
            {
                SetEnabled(startEnabled);
            }
        }

        private void OnDisable()
        {
            UnsubscribeSignalSources();
        }

        private void UnsubscribeSignalSources()
        {
            // unsubscribe signals

            if (toggleAction != null)
            {
                toggleAction.performed -= ToggleActionHandler;
            }

            if (closeAction != null)
            {
                closeAction.performed -= CloseActionHandler;
            }

            if (toggleActionReference != null)
            {
                toggleActionReference.action.performed -= ToggleActionHandler;
            }

            if (closeActionReference != null)
            {
                closeActionReference.action.performed -= CloseActionHandler;
            }

            if (openButton != null)
            {
                openButton.onClick.RemoveListener(OpenSignalHandler);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(CloseSignalHandler);
            }

            if (toggle != null)
            {
                toggle.onValueChanged.RemoveListener(SignalEnabled);
            }
        }

        private void SubscribeToSignalSources()
        {
            if (toggleAction != null)
            {
                toggleAction.performed += ToggleActionHandler;
                toggleAction.Enable();
            }

            if (closeAction != null)
            {
                closeAction.performed += CloseActionHandler;
                closeAction.Enable();
            }

            if (toggleActionReference != null)
            {
                toggleActionReference.action.performed += ToggleActionHandler;
                if (enableActions)
                {
                    toggleActionReference.action.Enable();
                }
            }

            if (closeActionReference != null)
            {
                closeActionReference.action.performed += CloseActionHandler;
                if (enableActions)
                {
                    closeActionReference.action.Enable();
                }
            }

            if (openButton != null)
            {
                openButton.onClick.AddListener(SignalToggle);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseSignalHandler);
            }

            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(SignalEnabled);
            }
        }

        private void ToggleActionHandler(InputAction.CallbackContext ctx)
        {
            if (ctx.action.WasPressedThisFrame())
            {
                SignalToggle();
            }
        }

        private void CloseActionHandler(InputAction.CallbackContext ctx)
        {
            if (ctx.action.WasPressedThisFrame())
            {
                SignalEnabled(false);
            }
        }

        private void OpenSignalHandler() => SignalEnabled(true);

        private void CloseSignalHandler() => SignalEnabled(false);

        /// <summary>
        /// Toggles the enabled state of the canvas. A call to this method will be interpreted as a signal,
        /// consequently the <see cref="SignalReceived"/> and <see cref="Changed"/> event will be invoked.
        /// Calling this method in response to <see cref="SignalReceived"/> will result in a stack overflow.
        /// </summary>
        /// <remarks>
        /// This is an internal method that is provided as a hook for extensions to this base implementation.
        /// </remarks>
        public void SignalToggle()
        {
            if (!targetCanvas)
            {
                return;
            }

            SignalEnabled(!targetCanvas.enabled);
        }

        /// <summary>
        /// Sets the enabled state of the canvas. A call to this method will be interpreted as a signal,
        /// consequently the <see cref="SignalReceived"/> and <see cref="Changed"/> event will be invoked.
        /// Calling this method in response to <see cref="SignalReceived"/> will result in a stack overflow.
        /// </summary>
        /// <param name="isOn">The desired enabled state of the canvas.</param>
        /// <remarks>
        /// This is an internal method that is provided as a hook for extensions to this base implementation.
        /// </remarks>
        /// <seealso cref="SetEnabled"/>
        public void SignalEnabled(bool isOn)
        {
            if (!targetCanvas)
            {
                return;
            }

            if (handleSignalsExternally)
            {
                if (isSignalSource)
                {
                    SignalReceived?.Invoke(isOn);
                }
            }
            else
            {
                SetEnabled(isOn);
            }
        }

        /// <summary>
        /// Toggles the enabled state of the canvas. A call to this method will <b>not</b> be interpreted as a signal,
        /// consequently only <see cref="Changed"/> event will be invoked.
        /// </summary>
        /// <param name="isEnabled">The desired enabled state of the canvas.</param>
        /// <seealso cref="SignalEnabled"/>
        public void SetEnabled(bool isEnabled)
        {
            if (!targetCanvas)
            {
                return;
            }

            if (_triggeredThisFrame)
            {
                return;
            }

            // will be reset in update
            _triggeredThisFrame = true;

            if (group)
            {
                group.interactable = isEnabled;
                group.blocksRaycasts = isEnabled;
            }

            if (toggle)
            {
                if (toggle.group.allowSwitchOff)
                {
                    toggle.group.SetAllTogglesOff();
                }

                toggle.SetIsOnWithoutNotify(isEnabled);
            }

            if (closeButton)
            {
                closeButton.interactable = isEnabled;
            }

            if (targetCanvas.enabled != isEnabled)
            {
                targetCanvas.enabled = isEnabled;

                if (audioSource)
                {
                    if (openSfx && isEnabled)
                    {
                        audioSource.PlayOneShot(openSfx);
                    }

                    if (closeSfx && !isEnabled)
                    {
                        audioSource.PlayOneShot(closeSfx);
                    }

                    if (loopSfx && isEnabled)
                    {
                        audioSource.clip = loopSfx;
                        audioSource.Stop();
                        if (openSfx)
                        {
                            audioSource.PlayDelayed(openSfx.length);
                        }
                        else
                        {
                            audioSource.Play();
                        }
                    }

                    if (!isEnabled)
                    {
                        audioSource.Stop();
                    }
                }

                if (isEnabled)
                {
                    if (restoreLastSelected && _lastSelected)
                    {
                        _lastSelected.Select();
                    }
                    else if (firstSelected)
                    {
                        firstSelected.Select();
                    }
                    else if (toggle)
                    {
                        toggle.Select();
                    }

                    // since we only toggle the canvas and not the entire gameObject, to prevent a needless re-layout
                    // on activation. However, in some cases we end up in a situation where the layout is not updated
                    // at all and the panel will be invisible until an update is triggered from somewhere. To fix this
                    // we request a re-layout whenever the canvas is enabled. In the future we might want to have a
                    // look at this again and figure out an efficient solution that detects these edge-cases.
                    LayoutRebuilder.MarkLayoutForRebuild((RectTransform)targetCanvas.transform);
                }
                else
                {
                    if (EventSystem.current)
                    {
                        if (targetCanvas &&
                            EventSystem.current.currentSelectedGameObject &&
                            EventSystem.current.currentSelectedGameObject.transform.IsChildOf(targetCanvas.transform) &&
                            EventSystem.current.currentSelectedGameObject.TryGetComponent(out Selectable selectable)
                        )
                        {
                            _lastSelected = selectable;
                            EventSystem.current.SetSelectedGameObject(null);
                        }
                    }
                }

                onChanged?.Invoke(targetCanvas.enabled);
                Changed?.Invoke(targetCanvas.enabled);
            }
        }
    }

    [Serializable]
    public class CanvasEnabledUnityEvent : UnityEvent<bool>
    {
    }
}