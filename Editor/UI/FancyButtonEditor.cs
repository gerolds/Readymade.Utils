using Readymade.Utils.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace Readymade.Utils.Editor.UI
{
    /// <summary>
    /// Custom Editor for the <see cref="FancyButton"/> Component.
    /// </summary>
    [CustomEditor(typeof(FancyButton), true)]
    [CanEditMultipleObjects]
    public class FancyButtonEditor : ButtonEditor
    {
        SerializedProperty m_whenDisabledProperty;
        SerializedProperty m_onMiddleClickPropery;
        SerializedProperty m_onRightClickPropery;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_whenDisabledProperty = serializedObject.FindProperty("m_whenDisabled");
            m_onMiddleClickPropery = serializedObject.FindProperty("m_OnMiddleClick");
            m_onRightClickPropery = serializedObject.FindProperty("m_OnRightClick");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            GUILayout.Label("Fancy Properties", EditorStyles.boldLabel);

            serializedObject.Update();
            EditorGUILayout.PropertyField(m_whenDisabledProperty);
            EditorGUILayout.PropertyField(m_onMiddleClickPropery);
            EditorGUILayout.PropertyField(m_onRightClickPropery);
            serializedObject.ApplyModifiedProperties();
        }
    }
}