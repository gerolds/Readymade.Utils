using System.Globalization;
using System.Threading;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Readymade.Utils.UI
{
    /// <summary>
    /// Allows an input field to be modified by scrolling/dragging on it or via UnityEvent callbacks.
    /// </summary>
    public class TMP_FancyInputField : TMP_InputField
    {
        [Tooltip("This graphic is toggled based on the interactable-state of the button.")]
        [SerializeField]
        public Graphic m_whenDisabled;

#if UNITY_EDITOR
        /// <summary>
        /// Event function.
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            EnsureContentType();
        }
#endif

        /// <summary>
        /// Modifies the count of the <see cref="Count"/> <see cref="TMP_InputField"/>.
        /// </summary>
        /// <param name="delta">The value to add to the count. Can be negative.</param>
        /// <remarks>Call this from UnityEvents that are part of the prefab that this display
        /// is part of. This is useful when the input field should be controlled by a button
        /// group for incrementing and decrementing its value.</remarks>
        public void AddToValue(int delta)
        {
            if (int.TryParse(text, NumberStyles.Integer, Thread.CurrentThread.CurrentCulture, out int count))
            {
                count += delta;
                text = count.ToString();
            }
        }

        private void EnsureContentType()
        {
            // set content type to integer number
            contentType = ContentType.IntegerNumber;
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            EnsureContentType();
        }
#endif

        [SerializeField]
#if ENABLE_INPUT_SYSTEM
        public Key m_shiftKey = Key.LeftShift;

        [SerializeField]
        public float m_dragSensitivity = 1f;

        [Min(0)]
        [SerializeField]
        public float m_baseIncrement = 1f;

        [Min(0)]
        [SerializeField]
        public float m_shiftIncrement = 10f;

        private float _acc;
#else
    private KeyCode m_shiftKey = KeyCode.LeftShift;
#endif

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (lineType != LineType.SingleLine && contentType != ContentType.IntegerNumber)
            {
                base.OnBeginDrag(eventData);
                return;
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (lineType != LineType.SingleLine && contentType != ContentType.IntegerNumber)
            {
                base.OnEndDrag(eventData);
                return;
            }
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (lineType != LineType.SingleLine && contentType != ContentType.IntegerNumber)
            {
                base.OnDrag(eventData);
                return;
            }

            if (eventData.delta.y != 0)
            {
#if ENABLE_INPUT_SYSTEM
                float scale = Keyboard.current != null && Keyboard.current[m_shiftKey].isPressed
                    ? m_shiftIncrement
                    : m_baseIncrement;
#else
                int scale = Input.GetKey ( m_shiftKey ) ? 10 : 1;
#endif
                int parsedCurrentValue = int.Parse(text, Thread.CurrentThread.CurrentCulture.NumberFormat);
                _acc += (eventData.delta.y > 0 ? scale : -scale) * m_dragSensitivity;
                int delta = _acc > 0 ? Mathf.FloorToInt(_acc) : Mathf.CeilToInt(_acc);
                text = (parsedCurrentValue + delta).ToString("D");
                _acc -= delta;

                m_CaretVisible = false;
                m_isSelectAll = false;
                m_CaretSelectPosition = 0;
                m_CaretPosition = 0;
            }
        }

        public override void OnScroll(PointerEventData eventData)
        {
            // Run base implementation on
            if (lineType != LineType.SingleLine && contentType != ContentType.IntegerNumber)
            {
                base.OnScroll(eventData);
                return;
            }

            if (!interactable)
            {
                return;
            }

            if (eventData.scrollDelta.y != 0)
            {
#if ENABLE_INPUT_SYSTEM
                float scale = Keyboard.current != null && Keyboard.current[m_shiftKey].isPressed
                    ? m_shiftIncrement
                    : m_baseIncrement;
#else
                int scale = Input.GetKey ( KeyCode.LeftShift ) ? 10 : 1;
#endif
                int parsedCurrentValue = int.Parse(text, Thread.CurrentThread.CurrentCulture.NumberFormat);
                _acc += (eventData.scrollDelta.y > 0 ? scale : -scale) * m_dragSensitivity;
                int delta = _acc > 0 ? Mathf.FloorToInt(_acc) : Mathf.CeilToInt(_acc);
                text = (parsedCurrentValue + delta).ToString("D");
                _acc -= delta;

                m_CaretVisible = false;
                m_isSelectAll = false;
                m_CaretSelectPosition = 0;
                m_CaretPosition = 0;
            }
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
    }
}