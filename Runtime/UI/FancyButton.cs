using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Readymade.Utils.UI
{
    /// <summary>
    /// A button that has additional features.
    /// </summary>
    /// <remarks>
    /// Added features:
    /// <list type="bullets">
    /// <item>Separate graphic that is toggled based on interactable-state.</item>
    /// <item>Right- and middle-click events.</item>
    /// </list>
    /// </remarks>
    public class FancyButton : Button
    {
        [Tooltip("This graphic is toggled based on the interactable-state of the button.")]
        [SerializeField]
        private Graphic m_whenDisabled;

        [SerializeField] private Button.ButtonClickedEvent m_OnRightClick = new();
        [SerializeField] private Button.ButtonClickedEvent m_OnMiddleClick = new();

        public Button.ButtonClickedEvent onRightClick
        {
            get { return m_OnRightClick; }
            set { m_OnRightClick = value; }
        }

        public Button.ButtonClickedEvent onMiddleClick
        {
            get { return m_OnMiddleClick; }
            set { m_OnMiddleClick = value; }
        }

        private void PressMiddle()
        {
            if (!IsActive() || !IsInteractable())
                return;

            UISystemProfilerApi.AddMarker("Button.onMiddleClick", this);
            m_OnMiddleClick.Invoke();
        }

        private void PressRight()
        {
            if (!IsActive() || !IsInteractable())
                return;

            UISystemProfilerApi.AddMarker("Button.onRightClick", this);
            m_OnRightClick.Invoke();
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