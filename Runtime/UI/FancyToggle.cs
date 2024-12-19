using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Readymade.Utils.UI
{
    /// <summary>
    /// A toggle that has additional features.
    /// </summary>
    /// <remarks>
    /// Added features:
    /// <list type="bullets">
    /// <item>Separate graphic that is toggled based on interactable-state.</item>
    /// <item>Right- and middle-click events.</item>
    /// </list>
    /// </remarks>
    public class FancyToggle : Toggle
    {
        [Tooltip("This graphic is toggled based on the interactable-state of the button.")]
        [SerializeField]
        private Graphic m_whenDisabled;

        [SerializeField] private ToggleEvent m_OnRightClick = new();
        [SerializeField] private ToggleEvent m_OnMiddleClick = new();

        public ToggleEvent onRightClick
        {
            get { return m_OnRightClick; }
            set { m_OnRightClick = value; }
        }

        public ToggleEvent onMiddleClick
        {
            get { return m_OnMiddleClick; }
            set { m_OnMiddleClick = value; }
        }

        private void PressMiddle()
        {
            if (!IsActive() || !IsInteractable())
                return;

            UISystemProfilerApi.AddMarker("Toggle.onMiddleClick", this);
            m_OnMiddleClick.Invoke(isOn);
        }

        private void PressRight()
        {
            if (!IsActive() || !IsInteractable())
                return;

            UISystemProfilerApi.AddMarker("Toggle.onRightClick", this);
            m_OnRightClick.Invoke(isOn);
        }

        /// <inheritdoc />
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            if (m_whenDisabled)
            {
                m_whenDisabled.gameObject.SetActive(state == SelectionState.Disabled);
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                PressRight();
            }
            else if (eventData.button == PointerEventData.InputButton.Middle)
            {
                PressMiddle();
            }
            else
            {
                base.OnPointerClick(eventData);
            }
        }
    }
}