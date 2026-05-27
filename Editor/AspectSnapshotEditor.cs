using UnityEngine;
using UnityEditor;

namespace AspectSwitcher
{
    [CustomEditor(typeof(AspectSnapshot), true)]
    public class AspectSnapshotEditor : Editor
    {
        private AspectSnapshot _target;

        private void OnEnable() => _target = (AspectSnapshot)target;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_switcher"), new GUIContent("Switcher"));
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            if (_target.Switcher == null)
            {
                EditorGUILayout.HelpBox(
                    "Assign an AspectRatioStateSwitcher. The snapshot self-registers on Enable.",
                    MessageType.Warning);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            if (_target.Switcher.config == null)
            {
                EditorGUILayout.HelpBox(
                    "The assigned Switcher has no State Config. Add one to the Switcher first.",
                    MessageType.Warning);
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("transitionOverride"), true);

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
                entriesProp.arraySize++;
                var newElem    = entriesProp.GetArrayElementAtIndex(entriesProp.arraySize - 1);
                var statesProp = newElem.FindPropertyRelative("states");
                statesProp.arraySize = 1;
                statesProp.GetArrayElementAtIndex(0).intValue = 0;
            }
            if (GUILayout.Button("Capture All") && _target.target != null)
            {
                serializedObject.ApplyModifiedProperties();
                Undo.RecordObject(_target, "Capture All Snapshots");
                for (int i = 0; i < entriesProp.arraySize; i++)
                    _target.GetDataAt(i)?.CaptureFrom(_target.target);
                EditorUtility.SetDirty(_target);
                serializedObject.Update();
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private bool DrawEntry(SerializedProperty entry, int index)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            var statesProp = entry.FindPropertyRelative("states");

            if (statesProp.arraySize == 0)
            {
                statesProp.arraySize = 1;
                statesProp.GetArrayElementAtIndex(0).intValue = 0;
            }

            int toRemoveState = -1;
            for (int si = 0; si < statesProp.arraySize; si++)
            {
                var sp = statesProp.GetArrayElementAtIndex(si);
                sp.intValue = (int)(AspectState)EditorGUILayout.EnumPopup(
                    (AspectState)sp.intValue, GUILayout.MinWidth(80f), GUILayout.MaxWidth(150f));

                if (statesProp.arraySize > 1 && GUILayout.Button("✕", GUILayout.Width(18f)))
                    toRemoveState = si;
            }

            if (GUILayout.Button("+ State", GUILayout.Width(56f)))
            {
                statesProp.arraySize++;
                statesProp.GetArrayElementAtIndex(statesProp.arraySize - 1).intValue = 0;
            }

            GUILayout.FlexibleSpace();
            bool deleted = GUILayout.Button("✕", GUILayout.Width(22f));
            EditorGUILayout.EndHorizontal();

            if (toRemoveState >= 0)
                statesProp.DeleteArrayElementAtIndex(toRemoveState);

            if (deleted) { EditorGUILayout.EndVertical(); return false; }

            var dataProp = entry.FindPropertyRelative("data");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(dataProp, new GUIContent("Data"), true);
            EditorGUI.indentLevel--;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Capture") && _target.target != null)
            {
                serializedObject.ApplyModifiedProperties();
                Undo.RecordObject(_target, "Capture Snapshot");
                _target.GetDataAt(index).CaptureFrom(_target.target);
                EditorUtility.SetDirty(_target);
                serializedObject.Update();
            }
            if (GUILayout.Button("Preview") && _target.target != null)
            {
                serializedObject.ApplyModifiedProperties();
                Undo.RecordObjects(new UnityEngine.Object[] { _target, _target.target }, "Preview Snapshot");
                _target.GetDataAt(index).ApplyTo(_target.target, null, 1f);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            return true;
        }
    }
}
