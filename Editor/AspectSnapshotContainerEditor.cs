using UnityEngine;
using UnityEditor;

namespace AspectSwitcher
{
    [CustomEditor(typeof(AspectSnapshotContainer))]
    public class AspectSnapshotContainerEditor : Editor
    {
        private AspectSnapshotContainer _target;

        private void OnEnable() => _target = (AspectSnapshotContainer)target;

        public override void OnInspectorGUI()
        {
            if (GUI.skin == null) { Repaint(); return; }
            serializedObject.Update();

            // ── Config (required) ───────────────────────────────────────────────────
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("config"),
                new GUIContent("State Config"));
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            if (_target.config == null)
            {
                EditorGUILayout.HelpBox(
                    "Assign a State Config to enable snapshot editing.",
                    MessageType.Warning);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            // ── Snapshot type / target ──────────────────────────────────────────────
            EditorGUILayout.Space(4f);
            var typeProp = serializedObject.FindProperty("type");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(typeProp);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                var defaultTarget = _target.type.GetDefaultTarget(_target.gameObject);
                if (defaultTarget != null)
                {
                    Undo.RecordObject(_target, "Auto-assign Snapshot Target");
                    serializedObject.FindProperty("target").objectReferenceValue = defaultTarget;
                }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("transitionOverride"), true);

            // ── Entries ─────────────────────────────────────────────────────────────
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("State Entries", EditorStyles.boldLabel);

            var entriesProp = serializedObject.FindProperty("entries");
            int toDelete    = -1;

            for (int i = 0; i < entriesProp.arraySize; i++)
            {
                if (!DrawEntry(entriesProp.GetArrayElementAtIndex(i), i))
                    toDelete = i;
            }
            if (toDelete >= 0)
                entriesProp.DeleteArrayElementAtIndex(toDelete);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Add Entry"))
            {
                serializedObject.ApplyModifiedProperties();
                Undo.RecordObject(_target, "Add State Entry");
                _target.entries.Add(new StateEntry
                {
                    state = (AspectState)0,
                    data  = _target.type.CreateData(),
                });
                EditorUtility.SetDirty(_target);
                serializedObject.Update();
            }
            if (GUILayout.Button("Capture All") && _target.target != null)
            {
                Undo.RecordObject(_target, "Capture All Snapshots");
                foreach (var e in _target.entries)
                    e.data?.CaptureFrom(_target.target);
                EditorUtility.SetDirty(_target);
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private bool DrawEntry(SerializedProperty entry, int index)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            var stateProp = entry.FindPropertyRelative("state");
            stateProp.intValue = (int)(AspectState)EditorGUILayout.EnumPopup(
                (AspectState)stateProp.intValue);

            bool deleted = GUILayout.Button("✕", GUILayout.Width(22f));
            EditorGUILayout.EndHorizontal();

            if (deleted) { EditorGUILayout.EndVertical(); return false; }

            var dataRef = entry.FindPropertyRelative("data");
            if (dataRef.managedReferenceValue == null)
            {
                if (GUILayout.Button("Initialize Data"))
                {
                    serializedObject.ApplyModifiedProperties();
                    if (index < _target.entries.Count)
                    {
                        Undo.RecordObject(_target, "Initialize Snapshot Data");
                        _target.entries[index].data = _target.type.CreateData();
                        EditorUtility.SetDirty(_target);
                        serializedObject.Update();
                    }
                }
            }
            else
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(dataRef, new GUIContent("Data"), true);
                EditorGUI.indentLevel--;

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Capture") && _target.target != null)
                {
                    serializedObject.ApplyModifiedProperties();
                    if (index < _target.entries.Count)
                    {
                        Undo.RecordObject(_target, "Capture Snapshot");
                        _target.entries[index].data.CaptureFrom(_target.target);
                        EditorUtility.SetDirty(_target);
                        serializedObject.Update();
                    }
                }
                if (GUILayout.Button("Preview") && _target.target != null)
                {
                    serializedObject.ApplyModifiedProperties();
                    if (index < _target.entries.Count)
                    {
                        Undo.RecordObjects(new UnityEngine.Object[] { _target, _target.target },
                            "Preview Snapshot");
                        _target.entries[index].data.ApplyTo(_target.target, null, 1f);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            return true;
        }
    }
}
