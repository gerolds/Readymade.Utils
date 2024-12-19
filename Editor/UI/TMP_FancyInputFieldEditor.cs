using Readymade.Utils.UI;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace Readymade.Utils.Editor.UI
{
    /// <summary>
    /// Custom Editor for the <see cref="FancyButton"/> Component.
    /// </summary>
    [CustomEditor(typeof(TMP_FancyInputField), true)]
    [CanEditMultipleObjects]
    public class TMP_FancyInputFieldEditor : TMP_InputFieldEditor
    {
        SerializedProperty m_shiftKey;
        SerializedProperty m_whenDisabledProperty;
        private SerializedProperty m_dragSensitivity;
        private SerializedProperty m_baseIncrement;
        private SerializedProperty m_shiftIncrement;
        private TMP_FancyInputField _instance;

        protected override void OnEnable()
        {
            base.OnEnable();
            _instance = (TMP_FancyInputField)target;
            m_shiftKey = serializedObject.FindProperty(nameof(_instance.m_shiftKey));
            m_whenDisabledProperty = serializedObject.FindProperty(nameof(_instance.m_whenDisabled));
            m_dragSensitivity = serializedObject.FindProperty(nameof(_instance.m_dragSensitivity));
            m_baseIncrement = serializedObject.FindProperty(nameof(_instance.m_baseIncrement));
            m_shiftIncrement = serializedObject.FindProperty(nameof(_instance.m_shiftIncrement));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.HelpBox(
                "This component provides scroll & drag-to-change functionality for the integer content type.",
                MessageType.Info);
            EditorGUILayout.Space();
            GUILayout.Label("Fancy Properties", EditorStyles.boldLabel);

            serializedObject.Update();
            EditorGUILayout.PropertyField(m_shiftKey);
            EditorGUILayout.PropertyField(m_whenDisabledProperty);
            EditorGUILayout.PropertyField(m_dragSensitivity);
            EditorGUILayout.PropertyField(m_baseIncrement);
            EditorGUILayout.PropertyField(m_shiftIncrement);
            serializedObject.ApplyModifiedProperties();
        }
    }
}