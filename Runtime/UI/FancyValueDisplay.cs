using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Readymade.Utils.UI
{
    public class FancyValueDisplay : MonoBehaviour
    {
        [SerializeField] public int initialValue;
        [SerializeField] private Vector2Int range = new(0, 100);
        [SerializeField] public CanvasGroup group;
        [SerializeField] public TMP_Text readout;
        [SerializeField] public TMP_Text minReadout;
        [SerializeField] public TMP_Text maxReadout;
        [SerializeField] public Slider slider;
        [SerializeField] public TMP_FancyInputField inputField;
        [SerializeField] public Button increment;
        [SerializeField] public Button decrement;
        [SerializeField] public Button reset;
        [SerializeField] public Image fill;
        [SerializeField] public CountUnityEvent onValueChanged;
        [SerializeField] public AudioClip tickClip;

        public int Value { get; private set; }

        private void Reset()
        {
            group = GetComponentInChildren<CanvasGroup>();
            readout = GetComponentInChildren<TMP_Text>();
            slider = GetComponentInChildren<Slider>();
            inputField = GetComponentInChildren<TMP_FancyInputField>();
            increment = GetComponentsInChildren<Button>()
                .FirstOrDefault(it =>
                    it.name.Contains("increment", StringComparison.InvariantCultureIgnoreCase) ||
                    it.name.Contains("add", StringComparison.InvariantCultureIgnoreCase) ||
                    it.name.Contains("plus", StringComparison.InvariantCultureIgnoreCase) ||
                    it.name.Contains("more", StringComparison.InvariantCultureIgnoreCase) ||
                    it.name.Contains("up", StringComparison.InvariantCultureIgnoreCase) ||
                    it.name.Contains("pos", StringComparison.InvariantCultureIgnoreCase) ||
                    it.name.Contains("right", StringComparison.InvariantCultureIgnoreCase)
                );
            decrement = GetComponentsInChildren<Button>()
                .FirstOrDefault(it =>
                    it.name.Contains("decrement", StringComparison.InvariantCultureIgnoreCase) ||
                    it.name.Contains("subtract", StringComparison.InvariantCultureIgnoreCase) ||
                    it.name.Contains("minus", StringComparison.InvariantCultureIgnoreCase) ||
                    it.name.Contains("down", StringComparison.InvariantCultureIgnoreCase) ||
                    it.name.Contains("less", StringComparison.InvariantCultureIgnoreCase) ||
                    it.name.Contains("neg", StringComparison.InvariantCultureIgnoreCase) ||
                    it.name.Contains("left", StringComparison.InvariantCultureIgnoreCase) ||
                    it.name.Contains("remove", StringComparison.InvariantCultureIgnoreCase)
                );
            reset = GetComponentsInChildren<Button>()
                .FirstOrDefault(it =>
                    it.name.Contains("clear", StringComparison.InvariantCultureIgnoreCase) ||
                    it.name.Contains("default", StringComparison.InvariantCultureIgnoreCase) ||
                    it.name.Contains("zero", StringComparison.InvariantCultureIgnoreCase) ||
                    it.name.Contains("reset", StringComparison.InvariantCultureIgnoreCase)
                );
            if (slider)
            {
                slider.wholeNumbers = true;
                range = new Vector2Int((int)slider.minValue, (int)slider.maxValue);
            }

            if (inputField)
            {
                inputField.characterValidation = TMP_InputField.CharacterValidation.Integer;
            }
        }

        private void Start()
        {
            Range = range;
            TrySetValueWithoutNotify(initialValue);
        }

        public void OnValidate()
        {
            if (slider)
            {
                slider.wholeNumbers = true;
            }

            if (inputField)
            {
                inputField.characterValidation = TMP_InputField.CharacterValidation.Integer;
            }
        }

        public void OnEnable()
        {
            if (slider)
            {
                slider.onValueChanged.AddListener(SliderHandler);
                slider.minValue = range.x;
                slider.maxValue = range.y;
            }

            if (inputField)
            {
                inputField.onValueChanged.AddListener(InputFieldHandler);
            }

            if (increment)
            {
                increment.onClick.AddListener(IncrementHandler);
            }

            if (decrement)
            {
                decrement.onClick.AddListener(DecrementHandler);
            }

            if (reset)
            {
                reset.onClick.AddListener(ResetHandler);
            }
        }

        private void OnDisable()
        {
            if (slider)
            {
                slider.onValueChanged.RemoveListener(SliderHandler);
            }

            if (inputField)
            {
                inputField.onValueChanged.RemoveListener(InputFieldHandler);
            }

            if (increment)
            {
                increment.onClick.RemoveListener(IncrementHandler);
            }

            if (decrement)
            {
                decrement.onClick.RemoveListener(DecrementHandler);
            }

            if (reset)
            {
                reset.onClick.RemoveListener(ResetHandler);
            }
        }

        private void SliderHandler(float value) => TrySetValue(Mathf.RoundToInt(value));

        private void IncrementHandler() => TrySetValue(Value + 1);

        private void DecrementHandler() => TrySetValue(Value - 1);

        private void ResetHandler() => TrySetValue(initialValue);

        private void InputFieldHandler(string value)
        {
            if (int.TryParse(value, out int parsedValue))
            {
                TrySetValue(parsedValue);
            }
        }

        public Vector2Int Range
        {
            get => range;
            set
            {
                range = value;
                if (slider)
                {
                    slider.minValue = range.x;
                    slider.maxValue = range.y;
                }

                if (minReadout)
                {
                    minReadout.SetText("{0}", range.x);
                }

                if (maxReadout)
                {
                    maxReadout.SetText("{0}", range.y);
                }

                TrySetValue(Value);
            }
        }

        public bool TrySetValue(int value)
        {
            if (TrySetValueWithoutNotify(value))
            {
                onValueChanged.Invoke(Value);
                return true;
            }

            return false;
        }

        public bool TrySetValueWithoutNotify(int value)
        {
            int oldValue = Value;
            Value = Mathf.Clamp(value, range.x, range.y);
            readout?.SetText("{0}", readout);
            slider?.SetValueWithoutNotify(Value);
            inputField?.SetTextWithoutNotify(Value.ToString("D"));

            if (increment)
            {
                increment.interactable = Value < range.y;
            }

            if (decrement)
            {
                decrement.interactable = Value > range.x;
            }

            if (fill)
            {
                fill.fillAmount = (Value - range.x) / (float)(range.y - range.x);
            }

            return Value != oldValue;
        }

        public void SetRange(Vector2Int valueRange)
        {
            range = valueRange;
            TrySetValueWithoutNotify(Value);
        }
    }

    [Serializable]
    public class CountUnityEvent : UnityEvent<int>
    {
    }
}