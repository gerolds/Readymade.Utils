using System;
using com.convalise.UnityMaterialSymbols;
using TMPro;
using UnityEngine;

namespace Readymade.Utils.Feedback
{
    public class TMPMaterialSymbol: TextMeshPro
    {
        [SerializeField] private MaterialSymbolData symbol;

        public MaterialSymbolData Symbol
        {
            get => symbol;
            set
            {
                symbol = value;
                UpdateSymbol();
            }
        }

        public char Code
        {
            get => symbol.code;
            set
            {
                symbol.code = value;
                UpdateSymbol();
            }
        }

        public bool Fill
        {
            get => symbol.fill;
            set
            {
                symbol.fill = value;
                UpdateSymbol();
            }
        }

        protected override void Start()
        {
            base.Start();

            if (string.IsNullOrEmpty(base.text))
            {
                Init();
            }

            if (font == null)
            {
                UpdateSymbol();
            }
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            Init();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            UpdateSymbol();
        }
#endif

        /// <summary> Properly initializes base Text class. </summary>
        private void Init()
        {
            symbol = new MaterialSymbolData('\uef55', false);

            base.text = default;
            base.color = Color.white;
            alignment = TextAlignmentOptions.Center;
            richText = false;
            base.autoSizeTextContainer = true;
            extraPadding = true;
            fontStyle = FontStyles.Normal;
            overflowMode = TextOverflowModes.Overflow;

            UpdateSymbol();
        }

        /// <summary> Updates font based on fill state. </summary>
        private void UpdateSymbol()
        {
            base.text = symbol.code.ToString();
        }

        /// <summary> Converts from unicode char to hexadecimal string representation. </summary>
        public static string ConvertCharToHex(char code)
        {
            try
            {
                return Convert.ToString(code, 16);
            }
            catch (Exception)
            {
                return default(string);
            }
        }

        /// <summary> Converts from hexadecimal string representation to unicode char. </summary>
        public static char ConvertHexToChar(string hex)
        {
            try
            {
                return Convert.ToChar(Convert.ToInt32(hex, 16));
            }
            catch (Exception)
            {
                return default(char);
            }
        }
    }
}