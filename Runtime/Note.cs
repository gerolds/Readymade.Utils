using System;
using UnityEngine;

[AddComponentMenu("Note", 0)]
public class Note : MonoBehaviour
{
    [SerializeField]
    [HideInInspector]
    public string m_LastModified;

    [TextArea(5, 50)]
    [SerializeField]
    public string m_Note = "No comment";

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(Note))]
    [UnityEditor.CanEditMultipleObjects]
    public class NoteInspector : UnityEditor.Editor
    {
        private UnityEditor.SerializedProperty _note;
        private UnityEditor.SerializedProperty _lastModified;
        private bool _isEditing;
        private string _prevNote;

        private void OnEnable()
        {
            _note = serializedObject.FindProperty(nameof(Note.m_Note));
            _lastModified = serializedObject.FindProperty(nameof(Note.m_LastModified));
        }


        public override void OnInspectorGUI()
        {
            Note note = (Note) target;
            serializedObject.Update();


            /* REMOVE THIS
            SerializedProperty myProperty = null;
            IEnumerable<Texture2D> clean = RemoveDuplicates<Texture2D>(myProperty.GetEnumerator());
            IEnumerable<T> RemoveDuplicates<T>(IEnumerator input)
            {
                HashSet<T> result = new();
                while (input.MoveNext())
                    result.Add((T)input.Current);
                return result.ToHashSet();
            }
            */

            var textColor = new Color(0.85f, 0.77f, 0.38f);
            var bgColorLight = new Color(0.48f, 0.41f, 0.24f, 0.78f);
            var bgColorDark = new Color(.3f, .29f, .03f);
            var screenRect = GUILayoutUtility.GetRect(1, 1);
            var vertRect = UnityEditor.EditorGUILayout.BeginVertical();
            Color originalTextColor = UnityEditor.EditorStyles.label.normal.textColor;
            Color originalBgColor = GUI.backgroundColor;

            UnityEditor.EditorGUI.DrawRect(new Rect(screenRect.x - 18, screenRect.y - 4, screenRect.width + 22, vertRect.height + 18), bgColorDark);
            UnityEditor.EditorGUI.DrawRect(new Rect(screenRect.x - 18, screenRect.y - 4, 4, vertRect.height + 18), bgColorLight);
            if (_note.prefabOverride)
                UnityEditor.EditorGUI.DrawRect(new Rect(screenRect.x - 18, screenRect.y - 4, 2, vertRect.height + 18), textColor);


            if (_isEditing)
            {
                UnityEditor.EditorGUILayout.PropertyField(_note);
                if (GUILayout.Button("Save Note", UnityEditor.EditorStyles.miniButton))
                {
                    _isEditing = false;
                    _lastModified.stringValue = DateTimeOffset.UtcNow.ToString();

                    if (LevenshteinDistance(_prevNote, _note.stringValue) > 2)
                    {
                        _lastModified.stringValue = DateTimeOffset.UtcNow.ToString();
                        _prevNote = _note.stringValue;
                    }
                }
            }
            else
            {
                GUIStyle noteTextStyle = UnityEditor.EditorStyles.label;
                if (_note.prefabOverride)
                    noteTextStyle.fontStyle = FontStyle.Bold;
                noteTextStyle.normal.textColor = textColor;
                noteTextStyle.wordWrap = true;
                GUI.backgroundColor = textColor;

                UnityEditor.EditorGUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label(note.m_Note, noteTextStyle);
                UnityEditor.EditorGUILayout.EndVertical();

                noteTextStyle.normal.textColor = originalTextColor;
                noteTextStyle.fontStyle = FontStyle.Normal;
                GUI.backgroundColor = originalBgColor;

                GUILayout.BeginHorizontal();
                GUILayout.Label("Last Modified", UnityEditor.EditorStyles.miniLabel);
                GUILayout.Label(LastSaved, UnityEditor.EditorStyles.miniLabel);
                if (GUILayout.Button("Edit Note", UnityEditor.EditorStyles.miniButtonLeft))
                {
                    _isEditing = true;
                }

                GUILayout.EndHorizontal();
            }


            UnityEditor.EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }

        private string LastSaved =>
            !string.IsNullOrEmpty(_lastModified.stringValue) && DateTimeOffset.TryParse(_lastModified.stringValue, out DateTimeOffset parsed)
                ? $"{RelativeTimespan(DateTimeOffset.UtcNow - parsed)} "
                //+ $"{DateTimeOffset.Parse(_lastModified.stringValue):yyyy-M-d HH:mm:ss}"
                : "Never saved";

        private string RelativeTimespan(TimeSpan utcNow)
        {
            if (utcNow.TotalMinutes < 0)
                return "n/a";

            if (utcNow.TotalMinutes < 1)
                return "a few seconds ago";

            if (utcNow.TotalHours < 1)
                return $"{utcNow.TotalMinutes:0} minutes ago";

            if (utcNow.TotalHours < 12)
                return $"{utcNow.TotalHours:0} hours ago";

            if (utcNow.TotalDays < 1)
                return $"Today";

            if (utcNow.TotalDays < 2)
                return $"Yesterday";

            if (utcNow.TotalDays < 5)
                return $"{utcNow.TotalDays:0} ago";

            if (utcNow.TotalDays < 7)
                return $"This week";

            if (utcNow.TotalDays < 14)
                return $"Last week";

            if (utcNow.TotalDays < 30)
                return $"This month";

            if (utcNow.TotalDays < 60)
                return $"Last month";

            if (utcNow.TotalDays < 365)
                return $"This year to date";

            return $"Over a year ago";
        }

        private static int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(t))
                    return 0;
                return t.Length;
            }

            if (string.IsNullOrEmpty(t))
            {
                return s.Length;
            }

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // initialize the top and right of the table to 0, 1, 2, ...
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }

            return d[n, m];
        }
    }
#endif
}